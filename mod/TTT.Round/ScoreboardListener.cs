using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;

namespace TTT.Round;

public static class ScoreboardListener
{
    public static void ModifyScoreBoard(this CCSPlayerController player)
    {
        if (!player.IsValid) return;
        var actionService = player.ActionTrackingServices;
        if (actionService == null) return;


        RemoveKills(player);
        RemoveDamage(player);
        RemoveDeaths(player);
        RemoveAssists(player);
        RemoveAdvancedScore(player);
        
    }

    private static void RemoveKills(CCSPlayerController player)
    {
        var matchStats = player.ActionTrackingServices.MatchStats;
        matchStats.Kills = 0;
        Utilities.SetStateChanged(player, "CSPerRoundStats_t", "m_iKills");
    }
    
    private static void RemoveDamage(CCSPlayerController player)
    {
        var matchStats = player.ActionTrackingServices.MatchStats;
        matchStats.Damage = 0;
        matchStats.UtilityDamage = 0;
        Utilities.SetStateChanged(player, "CSPerRoundStats_t", "m_iUtilityDamage");
        Utilities.SetStateChanged(player, "CSPerRoundStats_t", "m_iDamage");
    }
    
    private static void RemoveDeaths(CCSPlayerController player)
    {
        var matchStats = player.ActionTrackingServices.MatchStats;
        matchStats.Deaths = 0;
        Utilities.SetStateChanged(player, "CSPerRoundStats_t", "m_iDeaths");
    }
    
    private static void RemoveAssists(CCSPlayerController player)
    {
        var matchStats = player.ActionTrackingServices.MatchStats;
        matchStats.Assists = 0;
        Utilities.SetStateChanged(player, "CSPerRoundStats_t", "m_iAssists");
    }

    private static void RemoveAdvancedScore(CCSPlayerController player)
    {
        var matchStats = player.ActionTrackingServices.MatchStats;
        matchStats.Flash_Successes = 0;
        matchStats.HeadShotKills = 0;
        matchStats.CashEarned = 0;
        matchStats.KillReward = 0;
        Utilities.SetStateChanged(player, "CSPerRoundStats_t", "m_iFlash_Successes");
        Utilities.SetStateChanged(player, "CSPerRoundStats_t", "m_iHeadShotKills");
        Utilities.SetStateChanged(player, "CSPerRoundStats_t", "m_iCashEarned");
        Utilities.SetStateChanged(player, "CSPerRoundStats_t", "m_iKillReward");
    }
}