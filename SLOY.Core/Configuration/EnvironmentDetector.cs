namespace SLOY.Core.Configuration;

public static class EnvironmentDetector
{
    public static bool IsAndroid => OperatingSystem.IsAndroid();
    public static bool IsIOS => OperatingSystem.IsIOS();
    public static bool IsWindows => OperatingSystem.IsWindows();
    public static bool IsMacOS => OperatingSystem.IsMacOS();
    public static bool IsLinux => OperatingSystem.IsLinux();
    public static bool IsMobile => IsAndroid || IsIOS;
    public static bool IsDesktop => IsWindows || IsMacOS || IsLinux;
    public static string OS => IsAndroid ? "android" : IsIOS ? "ios" : IsWindows ? "windows" : IsMacOS ? "macos" : IsLinux ? "linux" : "unknown";
}