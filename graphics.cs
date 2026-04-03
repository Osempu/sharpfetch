using Spectre.Console;
using Spectre.Console.Rendering;

namespace sharpfetch;

public class SharpFetchGraphics
{
    public static void printSysInfo(bool hideIcons, bool isMinimal)
    {
        var sysInfo = new Sysinfo();

        ConsoleOutput.PrintAsPanels(sysInfo);

        ConsoleOutput.PrintAsTrees(sysInfo);

        ConsoleOutput.PrintMinmal(sysInfo);

        ConsoleOutput.PrintAsLeftPanel(sysInfo);
    }
}