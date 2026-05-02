using System.Security.Cryptography;

namespace SLOY.Persistence.Secure.Vfs;

public class VirtualFileSystem
{
    private readonly Dictionary<string, VfsNode> _nodes = new();
    private readonly byte[] _rootKey;
    private readonly string _rootPath;

    public VfsNode Root { get; }

    public VirtualFileSystem(string mountPoint, byte[] encryptionKey)
    {
        _rootPath = mountPoint;
        _rootKey = encryptionKey;
        Root = new VfsNode("/", VfsNodeType.Directory);
        _nodes["/"] = Root;

        if (!Directory.Exists(mountPoint))
            Directory.CreateDirectory(mountPoint);
    }

    public VfsNode? CreateFile(string path, byte[] content)
    {
        if (_nodes.ContainsKey(path)) return null;

        var parent = GetParentPath(path);
        if (!_nodes.ContainsKey(parent)) return null;

        var encrypted = EncryptContent(content);
        var physicalPath = GetPhysicalPath(path);
        File.WriteAllBytes(physicalPath, encrypted);

        var node = new VfsNode(path, VfsNodeType.File)
        {
            Size = content.Length,
            PhysicalPath = physicalPath
        };

        _nodes[path] = node;
        _nodes[parent].Children.Add(node);

        return node;
    }

    public VfsNode? CreateDirectory(string path)
    {
        if (_nodes.ContainsKey(path)) return null;

        var parent = GetParentPath(path);
        if (!_nodes.ContainsKey(parent)) return null;

        var node = new VfsNode(path, VfsNodeType.Directory);
        _nodes[path] = node;
        _nodes[parent].Children.Add(node);

        return node;
    }

    public byte[]? ReadFile(string path)
    {
        if (!_nodes.TryGetValue(path, out var node) || node.Type != VfsNodeType.File)
            return null;

        if (!File.Exists(node.PhysicalPath)) return null;

        var encrypted = File.ReadAllBytes(node.PhysicalPath);
        return DecryptContent(encrypted);
    }

    public bool WriteFile(string path, byte[] content)
    {
        if (!_nodes.TryGetValue(path, out var node) || node.Type != VfsNodeType.File)
            return false;

        var encrypted = EncryptContent(content);
        File.WriteAllBytes(node.PhysicalPath!, encrypted);
        node.Size = content.Length;
        return true;
    }

    public bool Delete(string path)
    {
        if (!_nodes.TryGetValue(path, out var node)) return false;

        if (node.Type == VfsNodeType.Directory)
        {
            foreach (var child in node.Children.ToList())
                Delete(child.Path);
        }

        if (node.PhysicalPath != null && File.Exists(node.PhysicalPath))
            File.Delete(node.PhysicalPath);

        _nodes.Remove(path);
        var parent = GetParentPath(path);
        if (_nodes.TryGetValue(parent, out var parentNode))
            parentNode.Children.Remove(node);

        return true;
    }

    public bool Exists(string path) => _nodes.ContainsKey(path);

    public VfsNode? GetNode(string path) => _nodes.TryGetValue(path, out var node) ? node : null;

    public List<VfsNode> ListDirectory(string path)
    {
        if (!_nodes.TryGetValue(path, out var node) || node.Type != VfsNodeType.Directory)
            return new List<VfsNode>();
        return node.Children.ToList();
    }

    private static string GetParentPath(string path)
    {
        var normalized = path.TrimEnd('/');
        var lastSlash = normalized.LastIndexOf('/');
        return lastSlash <= 0 ? "/" : normalized[..lastSlash];
    }

    private string GetPhysicalPath(string virtualPath)
    {
        var hash = Convert.ToHexString(SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(virtualPath)
        ))[..16];
        return Path.Combine(_rootPath, hash);
    }

    private byte[] EncryptContent(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = _rootKey;
        aes.IV = _rootKey[..16];

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    private byte[] DecryptContent(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = _rootKey;
        aes.IV = _rootKey[..16];

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }
}

public class VfsNode
{
    public string Path { get; }
    public VfsNodeType Type { get; }
    public long Size { get; set; }
    public string? PhysicalPath { get; set; }
    public List<VfsNode> Children { get; } = new();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public VfsNode(string path, VfsNodeType type)
    {
        Path = path;
        Type = type;
    }

    public override string ToString() => $"[{Type}] {Path} ({Size} bytes)";
}

public enum VfsNodeType
{
    File,
    Directory
}