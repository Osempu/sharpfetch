using System.ComponentModel;
using Spectre.Console;

namespace sharpfetch;

public class SharpFetchGraphics
{
    public static void printSysInfo(bool hideIcons)
    {
        var sysInfo = new Sysinfo();

        var grid = new Grid()
               .AddColumn(new GridColumn().NoWrap().PadRight(4))
               .AddColumn()
               .AddRow("[b] OS [/]", $"{Sysinfo.GetOSName()}")
               .AddRow("[b] OS Description[/]", $"{sysInfo.OsDescription}")
               .AddRow("[b] OS Version[/]", $"{sysInfo.OsVersion}")
               .AddRow("[b] CPU[/]", $"{sysInfo.CPU}")
               .AddRow("[b] Memory[/]", $"{sysInfo.MemoryInfo}")
               .AddRow("[b] Uptime[/]", $"{sysInfo.Uptime}")
               .AddRow("[b] Disk Space[/]", $"{sysInfo.DiskInfo}");

        var gridNoIcons = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[b]OS [/]", $"{Sysinfo.GetOSName()}")
            .AddRow("[b]OS Description[/]", $"{sysInfo.OsDescription}")
            .AddRow("[b]OS Version[/]", $"{sysInfo.OsVersion}")
            .AddRow("[b]CPU[/]", $"{sysInfo.CPU}")
            .AddRow("[b]Memory[/]", $"{sysInfo.MemoryInfo}")
            .AddRow("[b]Uptime[/]", $"{sysInfo.Uptime}")
            .AddRow("[b]Disk Space[/]", $"{sysInfo.DiskInfo}");

        AnsiConsole.Write(
            new Panel(hideIcons ? gridNoIcons : grid)
                .Header(hideIcons ? $"{sysInfo.User}@{sysInfo.Machine}" : $" {sysInfo.User}@{sysInfo.Machine}", Justify.Center)
                .RoundedBorder()
                .BorderColor(Color.Green));
    }

}