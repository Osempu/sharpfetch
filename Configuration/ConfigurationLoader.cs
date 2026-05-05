using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace sharpfetch.Configuration;

public class ConfigurationLoader
{
    private const string DefaultConfigFilename = "config.json";
    private const string UserConfigFileName = ".sharpfetch.json";

    public static SharpFetchConfiguration Load(
        string[]? args = null,
        string? customConfigPath = null)
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory());

        var userConfigPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            DefaultConfigFilename);

        if (File.Exists(userConfigPath) && string.IsNullOrEmpty(customConfigPath))
        {
            builder.AddJsonFile(userConfigPath, optional: true, reloadOnChange: true);
        }

        if (!string.IsNullOrEmpty(customConfigPath) && File.Exists(customConfigPath))
        {
            builder.AddJsonFile(customConfigPath, optional: false);
        }

        if (args != null && args.Length > 0)
        {
            builder.AddCommandLine(args);
        }

        var configuration = builder.Build();

        var config = new SharpFetchConfiguration();
        configuration.Bind(config);

        return config;
    }

    public static void CreateDefaultConfigFile(string path)
    {
        var defaultConfig = new SharpFetchConfiguration
        {
            Modules = new ModuleConfiguration
            {
                Modules = [],   // empty = all default-enabled modules
                ParallelExecution = true,
                TimeoutMs = 5000,
                ShowExecutionTime = false,
                Groups =
                [
                    new GroupConfiguration { Id = "system",      DisplayName = "System Info",      Color = "green"   },
                    new GroupConfiguration { Id = "hardware",    DisplayName = "Hardware Info",    Color = "cyan"    },
                    new GroupConfiguration { Id = "environment", DisplayName = "Environment Info", Color = "blue"    },
                    new GroupConfiguration { Id = "status",      DisplayName = "Status Info",      Color = "magenta" },
                ]
            },
            Display = new DisplayConfiguration
            {
                Format = "panels",
                ShowIcons = true,
                IconStyle = IconStyle.Emoji,
                ColorScheme = "default",
                ShowCharts = true,
                GroupModules = true
            }
        };

        var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        });

        File.WriteAllText(path, json);
    }

    public static void CreateDefaultConfigFile(SharpFetchConfiguration config)
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory());

        var userConfigPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            DefaultConfigFilename);

        var newConfig = config;

        if (newConfig.Display.GroupModules is true)
        {
            newConfig.Modules.Groups = new List<GroupConfiguration>
            {
                new GroupConfiguration { Id = "system",      DisplayName = "System Info",      Color = "green"   },
                new GroupConfiguration { Id = "hardware",    DisplayName = "Hardware Info",    Color = "cyan"    },
                new GroupConfiguration { Id = "environment", DisplayName = "Environment Info", Color = "blue"    },
                new GroupConfiguration { Id = "status",      DisplayName = "Status Info",      Color = "magenta" },
            };
        }

        var json = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        });

        File.WriteAllText(userConfigPath, json);
    }
}