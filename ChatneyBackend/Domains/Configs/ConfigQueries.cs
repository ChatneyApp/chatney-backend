using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Configs;

public class ConfigQueries
{
    public async Task<Config?> GetConfigById(PgRepo<Config, int> repo, int id) => await repo.GetById(id);

    public async Task<Config?> GetConfigByName(PgRepo<Config, int> repo, string name) =>
        await repo.GetOne(config => config.Name == name);

    public async Task<List<Config>> GetList(PgRepo<Config, int> repo) => await repo.GetList();
}
