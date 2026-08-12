using System.Security.Claims;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.DraftMessages;

public class DraftMessageQueries
{
    [Authorize]
    public async Task<List<DraftMessage>> GetDraftMessages(
        ClaimsPrincipal principal,
        AppRepos repos,
        IPermissionResolver resolver)
    {
        var userId = principal.GetUserGuid();
        var drafts = await repos.DraftMessages.GetList(m => m.UserId == userId);

        var channelIds = drafts.Select(d => d.ChannelId).Distinct().ToList();
        var channels = await repos.Channels.GetList(c => channelIds.Contains(c.Id));
        var channelMap = channels.ToDictionary(c => c.Id);

        var readableDrafts = new List<DraftMessage>();
        foreach (var draft in drafts)
        {
            if (!channelMap.TryGetValue(draft.ChannelId, out var channel))
            {
                continue;
            }

            var permissions = await resolver.ForChannel(channel);
            if (permissions.Can(Permission.ChannelReadChannel))
            {
                readableDrafts.Add(draft);
            }
        }

        return readableDrafts;
    }
}
