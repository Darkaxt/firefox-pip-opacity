namespace PipOpacity.Tray;

using System.Text.Json;
using PipOpacity.Core;

internal sealed class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string path;
    private readonly FileLogger logger;

    public ConfigStore(string path, FileLogger logger)
    {
        this.path = path;
        this.logger = logger;
    }

    public PipOpacityConfig Load()
    {
        try
        {
            if (!File.Exists(path))
            {
                return PipOpacityConfig.CreateDefault();
            }

            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<PipOpacityConfig>(json, JsonOptions);
            return (config ?? PipOpacityConfig.CreateDefault()).Normalize();
        }
        catch (Exception exception)
        {
            logger.Error("Failed to load config; using defaults.", exception);
            return PipOpacityConfig.CreateDefault();
        }
    }

    public void Save(PipOpacityConfig config)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(config.Normalize(), JsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception exception)
        {
            logger.Error("Failed to save config.", exception);
        }
    }
}
