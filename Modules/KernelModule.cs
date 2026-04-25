using System.Runtime.InteropServices;

namespace sharpfetch.Modules;

/// <summary>Displays the kernel / OS build version.</summary>
public class KernelModule : ModuleBase
{
    public override string Id => "kernel";
    public override string DisplayName => "Kernel";
    public override string Description => "Kernel / OS build version";
    public override string Group => "system";
    public override int Order => 15;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
        => RunAsync(GetKernelVersion, cancellationToken);

    private string GetKernelVersion()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.OSVersion.Version.ToString();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Helpers.Execute("uname", "-r");
        }

        return "Unknown";
    }
}
