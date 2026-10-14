using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceMutations
{
    /// <summary>Creates a workspace. Requires WorkspaceCreateWorkspace globally.</summary>
    [Authorize]
    public async Task<Workspace> AddWorkspace(
        AppRepos repos,
        IPermissionResolver resolver,
        WorkspaceDto workspaceDto,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.WorkspaceCreateWorkspace);

        var workspace = Workspace.FromDto(workspaceDto);
        workspace.SecObjId = await SecureObjectHelper.Create(
            repos,
            new SecureObjectDescription { Kind = "workspace", Name = workspace.Name });
        workspace.Id = await repos.Workspaces.InsertOne(workspace);
        await webSocketConnector.SendNewWorkspaceAsync(WebsocketWorkspacePayload.FromWorkspace(workspace));
        resolver.Invalidate();
        return workspace;
    }

    /// <summary>Updates a workspace, matched by id. Requires WorkspaceUpdateWorkspace on it.</summary>
    /// <returns>The saved workspace, or null if it doesn't exist.</returns>
    [Authorize]
    public async Task<Workspace?> UpdateWorkspace(
        AppRepos repos,
        IPermissionResolver resolver,
        Workspace workspace,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.ForWorkspace(workspace);
        permissions.Require(Permission.WorkspaceUpdateWorkspace);

        var updated = await repos.Workspaces.UpdateOne(workspace);
        if (updated)
        {
            await webSocketConnector.SendUpdatedWorkspaceAsync(WebsocketWorkspacePayload.FromWorkspace(workspace));
        }
        return updated ? workspace : null;
    }

    /// <summary>Deletes a workspace. Requires WorkspaceDeleteWorkspace on it.</summary>
    /// <returns>True if the workspace existed and was deleted.</returns>
    [Authorize]
    public async Task<bool> DeleteWorkspace(
        AppRepos repos,
        IPermissionResolver resolver,
        int id,
        WebSocketConnector webSocketConnector)
    {
        var workspace = await repos.Workspaces.GetById(id);
        if (workspace == null)
        {
            return false;
        }

        var permissions = await resolver.ForWorkspace(workspace);
        permissions.Require(Permission.WorkspaceDeleteWorkspace);

        var deleted = await repos.Workspaces.DeleteById(id);
        if (deleted)
        {
            await webSocketConnector.SendDeletedWorkspaceAsync(new WebsocketWorkspaceDeletedPayload { Id = id });
        }
        return deleted;
    }
}
