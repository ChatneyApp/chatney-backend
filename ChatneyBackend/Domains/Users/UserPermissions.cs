namespace ChatneyBackend.Domains.Users;

public class UserPermissions
{
    public const string DeleteUser = DomainSettings.PermissionsPrefix + ".deleteUser";
    public const string EditUser   = DomainSettings.PermissionsPrefix + ".editUser";
    public const string CreateUser = DomainSettings.PermissionsPrefix + ".createUser";
    public const string ReadUser   = DomainSettings.PermissionsPrefix + ".readUser";
}
