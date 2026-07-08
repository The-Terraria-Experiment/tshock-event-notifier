using System.Collections.Concurrent;
using EventNotifier.Core.Events;

namespace EventNotifier.Plugin.Events;

/// <summary>
/// Tracks the most recently observed player info per slot index so events that
/// fire after TShock has already cleared the live player object (e.g. leave)
/// can fall back to a recent snapshot instead of an empty placeholder.
/// </summary>
public sealed class PlayerContextCache
{
    private readonly ConcurrentDictionary<int, PlayerInfo> _lastKnown = new();

    /// <summary>
    /// Records the most recently observed live player info for a slot index.
    /// </summary>
    public void Remember(int index, PlayerInfo info)
    {
        _lastKnown[index] = info;
    }

    /// <summary>
    /// Retrieves the last known player info for a slot index, if any.
    /// </summary>
    public bool TryGet(int index, out PlayerInfo? info)
    {
        return _lastKnown.TryGetValue(index, out info);
    }

    /// <summary>
    /// Clears the cached entry for a slot index, e.g. once a player has fully left.
    /// </summary>
    public void Forget(int index)
    {
        _lastKnown.TryRemove(index, out _);
    }
}
