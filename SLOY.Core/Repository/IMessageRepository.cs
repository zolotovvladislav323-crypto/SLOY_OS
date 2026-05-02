using SLOY.Shared.Models;

namespace SLOY.Core.Repository;

public interface IMessageRepository
{
    Task SaveAsync(Message message);
    Task<IEnumerable<Message>> GetLatestAsync(int count = 20);
}