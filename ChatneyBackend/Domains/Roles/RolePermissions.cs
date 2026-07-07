namespace ChatneyBackend.Domains.Roles;

public class RolePermissions
{
    public const string AllMighty  = "AllMighty";

    public const string DeleteRole = DomainSettings.PermissionsPrefix + ".deleteRole";
    public const string EditRole   = DomainSettings.PermissionsPrefix + ".editRole";
    public const string CreateRole = DomainSettings.PermissionsPrefix + ".createRole";
}
