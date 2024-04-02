using CounterStrikeSharp.API.Core;
using TTT.Public.Mod.Role;

namespace TTT.Public.Player;

public interface IPlayerService
{
    Role PlayerRole();
    long Credits();
    CCSPlayerController? Killer();
    CRagdollProp? RagdollProp();
}