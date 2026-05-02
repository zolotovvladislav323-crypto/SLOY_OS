using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.Infrastructure.Hardware.Audio;

public class AudioProcessor
{
    private readonly IAudioAdapter _audio;
    private readonly Queue<double> _sampleBuffer = new(44100);
    private bool _isProcessing;

    public event EventHandler<double>? OnDbLevelChanged;
    public event EventHandler<byte[]>? OnDataDecoded;

    public AudioProcessor(IAudioAdapter audio)
    {
        _audio = audio;
    }

    public async Task StartProcessingAsync(int sampleRate = 44100)
    {
        _isProcessing = true;

        _audio.OnAudioDataReceived += (_, data) =>
        {
            if (!_isProcessing) return;

            var samples = ConvertToSamples(data);
            var db = CalculateDbLevel(samples);
            OnDbLevelChanged?.Invoke(this, db);

            foreach (var sample in samples)
            {
                _sampleBuffer.Enqueue(sample);
                if (_sampleBuffer.Count > sampleRate)
                    _sampleBuffer.Dequeue();
            }

            var decoded = TryDecode();
            if (decoded != null)
                OnDataDecoded?.Invoke(this, decoded);
        };

        await _audio.StartRecordingAsync(sampleRate);
    }

    public Task StopProcessingAsync()
    {
        _isProcessing = false;
        return _audio.StopRecordingAsync();
    }

    private double[] ConvertToSamples(byte[] audioData)
    {
        var samples = new double[audioData.Length / 2];
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = BitConverter.ToInt16(audioData, i * 2) / (double)short.MaxValue;
        }
        return samples;
    }

    private double CalculateDbLevel(double[] samples)
    {
        if (samples.Length == 0) return -100;
        var rms = Math.Sqrt(samples.Average(s => s * s));
        return 20 * Math.Log10(Math.Max(rms, 1e-10));
    }

    private byte[]? TryDecode()
    {
        if (_sampleBuffer.Count < 4410) return null;
        return null;
    }
}