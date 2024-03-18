using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Memory;

namespace TTT.Round;

public static class ScoreboardListener
{
    public static void ModifyScoreBoard(this CCSPlayerController player)
    {
        if (!player.IsValid) return;
        
    }
}