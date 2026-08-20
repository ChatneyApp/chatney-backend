using ChatneyBackend.Domains.Users;
using ChatneyBackend.Tests.Support;
using HotChocolate;

namespace ChatneyBackend.Tests.Domains.Users;

public class UserRoleMutationsTests
{
    [Fact]
    public async Task AssignRole_StoresUserRoleAndSendsWebSocketToTargetUser()
    {
        var context = new UserRoleMutationsTestContext();

        var result = await context.Mutations.AssignRole(
            context.Repos,
            context.Resolver,
            context.TargetUser.Id,
            context.AssignedRole.Id,
            context.WebSocket);

        Assert.Equal(context.TargetUser.Id, result.UserId);
        Assert.Equal(context.AssignedRole.Id, result.RoleId);

        var stored = context.UserRolesRepo.Items.Single(userRole => userRole.UserId == context.TargetUser.Id);
        Assert.Equal(context.AssignedRole.Id, stored.RoleId);

        Assert.Single(context.WebSocket.NewUserRoles);
        Assert.Equal(context.TargetUser.Id, context.WebSocket.NewUserRoles[0].UserId);
        Assert.Equal(context.AssignedRole.Id, context.WebSocket.NewUserRoles[0].RoleId);
    }

    [Fact]
    public async Task AssignRole_ThrowsWhenEditUserPermissionMissing()
    {
        var context = new UserRoleMutationsTestContext([]);

        var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
            context.Mutations.AssignRole(
                context.Repos,
                context.Resolver,
                context.TargetUser.Id,
                context.AssignedRole.Id,
                context.WebSocket));

        Assert.Equal(ChatneyBackend.Infra.ErrorCodes.ForbiddenAction, Assert.Single(exception.Errors).Code);
        Assert.DoesNotContain(context.UserRolesRepo.Items, userRole => userRole.UserId == context.TargetUser.Id);
        Assert.Empty(context.WebSocket.NewUserRoles);
    }

    [Fact]
    public async Task AssignRole_IsNoOpWhenAlreadyAssigned()
    {
        var context = new UserRoleMutationsTestContext();
        await context.Mutations.AssignRole(
            context.Repos,
            context.Resolver,
            context.TargetUser.Id,
            context.AssignedRole.Id,
            context.WebSocket);

        var result = await context.Mutations.AssignRole(
            context.Repos,
            context.Resolver,
            context.TargetUser.Id,
            context.AssignedRole.Id,
            context.WebSocket);

        Assert.Equal(context.AssignedRole.Id, result.RoleId);
        Assert.Single(context.UserRolesRepo.Items, userRole => userRole.UserId == context.TargetUser.Id);
        Assert.Single(context.WebSocket.NewUserRoles);
    }

    [Fact]
    public async Task UnassignRole_RemovesUserRoleAndSendsWebSocketToTargetUser()
    {
        var context = new UserRoleMutationsTestContext();
        context.UserRolesRepo.Seed(new UserRole { UserId = context.TargetUser.Id, RoleId = context.AssignedRole.Id });

        var deleted = await context.Mutations.UnassignRole(
            context.Repos,
            context.Resolver,
            context.TargetUser.Id,
            context.AssignedRole.Id,
            context.WebSocket);

        Assert.True(deleted);
        Assert.DoesNotContain(context.UserRolesRepo.Items, userRole => userRole.UserId == context.TargetUser.Id);
        Assert.Single(context.WebSocket.DeletedUserRoles);
        Assert.Equal(context.TargetUser.Id, context.WebSocket.DeletedUserRoles[0].UserId);
        Assert.Equal(context.AssignedRole.Id, context.WebSocket.DeletedUserRoles[0].RoleId);
    }

    [Fact]
    public async Task UnassignRole_ThrowsWhenEditUserPermissionMissing()
    {
        var context = new UserRoleMutationsTestContext([]);
        context.UserRolesRepo.Seed(new UserRole { UserId = context.TargetUser.Id, RoleId = context.AssignedRole.Id });

        var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
            context.Mutations.UnassignRole(
                context.Repos,
                context.Resolver,
                context.TargetUser.Id,
                context.AssignedRole.Id,
                context.WebSocket));

        Assert.Equal(ChatneyBackend.Infra.ErrorCodes.ForbiddenAction, Assert.Single(exception.Errors).Code);
        Assert.Single(context.UserRolesRepo.Items, userRole => userRole.UserId == context.TargetUser.Id);
        Assert.Empty(context.WebSocket.DeletedUserRoles);
    }

    [Fact]
    public async Task UnassignRole_OnNeverAssignedPair_IsNoOpAndDoesNotBroadcast()
    {
        var context = new UserRoleMutationsTestContext();

        var deleted = await context.Mutations.UnassignRole(
            context.Repos,
            context.Resolver,
            context.TargetUser.Id,
            context.AssignedRole.Id,
            context.WebSocket);

        Assert.False(deleted);
        Assert.Empty(context.WebSocket.DeletedUserRoles);
    }
}
