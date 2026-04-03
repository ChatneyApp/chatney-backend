using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;

namespace ChatneyBackend.Domains.Messages;

public class MyReactionsByMessageIdDataLoader : GroupedDataLoader<int, string>
{
    private readonly PgRepo<MessageReaction, MessageReactionKey> _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MyReactionsByMessageIdDataLoader(IBatchScheduler batchScheduler, PgRepo<MessageReaction, MessageReactionKey> repo, IHttpContextAccessor httpContextAccessor)
        : base(batchScheduler, new DataLoaderOptions())
    {
        _repo = repo;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<ILookup<int, string>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken
    )
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return Enumerable.Empty<MessageReaction>().ToLookup(r => r.MessageId, r => r.Code);
        }

        var userId = user.GetUserGuid();

        try
        {
            var reactions = await _repo.GetList(r => keys.Contains(r.MessageId) && r.UserId == userId);
            return reactions.ToLookup(r => r.MessageId, r => r.Code);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return new List<MessageReaction>().ToLookup(r => r.MessageId, r => r.Code);
    }
}

public class MessageReactionsTypeExtension : ObjectTypeExtension<Message>
{
    protected override void Configure(IObjectTypeDescriptor<Message> descriptor)
    {
        descriptor.Field("myReactions")
            .Resolve(async (ctx) =>
            {
                var dataLoader = ctx.DataLoader<MyReactionsByMessageIdDataLoader>();
                var message = ctx.Parent<Message>();
                var reactions = await dataLoader.LoadAsync(message.Id, ctx.RequestAborted);
                return reactions;
            })
            .Type<ListType<StringType>>();
    }
}
