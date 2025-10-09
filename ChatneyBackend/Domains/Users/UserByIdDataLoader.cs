using MongoDB.Driver;

namespace ChatneyBackend.Domains.Users;

public class UserByIdDataLoader : BatchDataLoader<string, User>
{
    private readonly Repo<User> _repo;

    public UserByIdDataLoader(IBatchScheduler batchScheduler, Repo<User> repo)
        : base(batchScheduler, new DataLoaderOptions()) => _repo = repo;

    protected override async Task<IReadOnlyDictionary<string, User>> LoadBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken
    )
    {
        var users = await _repo.GetList(Builders<User>.Filter.In(x => x.Id, keys));
        return users.ToDictionary(u => u.Id);
    }
}

public class HasUserIdTypeExtension<T> : ObjectTypeExtension<T> where T : class, IHasUserId
{
    protected override void Configure(IObjectTypeDescriptor<T> descriptor)
    {
        descriptor.Field("user")
            .Resolve(async (ctx) =>
            {
                var dataLoader = ctx.DataLoader<UserByIdDataLoader>();
                var source = ctx.Parent<T>();
                return await dataLoader.LoadAsync(source.UserId, ctx.RequestAborted);
            })
            .Type<NonNullType<ObjectType<User>>>();
    }
}
