using System.Security.Claims;
using ChannelsNamespaces = ChatneyBackend.Domains.Channels;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class MessageMutations
{
    [Authorize]
    public async Task<Message> AddMessage(HttpContext ctx, RoleManager roleManager, ClaimsPrincipal user,
        IMongoDatabase mongoDatabase, MessageDTO messageDto)
    {
        Message message = Message.FromDTO(messageDto, user.GetUserId());

        var channel = mongoDatabase
            .GetCollection<ChannelsNamespaces.Channel>(ChannelsNamespaces.DomainSettings.ChannelCollectionName)
            .Find(c => c.Id == message.ChannelId).FirstOrDefault();

        var currentRole = roleManager.GetRelevantRole(ctx.GetCurrentUser(), new RoleScope(
            WorkspaceId: channel.WorkspaceId,
            ChannelId: channel.Id,
            ChannelTypeId: channel.ChannelTypeId
        ));

        if (currentRole.Permissions.Contains(MessagePermissions.CreateMessage))
        {
            var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);
            await collection.InsertOneAsync(message);
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

    public async Task<bool> DeleteMessage(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
