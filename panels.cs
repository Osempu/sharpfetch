using System.Runtime.CompilerServices;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace sharpfetch;

public static class ConsoleOutput
{
    private static Sysinfo _sysInfo;

    private static void BuildSysInfo()
    {

    }

    public static void PrintAsPanels(Sysinfo sysInfo)
    {
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

        // Display System Info panel
        AnsiConsole.Write(
            new Panel(systemGrid)
                .Header("System Info", Justify.Center)
                .RoundedBorder()
                .BorderColor(Color.Green));

        // Display Hardware Info panel
        AnsiConsole.Write(
            new Panel(hardwareGrid)
                .Header("Hardware Info", Justify.Center)
                .RoundedBorder()
                .BorderColor(Color.Aqua));

        // Create memory chart
        var memoryChart = new BreakdownChart()
            .FullSize()
            .Width(60)
            .ShowPercentage()
            .AddItem("Used", Helpers.ToGBDouble((ulong)sysInfo.MemoryInfo.UsedPhysicalMemoryBytes), Color.Red)
            .AddItem("Free", Helpers.ToGBDouble((ulong)sysInfo.MemoryInfo.AvailablePhysicalMemoryBytes), Color.Green);

        // Display Memory Chart panel
        Render("Memory Usage", memoryChart);

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
        new Panel(environmentGrid)
            .Header("Environment Info", Justify.Center)
            .RoundedBorder()
            .BorderColor(Color.Blue));

