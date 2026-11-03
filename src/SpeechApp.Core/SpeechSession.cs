namespace SpeechApp.Core;

public enum SpeechActivity { Idle, Speaking, Paused, Listening, StoppingSpeech, StoppingRecognition }

/// <summary>Enforces exclusive audio activity, including asynchronous cancellation.</summary>
public sealed class SpeechSession
{
    public SpeechActivity Activity { get; private set; } = SpeechActivity.Idle;

    public bool TrySpeak() => Transition(SpeechActivity.Idle, SpeechActivity.Speaking);
    public bool TryListen() => Transition(SpeechActivity.Idle, SpeechActivity.Listening);
    public bool TryPause() => Transition(SpeechActivity.Speaking, SpeechActivity.Paused);
    public bool TryResume() => Transition(SpeechActivity.Paused, SpeechActivity.Speaking);

    public bool TryStopSpeech()
    {
        if (Activity is not (SpeechActivity.Speaking or SpeechActivity.Paused)) return false;
        Activity = SpeechActivity.StoppingSpeech;
        return true;
    }

    public bool TryStopRecognition() => Transition(SpeechActivity.Listening, SpeechActivity.StoppingRecognition);

    public void CompleteSpeech()
    {
        if (Activity is SpeechActivity.Speaking or SpeechActivity.Paused or SpeechActivity.StoppingSpeech)
            Activity = SpeechActivity.Idle;
    }

    public void CompleteRecognition()
    {
        if (Activity is SpeechActivity.Listening or SpeechActivity.StoppingRecognition)
            Activity = SpeechActivity.Idle;
    }

    private bool Transition(SpeechActivity from, SpeechActivity to)
    {
        if (Activity != from) return false;
        Activity = to;
        return true;
    }
}
