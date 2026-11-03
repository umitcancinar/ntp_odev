namespace FlappyBird.Core;
public enum GameState { Ready, Running, GameOver }
public sealed class Bird(float x = 160, float y = 200)
{
    public float X { get; } = x;
    public float Y { get; private set; } = y;
    public float Velocity { get; private set; }
    public const float Size = 26;
    public void Flap() => throw new NotImplementedException();
    public void Advance(float seconds) => throw new NotImplementedException();
    public bool CollidesWith(PipePair pipe) => throw new NotImplementedException();
}
public sealed class PipePair(float x, float gapTop)
{
    public float X { get; private set; } = x;
    public float GapTop { get; private set; } = gapTop;
    public const float Width = 68, GapHeight = 160;
    public void Advance(float seconds) => throw new NotImplementedException();
    public bool TryScore(float birdX) => throw new NotImplementedException();
    public void Recycle(float x, float gapTop) => throw new NotImplementedException();
}
public sealed class GameSession(int seed = 0)
{
    public const float WorldWidth = 840, WorldHeight = 480, GroundY = 440;
    public Bird Bird { get; private set; } = new();
    public IReadOnlyList<PipePair> Pipes { get; } = [new(560, 140), new(900, 140)];
    public GameState State { get; private set; }
    public int Score { get; private set; }
    public void Start() => throw new NotImplementedException();
    public bool PressFlap() => throw new NotImplementedException();
    public void ReleaseFlap() => throw new NotImplementedException();
    public void Advance(float seconds) => throw new NotImplementedException();
}
