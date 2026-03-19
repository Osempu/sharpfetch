using System.CommandLine;
using sharpfetch;

var rootCommand = new RootCommand("sharpfetch");

var iconOption = new Option<bool>("--no-icons")
{
    Description = "Whether to display icons or not",
    Arity = ArgumentArity.Zero
};

rootCommand.Options.Add(iconOption);

rootCommand.SetAction(parseResult =>
{
    bool hideIcons = parseResult.GetValue(iconOption);
    SharpFetchGraphics.printSysInfo(hideIcons);

    return 0;
});

return rootCommand.Parse(args).Invoke();

