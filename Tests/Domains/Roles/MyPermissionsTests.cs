using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Tests.Support;

namespace ChatneyBackend.Tests.Domains.Roles;

public class MyPermissionsTests
{
    [Fact]
    public async Task ResolveAll_ReturnsGlobalPermissions()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserReadUser);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var myPermissions = await resolver.ResolveAll();

        Assert.Contains(Permission.UserReadUser, myPermissions.Global);
        // D2: the global grant must not leak into any object namespace.
        Assert.Empty(myPermissions.Workspaces);
        Assert.Empty(myPermissions.Channels);
    }

    [Fact]
    public async Task ResolveAll_WorkspaceEntry_MatchesForWorkspace()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var myPermissions = await resolver.ResolveAll();
        var expected = (await resolver.ForWorkspace(ctx.W1)).Permissions;

        var entry = Assert.Single(myPermissions.Workspaces, w => w.Id == ctx.W1.Id);
        Assert.True(expected.SetEquals(entry.Permissions));
    }

    [Fact]
    public async Task ResolveAll_ChannelEntry_IncludesInheritedWorkspaceAndChannelTypePermissions()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.CtPublicSecObjId, Permission.ChannelReadChannel);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var myPermissions = await resolver.ResolveAll();

        var entry = Assert.Single(myPermissions.Channels, c => c.Id == ctx.C1.Id);
        Assert.Contains(Permission.WorkspaceReadWorkspace, entry.Permissions);
        Assert.Contains(Permission.ChannelReadChannel, entry.Permissions);
        Assert.Contains(Permission.ChannelReadMessage, entry.Permissions);
    }

    [Fact]
    public async Task ResolveAll_OmitsObjectsWithNoPermissions()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var myPermissions = await resolver.ResolveAll();

        Assert.Single(myPermissions.Channels, c => c.Id == ctx.C1.Id);
        Assert.DoesNotContain(myPermissions.Channels, c => c.Id == ctx.C2.Id);
        Assert.DoesNotContain(myPermissions.Channels, c => c.Id == ctx.C3.Id);
        Assert.DoesNotContain(myPermissions.Channels, c => c.Id == ctx.C4.Id);
        Assert.DoesNotContain(myPermissions.Channels, c => c.Id == ctx.C5.Id);
        Assert.DoesNotContain(myPermissions.Workspaces, w => w.Id == ctx.W1.Id);
        Assert.DoesNotContain(myPermissions.Workspaces, w => w.Id == ctx.W2.Id);
        Assert.DoesNotContain(myPermissions.ChannelTypes, ct => ct.Id == ctx.CtPublic.Id);
        Assert.DoesNotContain(myPermissions.ChannelTypes, ct => ct.Id == ctx.CtPrivate.Id);
    }

    [Fact]
    public async Task ResolveAll_PerObjectUserAclOverride_Respected()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C2SecObjId, Permission.ChannelReadMessage);

        // A user ACL on C1 only should override role ACLs for C1, while C2 keeps using role ACLs.
        ctx.GrantUserAcl(ctx.UserWithRoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var myPermissions = await resolver.ResolveAll();

        var c1Entry = Assert.Single(myPermissions.Channels, c => c.Id == ctx.C1.Id);
        var c2Entry = Assert.Single(myPermissions.Channels, c => c.Id == ctx.C2.Id);

        Assert.Equal(new HashSet<Permission> { Permission.ChannelCreateMessage }, c1Entry.Permissions.ToHashSet());
        Assert.Equal(new HashSet<Permission> { Permission.ChannelReadMessage }, c2Entry.Permissions.ToHashSet());
    }

    [Fact]
    public async Task ResolveAll_UserWithNothing_IsAllEmpty()
    {
        var ctx = new PermissionResolverTestContext();

        // Grants that exist in the fixture but must not appear anywhere for a role-less user.
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        ctx.GrantRoleAcl(ctx.RoleB.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleC.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserReadUser);
        ctx.GrantUserAcl(ctx.UserWithRoleA.Id, PermissionResolverTestContext.C2SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithNoRoles);
        var myPermissions = await resolver.ResolveAll();

        Assert.Empty(myPermissions.Global);
        Assert.Empty(myPermissions.Workspaces);
        Assert.Empty(myPermissions.ChannelTypes);
        Assert.Empty(myPermissions.Channels);
    }

    [Fact]
    public async Task ResolveAll_MatchesSingleObjectResolution_ForEveryObjectInFixture()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.GlobalSecObjId, Permission.UserReadUser);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.CtPublicSecObjId, Permission.ChannelReadChannel);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantUserAcl(ctx.UserWithRoleA.Id, PermissionResolverTestContext.C3SecObjId, Permission.ChannelCreateMessage);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var myPermissions = await resolver.ResolveAll();

        var expectedGlobal = (await resolver.Global()).Permissions;
        Assert.True(expectedGlobal.SetEquals(myPermissions.Global));

        foreach (var workspace in ctx.WorkspacesRepo.Items)
        {
            var expected = (await resolver.ForWorkspace(workspace)).Permissions;
            var entry = myPermissions.Workspaces.FirstOrDefault(w => w.Id == workspace.Id);
            Assert.True(expected.SetEquals(entry?.Permissions ?? []));
        }

        foreach (var channelType in ctx.ChannelTypesRepo.Items)
        {
            var expected = (await resolver.ForChannelType(channelType)).Permissions;
            var entry = myPermissions.ChannelTypes.FirstOrDefault(ct => ct.Id == channelType.Id);
            Assert.True(expected.SetEquals(entry?.Permissions ?? []));
        }

        foreach (var channel in ctx.ChannelsRepo.Items)
        {
            var expected = (await resolver.ForChannel(channel)).Permissions;
            var entry = myPermissions.Channels.FirstOrDefault(c => c.Id == channel.Id);
            Assert.True(expected.SetEquals(entry?.Permissions ?? []));
        }
    }
}
