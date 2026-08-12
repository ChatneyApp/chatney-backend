using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Configs;

public class ConfigMutations
{
    [Authorize]
    public async Task<Config?> UpdateConfig(
        AppRepos repos,
        IPermissionResolver resolver,
        Config config)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.ConfigUpdateValue);

        var updated = await repos.Configs.UpdateOne(config);
        return updated ? config : null;
    }
}
