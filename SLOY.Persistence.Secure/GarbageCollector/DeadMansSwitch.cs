namespace SLOY.Persistence.Secure.GarbageCollector;

public class DeadMansSwitch
{
    private readonly TimeSpan _timeout;
    private readonly Func<Task> _onTriggered;
    private DateTime _lastHeartbeat = DateTime.UtcNow;
    private CancellationTokenSource? _cts;

    public bool IsArmed { get; private set; }
    public TimeSpan TimeRemaining => IsArmed ? _timeout - (DateTime.UtcNow - _lastHeartbeat) : TimeSpan.Zero;

    public DeadMansSwitch(TimeSpan timeout, Func<Task> onTriggered)
    {
        _timeout = timeout;
        _onTriggered = onTriggered;
    }

    public async Task ArmAsync(CancellationToken cancellationToken = default)
    {
        IsArmed = true;
        _lastHeartbeat = DateTime.UtcNow;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        while (!_cts.Token.IsCancellationRequested && IsArmed)
        {
            if (DateTime.UtcNow - _lastHeartbeat > _timeout)
            {
                await _onTriggered();
                IsArmed = false;
                break;
            }

            await Task.Delay(1000, _cts.Token);
        }
    }

    public void Heartbeat()
    {
        _lastHeartbeat = DateTime.UtcNow;
    }

    public void Disarm()
    {
        IsArmed = false;
        _cts?.Cancel();
    }
}