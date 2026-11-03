namespace SpeechApp.Core;

public enum SpeechActivity { Idle, Speaking, Paused, Listening, StoppingSpeech, StoppingRecognition }

public sealed class SpeechSession
{
    public SpeechActivity Activity { get; private set; }
    public bool TrySpeak() => false;
    public bool TryListen() => false;
    public bool TryPause() => false;
    public bool TryResume() => false;
    public bool TryStopSpeech() => false;
    public bool TryStopRecognition() => false;
    public void CompleteSpeech() { }
    public void CompleteRecognition() { }
}
