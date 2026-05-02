using SLOY.Shared.Enums;

namespace SLOY.Orchestrator;

public class ProtocolSwitcher
{
    private readonly Dictionary<SignalType, bool> _availableChannels = new();
    private SignalType _currentChannel = SignalType.WiFiDirect;
    private readonly object _lock = new();

    public SignalType CurrentChannel => _currentChannel;
    public event EventHandler<SignalType>? OnChannelSwitched;

    public ProtocolSwitcher()
    {
        foreach (SignalType type in Enum.GetValues<SignalType>())
            _availableChannels[type] = false;
    }

    public void RegisterChannel(SignalType type, bool isAvailable)
    {
        lock (_lock)
        {
            _availableChannels[type] = isAvailable;
        }
    }

    public SignalType SelectBestChannel(int? requiredBandwidth = null)
    {
        lock (_lock)
        {
            var available = _availableChannels.Where(c => c.Value).Select(c => c.Key).ToList();

            if (available.Count == 0)
                return SignalType.BluetoothLE;

            SignalType selected = available switch
            {
                _ when requiredBandwidth > 100_000 && available.Contains(SignalType.WiFiDirect)
                    => SignalType.WiFiDirect,
                _ when available.Contains(SignalType.OpticalQR)
                    => SignalType.OpticalQR,
                _ when available.Contains(SignalType.BluetoothLE)
                    => SignalType.BluetoothLE,
                _ when available.Contains(SignalType.Acoustic)
                    => SignalType.Acoustic,
                _ => available[0]
            };

            if (selected != _currentChannel)
            {
                _currentChannel = selected;
                OnChannelSwitched?.Invoke(this, selected);
            }

            return selected;
        }
    }

    public async Task<bool> SwitchToAsync(SignalType target)
    {
        lock (_lock)
        {
            if (!_availableChannels.TryGetValue(target, out var available) || !available)
                return false;

            _currentChannel = target;
            OnChannelSwitched?.Invoke(this, target);
            return true;
        }
    }

    public List<SignalType> GetAvailableChannels()
    {
        lock (_lock)
        {
            return _availableChannels.Where(c => c.Value).Select(c => c.Key).ToList();
        }
    }
}