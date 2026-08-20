using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceQueries
{
    [Authorize]
    public async Task<Workspace?> GetWorkspaceById(
        AppRepos repos,
        IPermissionResolver resolver,
        int id)
    {
        var workspace = await repos.Workspaces.GetById(id);
        if (workspace == null)
        {
            return null;
        }

        var permissions = await resolver.ForWorkspace(workspace);
        permissions.Require(Permission.WorkspaceReadWorkspace);

        return workspace;
    }

    [Authorize]
    public async Task<Workspace?> GetWorkspaceByName(
        AppRepos repos,
        IPermissionResolver resolver,
        string name)
    {
        var workspace = await repos.Workspaces.GetOne(w => w.Name == name);
        if (workspace == null)
        {
            return null;
        }

        var permissions = await resolver.ForWorkspace(workspace);
        permissions.Require(Permission.WorkspaceReadWorkspace);

        return workspace;
    }

    [Authorize]
    public async Task<List<Workspace>> GetList(IPermissionResolver resolver) =>
        // Served from the resolver's AclSnapshot (already a full table read for permission
        // resolution) instead of a second `repos.Workspaces.GetList()` scan.
        await resolver.VisibleWorkspaces(Permission.WorkspaceReadWorkspace);
}
