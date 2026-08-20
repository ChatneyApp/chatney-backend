using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Configs;

public static class SystemConfigReader
{
    public static async Task<int?> GetIntByName(IPgRepo<Config, int> configs, string name)
    {
        var config = await configs.GetOne(c => c.Name == name);
        if (config == null)
        {
            return null;
        }

        return int.TryParse(config.Value, out var value) ? value : null;
    }
}
