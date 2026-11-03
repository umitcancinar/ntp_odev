using System.Net;
namespace ChatApp.Core;
public sealed record ClientInfo(string User, string Endpoint);
public sealed record ChatPacket(string Type, string? User = null, string? Text = null, DateTimeOffset? Timestamp = null, ClientInfo[]? Clients = null);
public sealed class ChatServer : IAsyncDisposable {
 public int Port => 0;
 public IReadOnlyList<ClientInfo> Clients => Array.Empty<ClientInfo>();
 public event Action<ChatPacket>? MessageReceived;
 public event Action? ClientsChanged;
 public void Start(int port, bool allowLan = false) => throw new NotImplementedException("TCP sunucusu henüz uygulanmadı");
 public Task StopAsync() => Task.CompletedTask;
 public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
public sealed class ChatClient : IAsyncDisposable {
 public bool IsConnected => false;
 public event Action<ChatPacket>? MessageReceived;
 public event Action<string>? Disconnected;
 public Task ConnectAsync(string host, int port, string user) => throw new NotImplementedException("TCP istemcisi henüz uygulanmadı");
 public Task SendAsync(string text) => throw new NotImplementedException();
 public Task DisconnectAsync() => Task.CompletedTask;
 public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
