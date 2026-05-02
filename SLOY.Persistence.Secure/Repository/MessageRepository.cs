using SLOY.Core.Repository;
using SLOY.Persistence.Secure.Database;
using SLOY.Shared.Models;

namespace SLOY.Persistence.Secure.Repository;

public class MessageRepository : IMessageRepository
{
    private readonly NoSqlEncryptedEngine _db;
    public MessageRepository(NoSqlEncryptedEngine db) => _db = db;
    public Task SaveAsync(Message m) { _db.Insert("messages", m.Id, m); return Task.CompletedTask; }
    public Task<IEnumerable<Message>> GetLatestAsync(int count = 20)
    {
        return Task.FromResult(_db.FindAll<Message>("messages").OrderByDescending(x => x.Timestamp).Take(count));
    }
}