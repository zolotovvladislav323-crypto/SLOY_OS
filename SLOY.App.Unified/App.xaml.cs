using SLOY.Shared.Models;

namespace SLOY.App.Unified;

public partial class App : Application
{
    public static Identity? CurrentIdentity { get; set; }

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}