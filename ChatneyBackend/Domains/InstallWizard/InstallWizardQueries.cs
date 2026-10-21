using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.InstallWizard;

public class InstallWizardQueries
{
    /// <summary>Whether the system installation completed successfully.</summary>
    public async Task<bool> IsInstalled(AppRepos repos)
    {
        var configsTableExists = await repos.Configs.ExecuteScalarAsync<bool>(
            "SELECT to_regclass('public.configs') IS NOT NULL");

        if (!configsTableExists)
        {
            return false;
        }

        return await repos.Configs.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1
                FROM configs
                WHERE name = @Name AND LOWER(value) = 'true'
            )
            """,
            new { Name = Configs.DomainSettings.SystemInstalled });
    }
}
