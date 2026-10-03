using System.Security.Claims;
using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Model;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using ChatneyBackend.Utils;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Attachments;

public class AttachmentMutations
{
    /// <summary>
    /// Uploads a file to storage and records its metadata. Requires AttachmentUpload.
    /// Reference the returned id in a message's attachmentIds.
    /// </summary>
    /// <param name="file">The file (multipart upload). Must not be empty.</param>
    /// <param name="asFile">Show as a downloadable file rather than inline media.</param>
    /// <param name="width">Media width in pixels, if known by the client.</param>
    /// <param name="height">Media height in pixels, if known by the client.</param>
    /// <param name="duration">Audio/video duration in seconds, if known by the client.</param>
    [Authorize]
    public async Task<Attachment> Upload(
        AppRepos repos,
        IPermissionResolver resolver,
        ClaimsPrincipal principal,
        IAmazonS3 s3Client,
        AppConfig appConfig,
        IFile file,
        bool asFile,
        int? width,
        int? height,
        int? duration
    )
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.AttachmentUpload);

        if (file == null)
        {
            throw new Exception("File is empty.");
        }

        var fileSize = file.Length ?? 0;

        if (fileSize == 0)
        {
            throw new Exception("File is empty.");
        }

        var userId = principal.GetUserGuid();
        var fileId = Guid.NewGuid().ToString();
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
                BucketName = appConfig.S3Bucket,
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
                    Width = width,
                    Height = height,
                    Duration = duration,
                    OriginalFileName = file.Name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Type = type,
                    AsFile = asFile,
                    UrlPath = s3Key
                };

                attachment.Id = await repos.Attachments.InsertOne(attachment);

                return attachment;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Error uploading file to S3.");
            }
        }
    }
}
