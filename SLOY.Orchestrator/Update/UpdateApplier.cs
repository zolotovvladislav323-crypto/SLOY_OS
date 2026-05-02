using System.IO.Compression;

namespace SLOY.Orchestrator.Update;

public class UpdateApplier
{
    private readonly DeltaPatcher _deltaPatcher;
    private readonly string _appDirectory;

    public UpdateApplier(string appDirectory)
    {
        _appDirectory = appDirectory;
        _deltaPatcher = new DeltaPatcher();
    }

    public async Task ApplyAsync(UpdatePackage package, CancellationToken ct = default)
    {
        var backupDir = Path.Combine(_appDirectory, ".backup", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        Directory.CreateDirectory(backupDir);

        if (package.IsDelta)
        {
            await ApplyDeltaAsync(package, backupDir, ct);
        }
        else
        {
            await ApplyFullAsync(package, backupDir, ct);
        }

        CleanupOldBackups(keepCount: 3);
    }

    private Task ApplyFullAsync(UpdatePackage package, string backupDir, CancellationToken ct)
    {
        var extractDir = Path.Combine(Path.GetTempPath(), "sloy_update");
        if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);

        File.WriteAllBytes(package.TempPath, package.Data);
        ZipFile.ExtractToDirectory(package.TempPath, extractDir);

        foreach (var file in package.Manifest.AffectedFiles)
        {
            var source = Path.Combine(extractDir, file);
            var dest = Path.Combine(_appDirectory, file);
            var backup = Path.Combine(backupDir, file);

            if (File.Exists(dest))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(backup)!);
                File.Copy(dest, backup, true);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(source, dest, true);
        }

        File.Delete(package.TempPath);
        Directory.Delete(extractDir, true);

        return Task.CompletedTask;
    }

    private Task ApplyDeltaAsync(UpdatePackage package, string backupDir, CancellationToken ct)
    {
        foreach (var file in package.Manifest.AffectedFiles)
        {
            var current = Path.Combine(_appDirectory, file);
            var backup = Path.Combine(backupDir, file);

            if (File.Exists(current))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(backup)!);
                File.Copy(current, backup, true);
            }

            var newContent = _deltaPatcher.Apply(File.ReadAllBytes(current), package.Data);
            File.WriteAllBytes(current, newContent);
        }

        return Task.CompletedTask;
    }

    private void CleanupOldBackups(int keepCount)
    {
        var backupRoot = Path.Combine(_appDirectory, ".backup");
        if (!Directory.Exists(backupRoot)) return;

        var backups = Directory.GetDirectories(backupRoot).OrderByDescending(d => d).ToList();
        foreach (var old in backups.Skip(keepCount))
            Directory.Delete(old, true);
    }
}