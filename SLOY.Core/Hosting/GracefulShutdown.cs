namespace SLOY.Core.Hosting;

public class GracefulShutdown
{
    private readonly List<ISloyService> _services;
    private readonly int _timeoutSeconds;

    public GracefulShutdown(IEnumerable<ISloyService> services, int timeoutSeconds = 10)
    {
        _services = services.ToList();
        _timeoutSeconds = timeoutSeconds;
    }

    public async Task ShutdownAsync()
    {
        var tasks = _services.Select(s => s.StopAsync());
        var timeout = Task.Delay(TimeSpan.FromSeconds(_timeoutSeconds));
        await Task.WhenAny(Task.WhenAll(tasks), timeout);
    }
}