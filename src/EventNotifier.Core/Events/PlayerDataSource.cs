namespace EventNotifier.Core.Events;

/// <summary>
/// Indicates how the <see cref="EventEnvelope.Player"/> data on a payload was obtained.
/// </summary>
public static class PlayerDataSource
{
    /// <summary>
    /// Player fields were read from a live player object at event time.
    /// </summary>
    public const string Live = "live";

    /// <summary>
    /// The live player object was gone; fields came from the last-known
    /// per-connection snapshot and may be a moment stale.
    /// </summary>
    public const string Cached = "cached";

    /// <summary>
    /// No player data was ever observed for this connection (e.g. a
    /// pre-join disconnect).
    /// </summary>
    public const string Unknown = "unknown";
}
