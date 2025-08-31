namespace ChatneyBackend.Domains.Messages;

public class MessagePermissions
{
    public const string DeleteMessage = DomainSettings.PermissionsPrefix + ".deleteOwnMessage";
    public const string EditMessage = DomainSettings.PermissionsPrefix + ".editOwnMessage";
    public const string CreateMessage = DomainSettings.PermissionsPrefix + ".createMessage";
    public const string ReadMessage = DomainSettings.PermissionsPrefix + ".readMessage";
}
