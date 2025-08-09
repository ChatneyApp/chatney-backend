using System.Diagnostics;
using ChatneyBackend.Models;

namespace ChatneyBackend.Domains.Configs;

[ExtendObjectType("Mutation")]
public class ConfigMutations
{
    public Config AddConfig(ApplicationDbContext dbContext, Config config)
    {
        dbContext.Configs.Add(config);
        try
        {
            dbContext.SaveChanges();
            return config;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"something went wrong {ex}");
        }

        return null;
    }

    public bool DeleteConfig(ApplicationDbContext dbContext, string id)
    {
        var config = dbContext.Configs.First(config => config.Id == id);

        try
        {
            dbContext.Configs.Remove(config);
            dbContext.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"something went wrong {ex}");
        }

        return false;
    }
} 