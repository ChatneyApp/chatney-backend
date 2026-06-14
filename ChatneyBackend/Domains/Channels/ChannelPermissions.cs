namespace ChatneyBackend.Domains.Channels;

public class ChannelPermissions
{
    public const string DeleteMessage = DomainSettings.PermissionsPrefix + ".deleteMessage";
    public const string EditMessage   = DomainSettings.PermissionsPrefix + ".editMessage";
    public const string CreateMessage = DomainSettings.PermissionsPrefix + ".createMessage";
    public const string ReadMessage   = DomainSettings.PermissionsPrefix + ".readMessage";

    public const string DeleteChannel = DomainSettings.PermissionsPrefix + ".deleteChannel";
    public const string EditChannel   = DomainSettings.PermissionsPrefix + ".editChannel";
    public const string CreateChannel = DomainSettings.PermissionsPrefix + ".createChannel";
    public const string ReadChannel   = DomainSettings.PermissionsPrefix + ".readChannel";

    public const string DeleteChannelType = DomainSettings.PermissionsPrefix + ".deleteChannelType";

    public const string EditChannelGroup = DomainSettings.PermissionsPrefix + ".editChannelGroup";
    public const string AddChannelGroup = DomainSettings.PermissionsPrefix + ".addChannelGroup";
    public const string DeleteChannelGroup = DomainSettings.PermissionsPrefix + ".deleteChannelGroup";

    public const string DeleteOwnMessage = DomainSettings.PermissionsPrefix + ".deleteOwnMessage";
    public const string EditOwnMessage = DomainSettings.PermissionsPrefix + ".editOwnMessage";

}
