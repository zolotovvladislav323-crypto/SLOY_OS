namespace SLOY.Orchestrator.Update;

public class UpdateRollback
{
    private readonly string _appDirectory;

    public UpdateRollback(string appDirectory)
    {
        _appDirectory = appDirectory;
    }

    public async Task<bool> RollbackAsync(CancellationToken ct = default)
    {
        var backupRoot = Path.Combine(_appDirectory, ".backup");
        if (!Directory.Exists(backupRoot)) return false;

        var latestBackup = Directory.GetDirectories(backupRoot)
                                    .OrderByDescending(d => d)
                                    .FirstOrDefault();

        if (latestBackup == null) return false;

        foreach (var backupFile in Directory.GetFiles(latestBackup, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(latestBackup, backupFile);
            var dest = Path.Combine(_appDirectory, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(backupFile, dest, true);
        }

        Directory.Delete(latestBackup, true);
        await Task.CompletedTask;
        return true;
    }

    public List<DateTime> GetAvailableBackups()
    {
        var backupRoot = Path.Combine(_appDirectory, ".backup");
        if (!Directory.Exists(backupRoot)) return new List<DateTime>();

        return Directory.GetDirectories(backupRoot)
                        .Select(d => DateTime.TryParseExact(Path.GetFileName(d), "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dt) ? dt : DateTime.MinValue)
                        .Where(dt => dt != DateTime.MinValue)
                        .OrderByDescending(dt => dt)
                        .ToList();
    }
}