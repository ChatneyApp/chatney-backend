using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
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
        RoleManager roleManager)
    {
        var user = await principal.GetRequiredUser(repos);
        var userId = user.Id;
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

            var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel));
            if (permissions.Can(ChannelPermissions.ReadChannel))
            {
                readableDrafts.Add(draft);
            }
        }

        return readableDrafts;
    }
}
