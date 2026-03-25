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
    public async Task<User?> GetUserById(PgRepo<User, Guid> repo, Guid id) => await repo.GetById(id);

    [Authorize]
    public async Task<User?> GetUserByName(PgRepo<User, Guid> repo, string name) => await repo.GetOne(u => u.Name == name);

    [Authorize]
    public async Task<List<User>> GetList(PgRepo<User, Guid> repo, UserFilter filter)
    {
        return await repo.GetList(u =>
            (filter.Active == null || u.Active == filter.Active) &&
            (filter.Banned == null || u.Banned == filter.Banned) &&
            (filter.Email == null || u.Email == filter.Email) &&
            (filter.Name == null || u.Name == filter.Name));
    }
}
