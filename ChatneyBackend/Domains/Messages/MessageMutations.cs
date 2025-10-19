using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class AddReactionOutput
{
    public required string status { get; set; }
    public string? message { get; set; }
}

public class MessageMutations
{
    [Authorize]
    public async Task<Message?> AddMessage(
        RoleManager roleManager,
        Repo<Channel> channelsRepo,
        Repo<Message> messagesRepo,
        Repo<User> usersRepo,
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
            await messagesRepo.InsertOne(message);
            await webSocketConnector.SendMessageAsync(MessageWithUser.Create(message, user));
            return message;
        }

        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(ErrorCodes.ForbiddenAction)
                .SetCode(ErrorCodes.ForbiddenAction)
                .Build());
    }

    public async Task<Message?> UpdateMessage(IMongoDatabase mongoDatabase, Message message)
    {
        var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);
        var filter = Builders<Message>.Filter.Eq("_id", message.Id);
        var result = await collection.ReplaceOneAsync(filter, message);
        return result.ModifiedCount > 0 ? message : null;
    }

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
            return false;
        }
    }

    public async Task<AddReactionOutput> AddReaction(
        WebSocketConnector webSocketConnector,
        Repo<MessageReactionDbModel> reactionRepo,
        Repo<Message> messageRepo,
        string code,
        string messageId,
        ClaimsPrincipal principal)
    {
        try
        {
            // 1. Checking user is valid
            var userId = principal.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return new AddReactionOutput()
                {
                    message = "invalid user",
                    status = "error"
                };
            }

            // 2. Trying to insert new reaction into message, if it fails - means duplicate
            var newReaction = new MessageReactionDbModel
            {
                Id = Guid.NewGuid().ToString(),
                MessageId = messageId,
                Code = code,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                await reactionRepo._collection.InsertOneAsync(newReaction);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return new AddReactionOutput()
                {
                    status = "error",
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

            var updateResult = await messageRepo._collection.UpdateOneAsync(msgFilter, msgUpdate);

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

                await messageRepo._collection.UpdateOneAsync(
                    Builders<Message>.Filter.Eq(m => m.Id, messageId),
                    pushUpdate
                );
            }
            return new AddReactionOutput()
            {
                status = "success"
            };
        }
        catch (Exception e)
        {
            return new AddReactionOutput()
            {
                message = e.ToString(),
                status = "error"
            };
;
        }
    }
}
