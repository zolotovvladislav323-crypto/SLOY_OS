namespace SLOY.ProtocolStack.L1_Physical;

public class PolarizationRotator
{
    private double _currentAngle;
    private readonly double _stepDegrees;

    public double CurrentAngle => _currentAngle;

    public PolarizationRotator(double stepDegrees = 45)
    {
        _stepDegrees = stepDegrees;
    }

    public (double I, double Q) Apply(double inPhase, double quadrature, double angleDegrees)
    {
        var angleRad = angleDegrees * Math.PI / 180;
        var cos = Math.Cos(angleRad);
        var sin = Math.Sin(angleRad);

        return (
            I: inPhase * cos - quadrature * sin,
            Q: inPhase * sin + quadrature * cos
        );
    }

    public double RotateNext()
    {
        _currentAngle = (_currentAngle + _stepDegrees) % 360;
        return _currentAngle;
    }

    public static double CalculateOptimalAngle(double signalQI, double signalII)
        => Math.Atan2(signalQI, signalII) * 180 / Math.PI;
}