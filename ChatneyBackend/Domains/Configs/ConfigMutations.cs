using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Configs;

public class ConfigMutations
{
    // TODO: ignore type field - it's constant for system config
    public async Task<Config?> UpdateConfig(AppRepos repos, Config config)
    {
        var updated = await repos.Configs.UpdateOne(config);
        return updated ? config : null;
    }
}
