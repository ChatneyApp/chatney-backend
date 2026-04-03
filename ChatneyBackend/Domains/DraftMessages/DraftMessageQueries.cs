using System.Security.Claims;
using HotChocolate.Authorization;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;

namespace ChatneyBackend.Domains.DraftMessages;

public class DraftMessageQueries
{
    [Authorize]
    public async Task<List<DraftMessage>> GetDraftMessages(ClaimsPrincipal principal, PgRepo<DraftMessage, int> repo) =>
        await repo.GetList(
            m => m.UserId == principal.GetUserGuid()
        );
}
