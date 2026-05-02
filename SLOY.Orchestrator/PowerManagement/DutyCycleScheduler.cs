namespace SLOY.Orchestrator.PowerManagement;

public class DutyCycleScheduler
{
    private readonly Dictionary<string, DutyCycle> _cycles = new();
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public event EventHandler<string>? OnWakeUp;
    public event EventHandler<string>? OnSleep;

    public void RegisterComponent(string componentId, double activeTimeMs, double sleepTimeMs)
    {
        _cycles[componentId] = new DutyCycle
        {
            ComponentId = componentId,
            ActiveTimeMs = activeTimeMs,
            SleepTimeMs = sleepTimeMs,
            IsActive = false,
            NextToggle = DateTime.UtcNow
        };
    }

    public void UnregisterComponent(string componentId)
    {
        _cycles.Remove(componentId);
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        _isRunning = true;

        while (!_cts.Token.IsCancellationRequested && _isRunning)
        {
            var now = DateTime.UtcNow;

            foreach (var cycle in _cycles.Values)
            {
                if (now >= cycle.NextToggle)
                {
                    cycle.IsActive = !cycle.IsActive;

                    if (cycle.IsActive)
                    {
                        cycle.NextToggle = now.AddMilliseconds(cycle.ActiveTimeMs);
                        OnWakeUp?.Invoke(this, cycle.ComponentId);
                    }
                    else
                    {
                        cycle.NextToggle = now.AddMilliseconds(cycle.SleepTimeMs);
                        OnSleep?.Invoke(this, cycle.ComponentId);
                    }
                }
            }

            await Task.Delay(50, _cts.Token);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _cts?.Cancel();
    }

    public bool IsComponentActive(string componentId)
    {
        return _cycles.TryGetValue(componentId, out var cycle) && cycle.IsActive;
    }

    public double GetDutyCyclePercent(string componentId)
    {
        if (!_cycles.TryGetValue(componentId, out var cycle)) return 0;
        return cycle.ActiveTimeMs / (cycle.ActiveTimeMs + cycle.SleepTimeMs) * 100;
    }

    public double GetTotalPowerEstimate()
    {
        return _cycles.Values.Sum(c => c.IsActive ? c.ActiveTimeMs / (c.ActiveTimeMs + c.SleepTimeMs) * 100 : 0);
    }

    private class DutyCycle
    {
        public string ComponentId { get; init; } = string.Empty;
        public double ActiveTimeMs { get; init; }
        public double SleepTimeMs { get; init; }
        public bool IsActive { get; set; }
        public DateTime NextToggle { get; set; }
    }
}