namespace FlappyBird.Core;

public enum GameState { Ready, Running, GameOver }

/// <summary>UI'dan bağımsız oyun döngüsü: fizik, skor, çarpışma ve sıfırlama.</summary>
public sealed class GameSession
{
    public const float WorldWidth = 840, WorldHeight = 480, GroundY = 440;
    private const float PipeSpacing = 340;
    private readonly Random random;
    private readonly List<PipePair> pipes = [new(560, 140), new(900, 140)];
    private bool flapHeld;
    public Bird Bird { get; private set; } = new();
    public IReadOnlyList<PipePair> Pipes { get; }
    public GameState State { get; private set; } = GameState.Ready;
    public int Score { get; private set; }

    public GameSession(int? seed = null)
    {
        random = seed.HasValue ? new Random(seed.Value) : new Random();
        Pipes = pipes.AsReadOnly();
    }

    public void Start()
    {
        Bird = new Bird();
        Score = 0;
        flapHeld = false;
        for (int i = 0; i < pipes.Count; i++) pipes[i].Recycle(560 + i * PipeSpacing, NextGap());
        State = GameState.Running;
    }

    public bool PressFlap()
    {
        if (State != GameState.Running || flapHeld) return false;
        flapHeld = true;
        Bird.Flap();
        return true;
    }

    public void ReleaseFlap() => flapHeld = false;

    public void Advance(float seconds)
    {
        if (!float.IsFinite(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
        if (State != GameState.Running) return;
        // Uzun bir UI takılmasında ışınlanma olmaz; küçük adımlar çarpışmayı atlamaz.
        float remaining = Math.Min(seconds, .25f);
        while (remaining > 0 && State == GameState.Running)
        {
            float dt = Math.Min(remaining, .01f);
            remaining -= dt;
            Bird.Advance(dt);
            foreach (var pipe in pipes) pipe.Advance(dt);
            if (Bird.Y <= 0 || Bird.Y + Bird.Size >= GroundY || pipes.Any(Bird.CollidesWith))
            {
                State = GameState.GameOver;
                return;
            }
            foreach (var pipe in pipes)
            {
                if (pipe.TryScore(Bird.X)) Score++;
                if (pipe.X + PipePair.Width < 0)
                    pipe.Recycle(pipes.Max(p => p.X) + PipeSpacing, NextGap());
            }
        }
    }

    private float NextGap() => random.Next(70, 211);
}
