namespace ChatneyBackend.Domains.Roles;

public class UserPermissions
{
    public string[] Permissions { get; }

    public UserPermissions(string[] permissions)
    {
        Permissions = permissions;
    }

    public bool Can(string permission) => Permissions.Contains(permission);
}
