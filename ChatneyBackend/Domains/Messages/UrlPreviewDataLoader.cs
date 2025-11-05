using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class UrlPreviewTypeExtension : ObjectTypeExtension<Message>
{
    protected override void Configure(IObjectTypeDescriptor<Message> descriptor)
    {
        descriptor.Field("urlPreviews")
            .Resolve(async (ctx) =>
            {
                var message = ctx.Parent<Message>();
                if (message.UrlPreviewIds.Count == 0)
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

public class UrlPreviewsByUrlPreviewIdDataLoader : BatchDataLoader<string, UrlPreview>
{
    private readonly Repo<UrlPreview> _repo;

    public UrlPreviewsByUrlPreviewIdDataLoader(IBatchScheduler batchScheduler, Repo<UrlPreview> repo)
        : base(batchScheduler, new DataLoaderOptions())
    {
        _repo = repo;
    }

    protected override async Task<IReadOnlyDictionary<string, UrlPreview>> LoadBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken)
    {
        var filter = Builders<UrlPreview>.Filter.In(x => x.Id, keys);
        var previews = await _repo.GetList(filter);
        return previews.ToDictionary(p => p.Id);
    }
}
