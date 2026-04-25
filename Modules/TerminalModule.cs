using System.Runtime.InteropServices;

namespace sharpfetch.Modules;

/// <summary>Displays the terminal emulator that is running SharpFetch.</summary>
public class TerminalModule : ModuleBase
{
    public override string Id => "terminal";
    public override string DisplayName => "Terminal";
    public override string Description => "Current terminal emulator";
    public override string Group => "environment";
    public override int Order => 55;

    protected override Task<string> GetValueAsync(CancellationToken cancellationToken)
        => FromResult(GetTerminal());

    private string GetTerminal()
    {
        // Cross-platform: TERM_PROGRAM is set by most terminals (iTerm2, Hyper, etc.)
        var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        if (!string.IsNullOrWhiteSpace(termProgram))
            return termProgram;

        var termEmulator = Environment.GetEnvironmentVariable("TERMINAL_EMULATOR");
        if (!string.IsNullOrWhiteSpace(termEmulator))
            return termEmulator;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows Terminal sets WT_SESSION
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WT_SESSION")))
                return "Windows Terminal";

            // VS Code integrated terminal
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VSCODE_GIT_IPC_HANDLE")))
                return "VS Code";

            // ConEmu / Cmder
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConEmuPID")))
                return "ConEmu";

            return "Windows Console Host";
        }

        // Linux / macOS fallback: $TERM describes the terminal type
        var term = Environment.GetEnvironmentVariable("TERM");
        return string.IsNullOrWhiteSpace(term) ? "Unknown" : term;
    }
}
