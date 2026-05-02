namespace SLOY.App.Unified.Services;

public class PermissionService
{
    private readonly Dictionary<string, PermissionStatus> _permissions = new();

    public async Task<bool> RequestAsync<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        var status = await Permissions.CheckStatusAsync<TPermission>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<TPermission>();

        _permissions[typeof(TPermission).Name] = status;
        return status == PermissionStatus.Granted;
    }

    public PermissionStatus GetStatus<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        var key = typeof(TPermission).Name;
        return _permissions.TryGetValue(key, out var status) ? status : PermissionStatus.Unknown;
    }

    public async Task<bool> RequestAllAsync()
    {
        var results = await Task.WhenAll(
            RequestAsync<Permissions.Camera>(),
            RequestAsync<Permissions.Microphone>(),
            RequestAsync<Permissions.LocationWhenInUse>(),
            RequestAsync<Permissions.Bluetooth>()
        );

        return results.All(r => r);
    }
}