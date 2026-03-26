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
               .AddRow("[red]󰒋 OS [/]", $"[red]{Sysinfo.GetOSName()}[/]")
               .AddRow("[blue] OS Description[/]", $"[blue]{sysInfo.OsDescription}[/]")
               .AddRow("[yellow] Kernel[/]", $"[yellow]{sysInfo.Kernel}[/]")
               .AddRow("[yellow] CPU[/]", $"[yellow]{sysInfo.CPU}[/]")
               .AddRow("[magenta] Memory[/]", $"[magenta]{sysInfo.MemoryInfo}[/]")
               .AddRow("[cyan]󰾲 GPU[/]", $"[cyan]{sysInfo.GPU}[/]")
               .AddRow("[red]󱑁 Uptime[/]", $"[red]{sysInfo.Uptime}[/]")
               .AddRow("[blue] Window Manager[/]", $"[blue]{sysInfo.WindowManager}[/]")
               .AddRow("[green] Terminal[/]", $"[green]{sysInfo.Terminal}[/]")
               .AddRow("[yellow] Shell[/]", $"[yellow]{sysInfo.Shell}[/]")
               .AddRow("[magenta]󰳗 BIOS[/]", $"[magenta]{sysInfo.Bios}[/]")
               .AddRow("[cyan] Date/Time[/]", $"[cyan]{sysInfo.DateTime}[/]")
               .AddRow("[DarkOliveGreen2] Disk Space[/]", $"[DarkOliveGreen2]{sysInfo.DiskInfo}[/]");

        var gridNoIcons = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[red]OS [/]", $"{Sysinfo.GetOSName()}")
            .AddRow("[blue]OS Description[/]", $"{sysInfo.OsDescription}")
            .AddRow("[green]OS Version[/]", $"{sysInfo.OsVersion}")
            .AddRow("[yellow]Kernel[/]", $"{sysInfo.Kernel}")
            .AddRow("[yellow]CPU[/]", $"{sysInfo.CPU}")
            .AddRow("[magenta]Memory[/]", $"{sysInfo.MemoryInfo}")
            .AddRow("[cyan]GPU[/]", $"{sysInfo.GPU}")
            .AddRow("[red]Uptime[/]", $"{sysInfo.Uptime}")
            .AddRow("[blue]Window Manager[/]", $"{sysInfo.WindowManager}")
            .AddRow("[green]Terminal[/]", $"{sysInfo.Terminal}")
            .AddRow("[yellow]Shell[/]", $"{sysInfo.Shell}")
            .AddRow("[magenta]BIOS[/]", $"{sysInfo.Bios}")
            .AddRow("[cyan]Date/Time[/]", $"{sysInfo.DateTime}")
            .AddRow("[DarkOliveGreen2]Disk Space[/]", $"{sysInfo.DiskInfo}");

        AnsiConsole.Write(
            new Panel(hideIcons ? gridNoIcons : grid)
                .Header(hideIcons ? $"{sysInfo.User}@{sysInfo.Machine}" : $" {sysInfo.User}@{sysInfo.Machine}", Justify.Center)
                .RoundedBorder()
                .BorderColor(Color.Green));
    }

}