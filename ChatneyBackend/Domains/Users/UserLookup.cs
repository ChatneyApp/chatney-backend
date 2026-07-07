using System.Linq.Expressions;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Users;

public static class UserLookup
{
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
        Expression<Func<User, bool>> candidateSearch = excludeUserId == null
            ? u => string.Equals(u.Nickname, nickname, StringComparison.OrdinalIgnoreCase)
            : u => u.Id != excludeUserId &&
                   string.Equals(u.Nickname, nickname, StringComparison.OrdinalIgnoreCase);

        return await repos.Users.GetOne(candidateSearch);
    }

    public static async Task<bool> IsNicknameTaken(
        AppRepos repos,
        string nickname,
        Guid? excludeUserId = null)
    {
        return await FindByNickname(repos, nickname, excludeUserId) != null;
    }
}
