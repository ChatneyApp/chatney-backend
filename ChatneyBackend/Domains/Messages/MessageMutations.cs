using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Users;
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
        Repo<Channel> channelsRepo,
        Repo<Message> messagesRepo,
        Repo<User> usersRepo,
        Repo<UrlPreview> urlPreviewRepo,
        ClaimsPrincipal principal,
        MessageDTO messageDto,
        WebSocketConnector webSocketConnector
    )
    {
        Message message = Message.FromDTO(messageDto, principal.GetUserId());
        var user = await usersRepo.GetById(principal.GetUserId());

        var channel = await channelsRepo.GetById(message.ChannelId);

        if (channel == null || user == null)
        {
            throw new InvalidOperationException("Channel or user is invalid");
        }

        var currentRole = roleManager.GetRelevantRole(user, new RoleScope(
            WorkspaceId: channel.WorkspaceId,
            ChannelId: channel.Id,
            ChannelTypeId: channel.ChannelTypeId
        ));

        Console.WriteLine(string.Join(" ", currentRole.Permissions));
        if (currentRole.Permissions.Contains(MessagePermissions.CreateMessage))
        {
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

            message.UrlPreviewIds = urlPreviewIds;

            await messagesRepo.InsertOne(message);
            await webSocketConnector.SendMessageAsync(MessageWithUser.Create(message, user, urlPreviews));
            return message;
        }

        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(ErrorCodes.ForbiddenAction)
                .SetCode(ErrorCodes.ForbiddenAction)
                .Build());
    }

    [Authorize]
    public async Task<Message?> UpdateMessage(IMongoDatabase mongoDatabase, Message message)
    {
        var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);
        var filter = Builders<Message>.Filter.Eq("_id", message.Id);
        var result = await collection.ReplaceOneAsync(filter, message);
        return result.ModifiedCount > 0 ? message : null;
    }

    [Authorize]
    public async Task<bool> DeleteMessage(WebSocketConnector webSocketConnector, Repo<Message> repo, string id)
    {
        var message = await repo.GetById(id);
        if (message == null) return false;

        try
        {
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
            Console.WriteLine(exception.ToString());
            return false;
        }
    }

    [Authorize]
    public async Task<ReactionEndpointOutput> AddReaction(
        WebSocketConnector webSocketConnector,
        Repo<MessageReaction> reactionRepo,
        Repo<Message> messageRepo,
        string code,
        string messageId,
        ClaimsPrincipal principal)
    {
        try
        {
            // 1. Checking user is valid
            var userId = principal.GetUserId();

            // 2. Trying to insert new reaction into message, if it fails - means duplicate
            var newReaction = new MessageReaction
            {
                Id = Guid.NewGuid().ToString(),
                MessageId = messageId,
                Code = code,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Message? message;
            try
            {
                message = await messageRepo.GetById(messageId);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return new ReactionEndpointOutput()
                {
                    status = "error",
                    message = "wrong message id"
                };
            }

            if (message == null)
            {
                return new ReactionEndpointOutput()
                {
                    status = "error",
                    message = "wrong message id"
                };
            }

            try
            {
                await reactionRepo.Collection.InsertOneAsync(newReaction);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return new ReactionEndpointOutput()
                {
                    status = "success",
                    message = "duplicate reaction"
                };
            }

            // 3. Incrementing reaction in message if exists
            var msgFilter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.Id, messageId),
                Builders<Message>.Filter.ElemMatch(m => m.Reactions, r => r.Code == code)
            );

            var msgUpdate = Builders<Message>.Update
                .Inc("reactions.$.count", 1)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);

            var updateResult = await messageRepo.Collection.UpdateOneAsync(msgFilter, msgUpdate);

            // 4. If increment did not work - means there is no element and we need to insert it
            if (updateResult.MatchedCount == 0)
            {
                var pushUpdate = Builders<Message>.Update
                    .Push(m => m.Reactions, new ReactionInMessage
                    {
                        Code = code,
                        Count = 1
                    })
                    .Set(m => m.UpdatedAt, DateTime.UtcNow);

                await messageRepo.Collection.UpdateOneAsync(
                    Builders<Message>.Filter.Eq(m => m.Id, messageId),
                    pushUpdate
                );
            }

            // 5. Send websocket update
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
            ;
        }
    }

    [Authorize]
    public async Task<ReactionEndpointOutput> DeleteReaction(
        WebSocketConnector webSocketConnector,
        Repo<MessageReaction> reactionRepo,
        Repo<Message> messageRepo,
        string code,
        string messageId,
        ClaimsPrincipal principal)
    {
        try
        {
            var userId = principal.GetUserId();

            Message? message;
            try
            {
                message = await messageRepo.GetById(messageId);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return new ReactionEndpointOutput()
                {
                    status = "error",
                    message = "wrong message id"
                };
            }

            if (message == null)
            {
                return new ReactionEndpointOutput()
                {
                    status = "error",
                    message = "wrong message id"
                };
            }

            // 2. Try to delete the user's reaction
            var deleteResult = await reactionRepo.Collection.DeleteOneAsync(
                Builders<MessageReaction>.Filter.And(
                    Builders<MessageReaction>.Filter.Eq(r => r.UserId, userId),
                    Builders<MessageReaction>.Filter.Eq(r => r.MessageId, messageId),
                    Builders<MessageReaction>.Filter.Eq(r => r.Code, code)
                )
            );

            if (deleteResult.DeletedCount == 0)
            {
                return new ReactionEndpointOutput
                {
                    status = "error",
                    message = "reaction not found"
                };
            }

            // 3. Decrement the count of the reaction in the message
            var msgFilter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.Id, messageId),
                Builders<Message>.Filter.ElemMatch(m => m.Reactions, r => r.Code == code)
            );

            var decrementUpdate = Builders<Message>.Update
                .Inc("reactions.$.count", -1)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);

            var updateResult = await messageRepo.Collection.UpdateOneAsync(msgFilter, decrementUpdate);

            // 4. Remove the reaction entry if count is now <= 0
            await messageRepo.Collection.UpdateOneAsync(
                Builders<Message>.Filter.Eq(m => m.Id, messageId),
                Builders<Message>.Update.PullFilter(m => m.Reactions, r => r.Code == code && r.Count <= 0)
            );

            // 5. Notify websocket listeners
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
