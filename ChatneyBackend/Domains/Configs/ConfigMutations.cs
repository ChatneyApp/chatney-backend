using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Configs;

public class ConfigMutations
{
    // TODO: ignore type field - it's constant for system config
    public async Task<Config?> UpdateConfig(PgRepo<Config, int> repo, Config config)
    {
        var updated = await repo.UpdateOne(config);
        return updated ? config : null;
    }
}
