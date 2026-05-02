using AVFoundation;
using Foundation;
using UIKit;
using UserNotifications;

namespace SLOY.App.Unified;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        RequestPermissions();
        return base.FinishedLaunching(application, launchOptions);
    }

    private async void RequestPermissions()
    {
        await UNUserNotificationCenter.Current.RequestAuthorizationAsync(
            UNAuthorizationOptions.Alert |
            UNAuthorizationOptions.Sound |
            UNAuthorizationOptions.CriticalAlert
        );

        var cameraStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaTypes.Video);
        if (cameraStatus == AVAuthorizationStatus.NotDetermined)
            await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaTypes.Video);

        var audioStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaTypes.Audio);
        if (audioStatus == AVAuthorizationStatus.NotDetermined)
            await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaTypes.Audio);
    }
}