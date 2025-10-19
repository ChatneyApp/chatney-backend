using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

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
}
