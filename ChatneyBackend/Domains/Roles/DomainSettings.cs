namespace ChatneyBackend.Domains.Roles;

public class DomainSettings
{
    public const string RoleCollectionName = "roles";
    public const string SecureObjectTableName = "secure_objects";
    public const string RoleAclTableName = "role_acls";
    public const string UserAclTableName = "user_acls";

    public const string AdminRoleName = "admin";
    public const string ModeratorRoleName = "moderator";
    public const string UserRoleName = "user";
}
