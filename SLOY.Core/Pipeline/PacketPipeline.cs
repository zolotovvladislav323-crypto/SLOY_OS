using SLOY.Shared.Models;

namespace SLOY.Core.Pipeline;

public class PacketPipeline
{
    private readonly List<IPacketMiddleware> _middlewares = new();

    public PacketPipeline Use(IPacketMiddleware middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public async Task<Packet> ExecuteAsync(Packet packet)
    {
        Func<Packet, Task<Packet>> handler = p => Task.FromResult(p);
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var m = _middlewares[i];
            var next = handler;
            handler = p => m.ProcessAsync(p, next);
        }
        return await handler(packet);
    }
}