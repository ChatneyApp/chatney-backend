using ChatneyBackend.Domains.Users;
using ChatneyBackend.Tests.Support;

namespace ChatneyBackend.Tests.Domains.Users;

public class UserQueriesTests
{
    [Fact]
    public async Task GetList_ReturnsAllUsers_WhenFilterIsEmpty()
    {
        var context = new UserRoleMutationsTestContext([UserPermissionNames.ReadUser]);
        var queries = new UserQueries();

        var users = await queries.GetList(
            context.Repos,
            context.RoleManager,
            context.Principal,
            new UserFilter());

        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u.Id == context.Admin.Id);
        Assert.Contains(users, u => u.Id == context.TargetUser.Id);
    }

    [Fact]
    public async Task GetList_FiltersByActive()
    {
        var context = new UserRoleMutationsTestContext([UserPermissionNames.ReadUser]);
        context.TargetUser.Active = false;
        await context.UsersRepo.UpdateOne(context.TargetUser);

        var queries = new UserQueries();
        var users = await queries.GetList(
            context.Repos,
            context.RoleManager,
            context.Principal,
            new UserFilter { Active = true });

        Assert.Single(users);
        Assert.Equal(context.Admin.Id, users[0].Id);
    }
}
