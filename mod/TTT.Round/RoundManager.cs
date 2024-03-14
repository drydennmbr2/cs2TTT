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

    private readonly IRoleService _roleService;
    private readonly BasePlugin _plugin;

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
            timer++;
            if (Utilities.GetPlayers().Where(player => player.IsReal()).ToList().Count < 3)
            {
                timer = 0;
                ForceEnd();
            }
            
            if (timer != 15) return;
            
            ForceStart();
            timer = 0;
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }
    
    public void ForceStart()
    {
        _roundStatus = RoundStatus.Started;
        _roleService.AddRoles();
    }

    public void ForceEnd()
    {
        if (_roundStatus == RoundStatus.Ended) return;
        _roundStatus = RoundStatus.Ended;
        VirtualFunctions.TerminateRound(1, RoundEndReason.Unknown, 2f, 2, 5);
    }
}