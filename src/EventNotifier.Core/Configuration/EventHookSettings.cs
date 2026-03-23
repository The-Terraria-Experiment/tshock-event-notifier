namespace EventNotifier.Core.Configuration;

/// <summary>
/// Allows each supported event hook to be enabled or disabled independently.
/// </summary>
public sealed class EventHookSettings
{
    /// <summary>
    /// Sends player join events.
    /// </summary>
    public bool Join { get; set; } = true;

    /// <summary>
    /// Sends player leave events.
    /// </summary>
    public bool Leave { get; set; } = true;

    /// <summary>
    /// Sends chat message events.
    /// </summary>
    public bool Chat { get; set; } = true;

    /// <summary>
    /// Sends death events.
    /// </summary>
    public bool Death { get; set; } = true;

    /// <summary>
    /// Sends spawn and respawn events.
    /// </summary>
    public bool Spawn { get; set; } = true;

    /// <summary>
    /// Sends world save events.
    /// </summary>
    public bool WorldSave { get; set; } = true;

    /// <summary>
    /// Sends TShock reload events.
    /// </summary>
    public bool Reload { get; set; } = true;
}
