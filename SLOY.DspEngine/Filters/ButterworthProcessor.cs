namespace SLOY.DspEngine.Filters;

public class ButterworthProcessor
{
    private readonly double[] _a;
    private readonly double[] _b;
    private readonly double[] _inputHistory;
    private readonly double[] _outputHistory;
    private int _historyIndex;

    public ButterworthProcessor(int order = 4, double cutoffHz = 1000, double sampleRateHz = 44100)
    {
        _a = new double[order + 1];
        _b = new double[order + 1];
        _inputHistory = new double[order + 1];
        _outputHistory = new double[order + 1];

        DesignLowPass(order, cutoffHz, sampleRateHz);
    }

    private void DesignLowPass(int order, double cutoffHz, double sampleRateHz)
    {
        var wc = 2 * Math.PI * cutoffHz / sampleRateHz;
        var wcTan = Math.Tan(wc / 2);

        _b[0] = 1;
        _a[0] = 1;

        for (int i = 1; i <= order; i++)
        {
            var pole = -wcTan * Math.Sin(Math.PI * (2 * i - 1) / (2 * order));
            var real = -wcTan * Math.Cos(Math.PI * (2 * i - 1) / (2 * order));
            var norm = 1 + 2 * wcTan * Math.Abs(real) + wcTan * wcTan;

            _b[i] = _b[i - 1] * wcTan * wcTan / norm;
            _a[i] = (_a[i - 1] * (1 - 2 * wcTan * Math.Abs(real) + wcTan * wcTan)) / norm;
        }
    }

    public double Process(double sample)
    {
        _inputHistory[_historyIndex % _inputHistory.Length] = sample;

        double output = 0;
        int idx = _historyIndex;

        for (int i = 0; i < _b.Length; i++)
            output += _b[i] * _inputHistory[(idx - i + _inputHistory.Length) % _inputHistory.Length];

        for (int i = 1; i < _a.Length; i++)
            output -= _a[i] * _outputHistory[(idx - i + _outputHistory.Length) % _outputHistory.Length];

        _outputHistory[_historyIndex % _outputHistory.Length] = output;
        _historyIndex++;

        return output;
    }

    public double[] ProcessBatch(double[] samples)
    {
        var result = new double[samples.Length];
        for (int i = 0; i < samples.Length; i++)
            result[i] = Process(samples[i]);
        return result;
    }
}