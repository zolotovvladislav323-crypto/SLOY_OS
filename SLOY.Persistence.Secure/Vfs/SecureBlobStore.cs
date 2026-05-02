using System.Security.Cryptography;

namespace SLOY.Persistence.Secure.Vfs;

public class SecureBlobStore
{
    private readonly VirtualFileSystem _vfs;
    private readonly string _storePath;
    private readonly Dictionary<string, BlobMetadata> _metadata = new();

    public SecureBlobStore(VirtualFileSystem vfs, string storePath = "/store/blobs")
    {
        _vfs = vfs;
        _storePath = storePath;

        if (!_vfs.Exists(storePath))
            _vfs.CreateDirectory(storePath);

        LoadMetadata();
    }

    public string Store(byte[] data, string? tag = null, TimeSpan? ttl = null)
    {
        var blobId = GenerateBlobId(data);
        var blobPath = $"{_storePath}/{blobId}";

        if (!_vfs.Exists(blobPath))
        {
            _vfs.CreateFile(blobPath, data);
            _metadata[blobId] = new BlobMetadata
            {
                Id = blobId,
                Size = data.Length,
                Tag = tag,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = ttl.HasValue ? DateTime.UtcNow + ttl.Value : null
            };
        }

        return blobId;
    }

    public byte[]? Retrieve(string blobId)
    {
        if (!_metadata.TryGetValue(blobId, out var meta)) return null;

        if (meta.ExpiresAt.HasValue && DateTime.UtcNow > meta.ExpiresAt.Value)
        {
            Delete(blobId);
            return null;
        }

        var blobPath = $"{_storePath}/{blobId}";
        return _vfs.ReadFile(blobPath);
    }

    public bool Delete(string blobId)
    {
        var blobPath = $"{_storePath}/{blobId}";
        _metadata.Remove(blobId);
        return _vfs.Delete(blobPath);
    }

    public List<BlobMetadata> FindByTag(string tag)
    {
        return _metadata.Values.Where(m => m.Tag == tag).ToList();
    }

    public void CleanupExpired()
    {
        var expired = _metadata.Values
            .Where(m => m.ExpiresAt.HasValue && DateTime.UtcNow > m.ExpiresAt.Value)
            .Select(m => m.Id)
            .ToList();

        foreach (var id in expired)
            Delete(id);
    }

    public long GetTotalSize() => _metadata.Values.Sum(m => m.Size);

    private static string GenerateBlobId(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash)[..32];
    }

    private void LoadMetadata()
    {
        var metadataPath = $"{_storePath}/.metadata";
        if (_vfs.Exists(metadataPath))
        {
            var data = _vfs.ReadFile(metadataPath);
            if (data != null)
            {
                var json = System.Text.Encoding.UTF8.GetString(data);
                var list = System.Text.Json.JsonSerializer.Deserialize<List<BlobMetadata>>(json);
                if (list != null)
                {
                    foreach (var meta in list)
                        _metadata[meta.Id] = meta;
                }
            }
        }
    }

    public void SaveMetadata()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_metadata.Values.ToList());
        var data = System.Text.Encoding.UTF8.GetBytes(json);
        var metadataPath = $"{_storePath}/.metadata";

        if (!_vfs.Exists(metadataPath))
            _vfs.CreateFile(metadataPath, data);
        else
            _vfs.WriteFile(metadataPath, data);
    }
}

public class BlobMetadata
{
    public string Id { get; init; } = string.Empty;
    public long Size { get; init; }
    public string? Tag { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
}