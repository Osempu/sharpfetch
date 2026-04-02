using Spectre.Console;
using Spectre.Console.Rendering;

namespace sharpfetch;

public class SharpFetchGraphics
{
    public static void printSysInfo(bool hideIcons, bool isMinimal)
    {
        var sysInfo = new Sysinfo();

        var grid = new Grid()
               .AddColumn(new GridColumn().NoWrap().PadRight(4))
               .AddColumn()
               .AddRow("[yellow] CPU[/]", $"[yellow]{sysInfo.CPU}[/]")
               .AddRow("[magenta] Memory[/]", $"[magenta]{sysInfo.MemoryInfo}[/]")
               .AddRow("[cyan] GPU[/]", $"[cyan]{sysInfo.GPU}[/]")
               .AddRow("[red] Uptime[/]", $"[red]{sysInfo.Uptime}[/]")
               .AddRow("[blue] Window Manager[/]", $"[blue]{sysInfo.WindowManager}[/]")
               .AddRow("[green] Terminal[/]", $"[green]{sysInfo.Terminal}[/]")
               .AddRow("[yellow] Shell[/]", $"[yellow]{sysInfo.Shell}[/]")
               .AddRow("[magenta] BIOS[/]", $"[magenta]{sysInfo.Bios}[/]")
               .AddRow("[cyan] Date/Time[/]", $"[cyan]{sysInfo.DateTime}[/]")
               .AddRow("[DarkOliveGreen2] Disk Space[/]", $"[DarkOliveGreen2]{sysInfo.DiskInfo}[/]");

        var systemGrid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[red] OS[/]", $"[red]{Sysinfo.GetOSName()}[/]")
            .AddRow("[blue] OS Description[/]", $"[blue]{sysInfo.OsDescription}[/]")
            .AddRow("[yellow] Kernel[/]", $"[yellow]{sysInfo.Kernel}[/]");

        var hardwareGrid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[yellow] CPU[/]", $"[yellow]{sysInfo.CPU}[/]")
            .AddRow("[magenta] Memory[/]", $"[magenta]{sysInfo.MemoryInfo}[/]")
            .AddRow("[cyan] GPU[/]", $"[cyan]{sysInfo.GPU}[/]")
            .AddRow("[magenta] BIOS[/]", $"[magenta]{sysInfo.Bios}[/]");

        // Create memory chart
        var memoryChart = new BreakdownChart()
            .FullSize()
            .Width(60)
            .ShowPercentage()
            .AddItem("Used", Helpers.ToGBDouble((ulong)sysInfo.MemoryInfo.UsedPhysicalMemoryBytes), Color.Red)
            .AddItem("Free", Helpers.ToGBDouble((ulong)sysInfo.MemoryInfo.AvailablePhysicalMemoryBytes), Color.Green);

        var gridNoIcons = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[red]OS [/]", $"{Sysinfo.GetOSName()}")
            .AddRow("[blue]OS Description[/]", $"{sysInfo.OsDescription}")
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

        // Display System Info panel
        AnsiConsole.Write(
            new Panel(hideIcons ? grid : systemGrid)
                .Header("System Info", Justify.Center)
                .RoundedBorder()
                .BorderColor(Color.Green));

        // Display Hardware Info panel
        AnsiConsole.Write(
            new Panel(hideIcons ? grid : hardwareGrid)
                .Header("Hardware Info", Justify.Center)
                .RoundedBorder()
                .BorderColor(Color.Aqua));

        // Display Memory Chart panel
        Render("Memory Usage", memoryChart);

        if (!isMinimal)
        {
            var (free, total) = sysInfo.GetDiskChart();

            // Create disk chart
            var diskChart = new BreakdownChart()
                .FullSize()
                .Width(60)
                .ShowPercentage()
                .AddItem("Used", total - free, Color.Red)
                .AddItem("Free", free, Color.Green);

            Render("Disk Space", diskChart);

            var environmentGrid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn()
                .AddRow("[red] Window Manager[/]", $"[red]{sysInfo.WindowManager}[/]")
                .AddRow("[green] Terminal[/]", $"[green]{sysInfo.Terminal}[/]")
                .AddRow("[yellow] Shell[/]", $"[yellow]{sysInfo.Shell}[/]");

            var status = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn()
                .AddRow("[cyan] Date/Time[/]", $"[cyan]{sysInfo.DateTime}[/]")
                .AddRow("[red] Uptime[/]", $"[red]{sysInfo.Uptime}[/]");

            AnsiConsole.Write(
            new Panel(hideIcons ? grid : environmentGrid)
                .Header("Environment Info", Justify.Center)
                .RoundedBorder()
                .BorderColor(Color.Blue));

            AnsiConsole.Write(
                new Panel(hideIcons ? grid : status)
                    .Header("Status Info", Justify.Center)
                    .RoundedBorder()
                    .BorderColor(Color.Pink1));
        }
    }

    private static void Render(string title, IRenderable chart)
    {
        AnsiConsole.Write(
            new Panel(chart)
                .Padding(1, 1)
                .Header(title));
    }

}
