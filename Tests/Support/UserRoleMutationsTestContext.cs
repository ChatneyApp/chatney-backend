using System.Security.Claims;
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
using ChatneyBackend.Infra.Middleware;

namespace ChatneyBackend.Tests.Support;

public sealed class UserRoleMutationsTestContext
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
    public InMemoryPgRepo<ChannelMember, ChannelMemberKey> ChannelMembersRepo { get; } = new();
    public InMemoryPgRepo<Config, int> ConfigsRepo { get; } = new();
    public InMemoryPgRepo<Workspace, int> WorkspacesRepo { get; } = new();
    public InMemoryPgRepo<SecureObject, int> SecureObjectsRepo { get; } = new();
    public InMemoryPgRepo<RoleAcl, RoleAclKey> RoleAclsRepo { get; } = new();
    public InMemoryPgRepo<UserAcl, UserAclKey> UserAclsRepo { get; } = new();

    public User Admin { get; }
    public User TargetUser { get; }
    public Role AdminRole { get; }
    public Role AssignedRole { get; }
    public Workspace Workspace { get; }
    public ClaimsPrincipal Principal { get; }
    public AppRepos Repos { get; }
    public IPermissionResolver Resolver { get; }
    public RecordingWebSocketConnector WebSocket { get; } = new();
    public UserRoleMutations Mutations { get; } = new();

    public UserRoleMutationsTestContext(Permission[]? adminPermissions = null)
    {
        AdminRole = new Role
        {
            Id = 1,
            Name = "admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        AssignedRole = new Role
        {
            Id = 2,
            Name = "moderator",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        Workspace = new Workspace
        {
            Id = 1,
            Name = "Main",
            SecObjId = 10,
        };

        Admin = new User
        {
            Id = Guid.NewGuid(),
            Nickname = "admin_user",
            FullName = "Admin",
            Email = "admin@example.com",
            Password = "password",
            Active = true,
            Verified = true,
            Banned = false,
            Muted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        TargetUser = new User
        {
            Id = Guid.NewGuid(),
            Nickname = "target_user",
            FullName = "Target User",
            Email = "target@example.com",
            Password = "password",
            Active = true,
            Verified = true,
            Banned = false,
            Muted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        RolesRepo.Seed(AdminRole, AssignedRole);
        UsersRepo.Seed(Admin, TargetUser);
        WorkspacesRepo.Seed(Workspace);
        UserRolesRepo.Seed(new UserRole { UserId = Admin.Id, RoleId = AdminRole.Id });

        RoleAclsRepo.Seed(new RoleAcl
        {
            RoleId = AdminRole.Id,
            SecObjId = SecureObjectIds.Global,
            Permissions = adminPermissions ?? [Permission.UserEditUser],
        });

        Principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Sid, Admin.Id.ToString()),
            new Claim(ClaimTypes.Email, Admin.Email),
        ],
        "TestAuth"));

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
            ChannelMembersRepo,
            ConfigsRepo,
            WorkspacesRepo,
            SecureObjectsRepo,
            RoleAclsRepo,
            UserAclsRepo);

        Resolver = PermissionResolver.For(Repos, Admin);
    }
}
