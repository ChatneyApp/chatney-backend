using ChatneyBackend.Infra;
using ChatneyBackend.Utils;

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

    /// <summary>
    /// Copies the install-time "system.*" configs into <paramref name="appConfig"/>. Leaves them
    /// null when the system is not installed yet (configs table does not exist).
    /// </summary>
    public static async Task LoadInto(AppConfig appConfig, IPgRepo<Config, int> configs)
    {
        var installed = await configs.ExecuteScalarAsync<bool>(
            $"SELECT to_regclass('{DomainSettings.ConfigTableName}') IS NOT NULL");

        if (!installed)
        {
            return;
        }

        var adminUserId = await configs.GetOne(c => c.Name == DomainSettings.SystemAdminUserId);
        appConfig.AdminUserId = Guid.TryParse(adminUserId?.Value, out var guid) ? guid : null;
        appConfig.DefaultUserRoleId = await GetIntByName(configs, DomainSettings.SystemDefaultUserRoleId);
    }
}
