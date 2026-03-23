# EventNotifier Plugin Scaffold – Summary

## Overview

A complete, well-documented TShock 6 plugin scaffold that sends server events to an HTTP endpoint via AWS API Gateway (or any HTTP service).

## What Was Built

### Core Library (`EventNotifier.Core`)
Platform-agnostic event transport, payload models, and HTTP dispatch:
- **Configuration**: `NotifierSettings`, `EventHookSettings` (JSON-serializable)
- **Events**: `EventType`, `EventEnvelope`, `ServerInfo`, `PlayerInfo` payload models with full XML docs
- **Serialization**: `EventSerializer` for consistent JSON encoding (camelCase, null handling)
- **Transport**:
  - `INotificationSender` interface for pluggable senders
  - `HttpNotificationSender` with one-retry-then-log policy, API key header injection, timeout handling
  - `NotificationDispatchQueue` using bounded `System.Threading.Channels` for non-blocking hook-safe dispatch
  - `NotificationDispatchResult` for outcome tracking (success count, failure count, dropped count)

### Plugin Layer (`EventNotifier.Plugin`)
TShock 6-specific integration:
- **Commands**: `/eventnotifier {reload|status|test|showconfig}` with permission gate (`eventnotifier.admin`)
- **Configuration**: `PluginConfigStore` loads/saves JSON config via TShock's `ConfigFile<T>` pattern
- **Events**: `EventFactory` maps TShock objects → verbose payload context
- **Hooks**: `HookRegistrar` wires 7 event hooks:
  - `ServerJoin` → `player.join`
  - `ServerLeave` → `player.leave`
  - `PlayerChat` → `player.chat`
  - `KillMe` (death) → `player.death`
  - `PlayerSpawn` → `player.spawn`
  - `WorldSave` → `world.save`
  - `ReloadEvent` → `server.reload`
  - Each hook respects per-event toggle in config
- **Main Plugin**: `EventNotifierPlugin : TerrariaPlugin` orchestrates lifecycle, hooks, commands, queue

### Tests (`EventNotifier.Tests`)
xUnit-based test coverage:
- `EventSerializerTests`: Validates schema version and event type in JSON payload
- `HttpNotificationSenderTests`: Confirms retry behavior, API key header injection, and attempt counting

## Key Features

✅ **Schema-versioned payloads** – `schemaVersion` field supports API contract evolution  
✅ **Verbose event data** – server state (name, player count, version) + player details (account, group, IP, login status) included  
✅ **Configurable hooks** – each event can be toggled on/off independently  
✅ **Non-blocking dispatch** – bounded queue prevents TShock game loop stalls  
✅ **Retry policy** – one automatic retry + fail-and-log (no server disruption)  
✅ **API key auth** – header-based authentication (configurable header name, default `x-api-key`)  
✅ **Well-documented** – XML documentation on all public APIs, IDEs show full context  
✅ **TShock 6 native** – uses `ConfigFile<T>`, `Command`, hook APIs from TShock 6.1.0  

## File Structure

```
tshock-event-notifier/
├── README.md                                    # Usage guide + next steps
├── .gitignore                                   # Standard C# ignores
├── EventNotifier.sln                            # Visual Studio solution
├── src/
│   ├── EventNotifier.Core/
│   │   ├── EventNotifier.Core.csproj            # net8.0 library
│   │   ├── Configuration/
│   │   │   ├── EventHookSettings.cs             # Per-hook toggles
│   │   │   └── NotifierSettings.cs              # Main config model
│   │   ├── Events/
│   │   │   ├── EventType.cs                     # Event type constants
│   │   │   └── EventEnvelope.cs                 # Payload envelope + server/player context
│   │   ├── Serialization/
│   │   │   └── EventSerializer.cs               # JSON serialization (camelCase)
│   │   └── Transport/
│   │       ├── INotificationSender.cs           # Sender interface
│   │       ├── HttpNotificationSender.cs        # Retry-once HTTP dispatch
│   │       ├── NotificationDispatchQueue.cs     # Non-blocking queue worker
│   │       └── NotificationDispatchResult.cs    # Dispatch outcome model
│   └── EventNotifier.Plugin/
│       ├── EventNotifier.Plugin.csproj          # net9.0 plugin (TShock 6)
│       ├── EventNotifierPlugin.cs               # Main plugin class
│       ├── Commands/
│       │   └── NotifierCommands.cs              # Admin command handler
│       ├── Configuration/
│       │   └── PluginConfigStore.cs             # Config load/save
│       ├── Events/
│       │   └── EventFactory.cs                  # Payload construction
│       └── Hooks/
│           └── HookRegistrar.cs                 # Hook registration + callbacks
└── tests/
    └── EventNotifier.Tests/
        ├── EventNotifier.Tests.csproj           # net8.0 tests (xUnit)
        ├── EventSerializerTests.cs              # Payload serialization tests
        └── HttpNotificationSenderTests.cs       # HTTP send + retry tests
```

## Design Decisions

1. **Separation of concerns**: Core library is TShock-agnostic; plugin layer handles TShock coupling
2. **Async queue**: `System.Threading.Channels` with bounded capacity and drop-write strategy
3. **Single endpoint**: All events POST to one URL; consumers filter by `eventType`
4. **Verbose payloads**: Full server + player context in every event for flexibility in handlers
5. **Configuration as source of truth**: Hook toggles live in JSON, checked on each event
6. **No blocking**: Hooks enqueue quickly; background worker handles retries/logging
7. **Graceful degradation**: Failed sends are logged but don't impact server; queue drops on saturation

## Testing Validation

✅ Unit tests pass (2 tests):
- `EventSerializerTests::Serialize_IncludesSchemaAndEventType` – validates JSON structure
- `HttpNotificationSenderTests::SendAsync_RetriesOnce_AndAddsApiKeyHeader` – validates retry + header behavior

## Next Steps for User

1. **Set API key**: Update `event-notifier.json` with real AWS API Gateway key
2. **Configure endpoint**: Point `EndpointUrl` to your actual gateway
3. **Test**: Use `/eventnotifier test` to send a sample payload
4. **Monitor**: Check `/eventnotifier status` for delivery stats
5. **Tune**: Enable/disable hooks in config based on your needs
6. **Deploy**: Copy built `EventNotifier.dll` to TShock plugins folder

## Migration Notes from TShock 4 (`terracord`)

- ✅ Replaced Discord-specific `Discord.cs` with generic `HttpNotificationSender`
- ✅ Replaced custom XML config with TShock 6's `ConfigFile<T>` JSON pattern
- ✅ Simplified hook management: `HookRegistrar` replaces scattered `ServerApi.Hooks.X.Register` calls
- ✅ Added queue worker for safe async dispatch (original Terracord used synchronous sends)
- ✅ Added schema versioning to payloads for future API compatibility
- ✅ Standardized event models: all payloads follow `EventEnvelope` contract

---

**Status**: ✅ Fully scaffolded, tested, and ready for deployment  
**Build**: `dotnet build ./EventNotifier.sln` (requires .NET 9.0 SDK)  
**Tests**: `dotnet test ./tests/EventNotifier.Tests/EventNotifier.Tests.csproj`  
**Documentation**: All public APIs have XML doc comments; see README.md for usage

