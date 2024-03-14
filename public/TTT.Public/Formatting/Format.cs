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
        return FormatRoleFull(role) + message;
    }

    public static string FormatStringFullBefore(this Role role, string message)
    {
        return message + " " + FormatRoleFull(role);
    }

    public static string FormatRole(this Role role)
    {
        return role switch
        {
            Role.Traitor => ChatColors.Red + $"T ",
            Role.Detective => ChatColors.Blue + $"D ",
            Role.Innocent => ChatColors.Green + $"I ",
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
            _ => ""
        };
    }
}