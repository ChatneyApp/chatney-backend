using HotChocolate.Authorization;
using MongoDB.Driver;

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
    public async Task<UserResponse?> GetUserById(Repo<User> repo, string id) => (await repo.GetById(id))?.ToResponse();

    [Authorize]
    public async Task<UserResponse?> GetUserByName(Repo<User> repo, string name) => (await repo.GetOne(u => u.Name == name))?.ToResponse();

    [Authorize]
    public async Task<List<UserResponse>> GetList(Repo<User> repo, UserFilter filter)
    {
        var f = Builders<User>.Filter.Empty;
        if (filter.Active != null)
        {
            f &= Builders<User>.Filter.Eq(u => u.Active, filter.Active);
        }
        if (filter.Banned != null)
        {
            f &= Builders<User>.Filter.Eq(u => u.Banned, filter.Banned);
        }
        if (filter.Email != null)
        {
            f &= Builders<User>.Filter.Eq(u => u.Email, filter.Email);
        }
        if (filter.Name != null)
        {
            f &= Builders<User>.Filter.Eq(u => u.Name, filter.Name);
        }

        var preList = await repo.GetList(f);
        return preList
            .Select(u => u.ToResponse())
            .ToList();
    }
}
