namespace SLOY.Orchestrator.Update;

public class UpdateDownloader
{
    private readonly HttpClient _http;

    public event EventHandler<UpdateProgress>? OnProgressChanged;

    public UpdateDownloader()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
    }

    public async Task<byte[]> DownloadAsync(string url, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        using var stream = await response.Content.ReadAsStreamAsync(ct);

        var buffer = new byte[8192];
        var data = new List<byte>();
        var totalRead = 0L;

        while (true)
        {
            var read = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
            if (read == 0) break;

            data.AddRange(buffer.Take(read));
            totalRead += read;

            OnProgressChanged?.Invoke(this, new UpdateProgress
            {
                Stage = UpdateStage.Downloading,
                BytesDownloaded = totalRead,
                TotalBytes = totalBytes,
                PercentComplete = totalBytes > 0 ? (double)totalRead / totalBytes * 100 : 0,
                StatusMessage = $"Скачано {totalRead / 1024} КБ"
            });
        }

        return data.ToArray();
    }
}