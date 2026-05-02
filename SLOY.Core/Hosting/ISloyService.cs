namespace SLOY.Core.Hosting;

public interface ISloyService
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync();
    string Name { get; }
    string Status { get; }
}