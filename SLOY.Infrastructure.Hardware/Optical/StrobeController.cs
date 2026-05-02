using SLOY.Infrastructure.Hardware.Abstractions;

namespace SLOY.Infrastructure.Hardware.Optical;

public class StrobeController
{
    private readonly IFlashlightAdapter _flashlight;
    private CancellationTokenSource? _cts;

    public bool IsTransmitting { get; private set; }
    public int BaudRate { get; set; } = 1200;

    public StrobeController(IFlashlightAdapter flashlight)
    {
        _flashlight = flashlight;
    }

    public async Task TransmitAsync(byte[] data)
    {
        if (!_flashlight.IsAvailable) return;

        _cts = new CancellationTokenSource();
        IsTransmitting = true;

        var bitsPerSymbol = BaudRate / 100;
        var bitDelay = 1000 / BaudRate;

        await _flashlight.TurnOnAsync();
        await Task.Delay(100); // Преамбула

        foreach (var b in data)
        {
            for (int bit = 7; bit >= 0; bit--)
            {
                if (_cts.Token.IsCancellationRequested) break;

                var isOne = ((b >> bit) & 1) == 1;

                if (isOne)
                    await _flashlight.TurnOnAsync();
                else
                    await _flashlight.TurnOffAsync();

                await Task.Delay(bitDelay);
            }
        }

        await _flashlight.TurnOffAsync();
        IsTransmitting = false;
    }

    public async Task TransmitBurstAsync(byte[] data, int repeatCount = 3)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            if (!IsTransmitting) break;
            await TransmitAsync(data);
            await Task.Delay(50);
        }
    }

    public void StopTransmission()
    {
        _cts?.Cancel();
        IsTransmitting = false;
    }

    public static byte[] AddChecksum(byte[] data)
    {
        var result = new byte[data.Length + 2];
        Array.Copy(data, result, data.Length);

        ushort crc = 0xFFFF;
        foreach (var b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
                crc = (ushort)((crc & 1) == 1 ? (crc >> 1) ^ 0xA001 : crc >> 1);
        }

        result[^2] = (byte)(crc >> 8);
        result[^1] = (byte)(crc & 0xFF);
        return result;
    }
}