        AnsiConsole.Write(
            new Panel(status)
                .Header("Status Info", Justify.Center)
                .RoundedBorder()
                .BorderColor(Color.Pink1));
    }

    public static void PrintAsTrees(Sysinfo sysInfo)
    {
        // var systemGrid = new Grid()
        //     .AddColumn(new GridColumn().NoWrap().PadRight(4))
        //     .AddColumn()
        //     .AddRow("[red] OS[/]", $"[red]{Sysinfo.GetOSName()}[/]")
        //     .AddRow("[blue] OS Description[/]", $"[blue]{sysInfo.OsDescription}[/]")
        //     .AddRow("[yellow] Kernel[/]", $"[yellow]{sysInfo.Kernel}[/]");

        // var hardwareGrid = new Grid()
        //     .AddColumn(new GridColumn().NoWrap().PadRight(4))
        //     .AddColumn()
        //     .AddRow("[yellow] CPU[/]", $"[yellow]{sysInfo.CPU}[/]")
        //     .AddRow("[magenta] Memory[/]", $"[magenta]{sysInfo.MemoryInfo}[/]")
        //     .AddRow("[cyan] GPU[/]", $"[cyan]{sysInfo.GPU}[/]")
        //     .AddRow("[magenta] BIOS[/]", $"[magenta]{sysInfo.Bios}[/]");

        // var environmentGrid = new Grid()
        //     .AddColumn(new GridColumn().NoWrap().PadRight(4))
        //     .AddColumn()
        //     .AddRow("[red] Window Manager[/]", $"[red]{sysInfo.WindowManager}[/]")
        //     .AddRow("[green] Terminal[/]", $"[green]{sysInfo.Terminal}[/]")
        //     .AddRow("[yellow] Shell[/]", $"[yellow]{sysInfo.Shell}[/]");

        // var status = new Grid()
        //     .AddColumn(new GridColumn().NoWrap().PadRight(4))
        //     .AddColumn()
        //     .AddRow("[cyan] Date/Time[/]", $"[cyan]{sysInfo.DateTime}[/]")
        //     .AddRow("[red] Uptime[/]", $"[red]{sysInfo.Uptime}[/]");

        // Hardware Info Tree
        var cpuRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[yellow] CPU[/]", $"[yellow]{sysInfo.CPU}[/]");

        var memRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[magenta] Memory[/]", $"[magenta]{sysInfo.MemoryInfo}[/]");

        var gpuRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[cyan] GPU[/]", $"[cyan]{sysInfo.GPU}[/]");

        var biosRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[magenta] BIOS[/]", $"[magenta]{sysInfo.Bios}[/]");

        var hardwareTree = new Tree("[green]System Info[/]")
            .Style(Style.Parse("green bold"));
        hardwareTree.AddNode(cpuRow);
        hardwareTree.AddNode(memRow);
        hardwareTree.AddNode(gpuRow);
        hardwareTree.AddNode(biosRow);

        // System Info Tree
        var userRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddRow($"[cyan]{sysInfo.User}@{sysInfo.Machine}[/]");

        var osRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[yellow] OS[/]", $"[yellow]{Sysinfo.GetOSName()}[/]");

        var kernelRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[magenta] Kernel[/]", $"[magenta]{sysInfo.Kernel}[/]");


        var tree = new Tree("[red]System Info[/]")
            .Style(Style.Parse("red bold"));

        tree.AddNode(userRow);
        tree.AddNode(osRow);
        tree.AddNode(kernelRow);

        // Environment Info Tree
        var windowRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[yellow] Window Manager[/]", $"[yellow]{sysInfo.WindowManager}[/]");

        var terminalRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[yellow] Terminal[/]", $"[yellow]{sysInfo.Terminal}[/]");

        var shellRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[magenta] Shell[/]", $"[magenta]{sysInfo.Shell}[/]");


        var environmentTree = new Tree("[yellow]Environment Info[/]")
            .Style(Style.Parse("yellow bold"));

        environmentTree.AddNode(windowRow);
        environmentTree.AddNode(terminalRow);
        environmentTree.AddNode(shellRow);

        // Status Info Tree
        var dateTimeRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[yellow] Date/Time[/]", $"[yellow]{sysInfo.DateTime}[/]");

        var uptimeRow = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[yellow] Uptime[/]", $"[yellow]{sysInfo.Uptime}[/]");

        var statusTree = new Tree("[blue]Status Info[/]")
            .Style(Style.Parse("blue bold"));

        statusTree.AddNode(dateTimeRow);
        statusTree.AddNode(uptimeRow);

        AnsiConsole.Write(tree);
        AnsiConsole.Write(hardwareTree);
        AnsiConsole.Write(environmentTree);
        AnsiConsole.Write(statusTree);
    }

    public static void PrintMinmal(Sysinfo sysInfo)
    {
        var minimalGrid = new Grid()
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

        AnsiConsole.Write(new Panel(minimalGrid)
            .Header($"{sysInfo.User}@{sysInfo.Machine}", Justify.Left)
            .BorderStyle(Style.Parse("green bold"))
            .BorderColor(Color.Green));
    }

    public static void PrintAsLeftPanel(Sysinfo sysInfo)
    {
        var leftPanel = new Grid()
            .AddColumn()
            .AddRow("[yellow] CPU[/]")
            .AddRow("[magenta] Memory[/]")
            .AddRow("[cyan] GPU[/]")
            .AddRow("[red] Uptime[/]")
            .AddRow("[blue] Window Manager[/]")
            .AddRow("[green] Terminal[/]")
            .AddRow("[yellow] Shell[/]")
            .AddRow("[magenta] BIOS[/]")
            .AddRow("[cyan] Date/Time[/]")
            .AddRow("[DarkOliveGreen2] Disk Space[/]");

        var rightPanel = new Grid()
            .AddColumn()
            .AddRow()
            .AddRow($"[yellow]{sysInfo.CPU}[/]")
            .AddRow($"[magenta]{sysInfo.MemoryInfo}[/]")
            .AddRow($"[cyan]{sysInfo.GPU}[/]")
            .AddRow($"[red]{sysInfo.Uptime}[/]")
            .AddRow($"[blue]{sysInfo.WindowManager}[/]")
            .AddRow($"[green]{sysInfo.Terminal}[/]")
            .AddRow($"[yellow]{sysInfo.Shell}[/]")
            .AddRow($"[magenta]{sysInfo.Bios}[/]")
            .AddRow($"[cyan]{sysInfo.DateTime}[/]")
            .AddRow($"[DarkOliveGreen2]{sysInfo.DiskInfo}[/]");

        var parentGrid = new Grid()
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn().PadRight(2))
            .AddRow(new Panel(leftPanel)
                .BorderStyle(Style.Parse("green bold"))
                .RoundedBorder()
                .Padding(0, 0, 1, 0), rightPanel);

        AnsiConsole.Write(parentGrid);
    }

    private static void Render(string title, IRenderable chart)
    {
        AnsiConsole.Write(
            new Panel(chart)
                .Padding(1, 1)
                .Header(title));
    }
}