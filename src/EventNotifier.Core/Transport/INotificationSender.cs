using EventNotifier.Core.Events;

namespace EventNotifier.Core.Transport;

/// <summary>
/// Sends a single event envelope to the configured endpoint.
/// </summary>
public interface INotificationSender
{
    /// <summary>
    /// Sends one envelope and returns the final outcome.
    /// </summary>
    Task<NotificationDispatchResult> SendAsync(EventEnvelope envelope, CancellationToken cancellationToken);
}
