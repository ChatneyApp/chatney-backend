using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Attachments;

public class AttachmentQueries
{
    [Authorize]
    public async Task<Attachment?> GetById(
        AppRepos repos,
        IPermissionResolver resolver,
        int id)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.AttachmentRead);

        return await repos.Attachments.GetById(id);
    }
}
