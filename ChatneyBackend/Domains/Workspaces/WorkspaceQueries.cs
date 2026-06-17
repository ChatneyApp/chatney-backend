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
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.Global());
        if (permissions.Can(WorkspacePermissions.ReadWorkspace))
        {
            return await repos.Workspaces.GetList();
        }

        var userRoles = await repos.UserRoles.GetList(r => r.UserId == user.Id);
        var roles = await repos.Roles.GetList(r => userRoles.Any(ur => ur.RoleId == r.Id) || r.Id == user.RoleId);
        
        var haveAccessToAnyWorkspace = roles.Any(r => 
            r.Id == user.RoleId && r.Permissions.Contains(ChannelPermissions.ReadMessage));
        if (haveAccessToAnyWorkspace)
        {
            return await repos.Workspaces.GetList();
        }

        var channels = await repos.Channels.GetList();

        var channelsByChannelTypesForUser = channels.FindAll(c => {
            var userRole = userRoles.Find(r => r.ChannelTypeId == c.ChannelTypeId);
            if (userRole == null)
            {
                return false;
            }

            var role = roles.Find(r => r.Id == userRole.RoleId);
            if (role == null)
            {
                return false;
            }

            return role.Permissions.Contains(ChannelPermissions.ReadMessage);
        });

        var channelsByworkspacesForUser = channels
            .FindAll(c => {
                var userRole = userRoles.Find(r => r.WorkspaceId == c.WorkspaceId);
                if (userRole == null)
                {
                    return false;
                }

                var role = roles.Find(r => r.Id == userRole.RoleId);
                if (role == null)
                {
                    return false;
                }

                return role.Permissions.Contains(ChannelPermissions.ReadMessage);
            });
            
        var channelsForUser = channels.FindAll(c => {
            var userRole = userRoles.Find(r => r.ChannelId == c.Id);
            if (userRole == null)
            {
                return false;
            }

            var role = roles.Find(r => r.Id == userRole.RoleId);
            if (role == null)
            {
                return false;
            }

            return role.Permissions.Contains(ChannelPermissions.ReadMessage);
        });
   

        // Combine all relevant channels for the user and take distinct workspace ids
        var allUserChannels = channelsByChannelTypesForUser
            .Concat(channelsByworkspacesForUser)
            .Concat(channelsForUser);

        var distinctWorkspaceIds = allUserChannels
            .Select(c => c.WorkspaceId)
            .Distinct()
            .ToList();
      
      
        var workspaces = await repos.Workspaces.GetList(w => distinctWorkspaceIds.Contains(w.Id));
        return workspaces;
    }
}
