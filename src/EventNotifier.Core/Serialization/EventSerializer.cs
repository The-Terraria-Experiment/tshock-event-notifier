using System.Text.Json;
using System.Text.Json.Serialization;
using EventNotifier.Core.Events;

namespace EventNotifier.Core.Serialization;

/// <summary>
/// Handles consistent JSON serialization for notifier payloads.
/// </summary>
public static class EventSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an event envelope to JSON.
    /// </summary>
    /// <param name="envelope">Envelope to serialize.</param>
    /// <returns>Compact JSON payload.</returns>
    public static string Serialize(EventEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        return JsonSerializer.Serialize(envelope, JsonOptions);
    }
}
