using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TTT.Public.Behaviors;
using TTT.Public.Mod.Role;

namespace TTT.Roles;

public class RDMListener : IPluginBehavior
{
    private readonly IRoleService _roleService;

    public RDMListener(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [GameEventHandler]
    private HookResult OnPlayerKill(EventPlayerDeath @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        var killedPlayer = @event.Userid;
        var attackerRole = _roleService.GetRole(attacker);
        var killedRole = _roleService.GetRole(killedPlayer);

        if (killedPlayer == null || attacker == null) return HookResult.Continue;

        if (attackerRole == Role.Traitor && killedRole != Role.Traitor) return HookResult.Continue;
        
        attacker.Health = 0;

        return HookResult.Continue;
    }
}