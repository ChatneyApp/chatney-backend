using System.Security.Claims;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class MessageMutations
{
    [Authorize]
    public async Task<Message?> AddMessage(MessagesDomainService messagesService, ClaimsPrincipal user, MessageDTO messageDto)
    {
        Message message = Message.FromDTO(messageDto, user.GetUserId());

        var result = await messagesService.AddMessage(message);

        if (result == null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(ErrorCodes.ForbiddenAction)
                    .SetCode(ErrorCodes.ForbiddenAction)
                    .Build());
        }

        return result;
    }

    public async Task<Message?> UpdateMessage(IMongoDatabase mongoDatabase, Message message)
    {
        var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);
        var filter = Builders<Message>.Filter.Eq("_id", message.Id);
        var result = await collection.ReplaceOneAsync(filter, message);
        return result.ModifiedCount > 0 ? message : null;
    }

    public async Task<bool> DeleteMessage(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
