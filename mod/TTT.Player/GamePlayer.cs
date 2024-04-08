using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using TTT.Public.Mod.Role;
using TTT.Public.Player;

namespace TTT.Player;

public class GamePlayer : IPlayerService
{

    private Role _playerRole;
    private int _playerId;
    private long _credits;
    private CCSPlayerController? _killer;
    private CRagdollProp? _ragdollProp;

    public GamePlayer(Role playerRole, long credits, CCSPlayerController? killer, CRagdollProp? ragdollProp, int playerId)
    {
        _playerRole = playerRole;
        _credits = credits;
        _killer = killer;
        _ragdollProp = ragdollProp;
        _playerId = playerId;
    }

    public CCSPlayerController Player()
    {
        return Utilities.GetPlayerFromUserid(_playerId);
    }

    public Role PlayerRole()
    {
        return _playerRole;
    }

    public void SetPlayerRole(Role role)
    {
        _playerRole = role;
    }

    public long Credits()
    {
        return _credits;
    }

    public CCSPlayerController? Killer()
    {
        return _killer;
    }

    public void SetKiller(CCSPlayerController killer)
    {
        _killer = killer;
    }

    public CRagdollProp? RagdollProp()
    {
        return _ragdollProp;
    }

    public void SetRagdollProp(CRagdollProp prop)
    {
        _ragdollProp = prop;
    }
}