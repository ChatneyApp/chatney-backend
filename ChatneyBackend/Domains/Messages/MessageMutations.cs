using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using ChatneyBackend.Utils;
using HotChocolate;
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
        var user = await principal.GetRequiredUser(repos);
        var userId = user.Id;
        Message message = Message.FromDto(messageDto, userId);

        var channel = await repos.Channels.GetById(message.ChannelId);
        if (channel == null)
        {
            throw new InvalidOperationException("Channel or user is invalid");
        }

        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel));
        permissions.Require(ChannelPermissions.CreateMessage);

        var parentMessage = message.ParentId != null
            ? await repos.Messages.GetById(message.ParentId.Value)
            : null;

        if (parentMessage != null)
        {
            var childrenCount = await MessageMutationQueries.IncrementChildrenCountAsync(
                repos.Messages,
                parentMessage.Id
            );
            await webSocketConnector.UpdateMessageChildrenCountAsync(new MessageChildrenCountUpdated
            {
                ChildrenCount = childrenCount,
                MessageId = parentMessage.Id
            });
        }

        message.UrlPreviewIds = await ExtractUrlPreviewIds(repos, message.Content);

        try
        {
            message.Id = await repos.Messages.InsertOne(message);
            var result = await MessageHydrator.HydrateAsync([message], repos, userId);

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
            throw;
        }
    }

    private static async Task<int[]> ExtractUrlPreviewIds(AppRepos repos, string messageContent)
    {
        var urls = UrlPreviewExtractor.ExtractUrls(messageContent);
        List<UrlPreview> newUrlPreviews = new List<UrlPreview>();

        List<int> urlPreviewIds = new List<int>();
        var existingUrlPreviews = await repos.UrlPreviews.GetList(x => urls.Contains(x.Url));
        foreach (var url in urls)
        {
            var urlPreview = existingUrlPreviews.FirstOrDefault(x => x.Url == url);
            if (urlPreview != null)
            {
                urlPreviewIds.Add(urlPreview.Id);
            }
            else
            {
                try
                {
                    urlPreview = await UrlPreviewExtractor.GetPreviewAsync(url);
                    if (urlPreview != null)
                    {
                        newUrlPreviews.Add(urlPreview);
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
            foreach (var url in newUrlPreviews)
            {
                await repos.UrlPreviews.InsertOne(url);
                urlPreviewIds.Add(url.Id);
            }
        }

        return urlPreviewIds.ToArray();
    }

    [Authorize]
    public async Task<bool> UpdateMessage(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        WebSocketConnector webSocketConnector,
        MessageUpdateDto message)
    {
        try
        {
            var user = await principal.GetRequiredUser(repos);
            var userId = user.Id;
            var existingMessage = await repos.Messages.GetById(message.Id);
            if (existingMessage == null)
            {
                ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
                return false;
            }

            var channel = await repos.Channels.GetById(existingMessage.ChannelId);
            if (channel == null)
            {
                ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
                return false;
            }

            var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel));
            if (!permissions.Can(ChannelPermissions.EditMessage) &&
                !(existingMessage.UserId == userId && permissions.Can(ChannelPermissions.EditOwnMessage)))
            {
                ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
                return false;
            }

            var urlPreviewIds = existingMessage.UrlPreviewIds ?? [];
            if (existingMessage.Content != message.Content)
            {
                urlPreviewIds = await ExtractUrlPreviewIds(repos, message.Content);
            }

            var attachmentIds = message.AttachmentIds ?? Array.Empty<int>();

            var updatedAt = await MessageMutationQueries.UpdateMessageAsync(
                repos.Messages,
                message.Id,
                message.Content,
                attachmentIds,
                urlPreviewIds
            );

            if (updatedAt != null)
            {
                existingMessage.Content = message.Content;
                existingMessage.AttachmentIds = attachmentIds;
                existingMessage.UrlPreviewIds = urlPreviewIds;
                existingMessage.UpdatedAt = updatedAt.Value;

                var result = await MessageHydrator.HydrateAsync([existingMessage], repos, userId);
                var messageWithUser = result.Messages.FirstOrDefault();
                if (messageWithUser != null)
                {
                    await webSocketConnector.SendEditedMessageAsync(messageWithUser);
                }
                return true;
            }

            return false;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return false;
        }
    }

    [Authorize]
    public async Task<bool> DeleteMessage(
        WebSocketConnector webSocketConnector,
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        int id)
    {
        var user = await principal.GetRequiredUser(repos);
        var userId = user.Id;
        var message = await repos.Messages.GetById(id);
        if (message == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
            return false;
        }

        var channel = await repos.Channels.GetById(message.ChannelId);
        if (channel == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
            return false;
        }

        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel));
        if (!permissions.Can(ChannelPermissions.DeleteMessage) &&
            !(message.UserId == userId && permissions.Can(ChannelPermissions.DeleteOwnMessage)))
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }

        try
        {
            if (message.ParentId == null)
            {
                await repos.Messages.Delete(r => r.ParentId == id);
            }
            else
            {
                var parentMessage = message.ParentId != null
                    ? await repos.Messages.GetById(message.ParentId.Value)
                    : null;
                if (parentMessage != null)
                {
                    var childrenCount = await MessageMutationQueries.DecrementChildrenCountAsync(
                        repos.Messages,
                        parentMessage.Id
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
        catch (GraphQLException)
        {
            throw;
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
        RoleManager roleManager,
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

            var channel = await repos.Channels.GetById(message.ChannelId);
            if (channel == null)
            {
                return new ReactionEndpointOutput()
                {
                    status = "error",
                    message = "channel not found"
                };
            }

            await MessageMutationQueries.InsertReactionAsync(
                repos.Reactions,
                messageId,
                userId,
                code
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
        catch (GraphQLException)
        {
            throw;
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
        RoleManager roleManager,
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

            var channel = await repos.Channels.GetById(message.ChannelId);
            if (channel == null)
            {
                return new ReactionEndpointOutput()
                {
                    status = "error",
                    message = "channel not found"
                };
            }

            var deletedAggregate = await MessageMutationQueries.DeleteReactionAsync(
                repos.Reactions,
                messageId,
                userId,
                code
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
        catch (GraphQLException)
        {
            throw;
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
