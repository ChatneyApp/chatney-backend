using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.DraftMessages;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Tests.Support;

/// <summary>
/// Fixture for PermissionResolver tests: two workspaces, two channel types, five channels (four
/// in W1 - two per channel type - and one in W2, so cross-workspace isolation is testable), three
/// roles and a handful of users. Every secured object gets its own distinct sec_obj_id. The global
/// secure object (id 1) IS seeded here, mirroring the real DB's seed migration - otherwise a newly
/// InMemoryPgRepo-created object would also get id 1 (the fake's identity assignment starts at 1
/// for an empty repo), letting a regression that writes the admin grant to the global object
/// instead of the real one pass unnoticed (see SecureObjectHelperTests).
/// </summary>
public sealed class PermissionResolverTestContext
{
    public InMemoryPgRepo<User, Guid> UsersRepo { get; } = new();
    public InMemoryPgRepo<UserRole, UserRoleKey> UserRolesRepo { get; } = new();
    public InMemoryPgRepo<Role, int> RolesRepo { get; } = new();
    public InMemoryPgRepo<Message, int> MessagesRepo { get; } = new();
    public InMemoryPgRepo<DraftMessage, int> DraftMessagesRepo { get; } = new();
    public InMemoryPgRepo<MessageReaction, MessageReactionKey> ReactionsRepo { get; } = new();
    public InMemoryPgRepo<Attachment, int> AttachmentsRepo { get; } = new();
    public InMemoryPgRepo<UrlPreview, int> UrlPreviewsRepo { get; } = new();
    public InMemoryPgRepo<Channel, int> ChannelsRepo { get; } = new();
    public InMemoryPgRepo<ChannelType, int> ChannelTypesRepo { get; } = new();
    public InMemoryPgRepo<ChannelGroup, int> ChannelGroupsRepo { get; } = new();
    public InMemoryPgRepo<Config, int> ConfigsRepo { get; } = new();
    public InMemoryPgRepo<Workspace, int> WorkspacesRepo { get; } = new();
    public InMemoryPgRepo<SecureObject, int> SecureObjectsRepo { get; } = new();
    public InMemoryPgRepo<RoleAcl, RoleAclKey> RoleAclsRepo { get; } = new();
    public InMemoryPgRepo<UserAcl, UserAclKey> UserAclsRepo { get; } = new();

    public AppRepos Repos { get; }

    // Secure object ids - every object gets a distinct one; 1 is reserved for global.
    public const int GlobalSecObjId = SecureObjectIds.Global;
    public const int W1SecObjId = 10;
    public const int W2SecObjId = 20;
    public const int CtPublicSecObjId = 30;
    public const int CtPrivateSecObjId = 40;
    public const int C1SecObjId = 50;
    public const int C2SecObjId = 60;
    public const int C3SecObjId = 70;
    public const int C4SecObjId = 80;
    public const int C5SecObjId = 90;

    public Workspace W1 { get; }
    public Workspace W2 { get; }
    public ChannelType CtPublic { get; }
    public ChannelType CtPrivate { get; }

    /// <summary>W1 / CtPublic</summary>
    public Channel C1 { get; }

    /// <summary>W1 / CtPublic</summary>
    public Channel C2 { get; }

    /// <summary>W1 / CtPrivate</summary>
    public Channel C3 { get; }

    /// <summary>W1 / CtPrivate</summary>
    public Channel C4 { get; }

    /// <summary>W2 / CtPublic</summary>
    public Channel C5 { get; }

    public Role RoleA { get; }
    public Role RoleB { get; }
    public Role RoleC { get; }

    public User UserWithRoleA { get; }
    public User UserWithRoleB { get; }
    public User UserWithRolesAAndB { get; }
    public User UserWithNoRoles { get; }

