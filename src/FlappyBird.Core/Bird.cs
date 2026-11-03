namespace FlappyBird.Core;

/// <summary>Kuşun dünya koordinatlarını ve dikey hızını yönetir.</summary>
public sealed class Bird(float x = 160, float y = 200)
{
    public const float Size = 26;
    private const float Gravity = 900;
    private const float FlapVelocity = -300;
    public float X { get; } = x;
    public float Y { get; private set; } = y;
    public float Velocity { get; private set; }

    public void Flap() => Velocity = FlapVelocity;

    public void Advance(float seconds)
    {
        Velocity += Gravity * seconds;
        Y += Velocity * seconds;
    }

    public bool CollidesWith(PipePair pipe) =>
        X < pipe.X + PipePair.Width && X + Size > pipe.X &&
        (Y < pipe.GapTop || Y + Size > pipe.GapTop + PipePair.GapHeight);
}
