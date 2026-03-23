namespace EventNotifier.Core.Transport;

/// <summary>
/// Final dispatch outcome for a single payload.
/// </summary>
public sealed class NotificationDispatchResult
{
    /// <summary>
    /// True when the payload was accepted by the endpoint.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Number of HTTP attempts made.
    /// </summary>
    public int Attempts { get; init; }

    /// <summary>
    /// Final HTTP status code if one was received.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Final failure reason when dispatch was not successful.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
