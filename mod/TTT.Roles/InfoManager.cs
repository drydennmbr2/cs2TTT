using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using TTT.Public.Extensions;
using TTT.Public.Formatting;
using TTT.Public.Mod.Role;
using TTT.Round;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace TTT.Roles;

public class InfoManager
{
    private readonly Dictionary<CCSPlayerController, Role> _playerLookAtRole = new();
    private readonly IRoleService _roleService;

    public InfoManager(IRoleService roleService, BasePlugin plugin)
    {
        _roleService = roleService;
        return;
        plugin.RegisterListener<Listeners.OnTick>(() =>
        {
            OnTick();
        });
        plugin.AddTimer(1f, OnTickAll
        ,TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }

    public void RegisterLookAtRole(CCSPlayerController player, Role role)
    {
        _playerLookAtRole.TryAdd(player, role);
    }

    public void RemoveLookAtRole(CCSPlayerController player)
    {
        _playerLookAtRole.Remove(player);
    }

    public void OnTick()
    {
        foreach (var player in _roleService.GetRoles().Keys.Where(player => player.IsValid))
        {
            //player.ModifyScoreBoard();
            var playerRole = _roleService.GetRole(player);
            if (playerRole == Role.Unassigned) continue;
                
            
            if (!_playerLookAtRole.TryGetValue(player, out var value))
            {
                Server.NextFrame(() => player.PrintToCenterHtml($"<font class='fontsize=m' color='red'>Your Role: {playerRole.GetCenterRole()}"));
                continue;
            }
            
            if (value == playerRole || playerRole == Role.Traitor || value == Role.Detective)
            {
                Server.NextFrame(() => player.PrintToCenterHtml($"<font class='fontsize=m' color='red'>Your Role: {playerRole.GetCenterRole()} <br>"
                                                          + $"<font class='fontsize=m' color='red'>Their Role: {value.GetCenterRole()}"));
                continue;
            }

            Server.NextFrame(() => player.PrintToCenterHtml($"<font class='fontsize=m' color='red'>Your Role: {playerRole.GetCenterRole()} <br>"
                                                            + $"<font class='fontsize=m' color='red'>Their Role: {Role.Unassigned.GetCenterRole()}"));
        }
    }

    public void OnTickAll()
    {
        //if this works i dont even know :P
        //removed natives
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid
            && player.IsReal() 
            && player.PawnIsAlive)
            .ToList();
        
        _playerLookAtRole.Clear();
        foreach (var player in players)
        {
            var playerAngles = player.PlayerPawn.Value.EyeAngles;
            Vector3 vec1 = new (playerAngles.X, playerAngles.Y, playerAngles.Z);
            foreach (var target in players)
            {
                if (player == target) continue;

                var targetAngles = target.PlayerPawn.Value.EyeAngles;
                Vector3 vec2 = new(targetAngles.X, targetAngles.Y, targetAngles.Z);
                
                if (vec1.Length() - vec2.Length() > 10) continue;
                
                var angleInRadians = Math.Acos(Vector3.Dot(vec1, vec2) / (vec1.Length() * vec2.Length()));
                var degree = (Math.PI * 2) / angleInRadians;
                if (degree is < 5 or > -5)
                    RegisterLookAtRole(player, _roleService.GetRole(target));
            }
        }
    }
}