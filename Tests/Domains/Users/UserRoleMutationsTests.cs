using ChatneyBackend.Domains.Users;
using ChatneyBackend.Tests.Support;
using HotChocolate;

namespace ChatneyBackend.Tests.Domains.Users;

public class UserRoleMutationsTests
{
    [Fact]
    public async Task AddUserRole_StoresUserRoleAndSendsWebSocketToTargetUser()
    {
        var context = new UserRoleMutationsTestContext();
        var userRole = context.CreateUserRole();

        var result = await context.Mutations.AddUserRole(
            context.Repos,
            context.RoleManager,
            context.Principal,
            userRole,
            context.WebSocket);

        Assert.Equal(context.TargetUser.Id, result.UserId);
        Assert.Equal(context.AssignedRole.Id, result.RoleId);
        Assert.Equal(context.Workspace.Id, result.WorkspaceId);
        Assert.Single(context.UserRolesRepo.Items);

        var stored = context.UserRolesRepo.Items.Single();
        Assert.Equal(context.TargetUser.Id, stored.UserId);
        Assert.Equal(context.AssignedRole.Id, stored.RoleId);

        Assert.Single(context.WebSocket.NewUserRoles);
        Assert.Empty(context.WebSocket.UpdatedUserRoles);
        Assert.Equal(context.TargetUser.Id, context.WebSocket.NewUserRoles[0].UserId);
        Assert.Equal(context.AssignedRole.Id, context.WebSocket.NewUserRoles[0].RoleId);
    }

    [Fact]
    public async Task AddUserRole_ThrowsWhenEditUserPermissionMissing()
    {
        var context = new UserRoleMutationsTestContext([]);
        var userRole = context.CreateUserRole();

        var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
            context.Mutations.AddUserRole(
                context.Repos,
                context.RoleManager,
                context.Principal,
                userRole,
                context.WebSocket));

        Assert.Equal(ChatneyBackend.Infra.ErrorCodes.ForbiddenAction, exception.Message);
        Assert.Empty(context.UserRolesRepo.Items);
        Assert.Empty(context.WebSocket.NewUserRoles);
    }

    [Fact]
    public async Task UpdateUserRole_UpdatesStoredUserRoleAndSendsWebSocketToTargetUser()
    {
        var context = new UserRoleMutationsTestContext();
        var userRole = context.CreateUserRole();
        context.UserRolesRepo.Seed(userRole);

        userRole.RoleId = context.AdminRole.Id;
        userRole.Allowlist = ["channel.readChannel"];

        var result = await context.Mutations.UpdateUserRole(
            context.Repos,
            context.RoleManager,
            context.Principal,
            userRole,
            context.WebSocket);

        Assert.Equal(context.AdminRole.Id, result.RoleId);
        Assert.Equal(["channel.readChannel"], result.Allowlist);

        var stored = context.UserRolesRepo.Items.Single();
        Assert.Equal(context.AdminRole.Id, stored.RoleId);
        Assert.Equal(["channel.readChannel"], stored.Allowlist);

        Assert.Empty(context.WebSocket.NewUserRoles);
        Assert.Single(context.WebSocket.UpdatedUserRoles);
        Assert.Equal(context.TargetUser.Id, context.WebSocket.UpdatedUserRoles[0].UserId);
        Assert.Equal(context.AdminRole.Id, context.WebSocket.UpdatedUserRoles[0].RoleId);
    }

    [Fact]
    public async Task DeleteUserRole_RemovesUserRoleAndSendsWebSocketToTargetUser()
    {
        var context = new UserRoleMutationsTestContext();
        var userRole = context.CreateUserRole();
        context.UserRolesRepo.Seed(userRole);

        var key = new UserRoleKey(
            userRole.UserId,
            userRole.ChannelId,
            userRole.ChannelTypeId,
            userRole.WorkspaceId);

        var deleted = await context.Mutations.DeleteUserRole(
            context.Repos,
            context.RoleManager,
            context.Principal,
            key,
            context.WebSocket);

        Assert.True(deleted);
        Assert.Empty(context.UserRolesRepo.Items);
        Assert.Single(context.WebSocket.DeletedUserRoles);
        Assert.Equal(context.TargetUser.Id, context.WebSocket.DeletedUserRoles[0].UserId);
        Assert.Equal(context.Workspace.Id, context.WebSocket.DeletedUserRoles[0].WorkspaceId);
    }
}
