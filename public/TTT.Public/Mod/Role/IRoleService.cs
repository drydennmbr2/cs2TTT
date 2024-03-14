using CounterStrikeSharp.API.Core;

namespace TTT.Public.Mod.Role;

    public interface IRoleService
    {
        Dictionary<CCSPlayerController, Role> GetRoles();
        Role GetRole(CCSPlayerController player);
        bool IsDetective(CCSPlayerController player);
        bool IsTraitor(CCSPlayerController player); 
        void AddDetective(CCSPlayerController player);
        void AddTraitor(CCSPlayerController player);
        void AddInnocents(IEnumerable<CCSPlayerController> players);
        void Clear();
    }

    public enum Role
    {
        Traitor = 0,
        Detective = 1,
        Innocent = 2,
        Unassigned = 3
    }

