using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.Infrastructure.Hardware.Audio;

public class AcousticModem
{
    private readonly IAudioAdapter _audio;
    private readonly double _carrierFrequency;
    private readonly int _baudRate;
    private readonly int _sampleRate;
    private CancellationTokenSource? _cts;
    private bool _isReceiving;

    public event EventHandler<byte[]>? OnDataReceived;
    public event EventHandler<string>? OnStatusChanged;

    public AcousticModem(IAudioAdapter audio, double carrierFrequency = 18000, int baudRate = 100, int sampleRate = 44100)
    {
        _audio = audio;
        _carrierFrequency = carrierFrequency;
        _baudRate = baudRate;
        _sampleRate = sampleRate;
    }

    public async Task TransmitAsync(byte[] data)
    {
        _cts = new CancellationTokenSource();
        OnStatusChanged?.Invoke(this, "Передача акустического сигнала...");

        var signal = Modulate(data);
        await _audio.PlayAsync(signal, _sampleRate);
        OnStatusChanged?.Invoke(this, "Передача завершена");
    }

    public async Task StartReceivingAsync()
    {
        _isReceiving = true;
        OnStatusChanged?.Invoke(this, "Прослушивание акустического канала...");

        _audio.OnAudioDataReceived += (_, audioData) =>
        {
            if (!_isReceiving) return;

            var decoded = Demodulate(audioData);
            if (decoded.Length > 0)
            {
                OnDataReceived?.Invoke(this, decoded);
                OnStatusChanged?.Invoke(this, $"Принято {decoded.Length} байт через акустический канал");
            }
        };

        await _audio.StartRecordingAsync(_sampleRate);
    }

    private byte[] Modulate(byte[] data)
    {
        var samplesPerBit = _sampleRate / _baudRate;
        var preamble = GeneratePreamble(samplesPerBit);
        var signal = new List<byte>(preamble);

        foreach (var b in data)
        {
            for (int bit = 7; bit >= 0; bit--)
            {
                var isOne = ((b >> bit) & 1) == 1;
                var frequency = isOne ? _carrierFrequency : _carrierFrequency * 0.5;

                for (int s = 0; s < samplesPerBit; s++)
                {
                    var t = (double)s / _sampleRate;
                    var sample = Math.Sin(2 * Math.PI * frequency * t) * 0.8;
                    var sample16 = (short)(sample * short.MaxValue);
                    signal.AddRange(BitConverter.GetBytes(sample16));
                }
            }
        }

        return signal.ToArray();
    }

    private byte[] Demodulate(byte[] audioData)
    {
        var samples = new short[audioData.Length / 2];
        Buffer.BlockCopy(audioData, 0, samples, 0, audioData.Length);

        var samplesPerBit = _sampleRate / _baudRate;
        var result = new List<byte>();
        byte currentByte = 0;
        int bitCount = 0;

        for (int i = 0; i < samples.Length - samplesPerBit; i += samplesPerBit)
        {
            var energyHigh = 0.0;
            var energyLow = 0.0;

            for (int s = 0; s < samplesPerBit; s++)
            {
                var sample = samples[i + s] / (double)short.MaxValue;
                var t = (double)s / _sampleRate;

                energyHigh += sample * Math.Sin(2 * Math.PI * _carrierFrequency * t);
                energyLow += sample * Math.Sin(2 * Math.PI * _carrierFrequency * 0.5 * t);
            }

            var isOne = energyHigh > energyLow;
            currentByte = (byte)((currentByte << 1) | (isOne ? 1u : 0u));
            bitCount++;

            if (bitCount == 8)
            {
                result.Add(currentByte);
                currentByte = 0;
                bitCount = 0;
            }
        }

        return RemovePreamble(result.ToArray());
    }

    private byte[] GeneratePreamble(int samplesPerBit)
    {
        var preamble = new List<byte>();
        for (int i = 0; i < 16; i++)
        {
            for (int s = 0; s < samplesPerBit; s++)
            {
                var t = (double)s / _sampleRate;
                var sample = Math.Sin(2 * Math.PI * _carrierFrequency * t) * 0.5;
                var sample16 = (short)(sample * short.MaxValue);
                preamble.AddRange(BitConverter.GetBytes(sample16));
            }
        }
        return preamble.ToArray();
    }

    private byte[] RemovePreamble(byte[] data)
    {
        var startIndex = 0;
        for (int i = 0; i < data.Length - 1; i++)
        {
            if (data[i] == 0xAA && data[i + 1] == 0xAA)
            {
                startIndex = i + 2;
                break;
            }
        }

        if (startIndex >= data.Length) return Array.Empty<byte>();
        return data.Skip(startIndex).ToArray();
    }

    public Task StopReceivingAsync()
    {
        _isReceiving = false;
        OnStatusChanged?.Invoke(this, "Прослушивание остановлено");
        return _audio.StopRecordingAsync();
    }

    public void Stop()
    {
        _cts?.Cancel();
    }
}