using EventNotifier.Core.Configuration;
using TShockAPI;
using TShockAPI.Configuration;

namespace EventNotifier.Plugin.Configuration;

/// <summary>
/// Reads and writes plugin configuration at tshock/event-notifier.json.
/// </summary>
public sealed class PluginConfigStore
{
    private readonly ConfigFile<NotifierSettings> _config = new();

    /// <summary>
    /// Path where configuration is persisted.
    /// </summary>
    public string ConfigPath => Path.Combine(TShock.SavePath, "event-notifier.json");

    /// <summary>
    /// Loads configuration from disk or creates defaults if missing.
    /// </summary>
    public NotifierSettings LoadOrCreateDefault()
    {
        var settings = _config.Read(ConfigPath, out var incompleteSettings) ?? new NotifierSettings();

        // Write defaults when missing and rewrite on schema expansion to keep files current.
        if (incompleteSettings)
        {
            _config.Settings = settings;
            _config.Write(ConfigPath);
        }

        return settings;
    }

    /// <summary>
    /// Persists updated settings to the config file.
    /// </summary>
    public void Save(NotifierSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _config.Settings = settings;
        _config.Write(ConfigPath);
    }
}
