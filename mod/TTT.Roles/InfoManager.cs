using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TTT.Public.Formatting;
using TTT.Public.Mod.Role;
using TTT.Round;

namespace TTT.Roles;

public class InfoManager
{
    private readonly Dictionary<CCSPlayerController, Role> _playerLookAtRole = new();
    private readonly IRoleService _roleService;

    public InfoManager(IRoleService roleService, BasePlugin plugin)
    {
        _roleService = roleService;
        plugin.RegisterListener<Listeners.OnTick>(OnTick);
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
            if (playerRole == Role.Unassigned) return;
                
            Server.NextFrame(() => player.PrintToCenterHtml($"<p>Your Role: </p>{playerRole.GetCenterRole()}"));
            Server.NextFrame(() => player.PrintToChat($"test"));
            
            if (!_playerLookAtRole.TryGetValue(player, out var value)) continue;
            
            if (value == playerRole || playerRole == Role.Traitor || value == Role.Detective)
            {
                player.PrintToCenterHtml($"<p>Their Role: </p><img src='{value.GetCenterRole()}'>");
                continue;
            }

            if (playerRole == Role.Innocent)
                player.PrintToCenterHtml($"<p>Their Role: </p><img src='{Role.Innocent.GetCenterRole()}'>");
        }
    }

    [GameEventHandler]
    private HookResult? OnPlayerInfo(EventPlayerInfo @event, GameEventInfo info)
    {
        //unused for now?
        //what event?
        return null;
    }
}