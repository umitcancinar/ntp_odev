using System.Text;
using KeyEventLogger.Core;
var stamp = new DateTimeOffset(2026,11,3,18,30,0,TimeSpan.FromHours(3));
var tests = new (string, Action)[] {
    ("Durdurulmuş oturum kayıt almaz", () => { var s=new LogSession(); Check(!s.Record(KeyEventKind.KeyDown,"A",stamp) && s.Entries.Count==0); }),
    ("Başlat üç olay türünü zaman damgasıyla saklar", () => { var s=new LogSession(); s.Start(); foreach(var kind in Enum.GetValues<KeyEventKind>()) Check(s.Record(kind,"İ",stamp)); Check(s.Entries.Count==3 && s.Entries[2].Timestamp==stamp && s.Entries[2].Kind==KeyEventKind.KeyPress); }),
    ("Durdur yeni kaydı engeller eski kaydı korur", () => { var s=new LogSession(); s.Start(); s.Record(KeyEventKind.KeyDown,"A",stamp); s.Stop(); Check(!s.IsRecording && !s.Record(KeyEventKind.KeyUp,"A",stamp) && s.Entries.Count==1); }),
    ("Temizle kayıtları siler oturum durumunu korur", () => { var s=new LogSession(); s.Start(); s.Record(KeyEventKind.KeyDown,"A",stamp); s.Clear(); Check(s.IsRecording && s.Entries.Count==0); }),
    ("Parola alanı odaktayken olay kaydedilmez", () => { var s=new LogSession(); s.Start(); Check(!s.Record(KeyEventKind.KeyPress,"x",stamp,true) && s.Entries.Count==0); }),
    ("UTF8 dosyası Türkçe karakter ve olay bilgisini korur", () => { var s=new LogSession(); s.Start(); s.Record(KeyEventKind.KeyPress,"İğüş",stamp); var path=Path.GetTempFileName(); try { s.Save(path); var text=File.ReadAllText(path,Encoding.UTF8); Check(text.Contains("İğüş") && text.Contains("KeyPress") && text.Contains("18:30:00") && text.Contains("+03:00")); } finally { File.Delete(path); } }),
    ("Dosya hatası çağırana bildirilir ve kayıtlar korunur", () => {
        var s = new LogSession();
        s.Start();
        s.Record(KeyEventKind.KeyDown, "A", stamp);
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "olaylar.txt");
        Throws<DirectoryNotFoundException>(() => s.Save(missing));
        Check(s.Entries.Count == 1 && s.IsRecording);
    }),
    ("Kontrol karakterleri tek satırda okunur", () => { var e=new KeyLogEntry(stamp,KeyEventKind.KeyPress,"\r\n\t"); Check(!e.ToString().Contains('\n') && !e.ToString().Contains('\r') && e.ToString().Contains("\\n")); }),
    ("Kayıt sınırında oturum durur ve eski kayıtlar korunur", () => { var s=new LogSession(); s.Start(); for(int i=0;i<LogSession.MaxEntries;i++) Check(s.Record(KeyEventKind.KeyDown,"A",stamp)); Check(!s.IsRecording && !s.Record(KeyEventKind.KeyDown,"B",stamp) && s.Entries.Count==LogSession.MaxEntries); }),
    ("Boş kayıt açıklaması reddedilir", () => { var s=new LogSession(); s.Start(); Throws<ArgumentException>(()=>s.Record(KeyEventKind.KeyDown,"",stamp)); })
};
int failures=0;
foreach(var (name,test) in tests) { try { test(); Console.WriteLine($"PASS {name}"); } catch(Exception ex) { failures++; Console.WriteLine($"FAIL {name}: {ex.Message}"); } }
Console.WriteLine($"{tests.Length-failures}/{tests.Length} başarılı");
return failures==0 ? 0 : 1;
static void Check(bool condition) { if(!condition) throw new Exception("Beklenen davranış gerçekleşmedi."); }
static void Throws<T>(Action action) where T:Exception { try { action(); } catch(T) { return; } throw new Exception($"{typeof(T).Name} bekleniyordu."); }
