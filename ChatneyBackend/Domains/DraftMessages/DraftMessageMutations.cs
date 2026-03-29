using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra;
using ChatneyInfra = ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.DraftMessages;

public class DraftMessageMutations
{
    public class ReactionEndpointOutput
    {
        public required string status { get; set; }
        public string? message { get; set; }
    }

    [Authorize]
    public async Task<DraftMessage?> AddDraftMessage(
        RoleManager roleManager,
        PgRepo<Channel, int> channelsRepo,
        Repo<DraftMessage> messagesRepo,
        PgRepo<User, Guid> usersRepo,
        PgRepo<UserRole, UserRoleKey> userRolesRepo,
        ClaimsPrincipal principal,
        MessageDTO messageDto
    )
    {
        if (!int.TryParse(messageDto.ChannelId, out var channelId))
        {
            throw new InvalidOperationException("Channel id is invalid");
        }

        DraftMessage message = DraftMessage.FromDTO(messageDto, principal.GetUserGuid());
        var user = await usersRepo.GetById(principal.GetUserGuid());

        var channel = await channelsRepo.GetById(channelId);

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
        if ((currentRole?.Permissions ?? []).Contains(ChannelPermissions.CreateMessage))
        {
            var parentMessage = message.ParentId != null
                ? await messagesRepo.GetById(message.ParentId)
                : null;

            if (message.ParentId != null && parentMessage == null)
            {
                throw new Exception("Parent not found");   
            }  

            await messagesRepo.InsertOne(message);
            return message;
        }

        throw new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(ChatneyInfra.ErrorCodes.ForbiddenAction)
                .SetCode(ChatneyInfra.ErrorCodes.ForbiddenAction)
                .Build());
    }

    [Authorize]
    public async Task<DraftMessage?> UpdateDraftMessage(IMongoDatabase mongoDatabase, DraftMessage message)
    {
        var collection = mongoDatabase.GetCollection<DraftMessage>(DomainSettings.MessageCollectionName);
        var filter = Builders<DraftMessage>.Filter.Eq("_id", message.Id);
        var result = await collection.ReplaceOneAsync(filter, message);
        return result.ModifiedCount > 0 ? message : null;
    }

    [Authorize]
    public async Task<bool> DeleteMessage(Repo<DraftMessage> repo, string id)
    {
        var message = await repo.GetById(id);
        if (message == null) return false;

        try
        {
            // remove all children messages under this thread
            if (message.ParentId == null)
            {
                await repo.Delete(Builders<DraftMessage>.Filter.Eq(r => r.ParentId, id));
            }
            else
            {
                // decrease children count in parent message if applicable
                var parentMessage = message.ParentId != null
                    ? await repo.GetById(message.ParentId)
                    : null;
                if (parentMessage != null)
                {
                    var msgFilter = Builders<DraftMessage>.Filter.And(
                        Builders<DraftMessage>.Filter.Eq(m => m.Id, message.ParentId)
                    );

                    var msgUpdate = Builders<DraftMessage>.Update
                        .Inc("childrenCount", -1)
                        .Set(m => m.UpdatedAt, DateTime.UtcNow);

                    await repo.Collection.UpdateOneAsync(msgFilter, msgUpdate);
                }
            }

            var result = await repo.DeleteById(id);
            return result;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.ToString());
            return false;
        }
    }
}
