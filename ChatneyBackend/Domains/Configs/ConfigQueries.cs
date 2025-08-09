using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Configs;

public class ConfigQueries
{
    public Config GetConfigById(ApplicationDbContext dbContext, string id)
        => dbContext.Configs.First(c => c.Id == id);

    public Config GetConfigByKey(ApplicationDbContext dbContext, string key)
        => dbContext.Configs.First(c => c.Key == key);

    public IQueryable<Config> GetList(ApplicationDbContext dbContext)
        => dbContext.Configs;
}
