namespace ChatneyBackend.Domains.Roles;

public class UserPermissions
{
    public IReadOnlyList<string> Permissions { get; }

    public UserPermissions(
        IEnumerable<string> rolePermissions,
        IEnumerable<string> allowlist,
        IEnumerable<string> denylist)
    {
        Permissions = rolePermissions
            .Concat(allowlist)
            .Except(denylist)
            .Distinct()
            .ToArray();
    }

    public bool Can(string permission) => Permissions.Contains(permission);
}
