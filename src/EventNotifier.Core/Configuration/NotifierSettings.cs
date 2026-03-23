namespace EventNotifier.Core.Configuration;

/// <summary>
/// Plugin configuration for outgoing event notifications.
/// </summary>
public sealed class NotifierSettings
{
    /// <summary>
    /// Version identifier written into every payload to support schema evolution.
    /// </summary>
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>
    /// Absolute HTTP(S) endpoint that receives all events.
    /// </summary>
    public string EndpointUrl { get; set; } = "https://example.execute-api.us-east-1.amazonaws.com/prod/events";

    /// <summary>
    /// API key value sent to API Gateway.
    /// </summary>
    public string ApiKey { get; set; } = "replace-me";

    /// <summary>
    /// Header name that carries the API key. API Gateway default is x-api-key.
    /// </summary>
    public string ApiKeyHeaderName { get; set; } = "x-api-key";

    /// <summary>
    /// Request timeout for an individual attempt.
    /// </summary>
    public int RequestTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Number of retries after the first attempt. User requested exactly one retry.
    /// </summary>
    public int RetryCount { get; set; } = 1;

    /// <summary>
    /// Delay between retry attempts.
    /// </summary>
    public int RetryDelayMs { get; set; } = 400;

    /// <summary>
    /// Maximum number of events buffered in-memory before dropping the newest event.
    /// </summary>
    public int QueueCapacity { get; set; } = 512;

    /// <summary>
    /// Per-event hook toggles.
    /// </summary>
    public EventHookSettings Events { get; set; } = new();
}

