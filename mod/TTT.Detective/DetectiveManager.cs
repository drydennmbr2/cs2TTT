using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TTT.Public.Behaviors;
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

    public void Start(BasePlugin parent) {
       
    }
    
    [GameEventHandler]
    private HookResult OnPlayerUse(EventPlayerPing @event, GameEventInfo info)
    {
        return HookResult.Stop;
    }

    [GameEventHandler]
    private HookResult OnPlayerShoot(EventPlayerHurt @event, GameEventInfo info)
    {
        var weapon = @event.Weapon;
        if (weapon != "weapon_taser") return HookResult.Continue;

        var attacker = @event.Attacker;
        var target = @event.Userid;

        if (attacker == null || target == null) return HookResult.Continue;

        @event.DmgHealth = 0;
        @event.DmgArmor = 0;
            
        var targetRole = _roleService.GetRole(target);
        
        var pawn = attacker.PlayerPawn.Value;
        if (pawn == null) return HookResult.Continue;
            
        var weaponServices = pawn.WeaponServices;
        if (weaponServices == null) return HookResult.Continue;

        var activeWeapon = weaponServices.ActiveWeapon.Value;

        if (activeWeapon == null) return HookResult.Continue;
        
        Server.NextFrame(() =>
        {
            attacker.PrintToChat(targetRole.FormatStringFullBefore("[TTT] You tased player: "));
            pawn.RemovePlayerItem(activeWeapon);
        });
        
        return HookResult.Changed;
    }

}