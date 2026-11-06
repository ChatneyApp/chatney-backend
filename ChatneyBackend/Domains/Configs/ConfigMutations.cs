using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Configs;

public class ConfigMutations
{
    /// <summary>Overwrites a config entry, matched by id. Requires ConfigUpdateValue.</summary>
    /// <returns>The saved entry, or null if no entry with that id exists.</returns>
    [Authorize]
    public async Task<Config?> UpdateConfig(
        AppRepos repos,
        IPermissionResolver resolver,
        Config config,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.ConfigUpdateValue);

        var updated = await repos.Configs.UpdateOne(config);
        if (updated)
        {
            await webSocketConnector.SendUpdatedConfigAsync(WebsocketConfigPayload.FromConfig(config));
        }
        return updated ? config : null;
    }
}
