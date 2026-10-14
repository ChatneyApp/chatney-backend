using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Configs;

public class ConfigQueries
{
    /// <summary>A config entry by id, or null. Requires ConfigReadValue.</summary>
    [Authorize]
    public async Task<Config?> GetConfigById(
        AppRepos repos,
        IPermissionResolver resolver,
        int id)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.ConfigReadValue);

        return await repos.Configs.GetById(id);
    }

    /// <summary>A config entry by name (e.g. "messages.sendCooldown"), or null. Requires ConfigReadValue.</summary>
    [Authorize]
    public async Task<Config?> GetConfigByName(
        AppRepos repos,
        IPermissionResolver resolver,
        string name)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.ConfigReadValue);

        return await repos.Configs.GetOne(config => config.Name == name);
    }

    /// <summary>All config entries. Requires ConfigReadValue.</summary>
    [Authorize]
    public async Task<List<Config>> GetList(
        AppRepos repos,
        IPermissionResolver resolver)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.ConfigReadValue);

        return await repos.Configs.GetList();
    }
}
