using System.Runtime.InteropServices;

namespace sharpfetch.Modules;

public class GpuModule : ModuleBase
{
    public override string Id => "gpu";
    public override string DisplayName => "GPU";
    public override string Description => "Graphics Card Information";
    public override string Group => "hardware";
    public override int Order => 30;
    public override bool EnabledByDefault => true;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return RunAsync(GetGpuInfo, cancellationToken);
    }

    private static string GetGpuInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                var gpuInfo = Helpers.Execute("powershell", "-NoProfile -Command \"Get-CimInstance -ClassName Win32_VideoController | Select-Object -ExpandProperty Name\"");

                if (!string.IsNullOrWhiteSpace(gpuInfo))
                {
                    var lines = gpuInfo.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (lines.Length > 0)
                        return string.Join(", ", lines);
                }
            }
            catch { }

            return "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                var lspci = Helpers.Execute("lspci", "");
                var gpuLines = lspci.Split('\n')
                    .Where(l => l.Contains("VGA compatible controller") || l.Contains("3D controller"))
                    .ToList();

                if (gpuLines.Count > 0)
                {
                    var gpus = gpuLines.Select(line =>
                    {
                        var parts = line.Split(':');
                        return parts.Length > 2 ? parts[2].Trim() : "Unknown";
                    });

                    return string.Join(", ", gpus);
                }
            }
            catch { }

            return "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            try
            {
                var gpuInfo = Helpers.Execute("system_profiler", "SPDisplaysDataType");
                foreach (var line in gpuInfo.Split('\n'))
                {
                    if (line.Contains("Chipset Model:"))
                        return line.Split(':').Last().Trim();
                }
            }
            catch { }

            return "Unknown";
        }

        return "Unknown";
    }
}
