using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyInfra = ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using ChatneyBackend.Utils;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Messages;

public class MessageMutations
{
    public class ReactionEndpointOutput
    {
        public required string status { get; set; }
        public string? message { get; set; }
    }

    [Authorize]
    public async Task<MessageWithUser?> AddMessage(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        MessageDto messageDto,
        WebSocketConnector webSocketConnector
    )
    {
        Message message = Message.FromDto(messageDto, principal.GetUserGuid());
        var user = await repos.Users.GetById(principal.GetUserGuid());

        var channel = await repos.Channels.GetById(message.ChannelId);

        if (channel == null || user == null)
        {
            throw new InvalidOperationException("Channel or user is invalid");
        }

        var userRoles = await repos.UserRoles.GetList(r => r.UserId == user.Id);
        var currentRole = await roleManager.GetRelevantRole(user, userRoles, new RoleScope(
            WorkspaceId: channel.WorkspaceId,
            ChannelId: channel.Id,
            ChannelTypeId: channel.ChannelTypeId
        ));

        Console.WriteLine(string.Join(" ", currentRole?.Permissions ?? []));
        if ((currentRole?.Permissions ?? []).Contains(MessagePermissions.CreateMessage))
        {
            var parentMessage = message.ParentId != null
                ? await repos.Messages.GetById(message.ParentId.Value)
                : null;

            if (parentMessage != null)
            {
                var childrenCount = await repos.Messages.ExecuteScalarAsync<int>(
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

            List<int> urlPreviewIds = new List<int>();
            var existingUrlPreviews = await repos.UrlPreviews.GetList(x => urls.Contains(x.Url));
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
                await repos.UrlPreviews.InsertBulk(newUrlPreviews);
            }

            message.UrlPreviewIds = urlPreviewIds.ToArray();


            try
            {
                message.Id = await repos.Messages.InsertOne(message);
                var result = await MessageHydrator.HydrateAsync([message], repos, principal.GetUserGuid());

                var messageWithUser = result.Messages.First();
                var replyRef = result.Refs.FirstOrDefault();

                await webSocketConnector.SendNewMessageAsync(new NewMessagePayload
                {
                    Message = messageWithUser,
                    ReplyTo = replyRef,
                });
                return messageWithUser;
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
    public async Task<bool> UpdateMessage(
        AppRepos repos,
        ClaimsPrincipal principal,
        WebSocketConnector webSocketConnector,
        MessageUpdateDto message)
    {
        try
        {
            var existingMessage = await repos.Messages.GetById(message.Id);
            if (existingMessage == null || existingMessage.UserId != principal.GetUserGuid())
                return false;

            var updated = await repos.Messages.ExecuteScalarAsync<int>(
                """
            UPDATE messages
            SET content = @Content,
                attachment_ids = COALESCE(@AttachmentIds::integer[], '{}'::integer[]),
                updated_at = NOW()
            WHERE id = @Id
            RETURNING id;
            """,
                new { message.Id, message.Content, AttachmentIds = message.AttachmentIds ?? [] }
            );

            if (updated > 0)
            {
                var dbMessage = await repos.Messages.GetById(message.Id);
                if (dbMessage != null)
                {
                    var result = await MessageHydrator.HydrateAsync([dbMessage], repos, principal.GetUserGuid());
                    var messageWithUser = result.Messages.FirstOrDefault();
                    if (messageWithUser != null)
                    {
                        await webSocketConnector.SendEditedMessageAsync(messageWithUser);
                    }
                }
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return false;
        }
    }

    [Authorize]
    public async Task<bool> DeleteMessage(WebSocketConnector webSocketConnector, AppRepos repos, int id)
    {
        var message = await repos.Messages.GetById(id);
        if (message == null) return false;

        try
        {
            // remove all children messages under this thread
            if (message.ParentId == null)
            {
                await repos.Messages.Delete(r => r.ParentId == id);
            }
            else
            {
                // decrease children count in parent message if applicable
                var parentMessage = message.ParentId != null
                    ? await repos.Messages.GetById(message.ParentId.Value)
                    : null;
                if (parentMessage != null)
                {
                    var childrenCount = await repos.Messages.ExecuteScalarAsync<int>(
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

            var result = await repos.Messages.DeleteById(id);
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
        AppRepos repos,
        string code,
        int messageId,
        ClaimsPrincipal principal)
    {
        try
        {
            var userId = principal.GetUserGuid();

            var message = await repos.Messages.GetById(messageId);

            if (message == null)
            {
                return new ReactionEndpointOutput()
                {
                    status = "error",
                    message = "wrong message id"
                };
            }

            await repos.Reactions.ExecuteAsync(
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
        AppRepos repos,
        string code,
        int messageId,
        ClaimsPrincipal principal)
    {
        try
        {
            var userId = principal.GetUserGuid();

            var message = await repos.Messages.GetById(messageId);

            if (message == null)
            {
                return new ReactionEndpointOutput()
                {
                    status = "error",
                    message = "wrong message id"
                };
            }

            var deletedAggregate = await repos.Reactions.ExecuteAsync(
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
