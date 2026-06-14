using System.Security.Claims;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

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
    public async Task<User?> GetUserById(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        Guid id)
    {
        var currentUserId = principal.GetUserGuid();
        if (currentUserId == id)
        {
            return await repos.Users.GetById(id);
        }

        var currentUser = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(currentUser, RoleScope.Global());
        permissions.Require(UserPermissionNames.ReadUser);

        return await repos.Users.GetById(id);
    }

    [Authorize]
    public async Task<User?> GetUserByName(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        string name)
    {
        var user = await repos.Users.GetOne(u => u.Name == name);
        if (user == null)
        {
            return null;
        }

        var currentUserId = principal.GetUserGuid();
        if (currentUserId == user.Id)
        {
            return user;
        }

        var currentUser = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(currentUser, RoleScope.Global());
        permissions.Require(UserPermissionNames.ReadUser);

        return user;
    }

    [Authorize]
    public async Task<List<User>> GetList(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        UserFilter filter)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(UserPermissionNames.ReadUser);

        return await repos.Users.GetList(u =>
            (filter.Active == null || u.Active == filter.Active) &&
            (filter.Banned == null || u.Banned == filter.Banned) &&
            (filter.Email == null || u.Email == filter.Email) &&
            (filter.Name == null || u.Name == filter.Name));
    }
}
