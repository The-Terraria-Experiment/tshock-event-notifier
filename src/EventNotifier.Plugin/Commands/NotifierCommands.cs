using EventNotifier.Core.Configuration;
using TShockAPI;

namespace EventNotifier.Plugin.Commands;

/// <summary>
/// Handles plugin administration commands.
/// </summary>
public sealed class NotifierCommands
{
    /// <summary>
    /// Permission required to use notifier admin commands.
    /// </summary>
    public const string Permission = "eventnotifier.admin";

    private readonly Func<NotifierSettings> _settingsProvider;
    private readonly Action _reload;
    private readonly Func<string> _statusProvider;
    private readonly Action<TSPlayer> _sendTestEvent;

    /// <summary>
    /// Creates a command handler with service delegates.
    /// </summary>
    public NotifierCommands(
        Func<NotifierSettings> settingsProvider,
        Action reload,
        Func<string> statusProvider,
        Action<TSPlayer> sendTestEvent)
    {
        _settingsProvider = settingsProvider;
        _reload = reload;
        _statusProvider = statusProvider;
        _sendTestEvent = sendTestEvent;
    }

    /// <summary>
    /// Dispatches command to appropriate subcommand handler.
    /// </summary>
    public void Handle(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            SendHelp(args.Player);
            return;
        }

        var subCommand = args.Parameters[0].ToLowerInvariant();
        switch (subCommand)
        {
            case "reload":
                _reload();
                args.Player.SendSuccessMessage("Event notifier config reloaded.");
                break;
            case "status":
                args.Player.SendInfoMessage(_statusProvider());
                break;
            case "test":
                _sendTestEvent(args.Player);
                args.Player.SendSuccessMessage("Test event queued.");
                break;
            case "showconfig":
                ShowConfig(args.Player);
                break;
            default:
                SendHelp(args.Player);
                break;
        }
    }

    private void ShowConfig(TSPlayer player)
    {
        var settings = _settingsProvider();
        player.SendInfoMessage($"Endpoint: {settings.EndpointUrl}");
        player.SendInfoMessage($"SchemaVersion: {settings.SchemaVersion}");
        player.SendInfoMessage($"RetryCount: {settings.RetryCount}");
        player.SendInfoMessage($"Enabled hooks: join={settings.Events.Join}, leave={settings.Events.Leave}, chat={settings.Events.Chat}, death={settings.Events.Death}, spawn={settings.Events.Spawn}, worldsave={settings.Events.WorldSave}, reload={settings.Events.Reload}");
    }

    private static void SendHelp(TSPlayer player)
    {
        player.SendInfoMessage("Usage: /eventnotifier <reload|status|test|showconfig>");
    }
}
