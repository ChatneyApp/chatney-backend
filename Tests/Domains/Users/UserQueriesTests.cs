using System.Security.Claims;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Tests.Support;

namespace ChatneyBackend.Tests.Domains.Users;

public class UserQueriesTests
{
    [Fact]
    public async Task GetList_ReturnsAllUsers_WhenFilterIsEmpty()
    {
        var context = new UserRoleMutationsTestContext([Permission.UserReadUser]);
        var queries = new UserQueries();

        var users = await queries.GetList(
            context.Repos,
            context.Resolver,
            new UserFilter());

        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u.Id == context.Admin.Id);
        Assert.Contains(users, u => u.Id == context.TargetUser.Id);
    }

    [Fact]
    public async Task GetList_FiltersByActive()
    {
        var context = new UserRoleMutationsTestContext([Permission.UserReadUser]);
        context.TargetUser.Active = false;
        await context.UsersRepo.UpdateOne(context.TargetUser);

        var queries = new UserQueries();
        var users = await queries.GetList(
            context.Repos,
            context.Resolver,
            new UserFilter { Active = true });

        Assert.Single(users);
        Assert.Equal(context.Admin.Id, users[0].Id);
    }

    [Fact]
    public async Task GetMyProfile_MultiRoleUser_ReturnsAllRoleNames()
    {
        var context = new UserRoleMutationsTestContext();
        var extraRole = new Role { Id = 5, Name = "extra", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.RolesRepo.Seed(extraRole);
        context.UserRolesRepo.Seed(new UserRole { UserId = context.Admin.Id, RoleId = extraRole.Id });
        var queries = new UserQueries();

        var profile = await queries.GetMyProfile(context.Repos, context.Principal);

        Assert.Equal(
            new HashSet<string> { context.AdminRole.Name, extraRole.Name },
            profile.RoleNames.ToHashSet());
    }

    [Fact]
    public async Task GetMyProfile_RoleLessUser_ReturnsEmptyListNotNull()
    {
        var context = new UserRoleMutationsTestContext();
        var roleLessPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Sid, context.TargetUser.Id.ToString()),
            new Claim(ClaimTypes.Email, context.TargetUser.Email),
        ],
        "TestAuth"));
        var queries = new UserQueries();

        var profile = await queries.GetMyProfile(context.Repos, roleLessPrincipal);

        Assert.NotNull(profile.RoleNames);
        Assert.Empty(profile.RoleNames);
    }

    [Fact]
    public async Task SearchByNickname_ReturnsPublicMatches_WithoutUserReadUser()
    {
        var context = new UserRoleMutationsTestContext([]);
        var queries = new UserQueries();

        var results = await queries.SearchByNickname(context.Repos, context.Principal, "tar");

        Assert.Single(results);
        Assert.Equal(context.TargetUser.Id, results[0].Id);
        Assert.Equal(context.TargetUser.Nickname, results[0].Nickname);
    }

    [Fact]
    public async Task SearchByNickname_ExcludesSelfAndEmptyPrefix()
    {
        var context = new UserRoleMutationsTestContext([]);
        var queries = new UserQueries();

        var selfMatches = await queries.SearchByNickname(context.Repos, context.Principal, "admin");
        var empty = await queries.SearchByNickname(context.Repos, context.Principal, "   ");

        Assert.Empty(selfMatches);
        Assert.Empty(empty);
    }
}
