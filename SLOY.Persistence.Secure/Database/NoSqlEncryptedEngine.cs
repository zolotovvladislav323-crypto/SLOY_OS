using System.Security.Cryptography;
using System.Text.Json;

namespace SLOY.Persistence.Secure.Database;

public class NoSqlEncryptedEngine
{
    private readonly string _storagePath;
    private readonly byte[] _encryptionKey;
    private readonly Dictionary<string, byte[]> _cache = new();

    public NoSqlEncryptedEngine(string storagePath, byte[] encryptionKey)
    {
        _storagePath = storagePath;
        _encryptionKey = encryptionKey;

        if (!Directory.Exists(storagePath))
            Directory.CreateDirectory(storagePath);

        LoadAllToCache();
    }

    public void Insert<T>(string collection, string id, T document)
    {
        var json = JsonSerializer.Serialize(document);
        var data = System.Text.Encoding.UTF8.GetBytes(json);
        var encrypted = Encrypt(data);

        var key = $"{collection}:{id}";
        _cache[key] = encrypted;

        var filePath = GetFilePath(collection, id);
        File.WriteAllBytes(filePath, encrypted);
    }

    public T? Find<T>(string collection, string id)
    {
        var key = $"{collection}:{id}";
        if (!_cache.TryGetValue(key, out var encrypted))
        {
            var filePath = GetFilePath(collection, id);
            if (!File.Exists(filePath)) return default;
            encrypted = File.ReadAllBytes(filePath);
            _cache[key] = encrypted;
        }

        var decrypted = Decrypt(encrypted);
        var json = System.Text.Encoding.UTF8.GetString(decrypted);
        return JsonSerializer.Deserialize<T>(json);
    }

    public List<T> FindAll<T>(string collection)
    {
        var results = new List<T>();
        var dir = Path.Combine(_storagePath, collection);

        if (!Directory.Exists(dir)) return results;

        foreach (var file in Directory.GetFiles(dir, "*.enc"))
        {
            var id = Path.GetFileNameWithoutExtension(file);
            var doc = Find<T>(collection, id);
            if (doc != null) results.Add(doc);
        }

        return results;
    }

    public void Delete(string collection, string id)
    {
        var key = $"{collection}:{id}";
        _cache.Remove(key);

        var filePath = GetFilePath(collection, id);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public bool Exists(string collection, string id)
    {
        var key = $"{collection}:{id}";
        return _cache.ContainsKey(key) || File.Exists(GetFilePath(collection, id));
    }

    public void WipeAll()
    {
        _cache.Clear();
        if (Directory.Exists(_storagePath))
            Directory.Delete(_storagePath, true);
        Directory.CreateDirectory(_storagePath);
    }

    private string GetFilePath(string collection, string id)
    {
        var dir = Path.Combine(_storagePath, collection);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var safeId = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(id)))[..16];
        return Path.Combine(dir, $"{safeId}.enc");
    }

    private void LoadAllToCache()
    {
        if (!Directory.Exists(_storagePath)) return;

        foreach (var collectionDir in Directory.GetDirectories(_storagePath))
        {
            var collection = Path.GetFileName(collectionDir);
            foreach (var file in Directory.GetFiles(collectionDir, "*.enc"))
            {
                var id = Path.GetFileNameWithoutExtension(file);
                var key = $"{collection}:{id}";
                _cache[key] = File.ReadAllBytes(file);
            }
        }
    }

    private byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = _encryptionKey[..16];

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    private byte[] Decrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = _encryptionKey[..16];

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }
}