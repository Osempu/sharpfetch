using System.Runtime.Serialization;
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
        var builder = new ConfigurationBuilder();

        var userConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            UserConfigFileName);

        if (File.Exists(userConfigPath))
        {
            builder.AddJsonFile(userConfigPath, optional: true);
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
                Modules = new List<string>(), // Empty = all default modules
                ParallelExecution = true,
                TimeoutMs = 5000,
                ShowExecutionTime = false
            },
            Display = new DisplayConfiguration
            {
                Format = "panels",
                ShowIcons = true,
                ColorScheme = "default",
                ShowCharts = true
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(defaultConfig, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }
}