using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
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
        parent.RegisterEventHandler<EventPlayerHurt>(OnPlayerShoot);
    }
    

    [GameEventHandler]
    private HookResult OnPlayerShoot(EventPlayerHurt @event, GameEventInfo info)
    {
        var weapon = @event.Weapon;
        if (weapon != "weapon_taser") return HookResult.Continue;
        var zeusWeapon = true;
        var attacker = @event.Attacker;
        var target = @event.Userid;

        if (attacker == null || target == null) return HookResult.Continue;
    
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(hook =>
        {
            if (!zeusWeapon) return HookResult.Continue;
            var damage = hook.GetParam<CTakeDamageInfo>(1);
            damage.Damage = 0;
            Utilities.SetStateChanged(target, "CTakeDamageInfo", "m_flDamage");
            zeusWeapon = false;
            return HookResult.Changed;
        }, HookMode.Pre);
        
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
    
    [EntityOutputHook("*", "OnPlayerUse")]
    private HookResult OnUse(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {
        IdentifyBody((CCSPlayerController)caller);
        return HookResult.Continue;
    }

    private void IdentifyBody(CCSPlayerController caller)
    {
        //add states
        
        var entity = GetNearbyEntity(caller);
        
        if (entity == null) return;
        
        var killerEntity = entity.Killer.Value;
        
        if (entity.RagdollSource.Value! is not CCSPlayerController controller) return;

        var controllerRole = _roleService.GetRole(controller);
        
        if (killerEntity == null)
        {
            Server.PrintToChatAll($"[TTT] Player {_roleService.GetRole(caller).FormatStringFullBefore(caller.PlayerName)} found body {controllerRole.FormatStringFullBefore(controller.PlayerName)}");
            _roleService.ApplyColorFromRole(controller, controllerRole);
            return;
        }
        
        if (!killerEntity.IsValid) return;
            
        if (killerEntity is not CCSPlayerController killer) return;
        
        
        if (_roleService.GetRole(caller) == Role.Detective)
        {
            Server.PrintToChatAll($"[TTT] {controllerRole.FormatStringFullBefore(controller.PlayerName)} was killed by {_roleService.GetRole(killer).FormatStringFullBefore(killer.PlayerName)}");
        }

    }

    private CRagdollProp? GetNearbyEntity(CCSPlayerController player)
    {
        var entities = Utilities
            .GetAllEntities()
            .Where(entity => entity.IsValid)
            .Where(entity => entity is CRagdollProp)
            .ToList();
        
        if (!entities.Any(entity => IsClose(player.AbsOrigin, ((CRagdollProp)entity).AbsOrigin))) return null;
        
        var entity = entities.First(entity => IsClose(player.AbsOrigin, ((CRagdollProp)entity).AbsOrigin));
        
        return (CRagdollProp)entity;
    }

    private static bool IsClose(Vector? from, Vector? to)
    {
        if (from == null || to == null) return false;
        var length = from.Length2D() - to.Length2D();
        return length is > 0 and < 10;
    }

}