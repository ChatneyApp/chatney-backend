using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceMutations
{
    public async Task<Workspace> AddWorkspace(AppRepos repos, WorkspaceDto workspaceDto)
    {
        var workspace = Workspace.FromDto(workspaceDto);
        workspace.Id = await repos.Workspaces.InsertOne(workspace);
        return workspace;
    }

    public async Task<Workspace?> UpdateWorkspace(AppRepos repos, Workspace workspace)
    {
        var updated = await repos.Workspaces.UpdateOne(workspace);
        return updated ? workspace : null;
    }

    public async Task<bool> DeleteWorkspace(AppRepos repos, int id) =>
        await repos.Workspaces.DeleteById(id);
}
