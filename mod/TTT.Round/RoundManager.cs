﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using TTT.Public.Extensions;
using TTT.Public.Formatting;
using TTT.Public.Mod.Role;
using TTT.Public.Mod.Round;

namespace TTT.Round;

public class RoundManager : IRoundService
{
    private readonly IRoleService _roleService;
    private readonly LogsListener _logs;
    private Round? _round;
    private RoundStatus _roundStatus = RoundStatus.Paused;

    public RoundManager(IRoleService roleService, BasePlugin plugin)
    {
        _roleService = roleService;
        _logs = new LogsListener(roleService, plugin, 1);
        plugin.RegisterListener<Listeners.OnTick>(TickWaiting);
        
        VirtualFunctions.SwitchTeamFunc.Hook(hook =>
        {
            if (_roundStatus != RoundStatus.Started) return HookResult.Continue;
            var playerPointer = hook.GetParam<nint>(0);
            var player = new CCSPlayerController(playerPointer);
            if (!player.IsValid) return HookResult.Continue;
            if (!player.IsReal()) return HookResult.Continue;
            if (_roleService.GetRole(player) != Role.Unassigned) return HookResult.Continue;
            Server.NextFrame(() => player.CommitSuicide(false, true));

            return HookResult.Continue;
        }, HookMode.Pre);
        
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(_ => _roundStatus != RoundStatus.Waiting ? HookResult.Continue : HookResult.Stop, HookMode.Pre);
    }


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
                _round = new Round(_roleService);
                break;
            case RoundStatus.Started:
                ForceStart();
                break;
            case RoundStatus.Paused:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(roundStatus), roundStatus, "Invalid round status.");
        }
    }

    public void TickWaiting()
    {
        if (_round == null)
        {
            _round = new Round(_roleService);
            return;
        }

        if (_roundStatus != RoundStatus.Waiting) return;

        _round.Tick();
        //AddGracePeriod();

        if (_round.GraceTime() != 0) return;

        if (Utilities.GetPlayers().Where(player => player is { IsValid: true, PawnIsAlive: true }).ToList().Count <= 2)
        {
            Server.PrintToChatAll(StringUtils.FormatTTT("Not enough players to start the round. Round has been ended."));
            _roundStatus = RoundStatus.Paused;
            return; 
        }
        
        SetRoundStatus(RoundStatus.Started); 
        
    }

    public void ForceStart()
    {
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal()).Where(player => player.IsReal())
                     .ToList()) player.VoiceFlags = VoiceFlags.Normal;
        //RemoveGracePeriod();
        _round!.Start(); 
    }

    public void ForceEnd()
    {
        if (_roundStatus == RoundStatus.Ended) return;
        _roundStatus = RoundStatus.Ended;
        _logs.IncrementRound();
        Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.TerminateRound(1,
            RoundEndReason.RoundDraw);
    }

    private void AddGracePeriod()
    {
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid)
            .Where(player => player.IsReal())
            .ToList();

        foreach (var player in players)
        {
            //buggy?
            var weapon = player.PlayerPawn.Value!.WeaponServices!.ActiveWeapon.Value!;
            weapon.NextPrimaryAttackTick = (int)(15 + Server.CurrentTime);
            Utilities.SetStateChanged(player, "CBasePlayerWeapon", "m_nNextPrimaryAttackTick");
        }

        //smth else?
        //disable +use
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
        //needed?
    }
}