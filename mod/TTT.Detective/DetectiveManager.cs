using System;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
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
        return;
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
            return attacker.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value?.DesignerName != "weapon_taser" ? HookResult.Continue : HookResult.Stop;
        }, HookMode.Pre);
    }

    [GameEventHandler]
    private HookResult OnPlayerShoot(EventPlayerHurt @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        var target = @event.Userid;

        if (attacker == null || target == null) return HookResult.Continue;
        if (!attacker.IsValid || !target.IsValid) return HookResult.Continue;
        if (attacker.PlayerPawn.Value.WeaponServices.ActiveWeapon.Value.DesignerName != "weapon_taser")
            return HookResult.Continue;
        if (_roleService.GetRole(attacker) != Role.Detective) return HookResult.Continue;

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

        var entity = GetNearbyEntity(caller);

        if (entity == null) return;

        var killerEntity = entity.Killer.Value;
        
        var controller = entity.RagdollSource.Value.As<CCSPlayerController>();

        var controllerRole = _roleService.GetRole(controller);

        string message;
        
        if (killerEntity == null || !killerEntity.IsValid)
        {
            message = StringUtils.FormatTTT(controllerRole.FormatStringFullAfter("was killed by world"));
        }
        else
        {
            message = StringUtils.FormatTTT(controllerRole.FormatStringFullAfter("was killed by ") + _roleService.GetRole((CCSPlayerController)killerEntity).FormatRoleFull());
        }
        
        if (_roleService.GetRole(caller) == Role.Detective)
            Server.PrintToChatAll(message);
    }

    private static CRagdollProp? GetNearbyEntity(CCSPlayerController player)
    {
        var entities = Utilities
            .GetAllEntities()
            .Where(entity => entity.IsValid)
            .Where(entity =>
            {
                return entity is CRagdollProp;
            })
            .ToList();
        
        if (!entities.Any(entity =>
            {
                return IsClose(player.AbsOrigin, ((CRagdollProp)entity).AbsOrigin);
            })) return null;
    
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