using System.Security.Claims;
using System.Threading.Channels;
using ChatneyBackend.Domains.Channels;
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

        var perms = await roleManager.GetUserPermissions(user, RoleScope.Global());

        var haveAccessToAnyWorkspace = perms.Can(ChannelPermissions.ReadMessage);
        if (haveAccessToAnyWorkspace)
        {
            return await repos.Workspaces.GetList();
        }

        var allUserChannels = await roleManager.GetPermittedChannels(repos, user.Id);
        var distinctWorkspaceIds = allUserChannels
            .Select(c => c.WorkspaceId)
            .Distinct()
            .ToList();

        var workspaces = await repos.Workspaces.GetList(w => distinctWorkspaceIds.Contains(w.Id));
        return workspaces;
    }
}
