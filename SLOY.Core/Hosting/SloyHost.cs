using Microsoft.Extensions.DependencyInjection;
using SLOY.Core.Health;
using SLOY.Core.Interfaces;

namespace SLOY.Core.Hosting;

public class SloyHost
{
    private readonly IServiceProvider _services;
    private readonly List<ISloyService> _hostedServices = new();
    private CancellationTokenSource? _cts;

    public SloyHost(IServiceProvider services)
    {
        _services = services;
    }

    public SloyHost Register<T>() where T : ISloyService
    {
        var service = _services.GetRequiredService<T>();
        _hostedServices.Add(service);
        return this;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        foreach (var service in _hostedServices)
            await service.StartAsync(_cts.Token);
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();
        foreach (var service in _hostedServices)
            await service.StopAsync();
    }
}