using ChatneyBackend.Models;

namespace ChatneyBackend.Domains.Configs;

[ExtendObjectType("Query")]
public class ConfigQueries
{
    public Config GetConfigById(ApplicationDbContext dbContext, string id)
        => dbContext.Configs.First(c => c.Id == id);

    public Config GetConfigByKey(ApplicationDbContext dbContext, string key)
        => dbContext.Configs.First(c => c.Key == key);

    public IQueryable<Config> GetConfigs(ApplicationDbContext dbContext)
        => dbContext.Configs;

    public IQueryable<Config> GetConfigsByWorkspaceId(ApplicationDbContext dbContext, string workspaceId)
        => dbContext.Configs.Where(c => c.WorkspaceId == workspaceId);
} 
