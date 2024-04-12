using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using TTT.Public.Mod.Role;
using TTT.Public.Player;

namespace TTT.Player;

public class GamePlayer
{

    private Role _playerRole;
    private int _playerId;
    private int _karma;
    private long _credits;
    private CCSPlayerController? _killer;
    private CRagdollProp? _ragdollProp;

    public GamePlayer(Role playerRole, long credits, int karma, int playerId)
    {
        _playerRole = playerRole;
        _credits = credits;
        _karma = karma;
        _killer = null;
        _ragdollProp = null;
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

    public int Karma()
    {
        return _karma;
    }

    public void AddKarma()
    {
        _karma += 2;
    }

    public void RemoveKarma()
    {
        _karma -= 5;
    }

    public void SetPlayerRole(Role role)
    {
        _playerRole = role;
    }

    public long Credits()
    {
        return _credits;
    }

    public void AddCredits(long increment)
    {
        _credits += increment;
    }

    public void RemoveCredits(long decrement)
    {
        _credits -= decrement;
    }

    public void ResetCredits()
    {
        _credits = 800; 
    }

    public CCSPlayerController? Killer()
    {
        return _killer;
    }

    public void SetKiller(CCSPlayerController? killer)
    {
        _killer = killer;
    }

    public CRagdollProp? RagdollProp()
    {
        return _ragdollProp;
    }

    public void SetRagdollProp(CRagdollProp? prop)
    {
        _ragdollProp = prop;
    }
}