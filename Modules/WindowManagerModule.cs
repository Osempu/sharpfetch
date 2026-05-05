using System.Runtime.InteropServices;

namespace sharpfetch.Modules;

/// <summary>Displays the active window manager or desktop environment.</summary>
public class WindowManagerModule : ModuleBase
{
    public override string Id => "windowmanager";
    public override string DisplayName => "Window Manager";
    public override string Description => "Active window manager or desktop environment";
    public override string Group => "environment";
    public override int Order => 60;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
        => RunAsync(GetWindowManager, cancellationToken);

    private string GetWindowManager()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Desktop Window Manager";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "Quartz Compositor";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return GetLinuxWindowManager();

        return "Unknown";
    }

    private string GetLinuxWindowManager()
    {
        // Prefer the environment variables set by display managers / session scripts
        string[] envVars = ["XDG_CURRENT_DESKTOP", "DESKTOP_SESSION", "GDMSESSION", "XDG_SESSION_DESKTOP"];
        foreach (var envVar in envVars)
        {
            var value = Environment.GetEnvironmentVariable(envVar);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        // Fall back to scanning running processes for well-known window manager names
        try
        {
            var processes = Helpers.Execute("ps", "-e");
            string[] knownWms =
            [
                "gnome-shell", "kwin_wayland", "kwin_x11", "kwin",
                "xfwm4", "openbox", "i3", "sway", "awesome",
                "dwm", "bspwm", "fluxbox", "icewm", "marco"
            ];

            foreach (var wm in knownWms)
            {
                if (processes.Contains(wm, StringComparison.OrdinalIgnoreCase))
                    return wm;
            }
        }
        catch { }

        return "Unknown";
    }
}
