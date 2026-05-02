using Android.App;
using Android.Content;
using Android.OS;

namespace SLOY.App.Unified.Platforms.Android.Services;

[Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
public class AndroidForegroundService : Service
{
    private const int NotificationId = 1001;
    private const string ChannelId = "sloy_mesh_channel";

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId, params Object[]? args)
    {
        CreateNotificationChannel();

        var notification = new Notification.Builder(this, ChannelId)
            .SetContentTitle("SLOY OS")
            .SetContentText("Меш-сеть активна")
            .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
            .SetOngoing(true)
            .Build();

        StartForeground(NotificationId, notification);

        return StartCommandResult.Sticky;
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                ChannelId,
                "SLOY Mesh Service",
                NotificationImportance.Low
            );
            var manager = GetSystemService(NotificationService) as NotificationManager;
            manager?.CreateNotificationChannel(channel);
        }
    }
}