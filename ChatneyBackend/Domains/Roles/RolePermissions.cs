namespace ChatneyBackend.Domains.Roles;

public class RolePermissions
{
    public const string DeleteRole = DomainSettings.PermissionsPrefix + ".deleteRole";
    public const string EditRole   = DomainSettings.PermissionsPrefix + ".editRole";
    public const string CreateRole = DomainSettings.PermissionsPrefix + ".createRole";
    public const string ReadRole   = DomainSettings.PermissionsPrefix + ".readRole";
}
