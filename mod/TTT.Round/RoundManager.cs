﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;
using TTT.Public.Mod.Round;

namespace TTT.Round;

public class RoundManager : IRoundService
{

    private readonly IRoleService _roleService;
    private readonly BasePlugin _plugin;
    private const int GracePeriod = 0;

    public RoundManager(IRoleService roleService, BasePlugin plugin)
    {
        _roleService = roleService;
        _plugin = plugin;
    }

    private RoundStatus _roundStatus = RoundStatus.Waiting;
    
    public RoundStatus GetRoundStatus()
    {
        return _roundStatus;
    }

    public void SetRoundStatus(RoundStatus roundStatus)
    {
        _roundStatus = roundStatus;
        switch (roundStatus)
        {
            case RoundStatus.Ended:
                ForceEnd();
                break;
            case RoundStatus.Waiting:
                TickWaiting();
                break;
            case RoundStatus.Started:
                ForceStart();
                break;
            case RoundStatus.Paused:
                ForceEnd();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(roundStatus), roundStatus, "Invalid round status.");
        }
    }

    public void TickWaiting()
    {
        var timer = 0;
        _plugin.AddTimer(1f, () =>
        {
            if (_roundStatus != RoundStatus.Waiting) return;
            var players = Utilities.GetPlayers()
                .Where(player => player.IsValid)
                .Where(player => player.IsReal())
                .ToList();
            if (timer == 0) AddGracePeriod();
            
            foreach (var player in players)
            {
                player.PrintToCenter($"Game is starting in: {15 - timer} seconds");
            }
            
            timer++;
            
            if (timer != 15) return;
            
            ForceStart();
            
            if (Utilities.GetPlayers().Where(player => player.IsReal()).ToList().Count < 3)
            {
                ForceEnd();
            }
            
            timer = 0;
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }
    
    public void ForceStart()
    {
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal()).Where(player => player.IsReal()).ToList())
        {
            player.VoiceFlags = VoiceFlags.Normal;
        }
        RemoveGracePeriod();
        _roundStatus = RoundStatus.Started;
        _roleService.AddRoles();
    }

    public void ForceEnd()
    {
        if (_roundStatus == RoundStatus.Ended) return;
        _roundStatus = RoundStatus.Ended;
        VirtualFunctions.TerminateRound(1, RoundEndReason.Unknown, 2f, 2, 5);
    }

    private void AddGracePeriod()
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid)
            .Where(player => player.IsReal())
            .ToList();

        foreach (var player in players)
        {
            var weapon = player.PlayerPawn.Value!.WeaponServices!.ActiveWeapon.Value!;
            weapon.NextPrimaryAttackTick = (int)(GracePeriod * Server.CurrentTime);
            Utilities.SetStateChanged(player, "CPlayer_WeaponServices", "m_hActiveWeapon");
        }
    }

    private void RemoveGracePeriod()
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid)
            .Where(player => player.IsReal())
            .ToList();

        foreach (var player in players)
        {
            var weapon = player.PlayerPawn.Value!.WeaponServices!.ActiveWeapon.Value!;
            weapon.NextPrimaryAttackTick = (int)Server.CurrentTime;
            Utilities.SetStateChanged(player, "CBasePlayerWeapon", "m_nNextPrimaryAttackTick");
        }
    }

    private void AllowShooting(bool allow = false)
    {
        
    }
}