using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Tests.Support;
using HotChocolate;
using ErrorCodes = ChatneyBackend.Infra.ErrorCodes;

namespace ChatneyBackend.Tests.Domains.Roles;

public class PermissionResolverD1SwitchTests
{
    [Fact]
    public async Task ForChannel_NoUserAcls_UsesRoleAcls()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.True(permissions.Can(Permission.ChannelReadMessage));
    }

    [Fact]
    public async Task ForChannel_UserAclInChain_SuppressesRoleAclsForThatCheck()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantUserAcl(ctx.UserWithRoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        // The user ACL is present somewhere in C1's chain, so role ACLs are ignored entirely -
        // ChannelReadMessage (granted only via role) must NOT leak through.
        Assert.True(permissions.Can(Permission.ChannelCreateMessage));
        Assert.False(permissions.Can(Permission.ChannelReadMessage));
    }

    [Fact]
    public async Task ForChannel_UserAclOnUnrelatedWorkspace_DoesNotSuppressRoleAclsForThisChannel()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        // D1 is evaluated per-check, over the CURRENT chain only: a user ACL on W2 must not
        // suppress role ACLs when checking a channel that lives in W1.
        ctx.GrantUserAcl(ctx.UserWithRoleA.Id, PermissionResolverTestContext.W2SecObjId, Permission.WorkspaceReadWorkspace);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.True(permissions.Can(Permission.ChannelReadMessage));
    }

    [Fact]
    public async Task ForChannel_UserAclAtWorkspaceLevel_SuppressesRoleAclsForEveryChannelInThatWorkspace()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        // The surprising, high-blast-radius consequence of D1: a single user ACL at the WORKSPACE
        // level is present in the chain of every channel in that workspace, so it suppresses role
        // grants for all of them - not just the channel it was granted on.
        ctx.GrantUserAcl(ctx.UserWithRoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.True(permissions.Can(Permission.WorkspaceReadWorkspace));
        Assert.False(permissions.Can(Permission.ChannelReadMessage));
    }

    [Fact]
    public async Task Global_UserAclOnGlobalSecObj_SuppressesRoleAclsOnGlobalSecObj()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserReadUser);
        ctx.GrantUserAcl(ctx.UserWithRoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.Global();

        Assert.True(permissions.Can(Permission.ChannelReadMessage));
        Assert.False(permissions.Can(Permission.UserReadUser));
    }
}

public class PermissionResolverAdditiveUnionTests
{
    [Fact]
    public async Task WorkspaceGrant_AppliesToChannelsBeneath()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.True(permissions.Can(Permission.ChannelReadMessage));
    }

    [Fact]
    public async Task ChannelTypeGrant_AppliesToChannelsOfThatTypeOnly()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.CtPublicSecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        Assert.True((await resolver.ForChannel(ctx.C1)).Can(Permission.ChannelReadMessage));
        Assert.True((await resolver.ForChannel(ctx.C2)).Can(Permission.ChannelReadMessage));
        Assert.False((await resolver.ForChannel(ctx.C3)).Can(Permission.ChannelReadMessage));
        Assert.False((await resolver.ForChannel(ctx.C4)).Can(Permission.ChannelReadMessage));
    }

    [Fact]
    public async Task ChannelGrant_AppliesToThatChannelOnly()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        Assert.True((await resolver.ForChannel(ctx.C1)).Can(Permission.ChannelReadMessage));
        Assert.False((await resolver.ForChannel(ctx.C2)).Can(Permission.ChannelReadMessage));
    }

    [Fact]
    public async Task GrantsAtAllThreeLevels_Union()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.CtPublicSecObjId, Permission.ChannelReadChannel);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.True(permissions.Can(
            Permission.WorkspaceReadWorkspace,
            Permission.ChannelReadChannel,
            Permission.ChannelReadMessage));
    }

    [Fact]
    public async Task LowerLevel_CannotRevokeHigherLevelGrant()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        // There is no deny/negation in this model - the channel-level row only adds, it can't
        // remove what the workspace level already granted.
        Assert.True(permissions.Can(Permission.ChannelReadMessage));
        Assert.True(permissions.Can(Permission.ChannelCreateMessage));
    }

    [Fact]
    public async Task DuplicatePermissionAcrossLevels_Collapses()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.Single(permissions.Permissions);
        Assert.Contains(Permission.ChannelReadMessage, permissions.Permissions);
    }
}

public class PermissionResolverWalkStopsAtTargetTests
{
    [Fact]
    public async Task ForWorkspace_IgnoresChannelTypeAndChannelGrants()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.CtPublicSecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForWorkspace(ctx.W1);

