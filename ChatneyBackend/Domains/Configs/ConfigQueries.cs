using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Configs;

public class ConfigQueries
{
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
