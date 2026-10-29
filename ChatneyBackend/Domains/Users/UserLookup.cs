using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Users;

public static class UserLookup
{
    public static async Task<User?> FindByLogin(AppRepos repos, string login)
    {
        return await repos.Users.GetOne(
            """
            SELECT *
            FROM users
            WHERE LOWER(email) = LOWER(@Login)
               OR LOWER(nickname) = LOWER(@Login)
            LIMIT 1;
            """,
            new { Login = login.Trim() });
    }

    public static async Task<User?> FindByLogin(AppRepos repos, string login, string passwordHash)
    {
        var loginTrimmed = login.Trim();
        var candidates = await repos.Users.GetList(u => u.Password == passwordHash);
        return candidates.FirstOrDefault(u =>
            string.Equals(u.Email, loginTrimmed, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(u.Nickname, loginTrimmed, StringComparison.OrdinalIgnoreCase));
    }

    public static async Task<User?> FindByNickname(
        AppRepos repos,
        string nickname,
        Guid? excludeUserId = null)
    {
        // Raw SQL: RepoDb can't translate a case-insensitive string.Equals, and silently drops it next to another condition.
        return await repos.Users.GetOne(
            """
            SELECT *
            FROM users
            WHERE LOWER(nickname) = LOWER(@Nickname)
              AND id != @ExcludeUserId
            LIMIT 1;
            """,
            new { Nickname = nickname.Trim(), ExcludeUserId = excludeUserId ?? Guid.Empty });
    }

    public static async Task<bool> IsNicknameTaken(
        AppRepos repos,
        string nickname,
        Guid? excludeUserId = null)
    {
        return await FindByNickname(repos, nickname, excludeUserId) != null;
    }
}
