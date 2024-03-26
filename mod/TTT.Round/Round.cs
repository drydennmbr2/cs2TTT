using System.Drawing;
using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using TTT.Public.Configuration;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;
using TTT.Public.Formatting;

namespace TTT.Round;

public class Round
{
    private readonly IRoleService _roleService;
    private float _graceTime = Config.TTTConfig.GraceTime * 64;

    public Round(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public void Tick()
    {
        _graceTime--;
        if (_graceTime % 64 != 0) return;
        
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid)
            .Where(player => player.IsReal())
            .ToList();
        
        var formattedColor = $"<font color=\"#{Color.Green.R:X2}{Color.Green.G:X2}{Color.Green.B:X2}\">";
        
        foreach (var player in players)
        {
            Server.NextFrame(() =>
            {
                player.PrintToCenterHtml(
                    $"{formattedColor}<b>[TTT] Game is starting in {Math.Floor(_graceTime / 64)} seconds</b></font>");
            });
        }
    }

    public float GraceTime()
    {
        return _graceTime;
    }

    public void Start()
    {
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid))
        { 
            player.PrintToChat(StringUtils.FormatTTT("A new round has started!"));
        }
        SendTraitorMessage();
        SendDetectiveMessage();
        //get teammates
        _roleService.AddRoles();
    }

    private void SendTraitorMessage()
    {
        StringBuilder message = new();
        message.AppendLine(StringUtils.FormatTTT("Your teammates:"));
        var traitors = _roleService.GetTraitors();
        foreach (var traitor in traitors)
        {
            message.AppendLine(StringUtils.FormatTTT(Role.Traitor.FormatStringFullAfter(traitor.PlayerName)));
        }

        foreach (var player in traitors)
        {
            Server.NextFrame(() => player.PrintToChat(message.ToString()));
        }
    }

    private void SendDetectiveMessage()
    {
        StringBuilder message = new();
        message.AppendLine(StringUtils.FormatTTT("Your teammates:"));
        var detectives = _roleService.GetDetectives();
        foreach (var detective in detectives)
        {
            message.AppendLine(StringUtils.FormatTTT(Role.Detective.FormatStringFullAfter(detective.PlayerName)));
        }

        foreach (var player in detectives)
        {
            Server.NextFrame(() => player.PrintToChat(message.ToString()));
        }
    }
}