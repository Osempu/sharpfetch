using System.CommandLine;
using sharpfetch;

var rootCommand = new RootCommand("sharpfetch");

var iconOption = new Option<bool>("--no-icons")
{
    Description = "Whether to display icons or not",
    Arity = ArgumentArity.Zero
};

var isMinimalOption = new Option<bool>("--minimal")
{
    Description = "Whether to display a minimal output or not",
    Arity = ArgumentArity.Zero
};

rootCommand.Options.Add(iconOption);
rootCommand.Options.Add(isMinimalOption);

rootCommand.SetAction(parseResult =>
{
    bool hideIcons = parseResult.GetValue(iconOption);
    bool isMinimal = parseResult.GetValue(isMinimalOption);

    SharpFetchGraphics.printSysInfo(hideIcons, isMinimal);

    return 0;
});

return rootCommand.Parse(args).Invoke();

