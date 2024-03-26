using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TTT.Public.Formatting;
using TTT.Public.Mod.Role;
using TTT.Round;

namespace TTT.Roles;

public class InfoManager
{
    private readonly Dictionary<CCSPlayerController, Role> _playerLookAtRole = new();
    private readonly BasePlugin _plugin;
    private readonly IRoleService _roleService;

    public InfoManager(IRoleService roleService, BasePlugin plugin)
    {
        _roleService = roleService;
        _plugin = plugin;
        OnTick();
    }

    public void RegisterLookAtRole(CCSPlayerController player, Role role)
    {
        _playerLookAtRole.Add(player, role);
    }

    public void RemoveLookAtRole(CCSPlayerController player)
    {
        _playerLookAtRole.Remove(player);
    }

    private void OnTick()
    {
        _plugin.RegisterListener<Listeners.OnTick>(() =>
        {
            foreach (var player in from value in _roleService.GetRoles()
                     let player = value.Key
                     let role = value.Value
                     select player)
            {
                player.ModifyScoreBoard();
                var playerRole = _roleService.GetRole(player);
                if (playerRole == Role.Unassigned) return;
                
                player.PrintToCenterHtml($"<p>Your Role: </p><img src='{playerRole.FormatRoleFull()}'>");
                
                if (!_playerLookAtRole.TryGetValue(player, out var value)) continue;

                if (value == playerRole || playerRole == Role.Traitor || value == Role.Detective)
                {
                    player.PrintToCenterHtml($"<p>Their Role: </p><img src='{value.FormatRoleFull()}'>");
                    continue;
                }

                if (playerRole == Role.Innocent)
                    player.PrintToCenterHtml($"<p>Their Role: </p><img src='{Role.Innocent.FormatRoleFull()}'>");
            }
        });
    }

    [GameEventHandler]
    private HookResult? OnPlayerInfo(EventPlayerInfo @event, GameEventInfo info)
    {
        //unused for now?
        //what event?
        return null;
    }
}