using SLOY.Shared.Models;

namespace SLOY.Core.Repository;

public interface IPacketRepository
{
    Task SaveAsync(Packet packet);
    Task<Packet?> GetByIdAsync(string id);
    Task DeleteAsync(string id);
}