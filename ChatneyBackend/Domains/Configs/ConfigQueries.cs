using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Configs;

public class ConfigQueries
{
    public async Task<Config?> GetConfigById(AppRepos repos, int id) => await repos.Configs.GetById(id);

    public async Task<Config?> GetConfigByName(AppRepos repos, string name) =>
        await repos.Configs.GetOne(config => config.Name == name);

    public async Task<List<Config>> GetList(AppRepos repos) => await repos.Configs.GetList();
}
