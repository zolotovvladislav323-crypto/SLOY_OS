namespace SLOY.Orchestrator;

public class ThermalManager
{
    private double _currentTemperature;
    private readonly double _warningThreshold;
    private readonly double _criticalThreshold;
    private readonly double _shutdownThreshold;
    private readonly List<IThermalMonitor> _monitors = new();
    private bool _isThrottling;

    public double CurrentTemperature => _currentTemperature;
    public bool IsThrottling => _isThrottling;
    public ThermalState State { get; private set; } = ThermalState.Normal;

    public event EventHandler<ThermalState>? OnThermalStateChanged;
    public event EventHandler<double>? OnTemperatureUpdated;

    public ThermalManager(double warningThreshold = 40, double criticalThreshold = 50, double shutdownThreshold = 60)
    {
        _warningThreshold = warningThreshold;
        _criticalThreshold = criticalThreshold;
        _shutdownThreshold = shutdownThreshold;
    }

    public void RegisterMonitor(IThermalMonitor monitor)
    {
        _monitors.Add(monitor);
    }

    public async Task StartMonitoringAsync(int intervalMs = 2000, CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_monitors.Count > 0)
            {
                _currentTemperature = _monitors.Average(m => m.GetTemperatureCelsius());
            }

            UpdateThermalState();
            OnTemperatureUpdated?.Invoke(this, _currentTemperature);

            if (_isThrottling)
            {
                ApplyThrottling();
            }

            await Task.Delay(intervalMs, ct);
        }
    }

    private void UpdateThermalState()
    {
        var newState = _currentTemperature switch
        {
            >= 60 => ThermalState.Shutdown,
            >= 50 => ThermalState.Critical,
            >= 40 => ThermalState.Warning,
            _ => ThermalState.Normal
        };

        if (newState != State)
        {
            State = newState;
            OnThermalStateChanged?.Invoke(this, State);

            if (State == ThermalState.Shutdown)
            {
                _ = ShutdownThermalAsync();
            }
        }
    }

    private void ApplyThrottling()
    {
        var throttleFactor = State switch
        {
            ThermalState.Critical => 0.3,
            ThermalState.Warning => 0.7,
            _ => 1.0
        };

        foreach (var monitor in _monitors)
        {
            monitor.SetThrottle(throttleFactor);
        }
    }

    private async Task ShutdownThermalAsync()
    {
        foreach (var monitor in _monitors)
        {
            monitor.SetThrottle(0);
        }

        await Task.Delay(1000);
        Environment.Exit(1);
    }
}

public interface IThermalMonitor
{
    double GetTemperatureCelsius();
    void SetThrottle(double factor);
}

public enum ThermalState
{
    Normal,
    Warning,
    Critical,
    Shutdown
}