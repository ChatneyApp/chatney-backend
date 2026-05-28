using System.Security.Claims;
using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Model;
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
        public required string MimeType { get; set; }
        public required long Size { get; set; }
    }

    [Authorize]
    public async Task<AttachmentUploadResponse> Upload(
        AppRepos repos,
        ClaimsPrincipal principal,
        IAmazonS3 s3Client,
        IFile file
    )
    {
        if (file == null)
            throw new Exception("File is empty.");

        var fileSize = file.Length ?? 0;

        if (fileSize == 0)
            throw new Exception("File is empty.");

        var userId = principal.GetUserGuid();
        var fileId = Guid.NewGuid().ToString();
        var bucketName = "chatney";
        var s3Folder = "attachments";
        var dateString = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var extMatch = Regex.Match(file.Name, "\\.([^\\.]+$)");
        string type = "binary";

        var contentType = file.ContentType ?? "binary";

        if (contentType.StartsWith("image/"))
        {
            if (contentType == "image/gif") {
                type = "gif";
            } else {
                type = "image";
            }
        } else if (contentType.StartsWith("video/"))
        {
            type = "video";
        } else if (contentType.StartsWith("audio/"))
        {
            type = "audio";
        }

        var ext = extMatch.Success ? extMatch.Groups[1].Value : "";

        switch (type)
        {
            case "audio":
                ext = "mp3";
                break;
            case "video":
                ext = "mp4";
                break;
            case "gif":
                ext = "gif";
                break;
        }
        string fullExt = ext == "" ? "" : "." + ext;
        var s3Key = $"{s3Folder}/{userId}/{dateString}/{fileId}{fullExt}";

        using (var fileStream = file.OpenReadStream())
        {
            var uploadRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = s3Key,
                InputStream = fileStream,
                ContentType = contentType
            };

            try
            {
                await s3Client.PutObjectAsync(uploadRequest);

                var attachment = new Attachment
                {
                    UserId = userId,
                    Extension = ext,
                    MimeType = contentType,
                    Size = fileSize,
                    OriginalFileName = file.Name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Type = type,
                    UrlPath = s3Key
                };

                attachment.Id = await repos.Attachments.InsertOne(attachment);

                var serviceUrl = s3Client.Config.ServiceURL.TrimEnd('/');

                return new AttachmentUploadResponse
                {
                    AttachmentId = attachment.Id,
                    S3Url = $"{serviceUrl}/{bucketName}/{s3Key}",
                    MimeType = contentType,
                    Size = fileSize
                };
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Error uploading file to S3.");
            }
        }
    }
}
