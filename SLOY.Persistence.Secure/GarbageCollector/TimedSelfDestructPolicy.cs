namespace SLOY.Persistence.Secure.GarbageCollector;

public class TimedSelfDestructPolicy : INukeTarget
{
    private readonly string _targetPath;
    private readonly TimeSpan _idleTimeout;
    private DateTime _lastActivity;
    private readonly object _lock = new();

    public int Priority => 10;
    public TimeSpan TimeUntilDestruct => _idleTimeout - (DateTime.UtcNow - _lastActivity);

    public TimedSelfDestructPolicy(string targetPath, TimeSpan idleTimeout)
    {
        _targetPath = targetPath;
        _idleTimeout = idleTimeout;
        _lastActivity = DateTime.UtcNow;
    }

    public void RecordActivity()
    {
        lock (_lock)
        {
            _lastActivity = DateTime.UtcNow;
        }
    }

    public bool ShouldDestruct()
    {
        lock (_lock)
        {
            return DateTime.UtcNow - _lastActivity > _idleTimeout;
        }
    }

    public async Task NukeAsync()
    {
        await Task.Run(() =>
        {
            if (Directory.Exists(_targetPath))
            {
                var shredder = new AutoShredder(_targetPath, TimeSpan.Zero);
                foreach (var file in Directory.GetFiles(_targetPath, "*", SearchOption.AllDirectories))
                {
                    var info = new FileInfo(file);
                    info.Attributes = FileAttributes.Normal;
                }
                Directory.Delete(_targetPath, true);
            }

            if (File.Exists(_targetPath))
            {
                var info = new FileInfo(_targetPath);
                info.Attributes = FileAttributes.Normal;
                File.Delete(_targetPath);
            }
        });
    }
}