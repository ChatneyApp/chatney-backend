using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Infra;
using ChatneyBackend.Tests.Support;

namespace ChatneyBackend.Tests.Infra;

public class SecureObjectHelperTests
{
    /// <summary>
    /// Guards SecureObjectHelper.ObjectScopedPermissions (derived from Permission.PgName prefixes)
    /// against drifting away from PermissionGroups' own "Workspace permissions"/"Channel
    /// permissions" groups (derived independently, from the group labels). If these two derivations
    /// ever disagree, one of them has drifted from the Permission enum.
    /// </summary>
    [Fact]
    public void ObjectScopedPermissions_MatchesWorkspaceAndChannelPermissionGroups()
    {
        var expected = PermissionGroups.All
            .Where(group => group.Label is "Workspace permissions" or "Channel permissions")
            .SelectMany(group => group.List)
            .ToHashSet();

        Assert.Equal(expected, SecureObjectHelper.ObjectScopedPermissions.ToHashSet());
    }

    [Fact]
    public void ObjectScopedPermissions_ExcludesGlobalOnlyPermissions()
    {
        Assert.DoesNotContain(Permission.UserReadUser, SecureObjectHelper.ObjectScopedPermissions);
        Assert.DoesNotContain(Permission.RoleEditRole, SecureObjectHelper.ObjectScopedPermissions);
        Assert.DoesNotContain(Permission.ConfigReadValue, SecureObjectHelper.ObjectScopedPermissions);
        Assert.DoesNotContain(Permission.AttachmentUpload, SecureObjectHelper.ObjectScopedPermissions);
    }

    [Fact]
    public void ObjectScopedPermissions_IncludesWorkspaceAndChannelPermissions()
    {
        Assert.Contains(Permission.WorkspaceReadWorkspace, SecureObjectHelper.ObjectScopedPermissions);
        Assert.Contains(Permission.ChannelReadMessage, SecureObjectHelper.ObjectScopedPermissions);
    }

    [Fact]
    public async Task Create_GrantsAdminRoleEveryObjectScopedPermission_OnTheNewObject()
    {
        var ctx = new PermissionResolverTestContext();
        var adminRole = new Role
        {
            Id = 100,
            Name = ChatneyBackend.Domains.Roles.DomainSettings.AdminRoleName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        ctx.RolesRepo.Seed(adminRole);
        ctx.AssignRole(ctx.UserWithRoleA.Id, adminRole.Id);

        var secObjId = await SecureObjectHelper.Create(
            ctx.Repos, new SecureObjectDescription { Kind = "workspace", Name = "New Workspace" });

        var stored = ctx.RoleAclsRepo.Items.Single(acl => acl.RoleId == adminRole.Id && acl.SecObjId == secObjId);
        Assert.Equal(
            SecureObjectHelper.ObjectScopedPermissions.ToHashSet(),
            stored.Permissions.ToHashSet());

        var newWorkspace = new Workspace { Id = 999, Name = "New Workspace", SecObjId = secObjId };
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForWorkspace(newWorkspace);

        Assert.True(permissions.Can(Permission.WorkspaceReadWorkspace));
    }

    [Fact]
    public async Task Create_GrantsNothingToNonAdminRole()
    {
        var ctx = new PermissionResolverTestContext();
        var adminRole = new Role
        {
            Id = 100,
            Name = ChatneyBackend.Domains.Roles.DomainSettings.AdminRoleName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        ctx.RolesRepo.Seed(adminRole);

        var secObjId = await SecureObjectHelper.Create(
            ctx.Repos, new SecureObjectDescription { Kind = "workspace", Name = "New Workspace" });

        Assert.DoesNotContain(ctx.RoleAclsRepo.Items, acl => acl.RoleId == ctx.RoleA.Id && acl.SecObjId == secObjId);

        var newWorkspace = new Workspace { Id = 999, Name = "New Workspace", SecObjId = secObjId };
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForWorkspace(newWorkspace);

        Assert.False(permissions.Can(Permission.WorkspaceReadWorkspace));
    }
}
