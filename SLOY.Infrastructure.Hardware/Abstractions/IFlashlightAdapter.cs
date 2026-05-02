namespace SLOY.Infrastructure.Hardware.Abstractions;

public interface IFlashlightAdapter
{
    bool IsAvailable { get; }
    bool IsOn { get; }
    Task<bool> TurnOnAsync();
    Task<bool> TurnOffAsync();
    Task ToggleAsync();
    Task StrobeAsync(int frequencyHz, int durationMs);
    Task PulseAsync(int pulseCount, int onMs, int offMs);
}