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
        public string SecureUrl { get; set; }
    }

    [Authorize]
    public async Task<AttachmentUploadResponse> Upload(
    RoleManager roleManager,
    Repo<User> usersRepo,
    Repo<Attachment> attachmentsRepo,
    ClaimsPrincipal principal,
    WebSocketConnector webSocketConnector,
    IAmazonS3 s3Client,
    IFormFile file
    )
    {
        if (file == null || file.Length == 0)
            throw new Exception("File is empty.");

        // 1️⃣ Validate user
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new Exception("Unauthorized.");

        var user = await usersRepo.GetById(userId);
        if (user == null)
            throw new Exception("User not found.");

        var attachment = new Attachment
        {
            Id = Guid.NewGuid().ToString(),
            FileName = file.FileName,
            S3Key = s3Key,
            UploadedById = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        await attachmentsRepo.AddAsync(attachment);

        var bucketName = "your-bucket-name";
        var s3Folder = "attachments";
        var dateString = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var s3Key = $"{s3Folder}/{userId}/{dateString}/{file.}";

        using (var fileStream = new FileStream(tmpFilePath, FileMode.Open))
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

        // 5️⃣ Save attachment record in DB
        var attachment = new Attachment
        {
            Id = Guid.NewGuid().ToString(),
            FileName = file.FileName,
            S3Key = s3Key,
            UploadedById = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        await attachmentsRepo.AddAsync(attachment);

        // 6️⃣ Cleanup tmp file
        if (File.Exists(tmpFilePath))
            File.Delete(tmpFilePath);

        // 7️⃣ Optional websocket notify
        await webSocketConnector.NotifyAsync("file_uploaded", new
        {
            attachmentId = attachment.Id,
            fileName = attachment.FileName
        });

        return new AttachmentUploadResponse
        {
            Success = true,
            AttachmentId = attachment.Id,
            Url = $"https://{bucketName}.s3.amazonaws.com/{s3Key}"
        };
    }
}
