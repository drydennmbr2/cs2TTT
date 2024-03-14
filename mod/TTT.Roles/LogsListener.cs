﻿using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TTT.Public.Action;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;

namespace TTT.Roles;

public class LogsListener : IPluginBehavior
{
    private readonly IRoleService _roleService;
    private readonly HashSet<IAction> _actions = new();
    
    public LogsListener(IRoleService roleService)
    {
        _roleService = roleService;
    }
    
    public void Start(BasePlugin parent)
    {
        parent.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        parent.RegisterEventHandler<EventPlayerHurt>(OnPlayerDamage);
        parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }
    
    [GameEventHandler]
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var killer = @event.Attacker;
        var deadPlayer = @event.Userid;

        if (killer == null || deadPlayer == null) return HookResult.Continue;
        
        _actions.Add(new KillAction(new Tuple<CCSPlayerController, Role>(killer, _roleService.GetRole(killer)),
            new Tuple<CCSPlayerController, Role>(deadPlayer, _roleService.GetRole(deadPlayer))
        ));
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerDamage(EventPlayerHurt @event, GameEventInfo info)
    {
        var killer = @event.Attacker;
        var deadPlayer = @event.Userid;
        var damage = @event.DmgHealth;
        
        if (killer == null || deadPlayer == null) return HookResult.Continue;
        
        //var hitbox = @event.Hitgroup; wip
        
        _actions.Add(new DamageAction(new Tuple<CCSPlayerController, Role>(killer, _roleService.GetRole(killer)),
            new Tuple<CCSPlayerController, Role>(deadPlayer, _roleService.GetRole(deadPlayer)),
            damage,
            0
        ));
        
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        var players = Utilities.GetPlayers();
        var message = CreateMessage();

        foreach (var player in players.Where(player => player.IsReal()))
        {
            Server.NextFrame(() =>
            {
                player.PrintToConsole(message);
            });
        }
        
        _actions.Clear();
        
        return HookResult.Continue;
    }

    private string CreateMessage()
    {
        var builder = new StringBuilder();
        builder.AppendLine("[TTT] Logs");

        foreach (var action in _actions)
        {
            builder.AppendLine(action.ActionMessage());
        }

        builder.AppendLine("[TTT] Logs ended!");

        return builder.ToString();
    }
}