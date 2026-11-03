using ChatApp.Core;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

var tests = new (string Name, Func<Task> Run)[] {
 ("İki kullanıcı ve zaman damgalı UTF-8 mesaj", UnicodeChat),
 ("İstemci listesi ve ayrılma", Membership),
 ("Sunucu durunca istemci kapanır ve tekrar bağlanır", Restart),
 ("Reddedilen bağlantıdan sonra yeniden bağlanır", FailedConnect),
 ("Boş kullanıcı ve boş/uzun mesaj reddedilir", Validation),
 ("Bozuk JSON bağlantıyı kapatır, sunucu çalışır", Malformed),
 ("Aşırı uzun çerçeve bağlantıyı kapatır", Oversized),
 ("Eşzamanlı mesajlar birbirine karışmaz", ConcurrentMessages),
 ("Yarım çerçeve sunucu durmasını engellemez", PartialShutdown),
 ("Tekrarlanan bağlantı ve ayrılma kaynakları bırakır", RepeatedSessions)
};
int failures = 0;
foreach (var test in tests) {
 try { await test.Run().WaitAsync(TimeSpan.FromSeconds(12)); Console.WriteLine($"PASS {test.Name}"); }
 catch (Exception ex) { failures++; Console.WriteLine($"FAIL {test.Name}: {ex.GetType().Name}: {ex.Message}"); }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} geçti; {failures} başarısız.");
return failures == 0 ? 0 : 1;

