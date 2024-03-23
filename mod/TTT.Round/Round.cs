using CounterStrikeSharp.API;
using TTT.Public.Configuration;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;

namespace TTT.Round;

public class Round
{
    private readonly IRoleService _roleService;
    private readonly int roundId;
    private float _graceTime = Config.TTTConfig.GraceTime;

    public Round(IRoleService roleService, int roundId)
    {
        _roleService = roleService;
        this.roundId = roundId;
    }

    public void Tick()
    {
        _graceTime--;
    }

    public float GraceTime()
    {
        return _graceTime;
    }

    public void Start()
    {
        Server.PrintToChatAll("[TTT] A new round has started!");
        
        _roleService.AddRoles();
    }
}