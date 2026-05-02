namespace SLOY.ProtocolStack.L1_Physical;

public class DsssModulator
{
    private readonly int[] _spreadingCode;
    private readonly int _chipRate;
    private readonly int _bitRate;

    public int SpreadingFactor => _spreadingCode.Length;

    public DsssModulator(int[]? spreadingCode = null, int chipRate = 11, int bitRate = 1)
    {
        _spreadingCode = spreadingCode ?? new[] { 1, -1, 1, 1, -1, 1, -1, -1, 1, 1, -1 };
        _chipRate = chipRate;
        _bitRate = bitRate;
    }

    public double[] Modulate(byte[] data)
    {
        var chips = new List<double>();

        foreach (var b in data)
        {
            for (int bit = 7; bit >= 0; bit--)
            {
                var bitValue = ((b >> bit) & 1) == 1 ? 1 : -1;
                foreach (var chip in _spreadingCode)
                {
                    chips.Add(bitValue * chip);
                }
            }
        }

        return chips.ToArray();
    }

    public byte[] Demodulate(double[] signal)
    {
        var bytes = new List<byte>();
        var bitsPerByte = 8;
        var chipsPerBit = _spreadingCode.Length;
        var totalBits = signal.Length / chipsPerBit;

        for (int i = 0; i < totalBits; i += bitsPerByte)
        {
            byte value = 0;
            for (int bit = 0; bit < bitsPerByte; bit++)
            {
                var correlation = 0.0;
                for (int chip = 0; chip < chipsPerBit; chip++)
                {
                    var idx = (i + bit) * chipsPerBit + chip;
                    if (idx < signal.Length)
                        correlation += signal[idx] * _spreadingCode[chip];
                }
                if (correlation > 0) value |= (byte)(1 << (7 - bit));
            }
            bytes.Add(value);
        }

        return bytes.ToArray();
    }

    public double CalculateProcessGainDb() => 10 * Math.Log10(SpreadingFactor);
}