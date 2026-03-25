using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Users;

public class UserByIdDataLoader : BatchDataLoader<string, User>
{
    private readonly PgRepo<User, Guid> _repo;

    public UserByIdDataLoader(IBatchScheduler batchScheduler, PgRepo<User, Guid> repo)
        : base(batchScheduler, new DataLoaderOptions()) => _repo = repo;

    protected override async Task<IReadOnlyDictionary<string, User>> LoadBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken
    )
    {
        var guids = keys.Select(Guid.Parse).ToList();
        var users = await _repo.GetList(u => guids.Contains(u.Id));
        return users.ToDictionary(u => u.Id.ToString());
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
