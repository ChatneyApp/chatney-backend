using System.Reflection;
using System.Text.Json;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using NpgsqlTypes;

namespace ChatneyBackend.Infra;

public static class SecureObjectHelper
{
    /// <summary>Convenience alias so callers don't need a direct reference to <see cref="SecureObjectIds"/>.</summary>
    public const int GlobalId = SecureObjectIds.Global;

    /// <summary>
    /// Permissions that apply to a securable OBJECT (workspace/channel-type/channel), as opposed to
    /// role.*/user.*/config.*/attachment.* which are global-only (see D2). Derived directly from the
    /// <c>permission</c> enum's Postgres names rather than a hand-copied list, so a new
    /// workspace.*/channel.* value can never silently fall out of sync; see
    /// <c>SecureObjectHelperTests</c> for a guard that cross-checks this against
    /// <see cref="PermissionGroups"/>.
    /// </summary>
    public static readonly IReadOnlyList<Permission> ObjectScopedPermissions =
        Enum.GetValues<Permission>().Where(IsObjectScoped).ToArray();

    private static bool IsObjectScoped(Permission permission)
    {
        var pgName = typeof(Permission)
            .GetField(permission.ToString())!
            .GetCustomAttribute<PgNameAttribute>()!
            .PgName;

        return pgName.StartsWith("workspace.", StringComparison.Ordinal) ||
               pgName.StartsWith("channel.", StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a new secure object with the given description and, if the admin role already
    /// exists, grants it every object-scoped permission on the new object. Without this, D2 means a
    /// newly created workspace/channel-type/channel would be invisible to EVERYONE, including admin.
    ///
    /// Ordering note: this only works if the admin role has already been seeded by the time the
    /// first workspace/channel-type/channel is created (true for the normal install flow - see
    /// InstallWizardMutations). If the admin role does not exist yet, no role_acls row is written
    /// here and the object is left without an admin grant rather than throwing.
    /// </summary>
    public static async Task<int> Create(AppRepos repos, SecureObjectDescription description)
    {
        // RepoDb 1.13.1 pins the @Description parameter's NpgsqlDbType to Text before Npgsql's
        // dynamic-JSON path is reachable, so InsertOne(new SecureObject { Description = ... }) throws
        // InvalidCastException against the jsonb column. Serializing explicitly and casting in SQL
        // sidesteps that - see SecureObjectHelperTests for the round-trip guard.
        var secObjId = await repos.SecureObjects.ExecuteScalarAsync<int>(
            "INSERT INTO secure_objects (description) VALUES (@Description::jsonb) RETURNING id",
            new { Description = JsonSerializer.Serialize(description) });

        var adminRole = await repos.Roles.GetOne(role => role.Name == DomainSettings.AdminRoleName);

        if (adminRole != null)
        {
            await repos.RoleAcls.Upsert(new RoleAcl
            {
                RoleId = adminRole.Id,
                SecObjId = secObjId,
                Permissions = [.. ObjectScopedPermissions],
            });
        }

        return secObjId;
    }
}
