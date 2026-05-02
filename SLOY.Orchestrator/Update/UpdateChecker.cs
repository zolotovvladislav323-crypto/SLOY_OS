using System.Text.Json;

namespace SLOY.Orchestrator.Update;

public class UpdateChecker
{
    private readonly string _updateServerUrl;
    private readonly string _currentVersion;
    private readonly UpdateChannel _channel;
    private readonly HttpClient _http;

    public UpdateChecker(string updateServerUrl, string currentVersion, UpdateChannel channel = UpdateChannel.Stable)
    {
        _updateServerUrl = updateServerUrl;
        _currentVersion = currentVersion;
        _channel = channel;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<UpdateManifest?> CheckAsync(CancellationToken ct = default)
    {
        try
        {
            var url = $"{_updateServerUrl}/api/v1/update/check?version={_currentVersion}&channel={_channel.ToString().ToLower()}";
            var response = await _http.GetStringAsync(url, ct);
            var manifest = JsonSerializer.Deserialize<UpdateManifest>(response);

            if (manifest != null && IsNewer(manifest.Version, _currentVersion))
                return manifest;

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsUpdateAvailableAsync(CancellationToken ct = default)
    {
        var manifest = await CheckAsync(ct);
        return manifest != null;
    }

    private static bool IsNewer(string newVersion, string currentVersion)
    {
        if (Version.TryParse(newVersion, out var nv) && Version.TryParse(currentVersion, out var cv))
            return nv > cv;
        return string.Compare(newVersion, currentVersion, StringComparison.Ordinal) > 0;
    }
}