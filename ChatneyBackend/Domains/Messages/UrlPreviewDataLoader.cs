using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Messages;

public class UrlPreviewTypeExtension : ObjectTypeExtension<Message>
{
    protected override void Configure(IObjectTypeDescriptor<Message> descriptor)
    {
        descriptor.Field("urlPreviews")
            .Resolve(async (ctx) =>
            {
                var message = ctx.Parent<Message>();
                if (message.UrlPreviewIds.Length == 0)
                {
                    return Array.Empty<UrlPreview>();
                }

                var dataLoader = ctx.DataLoader<UrlPreviewsByUrlPreviewIdDataLoader>();
                var previews = await dataLoader.LoadAsync(message.UrlPreviewIds, ctx.RequestAborted);
                return previews;
            })
            .Type<ListType<ObjectType<UrlPreview>>>();
    }
}

public class UrlPreviewsByUrlPreviewIdDataLoader : BatchDataLoader<int, UrlPreview>
{
    private readonly PgRepo<UrlPreview, int> _repo;

    public UrlPreviewsByUrlPreviewIdDataLoader(IBatchScheduler batchScheduler, PgRepo<UrlPreview, int> repo)
        : base(batchScheduler, new DataLoaderOptions())
    {
        _repo = repo;
    }

    protected override async Task<IReadOnlyDictionary<int, UrlPreview>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var previews = await _repo.GetList(x => keys.Contains(x.Id));
        return previews.ToDictionary(p => p.Id);
    }
}
