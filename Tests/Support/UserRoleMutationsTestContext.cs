using System.Security.Claims;
using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.DraftMessages;
using ChatneyBackend.Domains.Messages;
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
    public InMemoryPgRepo<Config, int> ConfigsRepo { get; } = new();
    public InMemoryPgRepo<Workspace, int> WorkspacesRepo { get; } = new();

    public User Admin { get; }
    public User TargetUser { get; }
    public Role AdminRole { get; }
    public Role AssignedRole { get; }
    public Workspace Workspace { get; }
    public ClaimsPrincipal Principal { get; }
    public AppRepos Repos { get; }
    public RoleManager RoleManager { get; }
    public RecordingWebSocketConnector WebSocket { get; } = new();
    public UserRoleMutations Mutations { get; } = new();

    public UserRoleMutationsTestContext(string[]? adminPermissions = null)
    {
        AdminRole = new Role
        {
            Id = 1,
            Name = "admin",
            Permissions = adminPermissions ?? [UserPermissionNames.EditUser],
            IsBase = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        AssignedRole = new Role
        {
            Id = 2,
            Name = "moderator",
            Permissions = [ChannelPermissions.EditMessage],
            IsBase = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        Workspace = new Workspace
        {
            Id = 1,
            Name = "Main",
        };

        Admin = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@example.com",
            Password = "password",
            RoleId = AdminRole.Id,
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
            Name = "Target User",
            Email = "target@example.com",
            Password = "password",
            RoleId = AdminRole.Id,
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
            ConfigsRepo,
            WorkspacesRepo);

        RoleManager = new RoleManager(RolesRepo, UserRolesRepo);
    }

    public UserRole CreateUserRole(
        Guid? userId = null,
        int? workspaceId = null,
        int? roleId = null,
        string[]? allowlist = null,
        string[]? denylist = null) => new()
    {
        UserId = userId ?? TargetUser.Id,
        WorkspaceId = workspaceId ?? Workspace.Id,
        ChannelId = null,
        ChannelTypeId = null,
        RoleId = roleId ?? AssignedRole.Id,
        Allowlist = allowlist ?? [],
        Denylist = denylist ?? [],
    };
}
