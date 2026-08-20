using System.Security.Claims;
using ChatneyBackend.Domains.Attachments;
using System.Linq;
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

public sealed class MessageMutationsTestContext
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

    public User User { get; }
    public Workspace Workspace { get; }
    public ChannelType ChannelType { get; }
    public Channel Channel { get; }
    public Role Role { get; }
    public ClaimsPrincipal Principal { get; }
    public AppRepos Repos { get; }
    public IPermissionResolver Resolver { get; }
    public RecordingWebSocketConnector WebSocket { get; } = new();
    public MessageMutations Mutations { get; } = new();

    public MessageMutationsTestContext(Permission[]? permissions = null)
    {
        User = new User
        {
            Id = Guid.NewGuid(),
            Nickname = "test_user",
            FullName = "Test User",
            Email = "test@example.com",
            Password = "password",
            Active = true,
            Verified = true,
            Banned = false,
            Muted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        Role = new Role
        {
            Id = 1,
            Name = "member",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        Workspace = new Workspace { Id = 1, Name = "workspace", SecObjId = 10 };
        ChannelType = new ChannelType { Id = 1, Name = "type", Key = "type", SecObjId = 20 };

        Channel = new Channel
        {
            Id = 1,
            Name = "general",
            WorkspaceId = Workspace.Id,
            ChannelTypeId = ChannelType.Id,
            SecObjId = 30,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        UsersRepo.Seed(User);
        RolesRepo.Seed(Role);
        WorkspacesRepo.Seed(Workspace);
        ChannelTypesRepo.Seed(ChannelType);
        ChannelsRepo.Seed(Channel);
        UserRolesRepo.Seed(new UserRole { UserId = User.Id, RoleId = Role.Id });

        RoleAclsRepo.Seed(new RoleAcl
        {
            RoleId = Role.Id,
            SecObjId = Channel.SecObjId,
            Permissions = permissions ??
            [
                Permission.ChannelCreateMessage,
                Permission.ChannelEditMessage,
                Permission.ChannelDeleteMessage,
                Permission.ChannelEditOwnMessage,
                Permission.ChannelDeleteOwnMessage,
                Permission.ChannelReadMessage,
                Permission.ChannelReadChannel,
            ],
        });

        Principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Sid, User.Id.ToString()),
            new Claim(ClaimTypes.Email, User.Email),
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
            WorkspacesRepo,
            SecureObjectsRepo,
            RoleAclsRepo,
            UserAclsRepo);

        Resolver = PermissionResolver.For(Repos, User);
    }

    public Message SeedMessage(
        string content = "hello",
        int? parentId = null,
        Guid? userId = null,
        int? id = null,
        int childrenCount = 0)
    {
        var messageId = id ?? (MessagesRepo.Items.Count == 0
            ? 1
            : MessagesRepo.Items.Cast<Message>().Max(message => message.Id) + 1);

        var message = new Message
        {
            Id = messageId,
            ChannelId = Channel.Id,
            UserId = userId ?? User.Id,
            Content = content,
            Status = "sent",
            ParentId = parentId,
            ChildrenCount = childrenCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        MessagesRepo.Seed(message);
        return message;
    }
}
