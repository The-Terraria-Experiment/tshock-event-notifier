using System.Threading.Channels;
using EventNotifier.Core.Events;

namespace EventNotifier.Core.Transport;

/// <summary>
/// In-memory queue and worker that avoids blocking hook execution paths.
/// </summary>
public sealed class NotificationDispatchQueue : IAsyncDisposable
{
    private readonly Channel<EventEnvelope> _channel;
    private readonly INotificationSender _sender;
    private readonly Action<string> _logInfo;
    private readonly Action<string> _logWarn;
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Task _workerTask;

    /// <summary>
    /// Creates a bounded queue and starts the worker task.
    /// </summary>
    public NotificationDispatchQueue(
        INotificationSender sender,
        int capacity,
        Action<string> logInfo,
        Action<string> logWarn)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logInfo = logInfo ?? throw new ArgumentNullException(nameof(logInfo));
        _logWarn = logWarn ?? throw new ArgumentNullException(nameof(logWarn));

        _channel = Channel.CreateBounded<EventEnvelope>(new BoundedChannelOptions(Math.Max(32, capacity))
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        });

        _workerTask = Task.Run(ProcessAsync);
    }

    /// <summary>
    /// Number of payloads dropped due to queue saturation.
    /// </summary>
    public int DroppedCount { get; private set; }

    /// <summary>
    /// Number of payloads successfully delivered.
    /// </summary>
    public int SuccessCount { get; private set; }

    /// <summary>
    /// Number of payloads that still failed after retries.
    /// </summary>
    public int FailureCount { get; private set; }

    /// <summary>
    /// Attempts to enqueue a payload without blocking.
    /// </summary>
    public bool TryEnqueue(EventEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (_channel.Writer.TryWrite(envelope))
        {
            return true;
        }

        DroppedCount++;
        _logWarn($"Event queue full; dropped event '{envelope.EventType}'.");
        return false;
    }

    private async Task ProcessAsync()
    {
        try
        {
            await foreach (var envelope in _channel.Reader.ReadAllAsync(_shutdown.Token).ConfigureAwait(false))
            {
                var result = await _sender.SendAsync(envelope, _shutdown.Token).ConfigureAwait(false);
                if (result.Success)
                {
                    SuccessCount++;
                }
                else
                {
                    FailureCount++;
                    _logWarn($"Failed to dispatch '{envelope.EventType}' after {result.Attempts} attempts: {result.ErrorMessage}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logInfo("Event dispatch worker stopped.");
        }
    }

    /// <summary>
    /// Stops the worker and releases queue resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();
        _shutdown.Cancel();

        try
        {
            await _workerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _shutdown.Dispose();
        }
    }
}
