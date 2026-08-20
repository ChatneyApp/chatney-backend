using ChatneyBackend.Infra;
using Npgsql;
using NpgsqlTypes;

namespace ChatneyBackend.Domains.Messages;

public static class MessageMutationQueries
{
    public const string IncrementChildrenCountSql =
        """
        UPDATE messages
        SET children_count = children_count + 1,
            updated_at = NOW()
        WHERE id = @Id
        RETURNING children_count;
        """;

    public const string DecrementChildrenCountSql =
        """
        UPDATE messages
        SET children_count = GREATEST(children_count - 1, 0),
            updated_at = NOW()
        WHERE id = @Id
        RETURNING children_count;
        """;

    public const string UpdateMessageSql =
        """
        UPDATE messages
        SET content = @Content,
            attachment_ids = CAST(@AttachmentIds AS integer[]),
            url_preview_ids = CAST(@UrlPreviewIds AS integer[]),
            updated_at = NOW()
        WHERE id = @Id
        RETURNING updated_at;
        """;

    public const string InsertReactionSql =
        """
        INSERT INTO message_reactions (message_id, user_id, code)
        VALUES (@MessageId, @UserId, @Code)
        ON CONFLICT (message_id, user_id, code)
        DO NOTHING;
        """;

    public const string DeleteReactionSql =
        """
        DELETE FROM message_reactions
        WHERE message_id = @MessageId
          AND user_id = @UserId
          AND code = @Code;
        """;

    public static Task<int> IncrementChildrenCountAsync(IPgRepo<Message, int> messages, int messageId) =>
        messages.ExecuteScalarAsync<int>(IncrementChildrenCountSql, new { Id = messageId });

    public static Task<int> DecrementChildrenCountAsync(IPgRepo<Message, int> messages, int messageId) =>
        messages.ExecuteScalarAsync<int>(DecrementChildrenCountSql, new { Id = messageId });

    public static Task<DateTime?> UpdateMessageAsync(
        IPgRepo<Message, int> messages,
        int messageId,
        string content,
        int[] attachmentIds,
        int[] urlPreviewIds) =>
        messages.ExecuteScalarAsync<DateTime?>(
            UpdateMessageSql,
            new NpgsqlParameter("Id", NpgsqlDbType.Integer) { Value = messageId },
            new NpgsqlParameter("Content", NpgsqlDbType.Text) { Value = content },
            new NpgsqlParameter("AttachmentIds", NpgsqlDbType.Array | NpgsqlDbType.Integer)
            {
                Value = attachmentIds
            },
            new NpgsqlParameter("UrlPreviewIds", NpgsqlDbType.Array | NpgsqlDbType.Integer)
            {
                Value = urlPreviewIds
            }
        );

    public static Task InsertReactionAsync(
        IPgRepo<MessageReaction, MessageReactionKey> reactions,
        int messageId,
        Guid userId,
        string code) =>
        reactions.ExecuteAsync(
            InsertReactionSql,
            new
            {
                MessageId = messageId,
                UserId = userId,
                Code = code
            }
        );

    public static Task<int> DeleteReactionAsync(
        IPgRepo<MessageReaction, MessageReactionKey> reactions,
        int messageId,
        Guid userId,
        string code) =>
        reactions.ExecuteAsync(
            DeleteReactionSql,
            new
            {
                MessageId = messageId,
                UserId = userId,
                Code = code
            }
        );
}
