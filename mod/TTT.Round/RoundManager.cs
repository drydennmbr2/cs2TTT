using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using TTT.Public.Configuration;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;
using TTT.Public.Mod.Round;

namespace TTT.Round;

public class RoundManager : IRoundService
{

    private readonly IRoleService _roleService;
    private readonly BasePlugin _plugin;
    private RoundStatus _roundStatus = RoundStatus.Paused;
    
    public RoundManager(IRoleService roleService, BasePlugin plugin)
    {
        _roleService = roleService;
        _plugin = plugin;
        
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
        var timer = Config.TTTConfig.GraceTime;
        _plugin.AddTimer(1f, () =>
        {
            if (_roundStatus != RoundStatus.Waiting) return;
            var players = Utilities.GetPlayers()
                .Where(player => player.IsValid)
                .Where(player => player.IsReal())
                .ToList();
            
            //AddGracePeriod();
            
            foreach (var player in players)
            {
                var timer1 = timer;
                Server.NextFrame(() => player.PrintToCenter($"Game is starting in: {timer1} seconds"));
                
            }
            
            timer--;
            
            if (timer != 0) return;
            
            ForceStart();
            
            if (Utilities.GetPlayers().Where(player => player.IsReal()).ToList().Count <= 2)
            {
                ForceEnd();
            }
            
            timer = 15;
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }
    
    public void ForceStart()
    {
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal()).Where(player => player.IsReal()).ToList())
        {
            player.VoiceFlags = VoiceFlags.Normal;
        }
        //RemoveGracePeriod();
        _roundStatus = RoundStatus.Started;
        _roleService.AddRoles();
    }

    public void ForceEnd()
    {
        if (_roundStatus == RoundStatus.Ended) return;
        _roundStatus = RoundStatus.Ended;
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