using CounterStrikeSharp.API.Core;
using TTT.Public.Mod.Role;

namespace TTT.Public.Player;

public interface IPlayerService
{
    Role PlayerRole();
    void SetPlayerRole(Role role);
    public CCSPlayerController Player();
    long Credits();
    CCSPlayerController? Killer();
    void SetKiller(CCSPlayerController killer);

    CRagdollProp? RagdollProp();
    void SetRagdollProp(CRagdollProp prop);

}