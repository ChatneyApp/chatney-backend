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

    /// <summary>All users matching the filter. Requires UserReadUser. roleIds is filled only if the caller also has UserEditUser.</summary>
    [Authorize]
    public async Task<List<User>> GetList(
        AppRepos repos,
        IPermissionResolver resolver,
        UserFilter filter)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.UserReadUser);

        var users = await repos.Users.GetList(
            """
            SELECT *
            FROM users
            WHERE (NOT @HasActive OR active = @Active)
              AND (NOT @HasBanned OR banned = @Banned)
              AND (NOT @HasEmail OR LOWER(email) = LOWER(@Email))
              AND (NOT @HasNickname OR LOWER(nickname) = LOWER(@Nickname));
            """,
            new
            {
                HasActive = filter.Active.HasValue,
                Active = filter.Active ?? false,
                HasBanned = filter.Banned.HasValue,
                Banned = filter.Banned ?? false,
                HasEmail = !string.IsNullOrWhiteSpace(filter.Email),
                Email = filter.Email?.Trim() ?? string.Empty,
                HasNickname = !string.IsNullOrWhiteSpace(filter.Nickname),
                Nickname = filter.Nickname?.Trim() ?? string.Empty,
            });

        if (users.Count == 0 || !permissions.Can(Permission.UserEditUser))
        {
            return users;
        }

        var userIds = users.Select(user => user.Id).ToList();
        var userRoles = await repos.UserRoles.GetList(userRole => userIds.Contains(userRole.UserId));
        var roleIdsByUser = userRoles.ToLookup(userRole => userRole.UserId, userRole => userRole.RoleId);

        foreach (var user in users)
        {
            user.RoleIds = roleIdsByUser[user.Id].ToList();
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
        var users = await repos.Users.GetList(
            """
            SELECT *
            FROM users
            WHERE id != @ActorId
              AND active
              AND NOT banned
              AND LEFT(LOWER(nickname), LENGTH(@Prefix)) = LOWER(@Prefix)
            ORDER BY LOWER(nickname)
            LIMIT 20;
            """,
            new { ActorId = actorId, Prefix = trimmed });

        return users
            .Select(user => new DirectMessageUser
            {
                Id = user.Id,
                Nickname = user.Nickname,
                AvatarUrl = user.AvatarUrl,
            })
            .ToList();
    }
}
