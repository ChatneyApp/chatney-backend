using System.Linq.Expressions;
using ChatneyBackend.Infra;
using RepoDb.Attributes;

namespace ChatneyBackend.Domains.Messages;

public readonly record struct MessageReactionKey(int MessageId, Guid UserId, string Code);

public class MessageReaction : IPgKey<MessageReaction, MessageReactionKey>, IPgTimestamped
{
    [Primary]
    [Map("message_id")]
    public required int MessageId { get; set; }

    [Primary]
    [Map("user_id")]
    public required Guid UserId { get; set; }

    [Primary]
    [Map("code")]
    public required string Code { get; set; }

    [Map("created_at")]
    public DateTime CreatedAt { get; set; }

    [Map("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public static Expression<Func<MessageReaction, bool>> MatchByKey(MessageReactionKey key) =>
        reaction => reaction.MessageId == key.MessageId &&
                    reaction.UserId == key.UserId &&
                    reaction.Code == key.Code;
}

public class WebsocketReactionPayload
{
    public required string Code { get; set; }
    public required Guid UserId { get; set; }
    public required int MessageId { get; set; }
    public required int ChannelId { get; set; }
}
