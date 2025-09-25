using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Users;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class MessagesDomainService
{
    private RoleManager roleManager;
    private Repo<Message> messagesRepo;
    private Repo<Channel> channelsRepo;
    private Repo<User> usersRepo;

    public MessagesDomainService(RoleManager roleManager, Repo<Message> messagesRepo, Repo<Channel> channelsRepo, Repo<User> usersRepo)
    {
        this.roleManager = roleManager;
        this.usersRepo = usersRepo;
        this.messagesRepo = messagesRepo;
        this.channelsRepo = channelsRepo;
    }

    public async Task<Message?> AddMessage(Message message)
    {
        var channel = await channelsRepo.GetById(message.ChannelId);
        var user = await usersRepo.GetById(message.UserId);

        if (channel == null || user == null)
        {
            throw new InvalidOperationException("Channel or user is invalid");
        }

        var currentRole = roleManager.GetRelevantRole(user, new RoleScope(
            WorkspaceId: channel.WorkspaceId,
            ChannelId: channel.Id,
            ChannelTypeId: channel.ChannelTypeId
        ));

        if (currentRole.Permissions.Contains(MessagePermissions.CreateMessage))
        {
            await messagesRepo.InsertOne(message);
            return message;
        }
        else
        {
            throw new Exception("Insufficient rights to send a message");
        }
    }
}