using EventNotifier.Core.Configuration;
using EventNotifier.Core.Events;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace EventNotifier.Plugin.Hooks;

/// <summary>
/// Encapsulates TShock hook registration and callback wiring.
/// </summary>
public sealed class HookRegistrar
{
    private readonly TerrariaPlugin _plugin;
    private readonly Func<NotifierSettings> _settingsProvider;
    private readonly Action<string, TSPlayer?, Dictionary<string, object?>> _publish;
    private readonly Action _reloadConfig;

    /// <summary>
    /// Creates a hook registrar bound to a plugin instance and publisher delegate.
    /// </summary>
    public HookRegistrar(
        TerrariaPlugin plugin,
        Func<NotifierSettings> settingsProvider,
        Action<string, TSPlayer?, Dictionary<string, object?>> publish,
        Action reloadConfig)
    {
        _plugin = plugin;
        _settingsProvider = settingsProvider;
        _publish = publish;
        _reloadConfig = reloadConfig;
    }

    /// <summary>
    /// Registers all enabled hooks.
    /// </summary>
    public void Register()
    {
        ServerApi.Hooks.ServerJoin.Register(_plugin, OnJoin);
        ServerApi.Hooks.ServerLeave.Register(_plugin, OnLeave);
        ServerApi.Hooks.WorldSave.Register(_plugin, OnWorldSave);
        PlayerHooks.PlayerChat += OnPlayerChat;
        GeneralHooks.ReloadEvent += OnReload;
        GetDataHandlers.KillMe += OnKillMe;
        GetDataHandlers.PlayerSpawn += OnPlayerSpawn;
    }

    /// <summary>
    /// Unregisters all hooks for clean shutdown.
    /// </summary>
    public void Deregister()
    {
        ServerApi.Hooks.ServerJoin.Deregister(_plugin, OnJoin);
        ServerApi.Hooks.ServerLeave.Deregister(_plugin, OnLeave);
        ServerApi.Hooks.WorldSave.Deregister(_plugin, OnWorldSave);
        PlayerHooks.PlayerChat -= OnPlayerChat;
        GeneralHooks.ReloadEvent -= OnReload;
        GetDataHandlers.KillMe -= OnKillMe;
        GetDataHandlers.PlayerSpawn -= OnPlayerSpawn;
    }

    private void OnJoin(JoinEventArgs args)
    {
        if (!_settingsProvider().Events.Join)
        {
            return;
        }

        var player = SafePlayerLookup(args.Who);
        _publish(EventType.PlayerJoin, player, new Dictionary<string, object?>
        {
            ["who"] = args.Who
        });
    }

    private void OnLeave(LeaveEventArgs args)
    {
        if (!_settingsProvider().Events.Leave)
        {
            return;
        }

        var player = SafePlayerLookup(args.Who);
        _publish(EventType.PlayerLeave, player, new Dictionary<string, object?>
        {
            ["who"] = args.Who
        });
    }

    private void OnWorldSave(WorldSaveEventArgs args)
    {
        if (!_settingsProvider().Events.WorldSave)
        {
            return;
        }

        _publish(EventType.WorldSave, null, new Dictionary<string, object?>
        {
            ["worldId"] = Main.worldID
        });
    }

    private void OnPlayerChat(PlayerChatEventArgs args)
    {
        if (!_settingsProvider().Events.Chat)
        {
            return;
        }

        _publish(EventType.PlayerChat, args.Player, new Dictionary<string, object?>
        {
            ["rawText"] = args.RawText,
            ["formattedText"] = args.TShockFormattedText
        });
    }

    private void OnKillMe(object? sender, GetDataHandlers.KillMeEventArgs args)
    {
        if (!_settingsProvider().Events.Death)
        {
            return;
        }

        _publish(EventType.PlayerDeath, args.Player, new Dictionary<string, object?>
        {
            ["playerId"] = args.PlayerId,
            ["damage"] = args.Damage,
            ["direction"] = args.Direction,
            ["pvp"] = args.Pvp,
            ["deathReason"] = args.PlayerDeathReason.ToString()
        });
    }

    private void OnPlayerSpawn(object? sender, GetDataHandlers.SpawnEventArgs args)
    {
        if (!_settingsProvider().Events.Spawn)
        {
            return;
        }

        _publish(EventType.PlayerSpawn, args.Player, new Dictionary<string, object?>
        {
            ["playerId"] = args.PlayerId,
            ["spawnX"] = args.SpawnX,
            ["spawnY"] = args.SpawnY,
            ["respawnTimer"] = args.RespawnTimer,
            ["deathsPve"] = args.NumberOfDeathsPVE,
            ["deathsPvp"] = args.NumberOfDeathsPVP,
            ["team"] = args.Team,
            ["spawnContext"] = args.SpawnContext.ToString()
        });
    }

    private void OnReload(ReloadEventArgs args)
    {
        _reloadConfig();

        if (!_settingsProvider().Events.Reload)
        {
            return;
        }

        _publish(EventType.ServerReload, args.Player, new Dictionary<string, object?>
        {
            ["by"] = args.Player?.Name
        });
    }

    private static TSPlayer? SafePlayerLookup(int who)
    {
        if (who < 0 || who >= TShock.Players.Length)
        {
            return null;
        }

        return TShock.Players[who];
    }
}
