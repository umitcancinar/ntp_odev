using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace ChatApp.Core;

public sealed class ChatServer : IAsyncDisposable
{
    private sealed class Peer(TcpClient socket) : IDisposable
    {
        public JsonLineConnection Wire { get; } = new(socket);
        public ClientInfo? Info { get; set; }
        public string Endpoint { get; } = socket.Client.RemoteEndPoint?.ToString() ?? "?";
        public void Dispose() => Wire.Dispose();
    }
    private readonly ConcurrentDictionary<Peer, byte> peers = new();
    private readonly List<Task> sessions = [];
    private readonly SemaphoreSlim broadcastGate = new(1, 1);
    private TcpListener? listener;
    private CancellationTokenSource? lifetime;
    private Task? acceptTask;
    public int Port { get; private set; }
    public bool IsRunning => listener != null;
    public IReadOnlyList<ClientInfo> Clients => peers.Keys.Select(p => p.Info).OfType<ClientInfo>().OrderBy(i => i.User).ToArray();
    public event Action<ChatPacket>? MessageReceived;
    public event Action? ClientsChanged;
    public event Action<string>? Error;

    // Start/Stop are lifecycle operations: caller serializes them (the UI does).
    public void Start(int port, bool allowLan = false)
    {
        if (listener != null) throw new InvalidOperationException("Sunucu zaten açık.");
        if (port is < 0 or > 65535) throw new ArgumentOutOfRangeException(nameof(port));
        var next = new TcpListener(allowLan ? IPAddress.Any : IPAddress.Loopback, port);
        next.Start();
        listener = next;
        Port = ((IPEndPoint)next.LocalEndpoint).Port;
        lifetime = new CancellationTokenSource();
        acceptTask = AcceptAsync(next, lifetime.Token);
    }

    private async Task AcceptAsync(TcpListener server, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var socket = await server.AcceptTcpClientAsync(token).ConfigureAwait(false);
                socket.NoDelay = true;
                var peer = new Peer(socket);
                peers.TryAdd(peer, 0);
                // Session catches expected connection errors; retain tasks until shutdown to observe all completion.
                lock (sessions)
                {
                    sessions.RemoveAll(t => t.IsCompletedSuccessfully);
                    sessions.Add(ServeAsync(peer, token));
                }
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or SocketException or ObjectDisposedException)
        { if (!token.IsCancellationRequested) Error?.Invoke($"Sunucu kabul hatası: {ex.Message}"); }
    }

    private async Task ServeAsync(Peer peer, CancellationToken token)
    {
        try
        {
            using var joinTimeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            joinTimeout.CancelAfter(TimeSpan.FromSeconds(10));
            var hello = await peer.Wire.ReadAsync(joinTimeout.Token).ConfigureAwait(false);
            if (hello?.Type != "join") throw new InvalidDataException("İlk paket join olmalı.");
            string user = ChatRules.User(hello.User ?? "");
            await peer.Wire.WriteAsync(new ChatPacket("welcome", user, Timestamp: DateTimeOffset.UtcNow), token).ConfigureAwait(false);
            // Publish only after welcome: another session must not broadcast before the handshake.
            peer.Info = new ClientInfo(user, peer.Endpoint);
            ClientsChanged?.Invoke();
            await BroadcastAsync(new ChatPacket("system", user, "sohbete katıldı", DateTimeOffset.UtcNow), token).ConfigureAwait(false);
            await BroadcastClientsAsync(token).ConfigureAwait(false);
            while (await peer.Wire.ReadAsync(token).ConfigureAwait(false) is { } packet)
            {
                if (packet.Type == "leave") break;
                if (packet.Type != "message") throw new InvalidDataException("Bilinmeyen paket türü.");
                string text = ChatRules.Message(packet.Text ?? "");
                await BroadcastAsync(new ChatPacket("message", user, text, DateTimeOffset.UtcNow), token).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or SocketException or OperationCanceledException or ObjectDisposedException or ArgumentException)
        { if (!token.IsCancellationRequested) Error?.Invoke($"{peer.Endpoint}: bağlantı sonlandı ({ex.Message})"); }
        finally
        {
            peers.TryRemove(peer, out _);
            peer.Dispose();
            if (peer.Info is { } info)
            {
                ClientsChanged?.Invoke();
                if (!token.IsCancellationRequested)
                {
                    try
                    {
                        await BroadcastAsync(new ChatPacket("system", info.User, "sohbetten ayrıldı", DateTimeOffset.UtcNow), token).ConfigureAwait(false);
                        await BroadcastClientsAsync(token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { }
                }
            }
        }
    }

    private Task BroadcastClientsAsync(CancellationToken token) =>
        BroadcastAsync(new ChatPacket("clients", Clients: Clients.ToArray()), token);

    private async Task BroadcastAsync(ChatPacket packet, CancellationToken token)
    {
        await broadcastGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            if (packet.Type != "clients") MessageReceived?.Invoke(packet);
            foreach (var peer in peers.Keys.Where(p => p.Info != null))
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeout.CancelAfter(TimeSpan.FromSeconds(3));
                try { await peer.Wire.WriteAsync(packet, timeout.Token).ConfigureAwait(false); }
                catch (Exception ex) when (ex is InvalidDataException or IOException or SocketException or OperationCanceledException or ObjectDisposedException)
                { peer.Dispose(); }
            }
        }
        finally { broadcastGate.Release(); }
    }

    public async Task StopAsync()
    {
        if (listener == null) return;
        lifetime!.Cancel();
        listener.Stop();
        foreach (var peer in peers.Keys) peer.Dispose();
        if (acceptTask != null) await acceptTask.ConfigureAwait(false);
        Task[] pending; lock (sessions) pending = sessions.ToArray();
        await Task.WhenAll(pending).ConfigureAwait(false);
        sessions.Clear();
        lifetime.Dispose(); lifetime = null; listener = null; acceptTask = null;
        ClientsChanged?.Invoke();
    }
    public async ValueTask DisposeAsync() => await StopAsync().ConfigureAwait(false);
}
