using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using TTT.Public.Mod.Role;

namespace TTT.Public.Formatting;

public static class Format
{
    public static string FormatStringAfter(this Role role, string message)
    {
        return FormatRole(role) + message;
    }

    public static string FormatStringBefore(this Role role, string message)
    {
        return message + " " + FormatRole(role);
    }
    
    public static string FormatStringFullAfter(this Role role, string message)
    {
        return role.FormatRoleFull() + message;
    }

    public static string FormatStringFullBefore(this Role role, string message)
    {
        return message + " " + role.FormatRoleFull();
    }

    public static string FormatRole(this Role role)
    {
        return role switch
        {
            Role.Traitor => ChatColors.Red + $"T",
            Role.Detective => ChatColors.Blue + $"D",
            Role.Innocent => ChatColors.Green + $"I",
            _ => ""
        };
    }
    
    public static string FormatRoleFull(this Role role)
    {
        return role switch
        {
            Role.Traitor => ChatColors.Red + $"Traitor",
            Role.Detective => ChatColors.Blue + $"Detective",
            Role.Innocent => ChatColors.Green + $"Innocent",
            Role.Unassigned => "",
            _ => ""
        };
    }

    public static string GetRoleUrl(this Role role)
    {
        return role switch
        {
            Role.Traitor =>
                "https://static.wikia.nocookie.net/trouble-in-terrorist-town/images/e/e5/Bar_traitor.png/revision/latest?cb=20230725204816",
            Role.Detective => "https://static.wikia.nocookie.net/trouble-in-terrorist-town/images/3/37/Bar_det.png/revision/latest?cb=20230725204834",
            Role.Innocent => "https://static.wikia.nocookie.net/trouble-in-terrorist-town/images/4/40/Bar_inno.png/revision/latest?cb=20230725204755",
            Role.Unassigned => "",
            _ => ""
        };
    }
}