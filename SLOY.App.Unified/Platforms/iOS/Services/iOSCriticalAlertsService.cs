using UserNotifications;

namespace SLOY.App.Unified.Platforms.iOS.Services;

public class iOSCriticalAlertsService
{
    private bool _authorized;

    public async Task<bool> RequestAuthorizationAsync()
    {
        var center = UNUserNotificationCenter.Current;
        var (granted, _) = await center.RequestAuthorizationAsync(
            UNAuthorizationOptions.Alert |
            UNAuthorizationOptions.Sound |
            UNAuthorizationOptions.CriticalAlert
        );

        _authorized = granted;
        return granted;
    }

    public async Task SendCriticalAlertAsync(string title, string body)
    {
        if (!_authorized) return;

        var content = new UNMutableNotificationContent
        {
            Title = title,
            Body = body,
            Sound = UNNotificationSound.DefaultCriticalSound
        };

        var request = UNNotificationRequest.FromIdentifier(
            Guid.NewGuid().ToString(),
            content,
            null
        );

        await UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
    }
}