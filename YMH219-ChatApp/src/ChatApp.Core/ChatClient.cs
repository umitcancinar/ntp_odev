using System.Net.Sockets;

namespace ChatApp.Core;

public sealed class ChatClient : IAsyncDisposable
{
    private JsonLineConnection? wire;
    private CancellationTokenSource? lifetime;
    private Task? receiveTask;
    private volatile bool connected;
    public bool IsConnected => connected;
    public event Action<ChatPacket>? MessageReceived;
    public event Action<string>? Disconnected;

    // Connect/Disconnect are serialized by caller. Send may run concurrently.
    public async Task ConnectAsync(string host, int port, string user)
    {
        user = ChatRules.User(user);
        if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("Sunucu adresi gerekli.");
        if (port is < 1 or > 65535) throw new ArgumentOutOfRangeException(nameof(port));
        if (connected) throw new InvalidOperationException("Önce bağlantıyı kesin.");
        await DisconnectAsync().ConfigureAwait(false);
        var socket = new TcpClient { NoDelay = true };
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(6));
        JsonLineConnection? next = null;
        try
        {
            await socket.ConnectAsync(host, port, tokenSource.Token).ConfigureAwait(false);
            next = new JsonLineConnection(socket);
            await next.WriteAsync(new ChatPacket("join", user), tokenSource.Token).ConfigureAwait(false);
            var welcome = await next.ReadAsync(tokenSource.Token).ConfigureAwait(false);
            if (welcome?.Type != "welcome") throw new InvalidDataException("Sunucu katılmayı onaylamadı.");
            tokenSource.CancelAfter(Timeout.InfiniteTimeSpan);
            wire = next; lifetime = tokenSource; connected = true;
            receiveTask = ReceiveAsync(next, tokenSource.Token);
        }
        catch { next?.Dispose(); socket.Dispose(); tokenSource.Dispose(); throw; }
    }

    public async Task SendAsync(string text)
    {
        text = ChatRules.Message(text);
        var active = wire;
        if (!connected || active == null || lifetime == null) throw new InvalidOperationException("Sunucuya bağlı değilsiniz.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        timeout.CancelAfter(TimeSpan.FromSeconds(3));
        try { await active.WriteAsync(new ChatPacket("message", Text: text), timeout.Token).ConfigureAwait(false); }
        catch { active.Dispose(); throw; }
    }

    private async Task ReceiveAsync(JsonLineConnection active, CancellationToken token)
    {
        string reason = "Sunucu bağlantıyı kapattı.";
        try
        {
            while (await active.ReadAsync(token).ConfigureAwait(false) is { } packet)
            {
                if (packet.Type is not ("message" or "system" or "clients")) throw new InvalidDataException("Bilinmeyen sunucu paketi.");
                MessageReceived?.Invoke(packet);
            }
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or SocketException or ObjectDisposedException or OperationCanceledException)
        { reason = token.IsCancellationRequested ? "Bağlantı kesildi." : $"Bağlantı sonlandı: {ex.Message}"; }
        finally { connected = false; active.Dispose(); Disconnected?.Invoke(reason); }
    }

    public async Task DisconnectAsync()
    {
        connected = false;
        lifetime?.Cancel(); wire?.Dispose();
        if (receiveTask != null) await receiveTask.ConfigureAwait(false);
        lifetime?.Dispose(); lifetime = null; wire = null; receiveTask = null;
    }
    public async ValueTask DisposeAsync() => await DisconnectAsync().ConfigureAwait(false);
}
