using System.Security.Claims;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Attachments;

public class AttachmentQueries
{
    [Authorize]
    public async Task<Attachment?> GetById(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var permissions = await roleManager.GetUserPermissions(
            repos, principal.GetUserGuid(), RoleScope.Global());
        if (!permissions.Can(AttachmentPermissions.Read)) ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();

        return await repos.Attachments.GetById(id);
    }
}
