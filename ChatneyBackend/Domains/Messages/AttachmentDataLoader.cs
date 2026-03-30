using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Messages;

public class AttachmentDataLoader : ObjectTypeExtension<Message>
{
    protected override void Configure(IObjectTypeDescriptor<Message> descriptor)
    {
        descriptor.Field("attachments")
            .Resolve(async (ctx) =>
            {
                var message = ctx.Parent<Message>();
                if (message.AttachmentIds.Length == 0)
                {
                    return Array.Empty<Attachment>();
                }

                var dataLoader = ctx.DataLoader<AttachmentsByAttachmentIdDataLoader>();
                // TODO: add fullUrl for the frontend based on domain, bucket, s3 key, etc
                var attachments = await dataLoader.LoadAsync(message.AttachmentIds, ctx.RequestAborted);
                return attachments;
            })
            .Type<ListType<ObjectType<Attachment>>>();
    }
}

public class AttachmentsByAttachmentIdDataLoader : BatchDataLoader<int, Attachment>
{
    private readonly PgRepo<Attachment, int> _repo;

    public AttachmentsByAttachmentIdDataLoader(IBatchScheduler batchScheduler, PgRepo<Attachment, int> repo)
        : base(batchScheduler, new DataLoaderOptions())
    {
        _repo = repo;
    }

    protected override async Task<IReadOnlyDictionary<int, Attachment>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var attachments = await _repo.GetList(x => keys.Contains(x.Id));
        return attachments.ToDictionary(p => p.Id);
    }
}
