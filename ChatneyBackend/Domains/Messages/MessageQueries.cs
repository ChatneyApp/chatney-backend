using System.Security.Claims;
using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;
using ChatneyBackend.Infra;

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

        var allMessageIds = messages.Select(m => m.Id).ToArray();
        var allAttachmentIds = messages.SelectMany(m => m.AttachmentIds).Distinct().ToArray();
        var allUrlPreviewIds = messages.SelectMany(m => m.UrlPreviewIds).Distinct().ToArray();
        var allUserIds = messages.Select(m => m.UserId).Distinct().ToArray();
        var allReplyToIds = messages.Select(m => m.ReplyTo).Where(id => id != null).Select(id => id!.Value).Distinct().ToArray();

        var attachmentsByIdTask = allAttachmentIds.Length > 0
            ? attachmentRepo.GetList(a => allAttachmentIds.Contains(a.Id))
            : Task.FromResult(new List<Attachment>());

        var urlPreviewsByIdTask = allUrlPreviewIds.Length > 0
            ? urlPreviewRepo.GetList(u => allUrlPreviewIds.Contains(u.Id))
            : Task.FromResult(new List<UrlPreview>());

        var usersByIdTask = allUserIds.Length > 0
            ? usersRepo.GetList(u => allUserIds.Contains(u.Id))
            : Task.FromResult(new List<User>());

        var reactionsByIdTask = allMessageIds.Length > 0
            ? reactionRepo.GetList(r => allMessageIds.Contains(r.MessageId))
            : Task.FromResult(new List<MessageReaction>());

        var replyToMessagesTask = allReplyToIds.Length > 0
            ? messageRepo.GetList(m => allReplyToIds.Contains(m.Id))
            : Task.FromResult(new List<Message>());

        await Task.WhenAll(attachmentsByIdTask, urlPreviewsByIdTask, usersByIdTask, reactionsByIdTask, replyToMessagesTask);

        var attachmentsById = (await attachmentsByIdTask).ToDictionary(a => a.Id);
        var urlPreviewsById = (await urlPreviewsByIdTask).ToDictionary(u => u.Id);
        var usersById = (await usersByIdTask).ToDictionary(u => u.Id);
        var reactionsByMessageId = (await reactionsByIdTask).GroupBy(r => r.MessageId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var replyToMessages = (await replyToMessagesTask)
            .Select(m => new ReplyToMessage { Id = m.Id, UserId = m.UserId, Content = m.Content })
            .ToList();

        var hydratedMessages = messages
            .Where(m => usersById.ContainsKey(m.UserId))
            .Select(m =>
            {
                var attachments = m.AttachmentIds
                    .Where(attachmentsById.ContainsKey)
                    .Select(id => attachmentsById[id])
                    .ToList();

                var urlPreviews = m.UrlPreviewIds
                    .Where(urlPreviewsById.ContainsKey)
                    .Select(id => urlPreviewsById[id])
                    .ToList();

                var messageReactions = reactionsByMessageId.GetValueOrDefault(m.Id, []);

                var reactions = messageReactions
                    .GroupBy(r => r.Code)
                    .Select(g => new ReactionInMessage { Code = g.Key, Count = g.Count() })
                    .ToList();

                var myReactions = messageReactions
                    .Where(r => r.UserId == currentUserId)
                    .Select(r => r.Code)
                    .ToArray();

                var messageWithUser = MessageWithUser.Create(m, usersById[m.UserId], urlPreviews, attachments);
                messageWithUser.Reactions = reactions;
                messageWithUser.MyReactions = myReactions;
                return messageWithUser;
            })
            .ToList();

        return new MessagesResult { Messages = hydratedMessages, Refs = replyToMessages };
    }
}
