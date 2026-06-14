namespace ChatneyBackend.Domains.Attachments;

public class AttachmentPermissions
{
    public const string Upload = DomainSettings.PermissionsPrefix + ".upload";
    public const string Read   = DomainSettings.PermissionsPrefix + ".read";
    public const string Delete = DomainSettings.PermissionsPrefix + ".delete";
}
