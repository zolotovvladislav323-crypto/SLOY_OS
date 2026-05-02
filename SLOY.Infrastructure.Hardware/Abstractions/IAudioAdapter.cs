namespace SLOY.Infrastructure.Hardware.Abstractions;

public interface IAudioAdapter
{
    bool IsSpeakerAvailable { get; }
    bool IsMicrophoneAvailable { get; }
    int SampleRate { get; }
    Task<bool> RequestPermissionAsync();
    Task StartRecordingAsync(int sampleRate = 44100);
    Task StopRecordingAsync();
    Task PlayAsync(byte[] audioData, int sampleRate = 44100);
    Task PlayToneAsync(double frequencyHz, int durationMs);
    event EventHandler<byte[]> OnAudioDataReceived;
}