using System.Security.Claims;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceQueries
{
    [Authorize]
    public async Task<Workspace?> GetWorkspaceById(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var workspace = await repos.Workspaces.GetById(id);
        if (workspace == null)
        {
            return null;
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromWorkspace(workspace));
        permissions.Require(WorkspacePermissions.ReadWorkspace);

        return workspace;
    }

    [Authorize]
    public async Task<Workspace?> GetWorkspaceByName(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        string name)
    {
        var workspace = await repos.Workspaces.GetOne(w => w.Name == name);
        if (workspace == null)
        {
            return null;
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromWorkspace(workspace));
        permissions.Require(WorkspacePermissions.ReadWorkspace);

        return workspace;
    }

    [Authorize]
    public async Task<List<Workspace>> GetList(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        if (permissions.Can(WorkspacePermissions.ReadWorkspace))
        {
            return await repos.Workspaces.GetList();
        }

        var workspaceIds = user.WorkspaceIds;
        if (workspaceIds.Length == 0)
        {
            return [];
        }

        var workspaces = await repos.Workspaces.GetList(w => workspaceIds.Contains(w.Id));
        var readable = new List<Workspace>();
        foreach (var workspace in workspaces)
        {
            var scopedPermissions = await roleManager.GetUserPermissions(user, RoleScope.FromWorkspace(workspace));
            if (scopedPermissions.Can(WorkspacePermissions.ReadWorkspace))
            {
                readable.Add(workspace);
            }
        }

        return readable;
    }
}
