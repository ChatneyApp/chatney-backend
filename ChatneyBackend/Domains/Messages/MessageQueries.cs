using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Messages;

public class MessageQueries
{
    [Authorize]
    public async Task<MessagesResult> GetListChannelMessages(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int channelId)
    {
        var channel = await repos.Channels.GetById(channelId);
        if (channel is null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel!));
        if (!permissions.Can(ChannelPermissions.ReadMessage))
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        return await GetMessagesHydrated(repos, user.Id,
            m => m.ChannelId == channelId && m.ParentId == null);
    }

    [Authorize]
    public async Task<MessagesResult> GetListThreadMessages(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int threadId)
    {
        var threadMessage = await repos.Messages.GetById(threadId);
        if (threadMessage is null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        var channel = await repos.Channels.GetById(threadMessage!.ChannelId);
        if (channel is null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        var user = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel!));
        if (!permissions.Can(ChannelPermissions.ReadMessage))
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        return await GetMessagesHydrated(repos, user.Id,
            m => m.ParentId == threadId);
    }

    private static async Task<MessagesResult> GetMessagesHydrated(
        AppRepos repos,
        Guid currentUserId,
        System.Linq.Expressions.Expression<Func<Message, bool>> where)
    {
        var messages = await repos.Messages.GetList(where);
        return await MessageHydrator.HydrateAsync(messages, repos, currentUserId);
    }
}
