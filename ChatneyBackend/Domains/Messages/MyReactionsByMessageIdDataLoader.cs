
using System.Security.Claims;
using ChatneyBackend.Infra.Middleware;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class MyReactionsByMessageIdDataLoader : GroupedDataLoader<string, string>
{
    private readonly Repo<MessageReaction> _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MyReactionsByMessageIdDataLoader(IBatchScheduler batchScheduler, Repo<MessageReaction> repo, IHttpContextAccessor httpContextAccessor)
        : base(batchScheduler, new DataLoaderOptions())
    {
        _repo = repo;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<ILookup<string, string>> LoadGroupedBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken
    )
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return Enumerable.Empty<MessageReaction>().ToLookup(r => r.MessageId, r => r.Code);
        }

        var userId = user.GetUserId();
        if (userId == null)
        {
            return Enumerable.Empty<MessageReaction>().ToLookup(r => r.MessageId, r => r.Code);
        }

        var filter = Builders<MessageReaction>.Filter.In(x => x.MessageId, keys) &
                     Builders<MessageReaction>.Filter.Eq(x => x.UserId, userId);

        var reactions = await _repo.GetList(filter);
        return reactions.ToLookup(r => r.MessageId, r => r.Code);
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
