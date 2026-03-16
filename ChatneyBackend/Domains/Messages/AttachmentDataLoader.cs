using ChatneyBackend.Domains.Attachments;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class AttachmentDataLoader : ObjectTypeExtension<Message>
{
    protected override void Configure(IObjectTypeDescriptor<Message> descriptor)
    {
        descriptor.Field("attachments")
            .Resolve(async (ctx) =>
            {
                var message = ctx.Parent<Message>();
                if (message.AttachmentIds.Count == 0)
                {
                    return Array.Empty<Attachment>();
                }

                var dataLoader = ctx.DataLoader<AttachmentsByAttachmentIdDataLoader>();
                var previews = await dataLoader.LoadAsync(message.AttachmentIds, ctx.RequestAborted);
                return previews;
            })
            .Type<ListType<ObjectType<Attachment>>>();
    }
}

public class AttachmentsByAttachmentIdDataLoader : BatchDataLoader<string, Attachment>
{
    private readonly Repo<Attachment> _repo;

    public AttachmentsByAttachmentIdDataLoader(IBatchScheduler batchScheduler, Repo<Attachment> repo)
        : base(batchScheduler, new DataLoaderOptions())
    {
        _repo = repo;
    }

    protected override async Task<IReadOnlyDictionary<string, Attachment>> LoadBatchAsync(
        IReadOnlyList<string> keys,
        CancellationToken cancellationToken)
    {
        var filter = Builders<Attachment>.Filter.In(x => x.Id, keys);
        var previews = await _repo.GetList(filter);
        return previews.ToDictionary(p => p.Id);
    }
}
