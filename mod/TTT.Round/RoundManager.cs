using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using TTT.Public.Mod.Round;

namespace TTT.Round;

public class RoundManager : IRoundService
{

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
                ForceEnd();
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

    public void ForceStart()
    {
        throw new NotImplementedException();
    }

    public void ForceEnd()
    {
        _roundStatus = RoundStatus.Ended;
        VirtualFunctions.TerminateRound(1, RoundEndReason.Unknown, 2f, 2, 5);
    }
}