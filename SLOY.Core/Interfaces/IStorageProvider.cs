namespace SLOY.Core.Interfaces;

public interface IStorageProvider
{
    Task SaveAsync(string key, byte[] data);
    Task<byte[]?> LoadAsync(string key);
    Task DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<IEnumerable<string>> ListKeysAsync();
    Task WipeAllAsync();
    Task<long> GetUsedSpaceBytesAsync();
}