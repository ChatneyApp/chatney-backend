using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Users;

/// <summary>Optional filters for users.list; null fields are ignored.</summary>
public record UserFilter
{
    public bool? Active { get; set; }
    public bool? Banned { get; set; }
    /// <summary>Exact email match, case-insensitive.</summary>
    public string? Email { get; set; }
    /// <summary>Exact nickname match, case-insensitive.</summary>
    public string? Nickname { get; set; }
}

/// <summary>The current user together with the names of their roles.</summary>
public record UserProfile
{
    public required User User { get; init; }

    public required IReadOnlyList<string> RoleNames { get; init; }
}

public class UserQueries
{
    /// <summary>The current user's profile and role names.</summary>
    [Authorize]
    public async Task<UserProfile> GetMyProfile(
        AppRepos repos,
        ClaimsPrincipal principal)
    {
        var user = await principal.GetRequiredUser(repos);
        var userRoles = await repos.UserRoles.GetList(userRole => userRole.UserId == user.Id);
        var roleIds = userRoles.Select(userRole => userRole.RoleId).ToList();

        // Matches the same "skip the query for an empty collection" guard AclSnapshotLoader uses -
        // Contains on an empty roleIds is fail-closed regardless, but this saves a query too.
        if (roleIds.Count == 0)
        {
            return new UserProfile { User = user, RoleNames = [] };
        }

        var roles = await repos.Roles.GetList(role => roleIds.Contains(role.Id));

        return new UserProfile
        {
            User = user,
            RoleNames = roles.Select(role => role.Name).ToList(),
        };
    }

    /// <summary>
    /// Everything the current user can do: global permissions plus effective permissions per workspace,
    /// channel type and channel. Use it to show or hide UI actions.
    /// </summary>
    [Authorize]
    public async Task<MyPermissions> MyPermissions(IPermissionResolver resolver) =>
        await resolver.ResolveAll();

    /// <summary>A user by id, or null. Requires UserReadUser unless it's the current user.</summary>
    [Authorize]
    public async Task<User?> GetUserById(
        AppRepos repos,
        IPermissionResolver resolver,
        ClaimsPrincipal principal,
        Guid id)
    {
        var currentUserId = principal.GetUserGuid();
        if (currentUserId == id)
        {
            return await repos.Users.GetById(id);
        }

        var permissions = await resolver.Global();
        permissions.Require(Permission.UserReadUser);

        return await repos.Users.GetById(id);
    }

    /// <summary>A user by nickname (case-insensitive), or null. Requires UserReadUser unless it's the current user.</summary>
    [Authorize]
    public async Task<User?> GetUserByNickname(
        AppRepos repos,
        IPermissionResolver resolver,
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

        var permissions = await resolver.Global();
        permissions.Require(Permission.UserReadUser);

        return user;
    }

    /// <summary>All users matching the filter. Requires UserReadUser.</summary>
    [Authorize]
    public async Task<List<User>> GetList(
        AppRepos repos,
        IPermissionResolver resolver,
        UserFilter filter)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.UserReadUser);

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

    /// <summary>Nickname autocomplete for starting direct messages: up to 20 active, non-banned users, excluding yourself.</summary>
    /// <param name="prefix">Case-insensitive nickname prefix. Blank returns an empty list.</param>
    [Authorize]
    public async Task<List<DirectMessageUser>> SearchByNickname(
        AppRepos repos,
        ClaimsPrincipal principal,
        string prefix)
    {
        var trimmed = prefix.Trim();
        if (trimmed.Length == 0)
        {
            return [];
        }

        var actorId = principal.GetUserGuid();
        var users = await repos.Users.GetList();
        const int limit = 20;

        return users
            .Where(user =>
                user.Id != actorId &&
                user.Active &&
                !user.Banned &&
                user.Nickname.StartsWith(trimmed, StringComparison.OrdinalIgnoreCase))
            .OrderBy(user => user.Nickname, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .Select(user => new DirectMessageUser
            {
                Id = user.Id,
                Nickname = user.Nickname,
                AvatarUrl = user.AvatarUrl,
            })
            .ToList();
    }
}
