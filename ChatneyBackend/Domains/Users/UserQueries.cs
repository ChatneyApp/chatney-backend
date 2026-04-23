using HotChocolate.Authorization;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Users;

public record UserFilter
{
    public bool? Active { get; set; }
    public bool? Banned { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}

public class UserQueries
{
    [Authorize]
    public async Task<User?> GetUserById(AppRepos repos, Guid id) => await repos.Users.GetById(id);

    [Authorize]
    public async Task<User?> GetUserByName(AppRepos repos, string name) => await repos.Users.GetOne(u => u.Name == name);

    [Authorize]
    public async Task<List<User>> GetList(AppRepos repos, UserFilter filter) =>
        await repos.Users.GetList(u =>
            (filter.Active == null || u.Active == filter.Active) &&
            (filter.Banned == null || u.Banned == filter.Banned) &&
            (filter.Email == null || u.Email == filter.Email) &&
            (filter.Name == null || u.Name == filter.Name));
}
