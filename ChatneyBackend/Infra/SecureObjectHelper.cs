using ChatneyBackend.Domains.Roles;

namespace ChatneyBackend.Infra;

public static class SecureObjectHelper
{
    public static Task<int> Create(AppRepos repos) =>
        repos.SecureObjects.ExecuteScalarAsync<int>(
            "INSERT INTO secure_objects DEFAULT VALUES RETURNING id");
}
