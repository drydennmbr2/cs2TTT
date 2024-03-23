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
    private readonly int roundId;
    private float _graceTime = Config.TTTConfig.GraceTime;

    public Round(IRoleService roleService, int roundId)
    {
        _roleService = roleService;
        this.roundId = roundId;
    }

    public void Tick()
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid)
            .Where(player => player.IsReal())
            .ToList();
        
        var formattedColor = $"<font color=\"#{Color.Yellow.R:X2}{Color.Yellow.G:X2}{Color.Yellow.B:X2}\">";

        foreach (var player in players)
        {
            Server.NextFrame(() => player.PrintToCenterHtml($"{formattedColor}<b>[TTT] Game is starting in {_graceTime--} seconds</b></font>"));
        }
    }

    public float GraceTime()
    {
        return _graceTime;
    }

    public void Start()
    {
        Server.PrintToChatAll($"{ChatColors.Yellow}[TTT] A new round has started!");
        
        _roleService.AddRoles();
    }
}