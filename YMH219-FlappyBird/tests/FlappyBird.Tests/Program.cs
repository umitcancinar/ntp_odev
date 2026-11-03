using FlappyBird.Core;
var tests = new (string, Action)[] {
    ("Yerçekimi kuşu aşağı hızlandırır", () => { var b = new Bird(); b.Advance(.1f); Check(b.Y > 200 && b.Velocity > 0); }),
    ("Zıplama yukarı hız verir", () => { var b = new Bird(); b.Flap(); b.Advance(.05f); Check(b.Y < 200 && b.Velocity < 0); }),
    ("Boru sola hareket eder", () => { var p = new PipePair(500,140); p.Advance(.5f); Check(p.X < 500); }),
    ("Geçiş skoru yalnızca bir kez alınır", () => { var p = new PipePair(100,140); Check(!p.TryScore(160)); p.Advance(.1f); Check(p.TryScore(160)); Check(!p.TryScore(160)); }),
    ("Yeniden kullanılan boru tekrar skor verir", () => { var p = new PipePair(0,140); Check(p.TryScore(160)); p.Recycle(0,180); Check(p.GapTop == 180 && p.TryScore(160)); }),
    ("Boşlukta çarpışma yoktur", () => { var b = new Bird(160,180); Check(!b.CollidesWith(new PipePair(150,140))); }),
    ("Üst ve alt borular çarpışır", () => { Check(new Bird(160,100).CollidesWith(new PipePair(150,140))); Check(new Bird(160,290).CollidesWith(new PipePair(150,140))); }),
    ("Yatayda ayrık nesneler çarpışmaz", () => Check(!new Bird().CollidesWith(new PipePair(500,250)))),
    ("Basılı tutma tekrar zıplatmaz", () => { var g = new GameSession(); g.Start(); Check(g.PressFlap()); g.Advance(.05f); var v = g.Bird.Velocity; Check(!g.PressFlap()); Check(g.Bird.Velocity == v); g.ReleaseFlap(); Check(g.PressFlap()); }),
    ("Zemin çarpışması oyunu durdurur", () => { var g = new GameSession(); g.Start(); for(int i=0;i<200;i++) g.Advance(.01f); Check(g.State == GameState.GameOver); var y = g.Bird.Y; var x = g.Pipes[0].X; g.Advance(.1f); Check(g.Bird.Y == y && g.Pipes[0].X == x && !g.PressFlap()); }),
    ("Yeniden başlatma fizik skor ve tuş durumunu sıfırlar", () => { var g = new GameSession(); g.Start(); g.PressFlap(); g.Advance(.1f); g.Start(); Check(g.Score == 0 && g.State == GameState.Running && g.Bird.Y == 200 && g.Bird.Velocity == 0 && g.Pipes.Count >= 2 && g.Pipes[0].X == 560 && g.PressFlap()); }),
    ("Geçersiz zaman adımı reddedilir", () => { var g = new GameSession(); g.Start(); Throws<ArgumentOutOfRangeException>(() => g.Advance(float.NaN)); Throws<ArgumentOutOfRangeException>(() => g.Advance(-1)); }),
    ("Oyun oturumu geçişleri sayar ve boruları yeniden kullanır", () => { var g = new GameSession(5); g.Start(); int recycled = 0; float previous = g.Pipes[0].X; for(int i=0;i<2000 && g.State == GameState.Running;i++) { var next = g.Pipes.Where(p => p.X + PipePair.Width >= g.Bird.X).OrderBy(p=>p.X).First(); float target = next.GapTop + PipePair.GapHeight/2 - Bird.Size/2; if(g.Bird.Y > target && g.Bird.Velocity > 0) { g.ReleaseFlap(); g.PressFlap(); } g.Advance(.01f); if(g.Pipes[0].X > previous) recycled++; previous=g.Pipes[0].X; } Check(g.Score >= 3 && recycled > 0 && g.State == GameState.Running); })
};
int failures=0;
foreach(var (name,test) in tests) { try { test(); Console.WriteLine($"PASS {name}"); } catch(Exception ex) { failures++; Console.WriteLine($"FAIL {name}: {ex.Message}"); } }
Console.WriteLine($"{tests.Length-failures}/{tests.Length} başarılı");
return failures==0 ? 0 : 1;
static void Check(bool condition) { if(!condition) throw new Exception("Beklenen davranış gerçekleşmedi."); }
static void Throws<T>(Action action) where T:Exception { try { action(); } catch(T) { return; } throw new Exception($"{typeof(T).Name} bekleniyordu."); }
