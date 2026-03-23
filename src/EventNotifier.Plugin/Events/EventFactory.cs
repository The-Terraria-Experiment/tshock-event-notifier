using EventNotifier.Core.Configuration;
using EventNotifier.Core.Events;
using Terraria;
using TShockAPI;

namespace EventNotifier.Plugin.Events;

/// <summary>
/// Builds verbose, schema-versioned payloads from TShock runtime objects.
/// </summary>
public static class EventFactory
{
    /// <summary>
    /// Creates a base event envelope populated with current server and plugin context.
    /// </summary>
    public static EventEnvelope CreateBase(NotifierSettings settings, string eventType)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new EventEnvelope
        {
            SchemaVersion = settings.SchemaVersion,
            EventType = eventType,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            PluginVersion = typeof(EventFactory).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            Server = BuildServerInfo()
        };
    }

    /// <summary>
    /// Extracts player details into a verbose context object.
    /// </summary>
    /// <param name="player">Player object or null.</param>
    /// <param name="fallbackIndex">Index to use if player is null.</param>
    /// <returns>Player context or null if player is null and no fallback index.</returns>
    public static PlayerInfo? BuildPlayerInfo(TSPlayer? player, int fallbackIndex = -1)
    {
        if (player is null)
        {
            return fallbackIndex >= 0
                ? new PlayerInfo { Index = fallbackIndex, Name = "unknown" }
                : null;
        }

        return new PlayerInfo
        {
            Index = player.Index,
            Name = player.Name,
            AccountName = player.Account?.Name,
            GroupName = player.Group?.Name,
            IpAddress = player.IP,
            IsLoggedIn = player.IsLoggedIn
        };
    }

    private static ServerInfo BuildServerInfo()
    {
        return new ServerInfo
        {
            Name = TShock.Config.Settings.ServerName,
            WorldName = Main.worldName,
            ActivePlayers = TShock.Utils.GetActivePlayerCount(),
            MaxSlots = TShock.Config.Settings.MaxSlots,
            Version = TShock.VersionNum?.ToString() ?? "unknown"
        };
    }
}


