using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TTT.Public.Action;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;

namespace TTT.Round;

public class LogsListener
{
    private readonly HashSet<IAction> _actions = new();
    private readonly IRoleService _roleService;
    private int _roundId = 0;

    public LogsListener(IRoleService roleService, BasePlugin parent, int roundId = 0)
    {
        _roleService = roleService;
        _roundId = roundId;
        parent.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        parent.RegisterEventHandler<EventPlayerHurt>(OnPlayerDamage);
        parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    public void IncrementRound()
    {
        _roundId++;
    }

    public int GetCurrentRoundId()
    {
        return _roundId;
    }
    
    [GameEventHandler]
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var killer = @event.Attacker;
        var deadPlayer = @event.Userid;

        if (!killer.IsValid || !deadPlayer.IsValid) return HookResult.Continue;

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

        if (!killer.IsValid || !deadPlayer.IsValid) return HookResult.Continue;

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
        var message = CreateMessage();

        Server.PrintToConsole(message);

        foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid).Where(player => player.IsReal())
                     .ToList()) Server.NextFrame(() => player.PrintToConsole(message));
        
        Server.PrintToConsole(message);

        _actions.Clear();

        return HookResult.Continue;
    }

    private void Save()
    {
        
    }

    private string CreateMessage()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"[TTT] Logs round {_roundId}");

        foreach (var action in _actions) builder.AppendLine(action.ActionMessage());

        builder.AppendLine("[TTT] Logs ended!");

        return builder.ToString();
    }
}