namespace FlappyBird.Core;

/// <summary>Tek bir üst-alt engel çifti; geçiş başına bir puan üretir.</summary>
public sealed class PipePair(float x, float gapTop)
{
    public const float Width = 68, GapHeight = 160;
    private const float Speed = 180;
    private bool scored;
    public float X { get; private set; } = x;
    public float GapTop { get; private set; } = gapTop;

    public void Advance(float seconds) => X -= Speed * seconds;

    public bool TryScore(float birdX)
    {
        if (scored || X + Width >= birdX) return false;
        scored = true;
        return true;
    }

    public void Recycle(float x, float gapTop)
    {
        X = x;
        GapTop = gapTop;
        scored = false;
    }
}
