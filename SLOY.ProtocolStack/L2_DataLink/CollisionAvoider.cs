namespace SLOY.ProtocolStack.L2_DataLink;

public class CollisionAvoider
{
    private readonly Random _random = new();
    private readonly int _minBackoffMs;
    private readonly int _maxBackoffMs;
    private readonly int _maxRetries;
    private DateTime _lastTransmission;

    public bool IsChannelClear => (DateTime.UtcNow - _lastTransmission).TotalMilliseconds > _minBackoffMs;

    public CollisionAvoider(int minBackoffMs = 10, int maxBackoffMs = 500, int maxRetries = 7)
    {
        _minBackoffMs = minBackoffMs;
        _maxBackoffMs = maxBackoffMs;
        _maxRetries = maxRetries;
    }

    public async Task<bool> WaitForClearChannelAsync(CancellationToken ct = default)
    {
        for (int attempt = 0; attempt < _maxRetries; attempt++)
        {
            if (IsChannelClear)
            {
                _lastTransmission = DateTime.UtcNow;
                return true;
            }

            var backoff = _random.Next(_minBackoffMs * (int)Math.Pow(2, attempt),
                                       _maxBackoffMs * (int)Math.Pow(2, attempt));
            await Task.Delay(Math.Min(backoff, 5000), ct);
        }

        return false;
    }

    public void SignalTransmission()
    {
        _lastTransmission = DateTime.UtcNow;
    }

    public void SignalCollision()
    {
        _lastTransmission = DateTime.MinValue;
    }
}