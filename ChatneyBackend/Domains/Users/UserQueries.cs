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
    public string? Nickname { get; set; }
}

public record UserProfile
{
    public required User User { get; init; }

    public string? GlobalRoleName { get; init; }
}

public class UserQueries
{
    [Authorize]
    public async Task<UserProfile> GetMyProfile(
        AppRepos repos,
        ClaimsPrincipal principal)
    {
        var user = await principal.GetRequiredUser(repos);
        var role = await repos.Roles.GetById(user.RoleId);

        return new UserProfile
        {
            User = user,
            GlobalRoleName = role?.Name,
        };
    }

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
    public async Task<User?> GetUserByNickname(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        string nickname)
    {
        var user = await UserLookup.FindByNickname(repos, nickname);
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

        var users = await repos.Users.GetList();

        if (filter.Active != null)
        {
            users = users.Where(u => u.Active == filter.Active).ToList();
        }

        if (filter.Banned != null)
        {
            users = users.Where(u => u.Banned == filter.Banned).ToList();
        }

        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            var email = filter.Email.Trim();
            users = users
                .Where(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(filter.Nickname))
        {
            var nickname = filter.Nickname.Trim();
            users = users
                .Where(u => string.Equals(u.Nickname, nickname, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return users;
    }
}
