using System.Numerics;

namespace SLOY.DspEngine.Transformation;

public class FastFourierTransform
{
    private readonly int _fftSize;
    private readonly Complex[] _twiddleFactors;

    public int FftSize => _fftSize;

    public FastFourierTransform(int fftSize = 1024)
    {
        _fftSize = fftSize;
        _twiddleFactors = PrecomputeTwiddleFactors(fftSize);
    }

    public Complex[] Forward(double[] samples)
    {
        if (samples.Length != _fftSize)
            Array.Resize(ref samples, _fftSize);

        var complex = samples.Select(s => new Complex(s, 0)).ToArray();
        Fft(complex, false);
        return complex;
    }

    public double[] Inverse(Complex[] spectrum)
    {
        var copy = spectrum.ToArray();
        Fft(copy, true);
        return copy.Select(c => c.Real / _fftSize).ToArray();
    }

    private void Fft(Complex[] data, bool inverse)
    {
        var n = data.Length;
        var sign = inverse ? 1 : -1;

        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; j >= bit; bit >>= 1)
                j -= bit;
            j += bit;
            if (i < j)
                (data[i], data[j]) = (data[j], data[i]);
        }

        for (int length = 2; length <= n; length <<= 1)
        {
            var halfLength = length / 2;
            for (int i = 0; i < n; i += length)
            {
                for (int j = 0; j < halfLength; j++)
                {
                    var twiddle = _twiddleFactors[j * _fftSize / length];
                    if (inverse) twiddle = Complex.Conjugate(twiddle);

                    var u = data[i + j];
                    var v = data[i + j + halfLength] * twiddle;

                    data[i + j] = u + v;
                    data[i + j + halfLength] = u - v;
                }
            }
        }
    }

    private static Complex[] PrecomputeTwiddleFactors(int size)
    {
        var factors = new Complex[size];
        for (int i = 0; i < size; i++)
        {
            var angle = -2 * Math.PI * i / size;
            factors[i] = Complex.FromPolarCoordinates(1, angle);
        }
        return factors;
    }

    public double[] GetPowerSpectrum(double[] samples)
    {
        var fft = Forward(samples);
        var spectrum = new double[_fftSize / 2];
        for (int i = 0; i < spectrum.Length; i++)
            spectrum[i] = fft[i].Magnitude;
        return spectrum;
    }
}