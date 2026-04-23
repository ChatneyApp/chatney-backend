using HotChocolate.Authorization;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Workspaces;

public class WorkspaceQueries
{
    [Authorize]
    public async Task<Workspace?> GetWorkspaceById(AppRepos repos, int id) => await repos.Workspaces.GetById(id);

    [Authorize]
    public async Task<Workspace?> GetWorkspaceByName(AppRepos repos, string name) =>
        await repos.Workspaces.GetOne(workspace => workspace.Name == name);

    [Authorize]
    public async Task<List<Workspace>> GetList(AppRepos repos) => await repos.Workspaces.GetList();
}
