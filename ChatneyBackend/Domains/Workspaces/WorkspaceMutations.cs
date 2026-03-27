using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceMutations
{
    public async Task<Workspace> AddWorkspace(PgRepo<Workspace, int> workspaceRepo, WorkspaceDTO workspaceDto)
    {
        var workspace = Workspace.FromDTO(workspaceDto);
        workspace.Id = await workspaceRepo.InsertOne(workspace);
        return workspace;
    }

    public async Task<Workspace?> UpdateWorkspace(PgRepo<Workspace, int> workspaceRepo, Workspace workspace)
    {
        var updated = await workspaceRepo.UpdateOne(workspace);
        return updated ? workspace : null;
    }

    public async Task<bool> DeleteWorkspace(PgRepo<Workspace, int> workspaceRepo, int id) =>
        await workspaceRepo.DeleteById(id);
}
