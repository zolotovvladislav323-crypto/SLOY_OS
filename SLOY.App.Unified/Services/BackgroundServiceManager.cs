namespace SLOY.App.Unified.Services;

public class BackgroundServiceManager
{
    private readonly KeepAliveService _keepAlive;
    private bool _isRunning;

    public bool IsRunning => _isRunning;

    public BackgroundServiceManager(KeepAliveService keepAlive)
    {
        _keepAlive = keepAlive;
    }

    public async Task StartAllAsync()
    {
        _isRunning = true;
        await _keepAlive.StartAsync();
    }

    public void StopAll()
    {
        _isRunning = false;
        _keepAlive.Stop();
    }
}