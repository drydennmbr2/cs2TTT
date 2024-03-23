using CounterStrikeSharp.API;
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
    private readonly BasePlugin _plugin;

    private readonly IRoleService _roleService;
    private Round? _round;
    private RoundStatus _roundStatus = RoundStatus.Paused;

    public RoundManager(IRoleService roleService, BasePlugin plugin)
    {
        _roleService = roleService;
        _plugin = plugin;
        _round = new Round(roleService, 1);

        VirtualFunctions.SwitchTeamFunc.Hook(hook =>
        {
            if (_roundStatus != RoundStatus.Started) return HookResult.Continue;
            var playerPointer = hook.GetParam<nint>(0);
            var player = new CCSPlayerController(playerPointer);
            if (!player.IsValid) return HookResult.Continue;
            if (!player.IsReal()) return HookResult.Continue;

            Server.NextFrame(() => player.CommitSuicide(false, true));

            return HookResult.Continue;
        }, HookMode.Pre);
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
                _round = new Round(_roleService, 1);
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
        _plugin.AddTimer(1f, () =>
        {
            if (_round == null)
            {
                _round = new Round(_roleService, 1);
                return;
            }
            
            if (_roundStatus != RoundStatus.Waiting) return;
            
            _round.Tick();

            if (_round.GraceTime() != 0) return;

            _round.Start();

            if (Utilities.GetPlayers().Where(player => player.IsReal()).ToList().Count <= 2) ForceEnd();
        }, TimerFlags.STOP_ON_MAPCHANGE | TimerFlags.REPEAT);
    }

    public void ForceStart()
    {
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal()).Where(player => player.IsReal())
                     .ToList()) player.VoiceFlags = VoiceFlags.Normal;
        //RemoveGracePeriod();
        _roundStatus = RoundStatus.Started;
        _round?.Start(); //shouldn't be null
    }

    public void ForceEnd()
    {
        if (_roundStatus == RoundStatus.Ended) return;
        _roundStatus = RoundStatus.Ended;
        _round = new Round(_roleService, 1);
        Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.TerminateRound(1, RoundEndReason.Unknown);
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
            weapon.NextPrimaryAttackTick = (int)(2 + Server.CurrentTime);
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