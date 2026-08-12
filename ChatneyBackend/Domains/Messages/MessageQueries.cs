using System.Security.Claims;
using ChatneyBackend.Domains.Permissions;
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
        IPermissionResolver resolver,
        ClaimsPrincipal principal,
        int channelId)
    {
        var channel = await repos.Channels.GetById(channelId);
        if (channel is null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        var permissions = await resolver.ForChannel(channel!);
        if (!permissions.Can(Permission.ChannelReadMessage))
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        var userId = principal.GetUserGuid();

        return await GetMessagesHydrated(repos, userId,
            m => m.ChannelId == channelId && m.ParentId == null);
    }

    [Authorize]
    public async Task<MessagesResult> GetListThreadMessages(
        AppRepos repos,
        IPermissionResolver resolver,
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

        var permissions = await resolver.ForChannel(channel!);
        if (!permissions.Can(Permission.ChannelReadMessage))
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        var userId = principal.GetUserGuid();

        return await GetMessagesHydrated(repos, userId,
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
