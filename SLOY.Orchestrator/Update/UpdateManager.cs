using Microsoft.Extensions.Logging;

namespace SLOY.Orchestrator.Update;

public class UpdateManager
{
    private readonly UpdateChecker _checker;
    private readonly UpdateDownloader _downloader;
    private readonly UpdateVerifier _verifier;
    private readonly UpdateApplier _applier;
    private readonly UpdateRollback _rollback;
    private readonly ILogger<UpdateManager> _logger;

    public event EventHandler<UpdateProgress>? OnProgressChanged;
    public event EventHandler<string>? OnUpdateAvailable;
    public event EventHandler? OnUpdateComplete;

    public UpdateManager(
        string updateServerUrl,
        string currentVersion,
        byte[] publicKey,
        string appDirectory,
        UpdateChannel channel = UpdateChannel.Stable,
        ILogger<UpdateManager>? logger = null)
    {
        _checker = new UpdateChecker(updateServerUrl, currentVersion, channel);
        _downloader = new UpdateDownloader();
        _verifier = new UpdateVerifier(publicKey);
        _applier = new UpdateApplier(appDirectory);
        _rollback = new UpdateRollback(appDirectory);
        _logger = logger;

        _downloader.OnProgressChanged += (_, p) => OnProgressChanged?.Invoke(this, p);
    }

    public async Task<bool> CheckAndUpdateAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Проверка обновлений...");

        ReportProgress(UpdateStage.Checking, 0, "Проверка обновлений...");

        var manifest = await _checker.CheckAsync(ct);
        if (manifest == null)
        {
            _logger?.LogInformation("Обновлений нет.");
            ReportProgress(UpdateStage.Complete, 100, "Обновлений нет.");
            return false;
        }

        OnUpdateAvailable?.Invoke(this, $"{manifest.Version} ({manifest.Channel})");
        _logger?.LogInformation("Найдено обновление: {Version}", manifest.Version);

        return await DownloadAndApplyAsync(manifest, ct);
    }

    public async Task<bool> DownloadAndApplyAsync(UpdateManifest manifest, CancellationToken ct = default)
    {
        try
        {
            var url = manifest.DownloadUrl;
            var isDelta = !string.IsNullOrEmpty(manifest.DeltaUrl) && manifest.DeltaSizeBytes < manifest.SizeBytes;

            if (isDelta)
            {
                url = manifest.DeltaUrl!;
                _logger?.LogInformation("Загрузка дельта-обновления...");
            }

            ReportProgress(UpdateStage.Downloading, 10, "Загрузка...");
            var data = await _downloader.DownloadAsync(url, ct);

            var package = new UpdatePackage
            {
                Manifest = manifest,
                Data = data,
                IsDelta = isDelta
            };

            ReportProgress(UpdateStage.Verifying, 80, "Проверка подписи...");
            if (!_verifier.Verify(package))
            {
                _logger?.LogError("Ошибка проверки подписи обновления.");
                ReportProgress(UpdateStage.Failed, 0, "Ошибка подписи.");
                return false;
            }

            ReportProgress(UpdateStage.Applying, 90, "Применение обновления...");
            await _applier.ApplyAsync(package, ct);

            ReportProgress(UpdateStage.Complete, 100, "Обновление установлено.");
            OnUpdateComplete?.Invoke(this, EventArgs.Empty);
            _logger?.LogInformation("Обновление успешно установлено.");

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Ошибка установки обновления.");
            ReportProgress(UpdateStage.Failed, 0, ex.Message);

            if (_rollback.GetAvailableBackups().Any())
            {
                _logger?.LogInformation("Выполняется откат...");
                ReportProgress(UpdateStage.RollingBack, 50, "Откат...");
                await _rollback.RollbackAsync(ct);
            }

            return false;
        }
    }

    public async Task<bool> RollbackAsync(CancellationToken ct = default)
    {
        return await _rollback.RollbackAsync(ct);
    }

    public List<DateTime> GetAvailableBackups() => _rollback.GetAvailableBackups();

    private void ReportProgress(UpdateStage stage, double percent, string message)
    {
        OnProgressChanged?.Invoke(this, new UpdateProgress
        {
            Stage = stage,
            PercentComplete = percent,
            StatusMessage = message
        });
    }
}