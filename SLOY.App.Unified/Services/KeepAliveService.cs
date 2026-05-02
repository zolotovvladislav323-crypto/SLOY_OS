namespace SLOY.App.Unified.Services;

public class KeepAliveService
{
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private readonly int _intervalMs;

    public bool IsRunning => _isRunning;
    public DateTime LastKeepAlive { get; private set; }

    public event EventHandler? OnKeepAlive;

    public KeepAliveService(int intervalMs = 30000)
    {
        _intervalMs = intervalMs;
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        _isRunning = true;
        LastKeepAlive = DateTime.UtcNow;

        while (!_cts.Token.IsCancellationRequested && _isRunning)
        {
            LastKeepAlive = DateTime.UtcNow;
            OnKeepAlive?.Invoke(this, EventArgs.Empty);
            await Task.Delay(_intervalMs, _cts.Token);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _cts?.Cancel();
    }

    public TimeSpan GetTimeSinceLastKeepAlive()
    {
        return DateTime.UtcNow - LastKeepAlive;
    }
}