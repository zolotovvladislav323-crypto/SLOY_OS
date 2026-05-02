using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SLOY.App.Unified;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize |
                           ConfigChanges.Orientation |
                           ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize |
                           ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            RequestPermissions(
                new[]
                {
                    Android.Manifest.Permission.Camera,
                    Android.Manifest.Permission.RecordAudio,
                    Android.Manifest.Permission.AccessFineLocation,
                    Android.Manifest.Permission.AccessCoarseLocation,
                    Android.Manifest.Permission.Bluetooth,
                    Android.Manifest.Permission.BluetoothAdmin,
                    Android.Manifest.Permission.BluetoothAdvertise,
                    Android.Manifest.Permission.BluetoothConnect,
                    Android.Manifest.Permission.BluetoothScan,
                    Android.Manifest.Permission.ForegroundService,
                    Android.Manifest.Permission.ForegroundServiceDataSync,
                    Android.Manifest.Permission.WakeLock,
                    Android.Manifest.Permission.Flashlight,
                    Android.Manifest.Permission.Vibrate,
                    Android.Manifest.Permission.Internet,
                    Android.Manifest.Permission.AccessNetworkState,
                    Android.Manifest.Permission.ChangeNetworkState,
                    Android.Manifest.Permission.AccessWifiState,
                    Android.Manifest.Permission.ChangeWifiState,
                    Android.Manifest.Permission.NearbyWifiDevices
                },
                0
            );
        }
    }
}