using System.Security.Claims;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Configs;

public class ConfigMutations
{
    [Authorize]
    public async Task<Config?> UpdateConfig(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        Config config)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(SystemConfigPermissions.UpdateValue);

        var updated = await repos.Configs.UpdateOne(config);
        return updated ? config : null;
    }
}
