using System.Security.Claims;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Messages;

public class MessageQueries
{
    /// <summary>
    /// A page of top-level messages of a channel (thread replies excluded), newest first.
    /// An empty messages list means the beginning of the channel history is reached.
    /// Requires ChannelReadMessage on the channel.
    /// </summary>
    /// <param name="limit">Maximum number of messages to return (capped at 100).</param>
    /// <param name="beforeId">Return only messages older than the message with this id. Omit to get the newest messages.</param>
    [Authorize]
    public async Task<MessagesResult> GetListChannelMessages(
        AppRepos repos,
        IPermissionResolver resolver,
        ClaimsPrincipal principal,
        int channelId,
        int limit,
        int? beforeId = null)
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
            "channel_id = @ScopeId AND parent_id IS NULL", channelId, limit, beforeId);
    }

    /// <summary>
    /// A page of replies in a thread, newest first.
    /// An empty messages list means the beginning of the thread is reached.
    /// Requires ChannelReadMessage on the thread's channel.
    /// </summary>
    /// <param name="threadId">Id of the thread's root message.</param>
    /// <param name="limit">Maximum number of messages to return (capped at 100).</param>
    /// <param name="beforeId">Return only messages older than the message with this id. Omit to get the newest messages.</param>
    [Authorize]
    public async Task<MessagesResult> GetListThreadMessages(
        AppRepos repos,
        IPermissionResolver resolver,
        ClaimsPrincipal principal,
        int threadId,
        int limit,
        int? beforeId = null)
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
            "parent_id = @ScopeId", threadId, limit, beforeId);
    }

    private static async Task<MessagesResult> GetMessagesHydrated(
        AppRepos repos,
        Guid currentUserId,
        string scopeSql,
        int scopeId,
        int limit,
        int? beforeId)
    {
        var pageSize = Math.Clamp(limit, 1, DomainSettings.MaxMessagesPageSize);
        var cursorSql = beforeId == null ? "" : "AND id < @BeforeId";

        object param = beforeId == null
            ? new { ScopeId = scopeId, Limit = pageSize }
            : new { ScopeId = scopeId, Limit = pageSize, BeforeId = beforeId.Value };

        var messages = await repos.Messages.GetList(
            $"""
            SELECT *
            FROM {DomainSettings.MessageTableName}
            WHERE {scopeSql}
            {cursorSql}
            ORDER BY id DESC
            LIMIT @Limit;
            """,
            param);

        return await MessageHydrator.HydrateAsync(messages, repos, currentUserId);
    }
}
