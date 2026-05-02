using Microsoft.Maui.Graphics;

namespace SLOY.Infrastructure.Hardware.Optical.ScreenModulator;

public class ColorShiftEncoder
{
    private readonly int _symbolRate;
    private Color _backgroundColor = Color.FromArgb("#000000");
    private CancellationTokenSource? _cts;

    public bool IsEncoding { get; private set; }

    public ColorShiftEncoder(int symbolRate = 30)
    {
        _symbolRate = symbolRate;
    }

    public async IAsyncEnumerable<Color> EncodeStreamAsync(byte[] data)
    {
        _cts = new CancellationTokenSource();
        IsEncoding = true;

        var preamble = new[] { (byte)0xAA, (byte)0xAA, (byte)0xAA };
        var fullData = preamble.Concat(data).Concat(new[] { (byte)0xFF }).ToArray();

        foreach (var b in fullData)
        {
            for (int shift = 0; shift < 4; shift++)
            {
                if (_cts?.Token.IsCancellationRequested == true)
                    yield break;

                var value = (b >> (shift * 2)) & 0b11;
                var color = value switch
                {
                    0b00 => Color.FromArgb("#000000"),
                    0b01 => Color.FromArgb("#0044FF"),
                    0b10 => Color.FromArgb("#00FF44"),
                    0b11 => Color.FromArgb("#FF4400"),
                    _ => Color.FromArgb("#000000")
                };

                yield return color;
                await Task.Delay(1000 / _symbolRate);
            }
        }

        IsEncoding = false;
    }

    public byte[] EncodeToColorSequence(byte[] data)
    {
        var colors = new List<byte>();
        var preamble = new[] { (byte)0xAA, (byte)0xAA, (byte)0xAA };
        var fullData = preamble.Concat(data).ToArray();

        foreach (var b in fullData)
        {
            for (int shift = 0; shift < 4; shift++)
            {
                var value = (b >> (shift * 2)) & 0b11;
                colors.Add((byte)value);
            }
        }

        return colors.ToArray();
    }

    public byte[] DecodeFromColorSequence(byte[] colorValues)
    {
        var data = new List<byte>();
        var preambleFound = false;

        for (int i = 0; i < colorValues.Length - 3; i += 4)
        {
            byte value = 0;
            for (int shift = 0; shift < 4; shift++)
            {
                if (i + shift >= colorValues.Length) break;
                value |= (byte)((colorValues[i + shift] & 0b11) << (shift * 2));
            }

            if (!preambleFound && value == 0xAA)
            {
                if (i >= 8 && colorValues[i - 4] == 0xAA && colorValues[i - 8] == 0xAA)
                {
                    preambleFound = true;
                    data.Clear();
                    continue;
                }
            }

            if (preambleFound)
            {
                if (value == 0xFF) break;
                data.Add(value);
            }
        }

        return data.ToArray();
    }

    public void Stop()
    {
        _cts?.Cancel();
        IsEncoding = false;
    }
}