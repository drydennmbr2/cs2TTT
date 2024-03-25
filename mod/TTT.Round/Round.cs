using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using TTT.Public.Configuration;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;

namespace TTT.Round;

public class Round
{
    private readonly IRoleService _roleService;
    private float _graceTime = Config.TTTConfig.GraceTime;

    public Round(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public void Tick()
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid)
            .Where(player => player.IsReal())
            .ToList();
        
        var formattedColor = $"<font color=\"#{Color.Green.R:X2}{Color.Green.G:X2}{Color.Green.B:X2}\">";
        
        foreach (var player in players)
        {
            Server.NextFrame(() => player.PrintToCenterHtml($"{formattedColor}<b>[TTT] Game is starting in {_graceTime} seconds</b></font>"));
        }

        _graceTime--;
    }

    public float GraceTime()
    {
        return _graceTime;
    }

    public void Start()
    {
        Server.PrintToChatAll("" + $"{ChatColors.Yellow}[TTT] A new round has started!");
        //get teammates
        _roleService.AddRoles();
    }
}