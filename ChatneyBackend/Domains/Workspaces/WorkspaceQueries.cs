using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceQueries
{
    /// <summary>A workspace by id, or null if it doesn't exist. Requires WorkspaceReadWorkspace on it.</summary>
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

    /// <summary>A workspace by exact name, or null if it doesn't exist. Requires WorkspaceReadWorkspace on it.</summary>
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

    /// <summary>Workspaces the current user has WorkspaceReadWorkspace on.</summary>
    [Authorize]
    public async Task<List<Workspace>> GetList(IPermissionResolver resolver) =>
        // Served from the resolver's AclSnapshot (already a full table read for permission
        // resolution) instead of a second `repos.Workspaces.GetList()` scan.
        await resolver.VisibleWorkspaces(Permission.WorkspaceReadWorkspace);
}
