using System.Security.Claims;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Configs;

public class ConfigQueries
{
    [Authorize]
    public async Task<Config?> GetConfigById(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(SystemConfigPermissions.ReadValue);

        return await repos.Configs.GetById(id);
    }

    [Authorize]
    public async Task<Config?> GetConfigByName(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        string name)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(SystemConfigPermissions.ReadValue);

        return await repos.Configs.GetOne(config => config.Name == name);
    }

    [Authorize]
    public async Task<List<Config>> GetList(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(SystemConfigPermissions.ReadValue);

        return await repos.Configs.GetList();
    }
}
