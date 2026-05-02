namespace SLOY.Infrastructure.Hardware.Radio;

public class RadioEnvironmentMap
{
    private readonly Dictionary<string, SignalSource> _sources = new();
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(30);

    public IReadOnlyDictionary<string, SignalSource> Sources => _sources;

    public void UpdateSource(string id, SignalType type, double signalStrength, double frequency)
    {
        _sources[id] = new SignalSource
        {
            Id = id,
            Type = type,
            SignalStrength = signalStrength,
            Frequency = frequency,
            LastSeen = DateTime.UtcNow
        };
    }

    public void RemoveSource(string id) => _sources.Remove(id);

    public void CleanupExpired()
    {
        var expired = _sources.Where(s => DateTime.UtcNow - s.Value.LastSeen > _ttl)
                              .Select(s => s.Key).ToList();
        foreach (var key in expired)
            _sources.Remove(key);
    }

    public List<SignalSource> GetBestSources(int count = 5)
        => _sources.Values.OrderByDescending(s => s.SignalStrength).Take(count).ToList();

    public double GetNoiseFloor() =>
        _sources.Values.Any() ? _sources.Values.Average(s => s.SignalStrength) * 0.3 : -90;
}

public class SignalSource
{
    public string Id { get; init; } = string.Empty;
    public SignalType Type { get; init; }
    public double SignalStrength { get; init; }
    public double Frequency { get; init; }
    public DateTime LastSeen { get; init; }
}

public enum SignalType
{
    WiFiDirect,
    BluetoothLE,
    Acoustic,
    Optical,
    Unknown
}