using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Tests.Support;
using HotChocolate;

namespace ChatneyBackend.Tests.Domains.Roles;

public class AclMutationsTests
{
    private static AclMutations Mutations => new();

    [Fact]
    public async Task SetRoleAcl_UpsertsAndInvalidatesAndBroadcasts()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.RoleEditRole);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        var result = await Mutations.SetRoleAcl(
            ctx.Repos, resolver, webSocket, ctx.RoleB.Id, PermissionResolverTestContext.W1SecObjId,
            [Permission.WorkspaceReadWorkspace]);

        Assert.NotNull(result);
        Assert.Equal([Permission.WorkspaceReadWorkspace], result.Permissions);
        var stored = ctx.RoleAclsRepo.Items.Single(acl =>
            acl.RoleId == ctx.RoleB.Id && acl.SecObjId == PermissionResolverTestContext.W1SecObjId);
        Assert.Equal([Permission.WorkspaceReadWorkspace], stored.Permissions);
        Assert.Single(webSocket.ChangedRoleAcls);
    }

    [Fact]
    public async Task SetRoleAcl_ThrowsWhenActorLacksRoleEditRole()
    {
        var ctx = new PermissionResolverTestContext();
        var resolver = ctx.CreateResolver(ctx.UserWithNoRoles);
        var webSocket = new RecordingWebSocketConnector();

        await Assert.ThrowsAsync<GraphQLException>(() => Mutations.SetRoleAcl(
            ctx.Repos, resolver, webSocket, ctx.RoleB.Id, PermissionResolverTestContext.W1SecObjId,
            [Permission.WorkspaceReadWorkspace]));
    }

    /// <summary>
    /// S8 symmetry guard: an empty permission list must delete the row, exactly like SetUserAcl,
    /// rather than leaving a dead empty-array row that would surface in roleAcls/objectAcls.
    /// </summary>
    [Fact]
    public async Task SetRoleAcl_WithEmptyPermissions_DeletesRowInsteadOfStoringEmptyArray()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.RoleEditRole);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        var result = await Mutations.SetRoleAcl(
            ctx.Repos, resolver, webSocket, ctx.RoleB.Id, PermissionResolverTestContext.W1SecObjId, []);

        Assert.Null(result);
        Assert.DoesNotContain(ctx.RoleAclsRepo.Items, acl =>
            acl.RoleId == ctx.RoleB.Id && acl.SecObjId == PermissionResolverTestContext.W1SecObjId);
        Assert.Single(webSocket.DeletedRoleAcls);
    }

    [Fact]
    public async Task DeleteRoleAcl_RemovesRowAndBroadcasts()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.RoleEditRole);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        var deleted = await Mutations.DeleteRoleAcl(
            ctx.Repos, resolver, webSocket, ctx.RoleB.Id, PermissionResolverTestContext.W1SecObjId);

        Assert.True(deleted);
        Assert.DoesNotContain(ctx.RoleAclsRepo.Items, acl =>
            acl.RoleId == ctx.RoleB.Id && acl.SecObjId == PermissionResolverTestContext.W1SecObjId);
        Assert.Single(webSocket.DeletedRoleAcls);
    }

    [Fact]
    public async Task SetUserAcl_WithPermissions_UpsertsRow()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserEditUser);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        var result = await Mutations.SetUserAcl(
            ctx.Repos, resolver, webSocket, ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId,
            [Permission.ChannelReadMessage]);

        Assert.NotNull(result);
        var stored = ctx.UserAclsRepo.Items.Single(acl =>
            acl.UserId == ctx.UserWithRoleB.Id && acl.SecObjId == PermissionResolverTestContext.C1SecObjId);
        Assert.Equal([Permission.ChannelReadMessage], stored.Permissions);
        Assert.Single(webSocket.ChangedUserAcls);
    }

    /// <summary>
    /// A present-but-empty user_acls row would still satisfy D1's "any user_acls row in the chain"
    /// switch and silently suppress every role permission for that chain - so an empty permission
    /// list must delete the row, never store it.
    /// </summary>
    [Fact]
    public async Task SetUserAcl_WithEmptyPermissions_DeletesRowInsteadOfStoringEmptyArray()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserEditUser);
        ctx.GrantUserAcl(ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        var result = await Mutations.SetUserAcl(
            ctx.Repos, resolver, webSocket, ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId, []);

        Assert.Null(result);
        Assert.DoesNotContain(ctx.UserAclsRepo.Items, acl =>
            acl.UserId == ctx.UserWithRoleB.Id && acl.SecObjId == PermissionResolverTestContext.C1SecObjId);
        Assert.Single(webSocket.DeletedUserAcls);
    }

    [Fact]
    public async Task SetUserAclForWorkspace_ResolvesSecObjIdFromWorkspaceId()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserEditUser);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetUserAclForWorkspace(
            ctx.Repos, resolver, webSocket, ctx.UserWithRoleB.Id, ctx.W1.Id, [Permission.WorkspaceReadWorkspace]);

        var stored = ctx.UserAclsRepo.Items.Single(acl => acl.UserId == ctx.UserWithRoleB.Id);
        Assert.Equal(PermissionResolverTestContext.W1SecObjId, stored.SecObjId);
    }

    [Fact]
    public async Task SetUserAcl_ThrowsWhenActorLacksEditUserPermission()
    {
        var ctx = new PermissionResolverTestContext();
        var resolver = ctx.CreateResolver(ctx.UserWithNoRoles);
        var webSocket = new RecordingWebSocketConnector();

        await Assert.ThrowsAsync<GraphQLException>(() => Mutations.SetUserAcl(
            ctx.Repos, resolver, webSocket, ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId,
            [Permission.ChannelReadMessage]));

        Assert.Empty(ctx.UserAclsRepo.Items);
    }

    [Fact]
    public async Task DeleteUserAcl_ThrowsWhenActorLacksEditUserPermission()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantUserAcl(ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        var resolver = ctx.CreateResolver(ctx.UserWithNoRoles);
        var webSocket = new RecordingWebSocketConnector();

        await Assert.ThrowsAsync<GraphQLException>(() => Mutations.DeleteUserAcl(
            ctx.Repos, resolver, webSocket, ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId));

        Assert.Single(ctx.UserAclsRepo.Items);
    }

    [Fact]
    public async Task DeleteUserAcl_RemovesRowAndBroadcasts()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserEditUser);
        ctx.GrantUserAcl(ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        var deleted = await Mutations.DeleteUserAcl(
            ctx.Repos, resolver, webSocket, ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId);

        Assert.True(deleted);
        Assert.Empty(ctx.UserAclsRepo.Items);
        Assert.Single(webSocket.DeletedUserAcls);
    }

    [Fact]
    public async Task SetRoleAcl_UpsertOverExistingCompositeKeyRow_ReplacesInsteadOfDuplicating()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.RoleEditRole);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetRoleAcl(
            ctx.Repos, resolver, webSocket, ctx.RoleB.Id, PermissionResolverTestContext.W1SecObjId,
            [Permission.WorkspaceUpdateWorkspace]);

        var matching = ctx.RoleAclsRepo.Items
            .Where(acl => acl.RoleId == ctx.RoleB.Id && acl.SecObjId == PermissionResolverTestContext.W1SecObjId)
            .ToList();
        var stored = Assert.Single(matching);
        Assert.Equal([Permission.WorkspaceUpdateWorkspace], stored.Permissions);
    }

    [Fact]
    public async Task SetUserAcl_UpsertOverExistingCompositeKeyRow_ReplacesInsteadOfDuplicating()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserEditUser);
        ctx.GrantUserAcl(ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetUserAcl(
            ctx.Repos, resolver, webSocket, ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId,
            [Permission.ChannelCreateMessage]);

        var matching = ctx.UserAclsRepo.Items
            .Where(acl => acl.UserId == ctx.UserWithRoleB.Id && acl.SecObjId == PermissionResolverTestContext.C1SecObjId)
            .ToList();
        var stored = Assert.Single(matching);
        Assert.Equal([Permission.ChannelCreateMessage], stored.Permissions);
    }

    [Fact]
    public async Task SetRoleAclForWorkspace_ResolvesSecObjIdFromWorkspaceId()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.RoleEditRole);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetRoleAclForWorkspace(
            ctx.Repos, resolver, webSocket, ctx.RoleB.Id, ctx.W1.Id, [Permission.WorkspaceReadWorkspace]);

        var stored = ctx.RoleAclsRepo.Items.Single(acl => acl.RoleId == ctx.RoleB.Id);
        Assert.Equal(PermissionResolverTestContext.W1SecObjId, stored.SecObjId);
    }

    [Fact]
    public async Task SetRoleAclForChannelType_ResolvesSecObjIdFromChannelTypeId()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.RoleEditRole);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetRoleAclForChannelType(
            ctx.Repos, resolver, webSocket, ctx.RoleB.Id, ctx.CtPublic.Id, [Permission.ChannelReadMessage]);

        var stored = ctx.RoleAclsRepo.Items.Single(acl => acl.RoleId == ctx.RoleB.Id);
        Assert.Equal(PermissionResolverTestContext.CtPublicSecObjId, stored.SecObjId);
    }

    [Fact]
    public async Task SetRoleAclForChannel_ResolvesSecObjIdFromChannelId()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.RoleEditRole);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetRoleAclForChannel(
            ctx.Repos, resolver, webSocket, ctx.RoleB.Id, ctx.C1.Id, [Permission.ChannelReadMessage]);

        var stored = ctx.RoleAclsRepo.Items.Single(acl => acl.RoleId == ctx.RoleB.Id);
        Assert.Equal(PermissionResolverTestContext.C1SecObjId, stored.SecObjId);
    }

    [Fact]
    public async Task SetRoleAclForGlobal_ResolvesToGlobalSecObjId()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.RoleEditRole);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetRoleAclForGlobal(ctx.Repos, resolver, webSocket, ctx.RoleB.Id, [Permission.UserReadUser]);

        var stored = ctx.RoleAclsRepo.Items.Single(acl => acl.RoleId == ctx.RoleB.Id);
        Assert.Equal(PermissionResolverTestContext.GlobalSecObjId, stored.SecObjId);
    }

    [Fact]
    public async Task SetUserAclForChannelType_ResolvesSecObjIdFromChannelTypeId()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserEditUser);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetUserAclForChannelType(
            ctx.Repos, resolver, webSocket, ctx.UserWithRoleB.Id, ctx.CtPublic.Id, [Permission.ChannelReadMessage]);

        var stored = ctx.UserAclsRepo.Items.Single(acl => acl.UserId == ctx.UserWithRoleB.Id);
        Assert.Equal(PermissionResolverTestContext.CtPublicSecObjId, stored.SecObjId);
    }

    [Fact]
    public async Task SetUserAclForChannel_ResolvesSecObjIdFromChannelId()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserEditUser);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetUserAclForChannel(
            ctx.Repos, resolver, webSocket, ctx.UserWithRoleB.Id, ctx.C1.Id, [Permission.ChannelReadMessage]);

        var stored = ctx.UserAclsRepo.Items.Single(acl => acl.UserId == ctx.UserWithRoleB.Id);
        Assert.Equal(PermissionResolverTestContext.C1SecObjId, stored.SecObjId);
    }

    [Fact]
    public async Task SetUserAclForGlobal_ResolvesToGlobalSecObjId()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserEditUser);
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetUserAclForGlobal(ctx.Repos, resolver, webSocket, ctx.UserWithRoleB.Id, [Permission.UserReadUser]);

        var stored = ctx.UserAclsRepo.Items.Single(acl => acl.UserId == ctx.UserWithRoleB.Id);
        Assert.Equal(PermissionResolverTestContext.GlobalSecObjId, stored.SecObjId);
    }

    /// <summary>
    /// CRITICAL regression: a present-but-empty user_acls row would satisfy D1's per-check
    /// "chain.Any(userAcls.ContainsKey)" switch and silently suppress ALL of that user's
    /// role-derived permissions for that chain. Assert BOTH that the row is gone AND that the
    /// user's role permissions for the chain still resolve after the empty-list call.
    /// </summary>
    [Fact]
    public async Task SetUserAcl_EmptyPermissions_RowGone_AndRolePermissionsStillResolveForChain()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserEditUser);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantUserAcl(ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);
        var actingResolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.SetUserAcl(
            ctx.Repos, actingResolver, webSocket, ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId, []);

        Assert.DoesNotContain(ctx.UserAclsRepo.Items, acl =>
            acl.UserId == ctx.UserWithRoleB.Id && acl.SecObjId == PermissionResolverTestContext.C1SecObjId);

        var targetResolver = ctx.CreateResolver(ctx.UserWithRoleB);
        var permissions = await targetResolver.ForChannel(ctx.C1);
        Assert.True(permissions.Can(Permission.ChannelReadMessage));
    }

    [Fact]
    public async Task RoleAcls_ReturnsRowsForRole_AndRequiresRoleEditRole()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.RoleEditRole);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleC.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);
        var queries = new AclQueries();

        var allowedResolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var rows = await queries.RoleAcls(ctx.Repos, allowedResolver, ctx.RoleB.Id);

        Assert.Equal(2, rows.Count);
        Assert.All(rows, acl => Assert.Equal(ctx.RoleB.Id, acl.RoleId));

        var deniedResolver = ctx.CreateResolver(ctx.UserWithNoRoles);
        await Assert.ThrowsAsync<GraphQLException>(() => queries.RoleAcls(ctx.Repos, deniedResolver, ctx.RoleB.Id));
    }

    [Fact]
    public async Task UserAcls_ReturnsRowsForUser_AndRequiresUserEditUser()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserEditUser);
        ctx.GrantUserAcl(ctx.UserWithRoleB.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        ctx.GrantUserAcl(ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantUserAcl(ctx.UserWithRoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);
        var queries = new AclQueries();

        var allowedResolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var rows = await queries.UserAcls(ctx.Repos, allowedResolver, ctx.UserWithRoleB.Id);

        Assert.Equal(2, rows.Count);
        Assert.All(rows, acl => Assert.Equal(ctx.UserWithRoleB.Id, acl.UserId));

        var deniedResolver = ctx.CreateResolver(ctx.UserWithNoRoles);
        await Assert.ThrowsAsync<GraphQLException>(() => queries.UserAcls(ctx.Repos, deniedResolver, ctx.UserWithRoleB.Id));
    }

    [Fact]
    public async Task ObjectAcls_ReturnsRoleAndUserRowsForSecObj_WhenActorHasBothPermissions()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(
            ctx.RoleA.Id,
            PermissionResolverTestContext.GlobalSecObjId,
            Permission.RoleEditRole,
            Permission.UserEditUser);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantUserAcl(ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);
        ctx.GrantRoleAcl(ctx.RoleC.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        var queries = new AclQueries();

        var allowedResolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var result = await queries.ObjectAcls(ctx.Repos, allowedResolver, PermissionResolverTestContext.C1SecObjId);

        var roleAcl = Assert.Single(result.RoleAcls);
        Assert.Equal(ctx.RoleB.Id, roleAcl.RoleId);
        var userAcl = Assert.Single(result.UserAcls);
        Assert.Equal(ctx.UserWithRoleB.Id, userAcl.UserId);

        var deniedResolver = ctx.CreateResolver(ctx.UserWithNoRoles);
        await Assert.ThrowsAsync<GraphQLException>(() =>
            queries.ObjectAcls(ctx.Repos, deniedResolver, PermissionResolverTestContext.C1SecObjId));
    }

    /// <summary>
    /// S1 regression guard: an actor with ONLY role.editRole (not user.editUser) must get role_acls
    /// rows but NO user_acls rows - otherwise a role-admin who is deliberately not a user-admin could
    /// enumerate every user's per-object grants through this endpoint.
    /// </summary>
    [Fact]
    public async Task ObjectAcls_WithOnlyRoleEditRole_ReturnsRoleRowsAndNoUserRows()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.RoleEditRole);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantUserAcl(ctx.UserWithRoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);
        var queries = new AclQueries();

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var result = await queries.ObjectAcls(ctx.Repos, resolver, PermissionResolverTestContext.C1SecObjId);

        var roleAcl = Assert.Single(result.RoleAcls);
        Assert.Equal(ctx.RoleB.Id, roleAcl.RoleId);
        Assert.Empty(result.UserAcls);
    }
}
