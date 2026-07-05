namespace ChatneyBackend.Domains.Roles;

public class UserPermissions
{
    public IReadOnlyList<string> Permissions { get; }

    private readonly bool _allMighty;

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

        _allMighty = Permissions.Contains(RolePermissions.AllMighty);
    }

    public bool Can(params string[] permissions) =>
        _allMighty || permissions.All(p => Permissions.Contains(p));

    public void Require(params string[] permissions)
    {
        if (_allMighty)
        {
            return;
        }

        foreach (var permission in permissions)
        {
            if (!Can(permission))
            {
                ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
            }
        }
    }
}
