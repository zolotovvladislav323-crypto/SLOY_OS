using SLOY.Core.Repository;
using SLOY.Persistence.Secure.Database;
using SLOY.Shared.Models;

namespace SLOY.Persistence.Secure.Repository;

public class PacketRepository : IPacketRepository
{
    private readonly NoSqlEncryptedEngine _db;
    public PacketRepository(NoSqlEncryptedEngine db) => _db = db;
    public Task SaveAsync(Packet packet) { _db.Insert("packets", packet.Id, packet); return Task.CompletedTask; }
    public Task<Packet?> GetByIdAsync(string id) => Task.FromResult(_db.Find<Packet>("packets", id));
    public Task DeleteAsync(string id) { _db.Delete("packets", id); return Task.CompletedTask; }
}