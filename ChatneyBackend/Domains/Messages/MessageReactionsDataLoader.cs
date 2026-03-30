using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Messages;

public class MessageReactionsByMessageIdDataLoader : GroupedDataLoader<int, ReactionInMessage>
{
    private readonly PgRepo<MessageReaction, MessageReactionKey> _repo;

    public MessageReactionsByMessageIdDataLoader(
        IBatchScheduler batchScheduler,
        PgRepo<MessageReaction, MessageReactionKey> repo)
        : base(batchScheduler, new DataLoaderOptions())
    {
        _repo = repo;
    }

    protected override async Task<ILookup<int, ReactionInMessage>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: group in DB, not in code
            var reactions = await _repo.GetList(r => keys.Contains(r.MessageId));

            var groupedReactions = reactions
                .GroupBy(
                    reaction => new { reaction.MessageId, reaction.Code },
                    reaction => reaction)
                .Select(group => new MessageReactionGroup
                {
                    MessageId = group.Key.MessageId,
                    Reaction = new ReactionInMessage
                    {
                        Code = group.Key.Code,
                        Count = group.Count()
                    }
                });

            return groupedReactions.ToLookup(group => group.MessageId, group => group.Reaction);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return Enumerable.Empty<MessageReactionGroup>()
            .ToLookup(group => group.MessageId, group => group.Reaction);
    }

    private sealed class MessageReactionGroup
    {
        public required int MessageId { get; init; }
        public required ReactionInMessage Reaction { get; init; }
    }
}

public class MessageReactionSummaryTypeExtension : ObjectTypeExtension<Message>
{
    protected override void Configure(IObjectTypeDescriptor<Message> descriptor)
    {
        descriptor.Field("reactions")
            .Resolve(async (ctx) =>
            {
                var dataLoader = ctx.DataLoader<MessageReactionsByMessageIdDataLoader>();
                var message = ctx.Parent<Message>();
                var reactions = await dataLoader.LoadAsync(message.Id, ctx.RequestAborted);
                return reactions?.ToArray() ?? Array.Empty<ReactionInMessage>();
            })
            .Type<ListType<ObjectType<ReactionInMessage>>>();
    }
}
