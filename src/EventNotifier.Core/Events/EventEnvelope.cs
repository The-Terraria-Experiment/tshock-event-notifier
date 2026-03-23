namespace EventNotifier.Core.Events;

/// <summary>
/// Root payload sent to the configured HTTP endpoint.
/// </summary>
public sealed class EventEnvelope
{
    /// <summary>
    /// Version identifier for payload contract compatibility.
    /// </summary>
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>
    /// Stable event type identifier.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp representing when this event occurred.
    /// </summary>
    public DateTimeOffset OccurredAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Correlation identifier used for end-to-end tracing.
    /// </summary>
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Plugin assembly version that produced this payload.
    /// </summary>
    public string PluginVersion { get; set; } = string.Empty;

    /// <summary>
    /// Server metadata snapshot captured at event time.
    /// </summary>
    public ServerInfo Server { get; set; } = new();

    /// <summary>
    /// Player details for player-scoped events.
    /// </summary>
    public PlayerInfo? Player { get; set; }

    /// <summary>
    /// Event-specific details.
    /// </summary>
    public Dictionary<string, object?> EventData { get; set; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Verbose server context attached to each message.
/// </summary>
public sealed class ServerInfo
{
    /// <summary>
    /// Display name of the Terraria server.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current world name.
    /// </summary>
    public string WorldName { get; set; } = string.Empty;

    /// <summary>
    /// Current active player count.
    /// </summary>
    public int ActivePlayers { get; set; }

    /// <summary>
    /// Configured maximum player slots.
    /// </summary>
    public int MaxSlots { get; set; }

    /// <summary>
    /// TShock version string.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Verbose player context when the event is associated with a player.
/// </summary>
public sealed class PlayerInfo
{
    /// <summary>
    /// In-game player index.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// In-game character name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// TShock account name when authenticated.
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// TShock group name.
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// Player IP address.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// True when player authentication completed.
    /// </summary>
    public bool IsLoggedIn { get; set; }
}
