namespace ChatneyBackend.Domains.Workspaces;

public class WorkspacePermissions
{
    public const string CreateWorkspace = DomainSettings.PermissionsPrefix + ".createWorkspace";
    public const string DeleteWorkspace = DomainSettings.PermissionsPrefix + ".deleteWorkspace";
    public const string UpdateWorkspace = DomainSettings.PermissionsPrefix + ".updateWorkspace";
    public const string ReadWorkspace   = DomainSettings.PermissionsPrefix + ".readWorkspace";
}
