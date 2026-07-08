using EventNotifier.Core.Configuration;
using EventNotifier.Core.Events;
using EventNotifier.Core.Transport;
using EventNotifier.Plugin.Commands;
using EventNotifier.Plugin.Configuration;
using EventNotifier.Plugin.Events;
using EventNotifier.Plugin.Hooks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace EventNotifier.Plugin;

/// <summary>
/// TShock plugin that emits schema-versioned server events to a single HTTP endpoint.
/// </summary>
[ApiVersion(2, 1)]
public sealed class EventNotifierPlugin : TerrariaPlugin
{
    private readonly PluginConfigStore _configStore = new();
    private readonly HttpClient _httpClient = new();
    private readonly PlayerContextCache _playerCache = new();

    private NotifierSettings _settings = new();
    private NotificationDispatchQueue? _queue;
    private HookRegistrar? _hookRegistrar;
    private Command? _command;

    /// <summary>
    /// Plugin display name.
    /// </summary>
    public override string Name => "EventNotifier";

    /// <summary>
    /// Plugin author attribution.
    /// </summary>
    public override string Author => "TTE";

    /// <summary>
    /// Plugin description.
    /// </summary>
    public override string Description => "Sends important TShock events to an HTTP endpoint.";

    /// <summary>
    /// Plugin semantic version.
    /// </summary>
    public override Version Version => new(0, 1, 0);

    /// <summary>
    /// Initializes a new plugin instance.
    /// </summary>
    public EventNotifierPlugin(Main game) : base(game)
    {
    }

    /// <summary>
    /// Registers hooks, commands, and the background dispatch worker.
    /// </summary>
    public override void Initialize()
    {
        ReloadSettings();

        var sender = new HttpNotificationSender(_httpClient, _settings);
        _queue = new NotificationDispatchQueue(sender, _settings.QueueCapacity, LogInfo, LogWarn);

        _hookRegistrar = new HookRegistrar(this, () => _settings, PublishEvent, ReloadSettings, ForgetPlayerSlot);
        _hookRegistrar.Register();

        var commands = new NotifierCommands(() => _settings, ReloadSettings, BuildStatusMessage, SendTestEvent);
        _command = new Command(NotifierCommands.Permission, commands.Handle, "eventnotifier", "enotify")
        {
            HelpText = "Manage EventNotifier: /eventnotifier <reload|status|test|showconfig>"
        };

        TShockAPI.Commands.ChatCommands.Add(_command);
        LogInfo("Initialized.");
    }

    /// <summary>
    /// Cleans up hooks, queue, and HTTP resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_command is not null)
            {
                TShockAPI.Commands.ChatCommands.Remove(_command);
                _command = null;
            }

            _hookRegistrar?.Deregister();

            if (_queue is not null)
            {
                _queue.DisposeAsync().AsTask().GetAwaiter().GetResult();
                _queue = null;
            }

            _httpClient.Dispose();
            LogInfo("Disposed.");
        }

        base.Dispose(disposing);
    }

    private void ReloadSettings()
    {
        _settings = _configStore.LoadOrCreateDefault();
        LogInfo($"Config loaded from '{_configStore.ConfigPath}'.");
    }

    private void PublishEvent(string eventType, TSPlayer? player, int fallbackIndex, Dictionary<string, object?> eventData)
    {
        if (_queue is null)
        {
            return;
        }

        var (playerInfo, dataSource) = ResolvePlayerInfo(player, fallbackIndex);

        var envelope = EventFactory.CreateBase(_settings, eventType);
        envelope.Player = playerInfo;
        envelope.PlayerDataSource = dataSource;
        envelope.EventData = eventData;

        _queue.TryEnqueue(envelope);
    }

    private (PlayerInfo? Player, string? DataSource) ResolvePlayerInfo(TSPlayer? player, int fallbackIndex)
    {
        var info = EventFactory.BuildPlayerInfo(player);
        if (info is not null)
        {
            if (fallbackIndex >= 0)
            {
                _playerCache.Remember(fallbackIndex, info);
            }

            return (info, PlayerDataSource.Live);
        }

        if (fallbackIndex >= 0 && _playerCache.TryGet(fallbackIndex, out var cached))
        {
            return (cached, PlayerDataSource.Cached);
        }

        return fallbackIndex >= 0
            ? (new PlayerInfo { Index = fallbackIndex, Name = "unknown" }, PlayerDataSource.Unknown)
            : (null, null);
    }

    /// <summary>
    /// Clears any cached player snapshot for a slot index. Called defensively when a
    /// new connection claims the slot, and after a leave has been processed, so a
    /// reused slot never inherits a previous occupant's identity.
    /// </summary>
    private void ForgetPlayerSlot(int index)
    {
        if (index >= 0)
        {
            _playerCache.Forget(index);
        }
    }

    private void SendTestEvent(TSPlayer sender)
    {
        PublishEvent("test.manual", sender, sender.Index, new Dictionary<string, object?>
        {
            ["note"] = "Manual test event generated by command.",
            ["commandSource"] = sender.Name
        });
    }

    private string BuildStatusMessage()
    {
        if (_queue is null)
        {
            return "Event notifier queue is not initialized.";
        }

        return $"Dispatch stats: success={_queue.SuccessCount}, failed={_queue.FailureCount}, dropped={_queue.DroppedCount}.";
    }

    private static void LogInfo(string message) => TShock.Log.ConsoleInfo($"[EventNotifier] {message}");
    private static void LogWarn(string message) => TShock.Log.ConsoleWarn($"[EventNotifier] {message}");
}

