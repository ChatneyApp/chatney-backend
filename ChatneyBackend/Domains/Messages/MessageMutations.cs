using System.Security.Claims;
using ChatneyBackend.Domains.Attachments;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra;
using ChatneyInfra = ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using ChatneyBackend.Utils;
using HotChocolate.Authorization;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class MessageMutations
{
    public class ReactionEndpointOutput
    {
        public required string status { get; set; }
        public string? message { get; set; }
    }

    [Authorize]
    public async Task<Message?> AddMessage(
        RoleManager roleManager,
        PgRepo<Channel, int> channelsRepo,
        PgRepo<Message, int> messagesRepo,
        PgRepo<User, Guid> usersRepo,
        PgRepo<UserRole, UserRoleKey> userRolesRepo,
        Repo<UrlPreview> urlPreviewRepo,
        Repo<Attachment> attachmentRepo,
        ClaimsPrincipal principal,
        MessageDto messageDto,
        WebSocketConnector webSocketConnector
    )
    {
        Message message = Message.FromDto(messageDto, principal.GetUserGuid());
        var user = await usersRepo.GetById(principal.GetUserGuid());

        var channel = await channelsRepo.GetById(message.ChannelId);

        if (channel == null || user == null)
        {
            throw new InvalidOperationException("Channel or user is invalid");
        }

        var userRoles = await userRolesRepo.GetList(r => r.UserId == user.Id);
        var currentRole = await roleManager.GetRelevantRole(user, userRoles, new RoleScope(
            WorkspaceId: channel.WorkspaceId,
            ChannelId: channel.Id,
            ChannelTypeId: channel.ChannelTypeId
        ));

        Console.WriteLine(string.Join(" ", currentRole?.Permissions ?? []));
        if ((currentRole?.Permissions ?? []).Contains(MessagePermissions.CreateMessage))
        {
            var parentMessage = message.ParentId != null
                ? await messagesRepo.GetById(message.ParentId.Value)
                : null;

            if (parentMessage != null)
            {
                var childrenCount = await messagesRepo.ExecuteScalarAsync<int>(
                    """
                    UPDATE messages
                    SET children_count = children_count + 1,
                        updated_at = NOW()
                    WHERE id = @Id
                    RETURNING children_count;
                    """,
                    new { Id = parentMessage.Id }
                );
                await webSocketConnector.UpdateMessageChildrenCountAsync(new MessageChildrenCountUpdated
                {
                    ChildrenCount = childrenCount,
                    MessageId = parentMessage.Id
                });
            }

            var urls = UrlPreviewExtractor.ExtractUrls(message.Content);
            List<UrlPreview> newUrlPreviews = new List<UrlPreview>();
            List<UrlPreview> urlPreviews = new List<UrlPreview>();

            List<string> urlPreviewIds = new List<string>();
            var existingUrlPreviews = await urlPreviewRepo.GetList(
                Builders<UrlPreview>.Filter.In(x => x.Url, urls)
            );
            foreach (var url in urls)
            {
                var urlPreview = existingUrlPreviews.FirstOrDefault(x => x.Url == url);
                if (urlPreview != null)
                {
                    urlPreviewIds.Add(urlPreview.Id);
                    urlPreviews.Add(urlPreview);
                }
                else
                {
                    try
                    {
                        urlPreview = await UrlPreviewExtractor.GetPreviewAsync(url);
                        if (urlPreview != null)
                        {
                            urlPreviewIds.Add(urlPreview.Id);
                            newUrlPreviews.Add(urlPreview);
                            urlPreviews.Add(urlPreview);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Failed to parse url {url} with the error: {ex.Message}");
                    }
                }
            }

            if (newUrlPreviews.Count > 0)
            {
                await urlPreviewRepo.InsertBulk(newUrlPreviews);
            }

            message.UrlPreviewIds = urlPreviewIds.ToArray();

            try
            {
                message.Id = await messagesRepo.InsertOne(message);
                // TODO: add fullUrl for the frontend based on domain, bucket, s3 key, etc
                var attachments = await attachmentRepo.GetList(
                    Builders<Attachment>.Filter.In(x => x.Id, message.AttachmentIds)
                );
                await webSocketConnector.SendMessageAsync(MessageWithUser.Create(message, user, urlPreviews, attachments));
                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(ChatneyInfra.ErrorCodes.ForbiddenAction)
                .SetCode(ChatneyInfra.ErrorCodes.ForbiddenAction)
                .Build());
    }

    [Authorize]
    public async Task<Message?> UpdateMessage(PgRepo<Message, int> repo, Message message)
    {
        return await repo.UpdateOne(message)
            ? message
            : null;
    }

    [Authorize]
    public async Task<bool> DeleteMessage(WebSocketConnector webSocketConnector, PgRepo<Message, int> repo, int id)
    {
        var message = await repo.GetById(id);
        if (message == null) return false;

        try
        {
            // remove all children messages under this thread
            if (message.ParentId == null)
            {
                await repo.Delete(r => r.ParentId == id);
            }
            else
            {
                // decrease children count in parent message if applicable
                var parentMessage = message.ParentId != null
                    ? await repo.GetById(message.ParentId.Value)
                    : null;
                if (parentMessage != null)
                {
                    var childrenCount = await repo.ExecuteScalarAsync<int>(
                        """
                        UPDATE messages
                        SET children_count = GREATEST(children_count - 1, 0),
                            updated_at = NOW()
                        WHERE id = @Id
                        RETURNING children_count;
                        """,
                        new { Id = parentMessage.Id }
                    );
                    await webSocketConnector.UpdateMessageChildrenCountAsync(new MessageChildrenCountUpdated
                    {
                        ChildrenCount = childrenCount,
                        MessageId = parentMessage.Id
                    });
                }
            }

            var result = await repo.DeleteById(id);
            await webSocketConnector.DeleteMessageAsync(new DeletedMessage
            {
                ChannelId = message.ChannelId,
                MessageId = message.Id
            });
            return result;
        }
        catch (Exception exception)
        {
            Console.WriteLine("Exception: " + exception.Message);
            return false;
        }
    }

    [Authorize]
    public async Task<ReactionEndpointOutput> AddReaction(
        WebSocketConnector webSocketConnector,
        PgRepo<MessageReaction, MessageReactionKey> reactionRepo,
        PgRepo<Message, int> messageRepo,
        string code,
        int messageId,
        ClaimsPrincipal principal)
    {
        try
        {
            // 1. Checking user is valid
            var userId = principal.GetUserGuid();

            // 2. Checking message exists.
            var message = await messageRepo.GetById(messageId);

            if (message == null)
            {
                return new ReactionEndpointOutput()
                {
                    status = "error",
                    message = "wrong message id"
                };
            }

            // 3. Upsert aggregate reaction in postgres.
            await reactionRepo.ExecuteAsync(
                """
                INSERT INTO message_reactions (message_id, user_id, code)
                VALUES (@MessageId, @UserId, @Code)
                ON CONFLICT (message_id, user_id, code)
                DO NOTHING;
                """,
                new
                {
                    MessageId = messageId,
                    UserId = userId,
                    Code = code
                }
            );

            // 4. Send websocket update
            await webSocketConnector.AddReactionAsync(new WebsocketReactionPayload()
            {
                Code = code,
                UserId = userId,
                MessageId = messageId,
                ChannelId = message.ChannelId
            });

            return new ReactionEndpointOutput()
            {
                status = "success"
            };
        }
        catch (Exception e)
        {
            return new ReactionEndpointOutput()
            {
                message = e.ToString(),
                status = "error"
            };
        }
    }

    [Authorize]
    public async Task<ReactionEndpointOutput> DeleteReaction(
        WebSocketConnector webSocketConnector,
        PgRepo<MessageReaction, MessageReactionKey> reactionRepo,
        PgRepo<Message, int> messageRepo,
        string code,
        int messageId,
        ClaimsPrincipal principal)
    {
        try
        {
            var userId = principal.GetUserGuid();

            var message = await messageRepo.GetById(messageId);

            if (message == null)
            {
                return new ReactionEndpointOutput()
                {
                    status = "error",
                    message = "wrong message id"
                };
            }

            // 2. Remove the user's reaction in postgres.
            var deletedAggregate = await reactionRepo.ExecuteAsync(
                """
                DELETE FROM message_reactions
                WHERE message_id = @MessageId
                  AND user_id = @UserId
                  AND code = @Code;
                """,
                new
                {
                    MessageId = messageId,
                    UserId = userId,
                    Code = code
                }
            );

            if (deletedAggregate == 0)
            {
                return new ReactionEndpointOutput
                {
                    status = "error",
                    message = "reaction not found"
                };
            }

            // 4. Notify websocket listeners
            await webSocketConnector.DeleteReactionAsync(new WebsocketReactionPayload()
            {
                Code = code,
                UserId = userId,
                MessageId = messageId,
                ChannelId = message.ChannelId
            });

            return new ReactionEndpointOutput
            {
                status = "success"
            };
        }
        catch (Exception ex)
        {
            return new ReactionEndpointOutput
            {
                message = ex.ToString(),
                status = "error"
            };
        }
    }

}