static void Check(bool value, string message) { if (!value) throw new Exception(message); }
static async Task Until(Func<bool> predicate) {
 using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(4));
 while (!predicate()) await Task.Delay(10, timeout.Token);
}
static Channel<ChatPacket> Inbox(ChatClient client) {
 var c = Channel.CreateUnbounded<ChatPacket>(); client.MessageReceived += p => c.Writer.TryWrite(p); return c;
}
static async Task<ChatPacket> Next(Channel<ChatPacket> c, Func<ChatPacket, bool> predicate) {
 using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(4));
 while (true) { var p = await c.Reader.ReadAsync(timeout.Token); if (predicate(p)) return p; }
}
static async Task Throws(Func<Task> action) {
 try { await action(); } catch (ArgumentException) { return; } catch (InvalidOperationException) { return; }
 throw new Exception("Geçersiz girdi kabul edildi");
}
static async Task UnicodeChat() {
 await using var s = new ChatServer(); s.Start(0);
 await using var a = new ChatClient(); await using var b = new ChatClient();
 var ai = Inbox(a); var bi = Inbox(b);
 await a.ConnectAsync("127.0.0.1", s.Port, "Çağrı"); await b.ConnectAsync("localhost", s.Port, "İpek");
 var before = DateTimeOffset.UtcNow; await a.SendAsync("Merhaba İpek, çığ öşü 🎉");
 var received = await Next(bi, p => p.Type == "message"); var echo = await Next(ai, p => p.Type == "message");
 Check(received.User == "Çağrı" && received.Text == "Merhaba İpek, çığ öşü 🎉", "Türkçe/emoji kayboldu");
 Check(received.Timestamp >= before && received.Timestamp <= DateTimeOffset.UtcNow, "Sunucu zamanı yok");
 Check(echo == received, "Gönderene aynı mesaj ulaşmadı");
 await b.SendAsync("Yanıt"); Check((await Next(ai, p => p.Type == "message")).User == "İpek", "İkinci istemci gönderemedi");
}
static async Task Membership() {
 await using var s = new ChatServer(); s.Start(0);
 await using var a = new ChatClient(); await using var b = new ChatClient(); var ai = Inbox(a);
 await a.ConnectAsync("127.0.0.1", s.Port, "Ali"); await b.ConnectAsync("127.0.0.1", s.Port, "Ece");
 var list = await Next(ai, p => p.Type == "clients" && p.Clients?.Length == 2);
 Check(list.Clients!.Any(c => c.User == "Ece" && c.Endpoint.Contains("127.0.0.1")), "Temel istemci bilgisi yok");
 await b.DisconnectAsync(); await Until(() => s.Clients.Count == 1);
 Check((await Next(ai, p => p.Type == "system" && p.User == "Ece" && p.Text!.Contains("ayrıldı"))).Timestamp != null, "Ayrılma zamanı yok");
 await Next(ai, p => p.Type == "clients" && p.Clients?.Length == 1);
}
static async Task Restart() {
 await using var s = new ChatServer(); s.Start(0); await using var a = new ChatClient();
 await a.ConnectAsync("127.0.0.1", s.Port, "Ali"); await s.StopAsync(); await Until(() => !a.IsConnected);
 s.Start(0); await a.ConnectAsync("127.0.0.1", s.Port, "Ali"); Check(a.IsConnected, "Tekrar bağlanamadı");
}
static async Task FailedConnect() {
 var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start(); int port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop();
 await using var a = new ChatClient();
 try { await a.ConnectAsync("127.0.0.1", port, "Ali"); throw new Exception("Bağlantı beklenmedik şekilde başarılı"); } catch (SocketException) { }
 Check(!a.IsConnected, "Hatalı bağlantı açık kaldı");
 await using var s = new ChatServer(); s.Start(0); await a.ConnectAsync("localhost", s.Port, "Ali"); Check(a.IsConnected, "Hata sonrası yeniden bağlanamadı");
}
static async Task Validation() {
 await using var s = new ChatServer(); s.Start(0); await using var a = new ChatClient();
 await Throws(() => a.ConnectAsync("127.0.0.1", s.Port, " "));
 await a.ConnectAsync("127.0.0.1", s.Port, "Ali"); await Throws(() => a.SendAsync(" ")); await Throws(() => a.SendAsync(new string('x', 2001)));
 Check(a.IsConnected, "Yerel doğrulama bağlantıyı bozdu");
}
static async Task Closed(TcpClient raw) {
 var buffer = new byte[4096]; using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(4));
 try { while (await raw.GetStream().ReadAsync(buffer, timeout.Token) != 0) { } } catch (IOException) { }
}
static async Task Malformed() {
 await using var s = new ChatServer(); s.Start(0); using var raw = new TcpClient(); await raw.ConnectAsync("127.0.0.1", s.Port);
 await raw.GetStream().WriteAsync(Encoding.UTF8.GetBytes("bozuk-json\n")); await Closed(raw);
 await using var a = new ChatClient(); await a.ConnectAsync("127.0.0.1", s.Port, "Sağlam"); Check(a.IsConnected, "Sunucu bozuk veride durdu");
}
static async Task Oversized() {
 await using var s = new ChatServer(); s.Start(0); using var raw = new TcpClient(); await raw.ConnectAsync("127.0.0.1", s.Port);
 await raw.GetStream().WriteAsync(Encoding.UTF8.GetBytes(new string('x', 17000))); await Closed(raw);
 Check(s.Clients.Count == 0, "Geçersiz istemci listede");
}
static async Task ConcurrentMessages() {
 await using var s = new ChatServer(); s.Start(0); await using var a = new ChatClient(); await using var b = new ChatClient(); var bi = Inbox(b);
 await a.ConnectAsync("localhost", s.Port, "Ali"); await b.ConnectAsync("localhost", s.Port, "Ece");
 await Task.WhenAll(Enumerable.Range(0, 30).Select(i => a.SendAsync($"ileti-{i}")));
 var seen = new HashSet<string>(); for (int i = 0; i < 30; i++) seen.Add((await Next(bi, p => p.Type == "message")).Text!);
 Check(seen.Count == 30 && b.IsConnected, "Mesajlar kayıp veya çerçeveler karıştı");
}
static async Task PartialShutdown() {
 await using var s = new ChatServer(); s.Start(0); using var raw = new TcpClient(); await raw.ConnectAsync("localhost", s.Port);
 await raw.GetStream().WriteAsync(Encoding.UTF8.GetBytes("{\"Type\":")); await s.StopAsync().WaitAsync(TimeSpan.FromSeconds(3));
}
static async Task RepeatedSessions() {
 await using var s = new ChatServer(); s.Start(0); await using var a = new ChatClient();
 for (int i = 0; i < 5; i++) { await a.ConnectAsync("localhost", s.Port, "Ali"); await a.DisconnectAsync(); await Until(() => s.Clients.Count == 0); }
}
