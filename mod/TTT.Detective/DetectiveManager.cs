using System;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using TTT.Player;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Formatting;
using TTT.Public.Mod.Detective;
using TTT.Public.Mod.Role;

namespace TTT.Detective;

public class DetectiveManager : IDetectiveService, IPluginBehavior
{
    private readonly IRoleService _roleService;

    public DetectiveManager(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public void Start(BasePlugin parent)
    {
        parent.RegisterEventHandler<EventPlayerHurt>(OnPlayerShoot);
        parent.RegisterListener<Listeners.OnTick>(() =>
        {
            foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid && player.IsReal()).Where(player => (player.Buttons & PlayerButtons.Use) != 0))
            {
                OnPlayerUse(player);
            }
        });
        
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(hook =>
        {
            var info = hook.GetParam<CTakeDamageInfo>(1);
            if (info.Attacker.Value == null || !info.Attacker.Value.IsValid) return HookResult.Continue;
            var attacker = info.Attacker.Value.As<CCSPlayerController>();
            if (attacker == hook.GetParam<CBaseEntity>(0)) return HookResult.Continue;

            if (!attacker.IsReal()) return HookResult.Continue;

            var weapon = attacker.GetActiveWeaponName();
            
            return weapon.Equals("weapon_taser") ? HookResult.Stop : HookResult.Continue;
        }, HookMode.Pre);
    }

    [GameEventHandler]
    private HookResult OnPlayerShoot(EventPlayerHurt @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        var target = @event.Userid;

        if (attacker == null || target == null) return HookResult.Continue;
        
       
        if (!attacker.GetActiveWeaponName().Equals("weapon_taser")) return HookResult.Continue;
        
        var targetRole = _roleService.GetRole(target);
        
        Server.NextFrame(() =>
        {
            attacker.PrintToChat(StringUtils.FormatTTT($"You tased player {target.PlayerName} they are a {targetRole.FormatRoleFull()}"));
        });
        
        return HookResult.Changed;
    }

    private void OnPlayerUse(CCSPlayerController player)
    {
        IdentifyBody(player);   
    }

    private void IdentifyBody(CCSPlayerController caller)
    {
        //add states

        var entity = caller.GetClientAimTarget("ragdoll");

        if (entity == null) return;

        CCSPlayerController? killerEntity = null;
        GamePlayer? plr = null;
        foreach (var player in _roleService.Players())
        {
            if (player.RagdollProp() == null) continue;
            if (!player.RagdollProp()!.Equals(entity)) continue;
            
            if (player.Killer() == null) continue;

            plr = player;
            killerEntity = player.Killer();
        }
        
        if (plr == null) return;
         
        string message;
        
        if (killerEntity == null || !killerEntity.IsValid)
        {
            message = StringUtils.FormatTTT(plr.PlayerRole().FormatStringFullAfter("was killed by world"));
        }
        else
        {
            message = StringUtils.FormatTTT(plr.PlayerRole().FormatStringFullAfter("was killed by ") + _roleService.GetRole(killerEntity).FormatRoleFull());
        }
        
        if (_roleService.GetRole(caller) != Role.Detective) return;
            
        Server.PrintToChatAll(message);
        plr.SetRagdollProp(null);
    }
}