using System.Security.Claims;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Messages;

public class MessageQueries
{
    [Authorize]
    public async Task<MessagesResult> GetListChannelMessages(
        AppRepos repos,
        ClaimsPrincipal principal,
        int channelId) =>
        await GetMessagesHydrated(repos, principal.GetUserGuid(),
            m => m.ChannelId == channelId && m.ParentId == null);

    [Authorize]
    public async Task<MessagesResult> GetListThreadMessages(
        AppRepos repos,
        ClaimsPrincipal principal,
        int threadId) =>
        await GetMessagesHydrated(repos, principal.GetUserGuid(),
            m => m.ParentId == threadId);

    private static async Task<MessagesResult> GetMessagesHydrated(
        AppRepos repos,
        Guid currentUserId,
        System.Linq.Expressions.Expression<Func<Message, bool>> where)
    {
        var messages = await repos.Messages.GetList(where);
        return await MessageHydrator.HydrateAsync(messages, repos, currentUserId);
    }
}
