using System.Security.Claims;
using ChatneyBackend.Domains.Attachments;
using System.Linq;
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

    public User User { get; }
    public Channel Channel { get; }
    public Role Role { get; }
    public ClaimsPrincipal Principal { get; }
    public AppRepos Repos { get; }
    public RoleManager RoleManager { get; }
    public RecordingWebSocketConnector WebSocket { get; } = new();
    public MessageMutations Mutations { get; } = new();

    public MessageMutationsTestContext(string[]? permissions = null)
    {
        User = new User
        {
            Id = Guid.NewGuid(),
            Nickname = "test_user",
            FullName = "Test User",
            Email = "test@example.com",
            Password = "password",
            RoleId = 1,
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
            Permissions = permissions ??
            [
                ChannelPermissions.CreateMessage,
                ChannelPermissions.EditMessage,
                ChannelPermissions.DeleteMessage,
                ChannelPermissions.EditOwnMessage,
                ChannelPermissions.DeleteOwnMessage,
                ChannelPermissions.ReadMessage,
                ChannelPermissions.ReadChannel,
            ],
            IsProtected = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        Channel = new Channel
        {
            Id = 1,
            Name = "general",
            WorkspaceId = 1,
            ChannelTypeId = 1,
            SecObjId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        UsersRepo.Seed(User);
        RolesRepo.Seed(Role);
        ChannelsRepo.Seed(Channel);

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
            SecureObjectsRepo);

        RoleManager = new RoleManager(RolesRepo, UserRolesRepo);
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
