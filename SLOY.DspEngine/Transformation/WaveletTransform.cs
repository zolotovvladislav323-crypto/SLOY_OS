namespace SLOY.DspEngine.Transformation;

public class WaveletTransform
{
    private readonly double[] _lowPass;
    private readonly double[] _highPass;

    public WaveletTransform()
    {
        _lowPass = new[] { 0.4829629131445341, 0.8365163037378079, 0.2241438680420134, -0.1294095225512604 };
        _highPass = new[] { -0.1294095225512604, -0.2241438680420134, 0.8365163037378079, -0.4829629131445341 };
    }

    public (double[] approximation, double[] detail) Decompose(double[] signal)
    {
        var n = signal.Length;
        var approx = new double[n / 2];
        var detail = new double[n / 2];

        for (int i = 0; i < n / 2; i++)
        {
            double sumLow = 0, sumHigh = 0;
            for (int j = 0; j < _lowPass.Length; j++)
            {
                var idx = (2 * i + j) % n;
                sumLow += signal[idx] * _lowPass[j];
                sumHigh += signal[idx] * _highPass[j];
            }
            approx[i] = sumLow;
            detail[i] = sumHigh;
        }

        return (approx, detail);
    }

    public double[] Reconstruct(double[] approximation, double[] detail)
    {
        var n = approximation.Length * 2;
        var signal = new double[n];

        for (int i = 0; i < approximation.Length; i++)
        {
            for (int j = 0; j < _lowPass.Length; j++)
            {
                var idx = (2 * i + j) % n;
                signal[idx] += approximation[i] * _lowPass[_lowPass.Length - 1 - j]
                             + detail[i] * _highPass[_highPass.Length - 1 - j];
            }
        }

        return signal;
    }

    public double[] Denoise(double[] signal, double threshold = 0.1)
    {
        var (approx, detail) = Decompose(signal);
        var maxDetail = detail.Max(Math.Abs);

        for (int i = 0; i < detail.Length; i++)
        {
            if (Math.Abs(detail[i]) < threshold * maxDetail)
                detail[i] = 0;
        }

        return Reconstruct(approx, detail);
    }
}