using System.Security.Claims;
using Amazon.S3;
using Amazon.S3.Model;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Attachments;

public class AttachmentMutations
{
    public class AttachmentUploadResponse
    {
        public required string S3Url { get; set; }
        public required string AttachmentId { get; set; }
    }

    [Authorize]
    public async Task<AttachmentUploadResponse> Upload(
    Repo<User> usersRepo,
    Repo<Attachment> attachmentsRepo,
    ClaimsPrincipal principal,
    IAmazonS3 s3Client,
    IFile file
    )
    {
        if (file == null || file.Length == 0)
            throw new Exception("File is empty.");

        var userId = principal.GetUserId();
        var id = Guid.NewGuid().ToString();
        var bucketName = "chatney";
        var s3Folder = "attachments";
        var dateString = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string ext = "txt";
        string fullExt = ext == "" ? "" : "." + ext;
        string type = "image";

        var s3Key = $"{s3Folder}/{userId}/{dateString}/{id}{fullExt}";

        PutObjectResponse s3Response;
        using (var fileStream = file.OpenReadStream())
        {
            var uploadRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = s3Key,
                InputStream = fileStream,
                ContentType = file.ContentType
            };

            s3Response = await s3Client.PutObjectAsync(uploadRequest);
        }

        var attachment = new Attachment
        {
            UserId = userId,
            Extension = ext,
            MimeType = file.ContentType,
            Id = id,
            OriginalFileName = file.Name,
            CreatedAt = DateTime.UtcNow,
            Type = type,
            UrlPath = s3Key
        };

        await attachmentsRepo.InsertOne(attachment);

        return new AttachmentUploadResponse
        {
            AttachmentId = attachment.Id,
            S3Url = $"{s3Client.Config.ServiceURL}/{s3Key}"
        };
    }
}
