using Microsoft.Extensions.DependencyInjection;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;

namespace TTT.Roles;

public static class RoleServiceExtension
{
    public static void AddTTTRoles(this IServiceCollection collection)
    {
        collection.AddPluginBehavior<IRoleService, RoleManager>();
        collection.AddPluginBehavior<LogsListener>();
        collection.AddPluginBehavior<RDMListener>();
    }
}