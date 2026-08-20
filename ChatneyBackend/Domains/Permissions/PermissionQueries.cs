namespace ChatneyBackend.Domains.Permissions;

public record PermissionGroup(string Label, Permission[] List);

public class PermissionQueries
{
    public PermissionGroup[] GetList()
    {
        return PermissionGroups.All;
    }
}
