using SLOY.Infrastructure.Hardware.Radio;

namespace SLOY.UI.Forensics.Diagnostics;

public class SignalLevelMonitor
{
    private readonly RadioEnvironmentMap _radioMap;
    private readonly Dictionary<string, List<double>> _history = new();
    private readonly int _maxHistoryPoints;

    public SignalLevelMonitor(RadioEnvironmentMap radioMap, int maxHistoryPoints = 100)
    {
        _radioMap = radioMap;
        _maxHistoryPoints = maxHistoryPoints;
    }

    public void RecordSample()
    {
        foreach (var source in _radioMap.Sources.Values)
        {
            if (!_history.ContainsKey(source.Id))
                _history[source.Id] = new List<double>();

            _history[source.Id].Add(source.SignalStrength);

            if (_history[source.Id].Count > _maxHistoryPoints)
                _history[source.Id].RemoveAt(0);
        }
    }

    public double GetAverageSignal(string sourceId)
    {
        if (!_history.TryGetValue(sourceId, out var history) || history.Count == 0)
            return -100;

        return history.Average();
    }

    public double GetSignalQuality(string sourceId)
    {
        var avg = GetAverageSignal(sourceId);
        if (avg < -90) return 0;
        if (avg > -30) return 100;
        return (avg + 90) / 60 * 100;
    }

    public string GetSignalCategory(string sourceId)
    {
        var quality = GetSignalQuality(sourceId);
        return quality switch
        {
            >= 80 => "Отличный",
            >= 60 => "Хороший",
            >= 40 => "Средний",
            >= 20 => "Слабый",
            _ => "Критический"
        };
    }

    public Dictionary<string, SignalReport> GetReport()
    {
        return _history.ToDictionary(
            h => h.Key,
            h => new SignalReport
            {
                SourceId = h.Key,
                AverageDb = h.Value.Count > 0 ? h.Value.Average() : -100,
                MinDb = h.Value.Count > 0 ? h.Value.Min() : -100,
                MaxDb = h.Value.Count > 0 ? h.Value.Max() : -100,
                Quality = GetSignalQuality(h.Key),
                Category = GetSignalCategory(h.Key)
            }
        );
    }
}

public class SignalReport
{
    public string SourceId { get; init; } = string.Empty;
    public double AverageDb { get; init; }
    public double MinDb { get; init; }
    public double MaxDb { get; init; }
    public double Quality { get; init; }
    public string Category { get; init; } = string.Empty;
}