using System.Runtime.InteropServices;

namespace sharpfetch.Modules;

/// <summary>Displays the current shell (e.g. bash, zsh, PowerShell).</summary>
public class ShellModule : ModuleBase
{
    public override string Id => "shell";
    public override string DisplayName => "Shell";
    public override string Description => "Current shell";
    public override string Group => "environment";
    public override int Order => 50;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
        => FromResult(GetShell());

    private string GetShell()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var psModulePath = Environment.GetEnvironmentVariable("PSModulePath");
            if (!string.IsNullOrWhiteSpace(psModulePath))
            {
                return psModulePath.Contains("PowerShell\\7") || psModulePath.Contains("PowerShell/7")
                    ? "PowerShell Core"
                    : "Windows PowerShell";
            }

            return "cmd.exe";
        }

        // Unix-like: $SHELL holds the path (e.g. /bin/zsh)
        var shell = Environment.GetEnvironmentVariable("SHELL");
        return string.IsNullOrWhiteSpace(shell) ? "Unknown" : Path.GetFileName(shell);
    }
}
