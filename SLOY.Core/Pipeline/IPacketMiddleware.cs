using SLOY.Shared.Models;

namespace SLOY.Core.Pipeline;

public interface IPacketMiddleware
{
    Task<Packet> ProcessAsync(Packet packet, Func<Packet, Task<Packet>> next);
}