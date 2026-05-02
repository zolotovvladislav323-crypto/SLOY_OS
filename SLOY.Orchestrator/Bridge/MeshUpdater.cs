using System.Security.Cryptography;

namespace SLOY.Orchestrator.Bridge;

public class MeshUpdater
{
    private readonly string _updateServerUrl;
    private readonly byte[] _publicKey;
    private bool _isUpdating;

    public Version CurrentVersion { get; }
    public Version? AvailableVersion { get; private set; }
    public bool IsUpdateAvailable => AvailableVersion != null && AvailableVersion > CurrentVersion;

    public event EventHandler<Version>? OnUpdateAvailable;
    public event EventHandler<double>? OnDownloadProgress;
    public event EventHandler? OnUpdateComplete;

    public MeshUpdater(Version currentVersion, byte[] publicKey, string updateServerUrl = "https://update.sloy.mesh")
    {
        CurrentVersion = currentVersion;
        _publicKey = publicKey;
        _updateServerUrl = updateServerUrl;
    }

    public async Task CheckForUpdatesAsync()
    {
        try
        {
            var latestVersion = await FetchLatestVersionAsync();
            AvailableVersion = latestVersion;

            if (IsUpdateAvailable)
                OnUpdateAvailable?.Invoke(this, AvailableVersion);
        }
        catch
        {
            // Офлайн — игнорируем
        }
    }

    public async Task<bool> DownloadAndApplyAsync(string tempPath)
    {
        if (!IsUpdateAvailable || _isUpdating) return false;

        _isUpdating = true;

        try
        {
            var updateData = await DownloadUpdateAsync();
            OnDownloadProgress?.Invoke(this, 100);

            if (!VerifySignature(updateData))
                return false;

            var updatePath = Path.Combine(tempPath, $"sloy_update_{AvailableVersion}.zip");
            await File.WriteAllBytesAsync(updatePath, updateData);

            ApplyUpdate(updatePath);
            OnUpdateComplete?.Invoke(this, EventArgs.Empty);

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private Task<Version> FetchLatestVersionAsync()
    {
        return Task.FromResult(new Version(0, 2, 0));
    }

    private async Task<byte[]> DownloadUpdateAsync()
    {
        var data = new byte[1024 * 1024];

        for (int i = 0; i <= 100; i += 10)
        {
            OnDownloadProgress?.Invoke(this, i);
            await Task.Delay(50);
        }

        return data;
    }

    private bool VerifySignature(byte[] data)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(_publicKey, out _);

            var signatureSize = rsa.KeySize / 8;
            var signature = data.Take(signatureSize).ToArray();
            var content = data.Skip(signatureSize).ToArray();

            return rsa.VerifyData(content, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pss);
        }
        catch
        {
            return false;
        }
    }

    private void ApplyUpdate(string updatePath)
    {
        var updateDir = Path.Combine(Path.GetTempPath(), "sloy_update");
        if (Directory.Exists(updateDir))
            Directory.Delete(updateDir, true);

        System.IO.Compression.ZipFile.ExtractToDirectory(updatePath, updateDir);

        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        foreach (var file in Directory.GetFiles(updateDir))
        {
            var dest = Path.Combine(currentDir, Path.GetFileName(file));
            File.Copy(file, dest, true);
        }

        File.Delete(updatePath);
        Directory.Delete(updateDir, true);
    }
}