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
    public AttachmentUploadResponse RequestUpload(
        RoleManager roleManager,
        Repo<User> usersRepo,
        Repo<Attachment> attachmentsRepo,
        ClaimsPrincipal principal,
        WebSocketConnector webSocketConnector,
        IAmazonS3 s3Client
    )
    {
        var req = new GetPreSignedUrlRequest
        {
            BucketName = "chatney",
            Key = "/media/fil1.jpg",
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(5)),
            ContentType = "image/jpeg",
        };

        // Если хочешь “привязать” дополнительные заголовки — добавляй их и требуй на клиенте.
        // req.Headers["x-amz-meta-..."] = "...";

        var url = s3Client.GetPreSignedURL(req);
        return new AttachmentUploadResponse()
        {
            SecureUrl = url
        };
    }
}
