using System.Runtime.InteropServices;

namespace SLOY.DspEngine.Native;

public static class NativeMathBridge
{
    private const string NeonLib = "SLOY.DspEngine.Native.Neon";
    private const string Avx2Lib = "SLOY.DspEngine.Native.AVX2";

    [DllImport(NeonLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void fft_radix4_neon(float[] real, float[] imag, int n, int inverse);

    [DllImport(Avx2Lib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void kalman_filter_avx2(double[] measurements, double[] output, int count, double processNoise, double measurementNoise);

    public static bool IsNeonAvailable => RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
    public static bool IsAvx2Available => System.Runtime.Intrinsics.X86.Avx2.IsSupported;

    public static void FftRadix4(float[] real, float[] imag, bool inverse = false)
    {
        var doubleReal = real.Select(f => (double)f).ToArray();
        var fft = new Transformation.FastFourierTransform(doubleReal.Length);
        var result = fft.Forward(doubleReal);

        for (int i = 0; i < real.Length; i++)
        {
            real[i] = (float)result[i].Real;
            imag[i] = (float)result[i].Imaginary;
        }
    }

    public static double[] KalmanFilterAvx2(double[] measurements, double processNoise, double measurementNoise)
    {
        var output = new double[measurements.Length];
        var filter = new Filters.KalmanFilter(processNoise, measurementNoise);
        for (int i = 0; i < measurements.Length; i++)
            output[i] = filter.Update(measurements[i]);
        return output;
    }
}