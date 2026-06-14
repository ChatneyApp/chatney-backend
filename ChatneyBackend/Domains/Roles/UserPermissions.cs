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

    public bool Can(params string[] permissions) => permissions.All(p => Permissions.Contains(p));

    public void Require(params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (!Can(permission))
            {
                ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
            }
        }
    }
}
