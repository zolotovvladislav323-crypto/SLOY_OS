namespace SLOY.Core.Resilience;

public class CircuitBreaker
{
    private int _failureCount;
    private DateTime _openUntil;
    private readonly int _threshold;
    private readonly TimeSpan _openDuration;

    public bool IsOpen => DateTime.UtcNow < _openUntil;

    public CircuitBreaker(int threshold = 5, int openDurationSeconds = 30)
    {
        _threshold = threshold;
        _openDuration = TimeSpan.FromSeconds(openDurationSeconds);
    }

    public void RecordSuccess() => _failureCount = 0;

    public void RecordFailure()
    {
        if (IsOpen) return;
        _failureCount++;
        if (_failureCount >= _threshold)
            _openUntil = DateTime.UtcNow + _openDuration;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
    {
        if (IsOpen) throw new InvalidOperationException("Circuit breaker is open.");
        try
        {
            var result = await action();
            RecordSuccess();
            return result;
        }
        catch { RecordFailure(); throw; }
    }
}