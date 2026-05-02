using Microsoft.Extensions.Logging;

namespace SLOY.Core.Logging;

public static class SloyLogger
{
    private static ILoggerFactory? _factory;
    private static readonly object _lock = new();

    public static void Initialize(ILoggerFactory factory)
    {
        lock (_lock)
        {
            _factory = factory;
        }
    }

    public static ILogger<T> Create<T>()
    {
        lock (_lock)
        {
            if (_factory == null)
                throw new InvalidOperationException("Logger not initialized. Call SloyLogger.Initialize() first.");

            return _factory.CreateLogger<T>();
        }
    }

    public static ILogger Create(string category)
    {
        lock (_lock)
        {
            if (_factory == null)
                throw new InvalidOperationException("Logger not initialized.");

            return _factory.CreateLogger(category);
        }
    }
}