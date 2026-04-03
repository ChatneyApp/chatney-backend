using HotChocolate.Authorization;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceQueries
{
    [Authorize]
    public async Task<Workspace?> GetWorkspaceById(PgRepo<Workspace, int> repo, int id) => await repo.GetById(id);

    [Authorize]
    public async Task<Workspace?> GetWorkspaceByName(PgRepo<Workspace, int> repo, string name) =>
        await repo.GetOne(workspace => workspace.Name == name);

    [Authorize]
    public async Task<List<Workspace>> GetList(PgRepo<Workspace, int> repo) => await repo.GetList();
}
