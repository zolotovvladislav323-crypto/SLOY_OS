using SLOY.Core.Interfaces;
using SLOY.Shared.Models;

namespace SLOY.Orchestrator.ChannelBonding;

public class ChannelBondingManager
{
    private readonly List<IMeshRouter> _channels;
    private int _roundRobinIndex;

    public ChannelBondingManager(List<IMeshRouter> channels)
    {
        _channels = channels;
    }

    public async Task SendAsync(Packet packet)
    {
        var tasks = _channels.Select(c => c.SendAsync(packet));
        await Task.WhenAll(tasks);
    }

    public async Task SendRoundRobinAsync(Packet packet)
    {
        var channel = _channels[_roundRobinIndex % _channels.Count];
        Interlocked.Increment(ref _roundRobinIndex);
        await channel.SendAsync(packet);
    }

    public async Task<Packet?> ReceiveAnyAsync(CancellationToken ct = default)
    {
        var tasks = _channels.Select(c => c.ReceiveAsync(ct));
        var completed = await Task.WhenAny(tasks);
        return await completed;
    }
}