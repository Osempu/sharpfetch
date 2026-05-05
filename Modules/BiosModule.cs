using System.Runtime.InteropServices;

namespace sharpfetch.Modules;

/// <summary>Displays the BIOS / firmware version.</summary>
public class BiosModule : ModuleBase
{
    public override string Id => "bios";
    public override string DisplayName => "BIOS";
    public override string Description => "BIOS / firmware version";
    public override string Group => "hardware";
    public override int Order => 35;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
        => RunAsync(GetBiosVersion, cancellationToken);

    private string GetBiosVersion()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                var version = Helpers.Execute(
                    "powershell",
                    "-NoProfile -Command \"Get-CimInstance -ClassName Win32_BIOS | Select-Object -ExpandProperty SMBIOSBIOSVersion\"");

                return string.IsNullOrWhiteSpace(version) ? "Unknown" : version;
            }
            catch
            {
                return "Unknown";
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                const string path = "/sys/class/dmi/id/bios_version";
                if (File.Exists(path))
                    return File.ReadAllText(path).Trim();
            }
            catch { }

            return "Unknown";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                var output = Helpers.Execute("system_profiler", "SPHardwareDataType");
                var line = output.Split('\n')
                    .FirstOrDefault(l => l.Contains("Boot ROM Version"));

                if (line != null)
                    return line.Split(':').Last().Trim();
            }
            catch { }

            return "Unknown";
        }

        return "Unknown";
    }
}
