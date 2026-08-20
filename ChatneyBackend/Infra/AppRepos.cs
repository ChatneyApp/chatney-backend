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
    IPgRepo<User, Guid> users,
    IPgRepo<UserRole, UserRoleKey> userRoles,
    IPgRepo<Role, int> roles,
    IPgRepo<Message, int> messages,
    IPgRepo<DraftMessage, int> draftMessages,
    IPgRepo<MessageReaction, MessageReactionKey> reactions,
    IPgRepo<Attachment, int> attachments,
    IPgRepo<UrlPreview, int> urlPreviews,
    IPgRepo<Channel, int> channels,
    IPgRepo<ChannelType, int> channelTypes,
    IPgRepo<ChannelGroup, int> channelGroups,
    IPgRepo<Config, int> configs,
    IPgRepo<Workspace, int> workspaces,
    IPgRepo<SecureObject, int> secureObjects,
    IPgRepo<RoleAcl, RoleAclKey> roleAcls,
    IPgRepo<UserAcl, UserAclKey> userAcls)
{
    public IPgRepo<User, Guid> Users { get; } = users;
    public IPgRepo<UserRole, UserRoleKey> UserRoles { get; } = userRoles;
    public IPgRepo<Role, int> Roles { get; } = roles;
    public IPgRepo<Message, int> Messages { get; } = messages;
    public IPgRepo<DraftMessage, int> DraftMessages { get; } = draftMessages;
    public IPgRepo<MessageReaction, MessageReactionKey> Reactions { get; } = reactions;
    public IPgRepo<Attachment, int> Attachments { get; } = attachments;
    public IPgRepo<UrlPreview, int> UrlPreviews { get; } = urlPreviews;
    public IPgRepo<Channel, int> Channels { get; } = channels;
    public IPgRepo<ChannelType, int> ChannelTypes { get; } = channelTypes;
    public IPgRepo<ChannelGroup, int> ChannelGroups { get; } = channelGroups;
    public IPgRepo<Config, int> Configs { get; } = configs;
    public IPgRepo<Workspace, int> Workspaces { get; } = workspaces;
    public IPgRepo<SecureObject, int> SecureObjects { get; } = secureObjects;
    public IPgRepo<RoleAcl, RoleAclKey> RoleAcls { get; } = roleAcls;
    public IPgRepo<UserAcl, UserAclKey> UserAcls { get; } = userAcls;
}
