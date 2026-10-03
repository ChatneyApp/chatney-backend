namespace ChatneyBackend.Domains.Permissions;

/// <summary>A labelled set of related permissions, for rendering the admin permission editor.</summary>
/// <param name="Label">Human-readable group name.</param>
/// <param name="List">Permissions in this group.</param>
public record PermissionGroup(string Label, Permission[] List);

public class PermissionQueries
{
    /// <summary>Every permission, grouped by domain. Each permission appears in exactly one group.</summary>
    public PermissionGroup[] GetList()
    {
        return PermissionGroups.All;
    }
}
