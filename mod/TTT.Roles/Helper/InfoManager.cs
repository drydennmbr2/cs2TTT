using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using TTT.Public.Extensions;
using TTT.Public.Formatting;
using TTT.Public.Mod.Role;
using TTT.Public.Mod.Round;
using TTT.Public.Player;
using TTT.Round;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace TTT.Roles;

public class InfoManager
{
    private readonly Dictionary<CCSPlayerController, Tuple<CCSPlayerController, Role>> _playerLookAtRole = new();
    private readonly RoleManager _roleService;
    private readonly IRoundService _manager;

    public InfoManager(RoleManager roleService, IRoundService manager, BasePlugin plugin)
    {
        _roleService = roleService;
        _manager = manager;
        plugin.RegisterListener<Listeners.OnTick>(OnTick);
        plugin.AddTimer(1.5f, OnTickAll
        ,TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }

    public void RegisterLookAtRole(CCSPlayerController player, Tuple<CCSPlayerController, Role> role)
    {
        _playerLookAtRole.TryAdd(player, role);
    }

    public void RemoveLookAtRole(CCSPlayerController player)
    {
        _playerLookAtRole.Remove(player);
    }

    public void OnTick()
    {
        foreach (var gamePlayer in _roleService.Players())
        {
            var player = gamePlayer.Player();
            if (!player.IsValid) continue;
            player.ModifyScoreBoard();
            var playerRole = gamePlayer.PlayerRole();
            if (playerRole == Role.Unassigned) continue;
            if (_manager.GetRoundStatus() != RoundStatus.Started) return;
            if (!player.PawnIsAlive) continue;
            if (gamePlayer.ShopOpen()) continue;
            
            if (!_playerLookAtRole.TryGetValue(player, out var value))
            {
                Server.NextFrame(() => player.PrintToCenterHtml($"<font class='fontsize=m' color='red'>Your Role: {playerRole.GetCenterRole()}"));
                continue;
            }

            if (playerRole == Role.Innocent || (value.Item2 == Role.Traitor && playerRole == Role.Detective))
            {
                Server.NextFrame(() => player.PrintToCenterHtml($"<font class='fontsize=m' color='red'>Your Role: {playerRole.GetCenterRole()} <br>"
                                                                + $"<font class='fontsize=m' color='red'>{value.Item1.PlayerName}'s Role: {Role.Unassigned.GetCenterRole()}"));
                continue;
            }
            
            if (value.Item2 == playerRole || playerRole == Role.Traitor || value.Item2 == Role.Detective)
            {
                Server.NextFrame(() => player.PrintToCenterHtml($"<font class='fontsize=m' color='red'>Your Role: {playerRole.GetCenterRole()} <br>"
                                                          + $"<font class='fontsize=m' color='red'>{value.Item1.PlayerName}'s Role: {value.Item2.GetCenterRole()}")); 
            }
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
            var target = player.GetClientAimTarget("player");
            if (target == null) continue;
            
            RegisterLookAtRole(player, new Tuple<CCSPlayerController, Role>(target, _roleService.GetRole(target)));
        }
    }
}