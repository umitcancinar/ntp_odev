using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatApp.Core;

// One reader per connection; all writers share the same gate.
internal sealed class JsonLineConnection(TcpClient socket) : IDisposable
{
    private const int MaxFrameBytes = 16_384;
    private readonly NetworkStream stream = socket.GetStream();
    private readonly SemaphoreSlim writeGate = new(1, 1);
    private readonly byte[] buffer = new byte[4096];
    private int position, length;
    private static readonly UTF8Encoding Utf8 = new(false, true);

    public async Task<ChatPacket?> ReadAsync(CancellationToken token)
    {
        using var line = new MemoryStream();
        while (true)
        {
            if (position == length)
            {
                length = await stream.ReadAsync(buffer, token).ConfigureAwait(false);
                position = 0;
                if (length == 0)
                {
                    if (line.Length != 0) throw new InvalidDataException("Eksik protokol satırı.");
                    return null;
                }
            }
            byte value = buffer[position++];
            if (value == (byte)'\n')
            {
                try
                {
                    return JsonSerializer.Deserialize<ChatPacket>(Utf8.GetString(line.ToArray()))
                        ?? throw new InvalidDataException("Boş JSON paketi.");
                }
                catch (Exception ex) when (ex is JsonException or DecoderFallbackException)
                { throw new InvalidDataException("Geçersiz UTF-8/JSON paketi.", ex); }
            }
            if (line.Length >= MaxFrameBytes) throw new InvalidDataException("Protokol satırı çok uzun.");
            line.WriteByte(value);
        }
    }

    public async Task WriteAsync(ChatPacket packet, CancellationToken token)
    {
        byte[] bytes = Utf8.GetBytes(JsonSerializer.Serialize(packet) + "\n");
        if (bytes.Length > MaxFrameBytes) throw new InvalidDataException("Protokol satırı çok uzun.");
        await writeGate.WaitAsync(token).ConfigureAwait(false);
        try { await stream.WriteAsync(bytes, token).ConfigureAwait(false); }
        finally { writeGate.Release(); }
    }

    public void Dispose() => socket.Dispose();
}