    public PermissionResolverTestContext()
    {
        W1 = new Workspace { Id = 1, Name = "W1", SecObjId = W1SecObjId };
        W2 = new Workspace { Id = 2, Name = "W2", SecObjId = W2SecObjId };

        CtPublic = new ChannelType { Id = 1, Name = "Public", Key = "public", SecObjId = CtPublicSecObjId };
        CtPrivate = new ChannelType { Id = 2, Name = "Private", Key = "private", SecObjId = CtPrivateSecObjId };

        C1 = new Channel { Id = 1, Name = "C1", WorkspaceId = W1.Id, ChannelTypeId = CtPublic.Id, SecObjId = C1SecObjId };
        C2 = new Channel { Id = 2, Name = "C2", WorkspaceId = W1.Id, ChannelTypeId = CtPublic.Id, SecObjId = C2SecObjId };
        C3 = new Channel { Id = 3, Name = "C3", WorkspaceId = W1.Id, ChannelTypeId = CtPrivate.Id, SecObjId = C3SecObjId };
        C4 = new Channel { Id = 4, Name = "C4", WorkspaceId = W1.Id, ChannelTypeId = CtPrivate.Id, SecObjId = C4SecObjId };
        C5 = new Channel { Id = 5, Name = "C5", WorkspaceId = W2.Id, ChannelTypeId = CtPublic.Id, SecObjId = C5SecObjId };

        RoleA = new Role { Id = 1, Name = "RoleA", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        RoleB = new Role { Id = 2, Name = "RoleB", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        RoleC = new Role { Id = 3, Name = "RoleC", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

        UserWithRoleA = CreateUser("user_role_a");
        UserWithRoleB = CreateUser("user_role_b");
        UserWithRolesAAndB = CreateUser("user_role_a_b");
        UserWithNoRoles = CreateUser("user_no_roles");

        SecureObjectsRepo.Seed(new SecureObject
        {
            Id = GlobalSecObjId,
            Description = new SecureObjectDescription { Kind = "global", Name = "System" },
        });
        WorkspacesRepo.Seed(W1, W2);
        ChannelTypesRepo.Seed(CtPublic, CtPrivate);
        ChannelsRepo.Seed(C1, C2, C3, C4, C5);
        RolesRepo.Seed(RoleA, RoleB, RoleC);
        UsersRepo.Seed(UserWithRoleA, UserWithRoleB, UserWithRolesAAndB, UserWithNoRoles);

        AssignRole(UserWithRoleA.Id, RoleA.Id);
        AssignRole(UserWithRoleB.Id, RoleB.Id);
        AssignRole(UserWithRolesAAndB.Id, RoleA.Id);
        AssignRole(UserWithRolesAAndB.Id, RoleB.Id);

        Repos = new AppRepos(
            UsersRepo,
            UserRolesRepo,
            RolesRepo,
            MessagesRepo,
            DraftMessagesRepo,
            ReactionsRepo,
            AttachmentsRepo,
            UrlPreviewsRepo,
            ChannelsRepo,
            ChannelTypesRepo,
            ChannelGroupsRepo,
            ConfigsRepo,
            WorkspacesRepo,
            SecureObjectsRepo,
            RoleAclsRepo,
            UserAclsRepo);
    }

    public static User CreateUser(string nickname) => new()
    {
        Id = Guid.NewGuid(),
        Nickname = nickname,
        FullName = nickname,
        Email = $"{nickname}@example.com",
        Password = "password",
        Active = true,
        Verified = true,
        Banned = false,
        Muted = false,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    public void AssignRole(Guid userId, int roleId) =>
        UserRolesRepo.Seed(new UserRole { UserId = userId, RoleId = roleId });

    public void GrantRoleAcl(int roleId, int secObjId, params Permission[] permissions) =>
        RoleAclsRepo.Seed(new RoleAcl { RoleId = roleId, SecObjId = secObjId, Permissions = permissions });

    public void GrantUserAcl(Guid userId, int secObjId, params Permission[] permissions) =>
        UserAclsRepo.Seed(new UserAcl { UserId = userId, SecObjId = secObjId, Permissions = permissions });

    public PermissionResolver CreateResolver(User user) => PermissionResolver.For(Repos, user);

    public PermissionResolver CreateAnonymousResolver() => PermissionResolver.Anonymous(Repos);
}
