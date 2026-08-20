using ChatneyBackend.Domains.Permissions;

namespace ChatneyBackend.Domains.Roles;

/// <summary>
/// Seed data for the three built-in roles. Roles themselves carry no permissions any more (see
/// D2/RBAC-ACL migration) - permissions live entirely in role_acls, keyed by secure object. Use
/// <see cref="CreateSeedRoles"/> to build the roles, insert them (populating their ids), then pass
/// the inserted roles into <see cref="CreateSeedRoleAcls"/> to build their role_acls rows.
/// </summary>
public static class DefaultRoles
{
    /// <summary>Global (non-object-scoped) permissions granted to the "user" role.</summary>
    public static readonly Permission[] UserGlobalPermissions =
    [
        Permission.AttachmentUpload,
        Permission.AttachmentRead,
    ];

    /// <summary>Object-scoped permissions granted to the "user" role on the seed workspace.</summary>
    public static readonly Permission[] UserObjectPermissions =
    [
        Permission.ChannelCreateMessage,
        Permission.ChannelReadChannel,
        Permission.ChannelReadMessage,
        Permission.ChannelEditOwnMessage,
        Permission.ChannelDeleteOwnMessage,
        Permission.WorkspaceReadWorkspace,
    ];

    /// <summary>Global (non-object-scoped) permissions granted to the "moderator" role.</summary>
    public static readonly Permission[] ModeratorGlobalPermissions =
    [
        ..UserGlobalPermissions,
        Permission.UserReadUser,
    ];

    /// <summary>Object-scoped permissions granted to the "moderator" role on the seed workspace.</summary>
    public static readonly Permission[] ModeratorObjectPermissions =
    [
        ..UserObjectPermissions,
        Permission.ChannelEditMessage,
        Permission.ChannelDeleteMessage,
    ];

    public static Role[] CreateSeedRoles(DateTime timestamp) =>
    [
        new()
        {
            Name = DomainSettings.AdminRoleName,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
        },
        new()
        {
            Name = DomainSettings.ModeratorRoleName,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
        },
        new()
        {
            Name = DomainSettings.UserRoleName,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
        },
    ];

    /// <summary>
    /// Builds the role_acls rows for the seed roles. Must be called AFTER <paramref name="roles"/>
    /// have been inserted (so <c>Role.Id</c> is populated).
    ///
    /// Admin's object-scoped grants on every seeded workspace/channel-type/channel are NOT built
    /// here - they come for free from <see cref="SecureObjectHelper.Create"/>, since the admin role
    /// already exists by the time those objects are created during install. This method only needs
    /// to add: admin's global "everything" grant, and moderator/user's global and seed-workspace
    /// grants (which SecureObjectHelper has no way to know about).
    /// </summary>
    public static RoleAcl[] CreateSeedRoleAcls(IReadOnlyList<Role> roles, int seedWorkspaceSecObjId)
    {
        var admin = roles.Single(role => role.Name == DomainSettings.AdminRoleName);
        var moderator = roles.Single(role => role.Name == DomainSettings.ModeratorRoleName);
        var user = roles.Single(role => role.Name == DomainSettings.UserRoleName);

        return
        [
            new RoleAcl
            {
                RoleId = admin.Id,
                SecObjId = SecureObjectIds.Global,
                Permissions = Enum.GetValues<Permission>(),
            },
            new RoleAcl
            {
                RoleId = moderator.Id,
                SecObjId = SecureObjectIds.Global,
                Permissions = ModeratorGlobalPermissions,
            },
            new RoleAcl
            {
                RoleId = user.Id,
                SecObjId = SecureObjectIds.Global,
                Permissions = UserGlobalPermissions,
            },
            new RoleAcl
            {
                RoleId = moderator.Id,
                SecObjId = seedWorkspaceSecObjId,
                Permissions = ModeratorObjectPermissions,
            },
            new RoleAcl
            {
                RoleId = user.Id,
                SecObjId = seedWorkspaceSecObjId,
                Permissions = UserObjectPermissions,
            },
        ];
    }
}
