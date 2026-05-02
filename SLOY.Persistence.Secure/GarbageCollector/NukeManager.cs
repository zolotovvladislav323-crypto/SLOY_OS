namespace SLOY.Persistence.Secure.GarbageCollector;

public class NukeManager
{
    private readonly List<INukeTarget> _targets = new();
    private readonly DeadMansSwitch _deadMansSwitch;

    public bool IsArmed => _deadMansSwitch.IsArmed;

    public NukeManager(TimeSpan timeout)
    {
        _deadMansSwitch = new DeadMansSwitch(timeout, TriggerNuke);
    }

    public void RegisterTarget(INukeTarget target)
    {
        _targets.Add(target);
    }

    public void UnregisterTarget(INukeTarget target)
    {
        _targets.Remove(target);
    }

    public async Task ArmAsync(CancellationToken ct = default)
    {
        await _deadMansSwitch.ArmAsync(ct);
    }

    public void Heartbeat()
    {
        _deadMansSwitch.Heartbeat();
    }

    public void Disarm()
    {
        _deadMansSwitch.Disarm();
    }

    public async Task TriggerManuallyAsync()
    {
        await TriggerNuke();
    }

    private async Task TriggerNuke()
    {
        foreach (var target in _targets.OrderByDescending(t => t.Priority))
        {
            await target.NukeAsync();
        }
    }
}

public interface INukeTarget
{
    int Priority { get; }
    Task NukeAsync();
}