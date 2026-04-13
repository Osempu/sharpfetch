using System.Runtime.InteropServices;

namespace sharpfetch.Modules;

public class CpuModule : ModuleBase
{
    public override string Id => "cpu";
    public override string DisplayName => "CPU";
    public override string Description => string.Empty;
    public override int Order => 10;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return RunAsync(GetCpuInfo, cancellationToken);
    }

    private string GetCpuInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment
            .GetEnvironmentVariable("PROCESSOR_IDENTIFIER", EnvironmentVariableTarget.Machine)
                ?? "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var line = File.ReadLines("/proc/cpuinfo")
                .FirstOrDefault(l => l.StartsWith("model name"));
            return line?.Split(':').Last().Trim() ?? "Unknown";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Helpers.Execute("sysctl", "-n machdep.cpu.brand_string");
        }

        return "Unknown";
    }
}