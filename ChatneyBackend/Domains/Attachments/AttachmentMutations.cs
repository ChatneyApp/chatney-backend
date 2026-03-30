using System.Security.Claims;
using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Model;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Attachments;

public class AttachmentMutations
{
    public class AttachmentUploadResponse
    {
        public required string S3Url { get; set; }
        public required int AttachmentId { get; set; }
    }

    [Authorize]
    public async Task<AttachmentUploadResponse> Upload(
        PgRepo<User, Guid> usersRepo,
        PgRepo<Attachment, int> attachmentsRepo,
        ClaimsPrincipal principal,
        IAmazonS3 s3Client,
        IFile file
    )
    {
        if (file == null || file.Length == 0)
            throw new Exception("File is empty.");

        var userId = principal.GetUserGuid();
        var fileId = Guid.NewGuid().ToString();
        var bucketName = "chatney";
        var s3Folder = "attachments";
        var dateString = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var extMatch = Regex.Match(file.Name, "\\.([^\\.]+$)");
        var ext = extMatch.Success ? extMatch.Groups[1].Value : "";
        string fullExt = ext == "" ? "" : "." + ext;
        string type = "image";

        var s3Key = $"{s3Folder}/{userId}/{dateString}/{fileId}{fullExt}";

        using (var fileStream = file.OpenReadStream())
        {
            var uploadRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = s3Key,
                InputStream = fileStream,
                ContentType = file.ContentType
            };

            await s3Client.PutObjectAsync(uploadRequest);
        }

        var attachment = new Attachment
        {
            UserId = userId,
            Extension = ext,
            MimeType = file.ContentType,
            OriginalFileName = file.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Type = type,
            UrlPath = s3Key
        };

        attachment.Id = await attachmentsRepo.InsertOne(attachment);

        var serviceUrl = s3Client.Config.ServiceURL.TrimEnd('/');

        return new AttachmentUploadResponse
        {
            AttachmentId = attachment.Id,
            S3Url = $"{serviceUrl}/{bucketName}/{s3Key}"
        };
    }
}
