using EventNotifier.Core.Events;
using EventNotifier.Core.Serialization;
using Xunit;

namespace EventNotifier.Tests;

public sealed class EventSerializerTests
{
    [Fact]
    public void Serialize_IncludesSchemaAndEventType()
    {
        var envelope = new EventEnvelope
        {
            SchemaVersion = "1.0",
            EventType = EventType.PlayerJoin,
            PluginVersion = "0.1.0",
            Server = new ServerInfo { Name = "Test", WorldName = "World", ActivePlayers = 1, MaxSlots = 8, Version = "6.1.0" }
        };

        var json = EventSerializer.Serialize(envelope);

        Assert.Contains("\"schemaVersion\":\"1.0\"", json);
        Assert.Contains("\"eventType\":\"player.join\"", json);
    }
}

