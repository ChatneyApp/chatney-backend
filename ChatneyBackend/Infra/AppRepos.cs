using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.DraftMessages;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;

namespace ChatneyBackend.Infra;

public class AppRepos(
    PgRepo<User, Guid> users,
    PgRepo<UserRole, UserRoleKey> userRoles,
    PgRepo<Role, int> roles,
    PgRepo<Message, int> messages,
    PgRepo<DraftMessage, int> draftMessages,
    PgRepo<MessageReaction, MessageReactionKey> reactions,
    PgRepo<Attachment, int> attachments,
    PgRepo<UrlPreview, int> urlPreviews,
    PgRepo<Channel, int> channels,
    PgRepo<ChannelType, int> channelTypes,
    PgRepo<ChannelGroup, int> channelGroups,
    PgRepo<Config, int> configs,
    PgRepo<Workspace, int> workspaces)
{
    public PgRepo<User, Guid> Users { get; } = users;
    public PgRepo<UserRole, UserRoleKey> UserRoles { get; } = userRoles;
    public PgRepo<Role, int> Roles { get; } = roles;
    public PgRepo<Message, int> Messages { get; } = messages;
    public PgRepo<DraftMessage, int> DraftMessages { get; } = draftMessages;
    public PgRepo<MessageReaction, MessageReactionKey> Reactions { get; } = reactions;
    public PgRepo<Attachment, int> Attachments { get; } = attachments;
    public PgRepo<UrlPreview, int> UrlPreviews { get; } = urlPreviews;
    public PgRepo<Channel, int> Channels { get; } = channels;
    public PgRepo<ChannelType, int> ChannelTypes { get; } = channelTypes;
    public PgRepo<ChannelGroup, int> ChannelGroups { get; } = channelGroups;
    public PgRepo<Config, int> Configs { get; } = configs;
    public PgRepo<Workspace, int> Workspaces { get; } = workspaces;
}
