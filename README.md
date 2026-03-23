# EventNotifier (TShock 6)

EventNotifier is a platform-agnostic TShock 6 plugin scaffold that emits important server events as HTTP POST requests to a single endpoint (for example, AWS API Gateway).

## What this scaffold includes

- **Schema-versioned payload envelope** (`schemaVersion`) for API contract evolution
- **API key authentication** (default header: `x-api-key`)
- **Seven event hooks** covering player lifecycle and server operations:
  - `player.join` – player connects and completes join sequence
  - `player.leave` – player disconnects
  - `player.chat` – chat message received
  - `player.death` – player death via damage
  - `player.spawn` / `player.respawn` – player spawns or respawns
  - `world.save` – world saved to disk
  - `server.reload` – TShock reload command issued
- **Per-hook enable/disable toggles** for selective event emission
- **Retry policy**: one automatic retry after initial failure
- **Bounded in-memory queue** prevents blocking TShock game loop
- **Comprehensive XML documentation** across all public APIs

## Requirements

- **.NET SDK 9.0+** – required to build (TShock 6.1.0 targets net9.0)
- **TShock 6.1.0+** – latest TShock API compatibility

## Project structure

- `src/EventNotifier.Core/` – platform-agnostic transport, serialization, and models
  - `Configuration/` – `NotifierSettings`, `EventHookSettings`
  - `Events/` – `EventEnvelope`, `EventType`, payload schema
  - `Serialization/` – JSON schema serialization
  - `Transport/` – HTTP sender, dispatch queue, retry logic
- `src/EventNotifier.Plugin/` – TShock 6 plugin integration
  - `Commands/` – `/eventnotifier` admin command handler
  - `Configuration/` – config file I/O via `ConfigFile<T>`
  - `Events/` – `EventFactory` for payload construction
  - `Hooks/` – hook registration and event mapping
  - `EventNotifierPlugin.cs` – main plugin class
- `tests/EventNotifier.Tests/` – xUnit tests for core behavior

## Configuration

On first run, plugin creates `tshock/event-notifier.json`:

```json
{
  "Settings": {
    "SchemaVersion": "1.0",
    "EndpointUrl": "https://example.execute-api.us-east-1.amazonaws.com/prod/events",
    "ApiKey": "replace-me",
    "ApiKeyHeaderName": "x-api-key",
    "RequestTimeoutMs": 5000,
    "RetryCount": 1,
    "RetryDelayMs": 400,
    "QueueCapacity": 512,
    "Events": {
      "Join": true,
      "Leave": true,
      "Chat": true,
      "Death": true,
      "Spawn": true,
      "WorldSave": true,
      "Reload": true
    }
  }
}
```

## Example payload

```json
{
  "schemaVersion": "1.0",
  "eventType": "player.join",
  "occurredAtUtc": "2026-03-21T12:34:56.789+00:00",
  "correlationId": "ec377fc0ab7f468a84f6e374ef41fce8",
  "pluginVersion": "0.1.0",
  "server": {
    "name": "My TShock Server",
    "worldName": "MyWorld",
    "activePlayers": 3,
    "maxSlots": 16,
    "version": "6.1.0"
  },
  "player": {
    "index": 5,
    "name": "Caleb",
    "accountName": "caleb",
    "groupName": "admin",
    "ipAddress": "127.0.0.1",
    "isLoggedIn": true
  },
  "eventData": {
    "who": 5
  }
}
```

## Commands

**Requires permission**: `eventnotifier.admin`

| Subcommand | Effect |
|---|---|
| `/eventnotifier reload` | Reload config from disk |
| `/eventnotifier status` | Display dispatch statistics |
| `/eventnotifier test` | Queue a test event from sender |
| `/eventnotifier showconfig` | Display active configuration |

## Testing locally

Run unit tests for transport and serialization:

```powershell
dotnet test .\tests\EventNotifier.Tests\EventNotifier.Tests.csproj
```

### Build requirements

To build the plugin project, install .NET 9.0 SDK:

```powershell
dotnet --version  # Should show 9.0.x or later
```

Then build:

```powershell
# Core library (net8.0)
dotnet build .\src\EventNotifier.Core\EventNotifier.Core.csproj

# Full solution (requires .NET 9.0)
dotnet build .\EventNotifier.sln
```

## Design highlights

- **Async non-blocking queue**: uses `System.Threading.Channels` so hooks never block the game thread
- **Graceful retry**: single retry with configurable backoff; failures logged but don't stop the server
- **Schema versioning**: payload `schemaVersion` allows safe API evolution
- **Verbose context**: every event includes full server + player state for recipient systems
- **Configuration-first**: hook toggles make it trivial to silence noisy events (e.g., chat)
- **Well-documented**: public APIs have XML doc comments for IDE/doc generation

## Next steps

1. Point `EndpointUrl` to your AWS API Gateway (or other endpoint)
2. Set `ApiKey` to your API key
3. Run `/eventnotifier showconfig` to verify settings
4. Run `/eventnotifier test` to send a sample payload
5. Monitor logs for success/failure counts via `/eventnotifier status`

---

For detailed payload schema, see **Event** and **EventEnvelope** types in `src/EventNotifier.Core/Events/EventEnvelope.cs`.
