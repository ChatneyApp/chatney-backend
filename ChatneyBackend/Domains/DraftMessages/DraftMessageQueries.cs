using System.Security.Claims;
using HotChocolate.Authorization;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;

namespace ChatneyBackend.Domains.DraftMessages;

public class DraftMessageQueries
{
    [Authorize]
    public async Task<List<DraftMessage>> GetDraftMessages(ClaimsPrincipal principal, AppRepos repos) =>
        await repos.DraftMessages.GetList(
            m => m.UserId == principal.GetUserGuid()
        );
}
