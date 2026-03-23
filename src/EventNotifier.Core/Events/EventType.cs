namespace EventNotifier.Core.Events;

/// <summary>
/// Supported notifier event types.
/// </summary>
public static class EventType
{
    /// <summary>
    /// Player completed connection and joined the server.
    /// </summary>
    public const string PlayerJoin = "player.join";

    /// <summary>
    /// Player disconnected from the server.
    /// </summary>
    public const string PlayerLeave = "player.leave";

    /// <summary>
    /// Player chat message observed by TShock chat hooks.
    /// </summary>
    public const string PlayerChat = "player.chat";

    /// <summary>
    /// Player death event sourced from KillMe packets.
    /// </summary>
    public const string PlayerDeath = "player.death";

    /// <summary>
    /// Player spawn or respawn event.
    /// </summary>
    public const string PlayerSpawn = "player.spawn";

    /// <summary>
    /// World save cycle event.
    /// </summary>
    public const string WorldSave = "world.save";

    /// <summary>
    /// TShock reload command event.
    /// </summary>
    public const string ServerReload = "server.reload";
}
