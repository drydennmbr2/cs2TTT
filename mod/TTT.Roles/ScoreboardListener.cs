using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;

namespace TTT.Roles;

public class ScoreboardListener : IPluginBehavior
{

    public void Start(BasePlugin parent) {
        parent.AddTimer(1f,() =>
        {
            var players = Utilities.GetPlayers()
                .Where(player => player.IsValid)
                .Where(player => player.IsReal())
                .ToList();

            foreach (var player in players)
            {
                var pawn = player.PlayerPawn.Value;
                
                if (pawn == null || !pawn.IsValid) return;
            }
        }, TimerFlags.REPEAT);
    }
}