        Assert.False(permissions.Can(Permission.ChannelReadMessage));
        Assert.False(permissions.Can(Permission.ChannelCreateMessage));
    }

    [Fact]
    public async Task ForChannelType_SeesNeitherWorkspaceNorChannelGrants()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannelType(ctx.CtPublic);

        Assert.False(permissions.Can(Permission.WorkspaceReadWorkspace));
        Assert.False(permissions.Can(Permission.ChannelCreateMessage));
    }
}

public class PermissionResolverMultiRoleTests
{
    [Fact]
    public async Task PermissionsUnionAcrossRolesOnSameObject()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRolesAAndB);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.True(permissions.Can(Permission.ChannelReadMessage, Permission.ChannelCreateMessage));
    }

    [Fact]
    public async Task PermissionsUnionAcrossRolesAndLevels()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRolesAAndB);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.True(permissions.Can(Permission.WorkspaceReadWorkspace, Permission.ChannelCreateMessage));
    }

    [Fact]
    public async Task RemovingOneRole_RemovesOnlyItsPermissions()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);

        // Simulate un-assigning RoleB from the user by removing the user_roles row.
        await ctx.UserRolesRepo.Delete(userRole =>
            userRole.UserId == ctx.UserWithRolesAAndB.Id && userRole.RoleId == ctx.RoleB.Id);

        var resolver = ctx.CreateResolver(ctx.UserWithRolesAAndB);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.True(permissions.Can(Permission.ChannelReadMessage));
        Assert.False(permissions.Can(Permission.ChannelCreateMessage));
    }
}

public class PermissionResolverGlobalNamespaceTests
{
    [Fact]
    public async Task GlobalGrant_DoesNotFlowToWorkspace()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.WorkspaceReadWorkspace);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForWorkspace(ctx.W1);

        Assert.False(permissions.Can(Permission.WorkspaceReadWorkspace));
    }

    [Fact]
    public async Task GlobalGrant_DoesNotFlowToChannelTypeOrChannel()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        Assert.False((await resolver.ForChannelType(ctx.CtPublic)).Can(Permission.ChannelReadMessage));
        Assert.False((await resolver.ForChannel(ctx.C1)).Can(Permission.ChannelReadMessage));
    }

    [Fact]
    public async Task GlobalPermissions_ResolveForGlobal()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserReadUser);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.Global();

        Assert.True(permissions.Can(Permission.UserReadUser));
    }
}

public class PermissionResolverDenialTests
{
    [Fact]
    public async Task NoRolesAndNoAcls_ResolvesEmpty()
    {
        var ctx = new PermissionResolverTestContext();

        // Grants that exist in the fixture but must never leak to a role-less user resolving C1:
        // other roles' grants (including on C1 itself), other users' grants, and a global grant.
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleC.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserReadUser);
        ctx.GrantUserAcl(ctx.UserWithRoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithNoRoles);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.Empty(permissions.Permissions);
        Assert.False(permissions.Can(Permission.ChannelReadMessage));
        Assert.False(permissions.Can(Permission.WorkspaceReadWorkspace));
        Assert.False(permissions.Can(Permission.ChannelCreateMessage));
        Assert.False(permissions.Can(Permission.UserReadUser));
    }

    [Fact]
    public void Require_ThrowsGraphQlExceptionWithForbiddenActionCode()
    {
        var permissions = new EffectivePermissions(new HashSet<Permission>());

        var exception = Assert.Throws<GraphQLException>(() => permissions.Require(Permission.ChannelReadMessage));

        Assert.Equal(ErrorCodes.ForbiddenAction, Assert.Single(exception.Errors).Code);
    }

    [Fact]
    public async Task RoleWithNoRoleAcls_GrantsNothing()
    {
        var ctx = new PermissionResolverTestContext();

        // RoleA (the acting user's only role) has no ACLs, but grants to OTHER roles on this same
        // channel must not leak through.
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleC.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.Empty(permissions.Permissions);
    }

    [Fact]
    public async Task AclOnUnrelatedObject_GrantsNothing()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W2SecObjId, Permission.WorkspaceReadWorkspace);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.Empty(permissions.Permissions);
    }

    [Fact]
    public async Task CanAndRequire_KeepAllOfSemantics()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(ctx.C1);

        Assert.False(permissions.Can(Permission.ChannelReadMessage, Permission.ChannelCreateMessage));
        Assert.Throws<GraphQLException>(() =>
            permissions.Require(Permission.ChannelReadMessage, Permission.ChannelCreateMessage));
    }

    [Fact]
    public async Task Anonymous_ResolvesEmpty()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserReadUser);

        var resolver = ctx.CreateAnonymousResolver();

        Assert.Empty((await resolver.Global()).Permissions);
        Assert.Empty((await resolver.ForWorkspace(ctx.W1)).Permissions);
        Assert.Empty((await resolver.ForChannelType(ctx.CtPublic)).Permissions);
        Assert.Empty((await resolver.ForChannel(ctx.C1)).Permissions);
        Assert.Empty(await resolver.VisibleChannels(null, Permission.ChannelReadMessage));
    }
}

