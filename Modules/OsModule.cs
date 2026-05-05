using System.Runtime.InteropServices;

namespace sharpfetch.Modules;

public class OsModule : ModuleBase
{
    public override string Id => "os";
    public override string DisplayName => "OS";
    public override string Description => "Operating System Information";
    public override string Group => "system";
    public override int Order => 10;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
    {
        return FromResult(GetOsName());
    }

    private static string GetOsName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "Windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "Linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "Mac OS";
        return "Unknown OS";
    }
}
