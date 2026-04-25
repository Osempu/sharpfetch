using System.Runtime.InteropServices;

namespace sharpfetch.Modules;

/// <summary>Displays how long the system has been running since last boot.</summary>
public class UptimeModule : ModuleBase
{
    public override string Id => "uptime";
    public override string DisplayName => "Uptime";
    public override string Description => "Time since last boot";
    public override string Group => "status";
    public override int Order => 70;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
        => RunAsync(GetUptime, cancellationToken);

    private string GetUptime()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Helpers.FormatTime(TimeSpan.FromMilliseconds(Environment.TickCount64));
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                var uptimeText = File.ReadAllText("/proc/uptime").Split(' ')[0];
                if (double.TryParse(uptimeText, out var seconds))
                    return Helpers.FormatTime(TimeSpan.FromSeconds(seconds));
            }
            catch { }

            return "Unknown";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                // kern.boottime returns something like: { sec = 1714000000, usec = 0 } ...
                var bootTimeOutput = Helpers.Execute("sysctl", "-n kern.boottime");
                var secPart = bootTimeOutput.Split(',').FirstOrDefault(p => p.Contains("sec ="));
                if (secPart != null)
                {
                    var secStr = secPart.Split('=').Last().Trim();
                    if (long.TryParse(secStr, out var bootEpoch))
                    {
                        var bootTime = DateTimeOffset.FromUnixTimeSeconds(bootEpoch);
                        return Helpers.FormatTime(DateTimeOffset.UtcNow - bootTime);
                    }
                }
            }
            catch { }

            return "Unknown";
        }

        return "Unknown";
    }
}
