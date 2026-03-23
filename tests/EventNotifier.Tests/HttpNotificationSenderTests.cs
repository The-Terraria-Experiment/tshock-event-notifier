using System.Net;
using System.Net.Http;
using EventNotifier.Core.Configuration;
using EventNotifier.Core.Events;
using EventNotifier.Core.Transport;
using Xunit;

namespace EventNotifier.Tests;

public sealed class HttpNotificationSenderTests
{
    [Fact]
    public async Task SendAsync_RetriesOnce_AndAddsApiKeyHeader()
    {
        var handler = new SequenceHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError),
            new HttpResponseMessage(HttpStatusCode.OK));

        using var client = new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        var settings = new NotifierSettings
        {
            EndpointUrl = "https://example.test/events",
            ApiKey = "secret-key",
            ApiKeyHeaderName = "x-api-key",
            RetryCount = 1,
            RetryDelayMs = 1,
            RequestTimeoutMs = 1000
        };

        var sender = new HttpNotificationSender(client, settings);
        var envelope = new EventEnvelope
        {
            SchemaVersion = "1.0",
            EventType = EventType.PlayerJoin,
            Server = new ServerInfo { Name = "Test", WorldName = "World", ActivePlayers = 0, MaxSlots = 8, Version = "6.1.0" }
        };

        var result = await sender.SendAsync(envelope, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.Attempts);
        Assert.Equal(2, handler.Requests.Count);
        Assert.True(handler.Requests[0].Headers.Contains("x-api-key"));
    }

    private sealed class SequenceHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;

        public SequenceHttpMessageHandler(params HttpResponseMessage[] responses)
        {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var response = _responses.Count > 0 ? _responses.Dequeue() : new HttpResponseMessage(HttpStatusCode.OK);
            return Task.FromResult(response);
        }
    }
}

