namespace ChatneyBackend.Domains.Users;

public class DomainSettings
{
    public const string UserTableName = "users";
    public const string UserRoleTableName = "user_roles";
    public const string PasswordResetRequestTableName = "password_reset_requests";
    public const string AvatarS3Folder = "avatars";
    // Avatars are square: both width and height must equal this, in pixels.
    public const int AvatarSideSize = 128;
    public const long AvatarMaxFileSize = 2 * 1024 * 1024;
}
