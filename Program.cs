using System.CommandLine;
using System.Runtime.InteropServices;
using Spectre.Console;

var rootCommand = new RootCommand("sharpfetch");

var iconOption = new Option<bool>("--no-icons")
{
    Description = "Whether to display icons or not",
    Arity = ArgumentArity.Zero
};

rootCommand.Options.Add(iconOption);

string user = Environment.UserName;
string host = Environment.MachineName;

string osDescription = RuntimeInformation.OSDescription;
string osVersion = Environment.OSVersion.Version.ToString();

var panel = new Panel(
        new Text($" OS: {GetOsName()}\n OsDescription: {osDescription}\n Os Version: {osVersion}")
    )
.Header($" {user}@{host}", Justify.Center)
.RoundedBorder()
.BorderColor(Color.Green)
.Padding(2, 1);

var panelNoIcons = new Panel(
        new Text($"OS: {GetOsName()}\nOsDescription: {osDescription}\nOs Version: {osVersion}")
    )
.Header($"{user}@{host}", Justify.Center)
.RoundedBorder()
.BorderColor(Color.Green)
.Padding(1, 1);

var newPanel = new Panel("Hello This is a panel")
    .Header("Panel Header", Justify.Center);

rootCommand.SetAction(parseResult =>
{
    bool hideIcons = parseResult.GetValue(iconOption);
    if (hideIcons)
    {
        AnsiConsole.Write(panelNoIcons);
    }
    else
    {
        AnsiConsole.Write(panel);
    }

    return 0;
});

return rootCommand.Parse(args).Invoke();

static string GetOsName()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        return "Windows";
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        return "Linux";
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        return "Mac OS";
    }
    else
    {
        return "Unknown OS";
    }
}