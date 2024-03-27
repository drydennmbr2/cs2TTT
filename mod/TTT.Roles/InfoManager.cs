using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
        plugin.RegisterListener<Listeners.OnTick>(() =>
        {
            OnTickAll();
            OnTick();
        });
    }

    public void RegisterLookAtRole(CCSPlayerController player, Role role)
    {
        _playerLookAtRole.Add(player, role);
    }

    public void RemoveLookAtRole(CCSPlayerController player)
    {
        _playerLookAtRole.Remove(player);
    }

    public void OnTick()
    {
        foreach (var player in _roleService.GetRoles().Keys.Where(player => player.IsValid))
        {
            player.ModifyScoreBoard();
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
                                                            + $"<font class='fontsize=m' color='red'>Their Role: {Role.Innocent.GetCenterRole()}"));
        }
    }

    public void OnTickAll()
    {
        //if this works i dont even know :P
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid)
            .Where(player => player.IsReal())
            .Where(player => player.PawnIsAlive)
            .ToList();
        
        _playerLookAtRole.Clear();
        foreach (var player in players)
        {
            var directionVec = new Vector();
            NativeAPI.AngleVectors(player.PlayerPawn.Value.EyeAngles.Handle, directionVec.Handle, IntPtr.Zero, IntPtr.Zero);
            foreach (var target in players)
            {
                if (player == target) continue;
                var targetDir = new Vector();
                
                NativeAPI.AngleVectors(target.PlayerPawn.Value.EyeAngles.Handle, targetDir.Handle, IntPtr.Zero,
                    IntPtr.Zero);
                
                if (directionVec.Length2D() - targetDir.Length2D() > 10) continue;
                
                
                Vector3 vec1 = new(directionVec.X, directionVec.Y, directionVec.Z);
                Vector3 vec2 = new(targetDir.X, targetDir.Y, targetDir.X);
                var angleInRadians = Math.Acos(Vector3.Dot(vec1, vec2) / (vec1.Length() * vec2.Length()));
                var degree = (Math.PI * 2) / angleInRadians;
                if (degree is < 5 or > -5)
                    RegisterLookAtRole(player, _roleService.GetRole(target));
            }
        }
    }
}