using System.Security.Claims;
using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Messages;

public class MessageQueries
{
    // [Authorize]
    // public async Task<Message?> GetMessageById(PgRepo<Message, int> repo, int id) => await repo.GetById(id);

    [Authorize]
    public async Task<MessagesResult> GetListChannelMessages(
        PgRepo<Message, int> repo,
        PgRepo<Attachment, int> attachmentRepo,
        PgRepo<User, Guid> usersRepo,
        PgRepo<UrlPreview, int> urlPreviewRepo,
        PgRepo<MessageReaction, MessageReactionKey> reactionRepo,
        ClaimsPrincipal principal,
        int channelId) =>
        await GetMessagesHydrated(repo, attachmentRepo, usersRepo, urlPreviewRepo, reactionRepo,
            principal.GetUserGuid(),
            m => m.ChannelId == channelId && m.ParentId == null);

    [Authorize]
    public async Task<MessagesResult> GetListThreadMessages(
        PgRepo<Message, int> repo,
        PgRepo<Attachment, int> attachmentRepo,
        PgRepo<User, Guid> usersRepo,
        PgRepo<UrlPreview, int> urlPreviewRepo,
        PgRepo<MessageReaction, MessageReactionKey> reactionRepo,
        ClaimsPrincipal principal,
        int threadId) =>
        await GetMessagesHydrated(repo, attachmentRepo, usersRepo, urlPreviewRepo, reactionRepo,
            principal.GetUserGuid(),
            m => m.ParentId == threadId);

    private static async Task<MessagesResult> GetMessagesHydrated(
        PgRepo<Message, int> messageRepo,
        PgRepo<Attachment, int> attachmentRepo,
        PgRepo<User, Guid> usersRepo,
        PgRepo<UrlPreview, int> urlPreviewRepo,
        PgRepo<MessageReaction, MessageReactionKey> reactionRepo,
        Guid currentUserId,
        System.Linq.Expressions.Expression<Func<Message, bool>> where)
    {
        var messages = await messageRepo.GetList(where);
        return await MessageHydrator.HydrateAsync(messages, messageRepo, attachmentRepo, usersRepo, urlPreviewRepo, reactionRepo, currentUserId);
    }
}
