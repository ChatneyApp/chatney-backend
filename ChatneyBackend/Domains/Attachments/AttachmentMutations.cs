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
    public AttachmentUploadResponse Upload(
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
            Key = "/media/mario-neutral.png",
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
            ContentType = "image/png"
        };

        return new AttachmentUploadResponse()
        {
            SecureUrl = ""
        };
    }
}