public class PermissionResolverVisibleChannelsTests
{
    [Fact]
    public async Task WorkspaceLevelGrant_MakesEveryChannelInThatWorkspaceVisible()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var visible = await resolver.VisibleChannels(null, Permission.ChannelReadMessage);

        Assert.Contains(visible, c => c.Id == ctx.C1.Id);
        Assert.Contains(visible, c => c.Id == ctx.C2.Id);
        Assert.Contains(visible, c => c.Id == ctx.C3.Id);
        Assert.Contains(visible, c => c.Id == ctx.C4.Id);
        Assert.DoesNotContain(visible, c => c.Id == ctx.C5.Id);
    }

    [Fact]
    public async Task WorkspaceFilter_ExcludesChannelsFromOtherWorkspaces()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W2SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var visible = await resolver.VisibleChannels(ctx.W1.Id, Permission.ChannelReadMessage);

        Assert.Contains(visible, c => c.Id == ctx.C1.Id);
        Assert.DoesNotContain(visible, c => c.Id == ctx.C5.Id);
    }

    [Fact]
    public async Task NullWorkspaceFilter_IncludesBothWorkspaces()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W2SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var visible = await resolver.VisibleChannels(null, Permission.ChannelReadMessage);

        Assert.Contains(visible, c => c.Id == ctx.C1.Id);
        Assert.Contains(visible, c => c.Id == ctx.C5.Id);
    }

    [Fact]
    public async Task ChannelTypeLevelGrant_MakesOnlyChannelsOfThatTypeVisible()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.CtPublicSecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var visible = await resolver.VisibleChannels(null, Permission.ChannelReadMessage);

        Assert.Contains(visible, c => c.Id == ctx.C1.Id);
        Assert.Contains(visible, c => c.Id == ctx.C2.Id);
        Assert.Contains(visible, c => c.Id == ctx.C5.Id);
        Assert.DoesNotContain(visible, c => c.Id == ctx.C3.Id);
        Assert.DoesNotContain(visible, c => c.Id == ctx.C4.Id);
    }

    [Fact]
    public async Task WorkspaceLevelGrantOnW1_DoesNotMakeW2ChannelVisible()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var visible = await resolver.VisibleChannels(null, Permission.ChannelReadMessage);

        Assert.DoesNotContain(visible, c => c.Id == ctx.C5.Id);
    }
}

/// <summary>A hand-written double proving <see cref="IAclSnapshotLoader"/> is a real, reachable seam.</summary>
public sealed class CountingAclSnapshotLoader : IAclSnapshotLoader
{
    public int CallCount { get; private set; }

    public async Task<AclSnapshot> LoadAsync(AppRepos repos, Guid actorId)
    {
        CallCount++;
        return await AclSnapshotLoader.Instance.LoadAsync(repos, actorId);
    }
}

public class PermissionResolverSnapshotSeamTests
{
    [Fact]
    public async Task PublicConstructor_AcceptsSubstituteLoader_AndCallsItExactlyOncePerInstance()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        var loader = new CountingAclSnapshotLoader();
        var resolver = new PermissionResolver(ctx.Repos, ctx.UserWithRoleA.Id, loader);

        await resolver.ForChannel(ctx.C1);
        await resolver.ForChannel(ctx.C2);
        await resolver.Global();

        Assert.Equal(1, loader.CallCount);
    }
}

public class PermissionResolverEfficiencyTests
{
    [Fact]
    public async Task ForChannel_RepeatedCalls_ProduceBoundedRepoReads()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        for (var i = 0; i < 100; i++)
        {
            await resolver.ForChannel(ctx.C1);
        }

        await resolver.ResolveAll();
        await resolver.VisibleChannels(null, Permission.ChannelReadMessage);

        // Exactly one read per underlying table, regardless of how many checks/ResolveAll/
        // VisibleChannels calls were made - each asserted individually so six reads all landing on
        // one repo (which would still sum to the old shared total) cannot pass unnoticed.
        Assert.Equal(1, ctx.UserRolesRepo.ReadCallCount);
        Assert.Equal(1, ctx.UserAclsRepo.ReadCallCount);
        Assert.Equal(1, ctx.RoleAclsRepo.ReadCallCount);
        Assert.Equal(1, ctx.WorkspacesRepo.ReadCallCount);
        Assert.Equal(1, ctx.ChannelTypesRepo.ReadCallCount);
        Assert.Equal(1, ctx.ChannelsRepo.ReadCallCount);
    }
}
