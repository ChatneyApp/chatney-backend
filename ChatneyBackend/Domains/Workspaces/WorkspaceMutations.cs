using System.Security.Claims;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceMutations
{
    [Authorize]
    public async Task<Workspace> AddWorkspace(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        WorkspaceDto workspaceDto)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        permissions.Require(WorkspacePermissions.CreateWorkspace);

        var workspace = Workspace.FromDto(workspaceDto);
        workspace.Id = await repos.Workspaces.InsertOne(workspace);
        return workspace;
    }

    [Authorize]
    public async Task<Workspace?> UpdateWorkspace(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        Workspace workspace)
    {
        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromWorkspace(workspace));
        permissions.Require(WorkspacePermissions.UpdateWorkspace);

        var updated = await repos.Workspaces.UpdateOne(workspace);
        return updated ? workspace : null;
    }

    [Authorize]
    public async Task<bool> DeleteWorkspace(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var workspace = await repos.Workspaces.GetById(id);
        if (workspace == null)
        {
            return false;
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromWorkspace(workspace));
        permissions.Require(WorkspacePermissions.DeleteWorkspace);

        return await repos.Workspaces.DeleteById(id);
    }
}
