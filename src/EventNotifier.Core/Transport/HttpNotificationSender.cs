using System.Net.Http.Headers;
using System.Text;
using EventNotifier.Core.Configuration;
using EventNotifier.Core.Events;
using EventNotifier.Core.Serialization;

namespace EventNotifier.Core.Transport;

/// <summary>
/// HTTP sender with a fixed retry policy intended for API Gateway event ingestion.
/// </summary>
public sealed class HttpNotificationSender : INotificationSender
{
    private readonly HttpClient _httpClient;
    private readonly NotifierSettings _settings;

    /// <summary>
    /// Creates an HTTP sender bound to current notifier settings.
    /// </summary>
    public HttpNotificationSender(HttpClient httpClient, NotifierSettings settings)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        if (_httpClient.Timeout == Timeout.InfiniteTimeSpan)
        {
            _httpClient.Timeout = TimeSpan.FromMilliseconds(Math.Max(500, _settings.RequestTimeoutMs));
        }
    }

    /// <summary>
    /// Sends an envelope with one retry after failure.
    /// </summary>
    public async Task<NotificationDispatchResult> SendAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var maxAttempts = Math.Max(1, _settings.RetryCount + 1);
        var lastError = string.Empty;
        var lastStatusCode = default(int?);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var request = BuildRequest(envelope);
                using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return new NotificationDispatchResult
                    {
                        Success = true,
                        Attempts = attempt,
                        StatusCode = (int)response.StatusCode
                    };
                }

                lastStatusCode = (int)response.StatusCode;
                lastError = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                lastError = "Request timed out.";
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(Math.Max(0, _settings.RetryDelayMs), cancellationToken).ConfigureAwait(false);
            }
        }

        return new NotificationDispatchResult
        {
            Success = false,
            Attempts = maxAttempts,
            StatusCode = lastStatusCode,
            ErrorMessage = lastError
        };
    }

    private HttpRequestMessage BuildRequest(EventEnvelope envelope)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _settings.EndpointUrl)
        {
            Content = new StringContent(EventSerializer.Serialize(envelope), Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            request.Headers.Remove(_settings.ApiKeyHeaderName);
            request.Headers.Add(_settings.ApiKeyHeaderName, _settings.ApiKey);
        }

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }
}
