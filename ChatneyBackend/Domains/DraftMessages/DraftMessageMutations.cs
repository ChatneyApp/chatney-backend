using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.DraftMessages;

public class DraftMessageMutations
{
    [Authorize]
    public async Task<DraftMessage?> UpdateDraftMessage(
        PgRepo<Channel, int> channelsRepo,
        PgRepo<DraftMessage, int> messagesRepo,
        PgRepo<User, Guid> usersRepo,
        ClaimsPrincipal principal,
        DraftMessageDto messageDto
    )
    {
        var user = await usersRepo.GetById(principal.GetUserGuid());

        var channel = await channelsRepo.GetById(messageDto.ChannelId);

        if (channel == null || user == null)
        {
            throw new InvalidOperationException("Channel or user is invalid");
        }

        var existingMessage = await messagesRepo.GetOne(m =>
            m.UserId == principal.GetUserGuid() &&
            m.ChannelId == messageDto.ChannelId &&
            m.ParentId == messageDto.ParentId
        );
        if (existingMessage != null)
        {
            existingMessage.Content = messageDto.Content;
            existingMessage.AttachmentIds = messageDto.AttachmentIds ?? [];
            await messagesRepo.UpdateOne(existingMessage);
            return existingMessage;
        }
        DraftMessage message = DraftMessage.FromDto(messageDto, principal.GetUserGuid());
        await messagesRepo.InsertOne(message);
        return message;
    }

    [Authorize]
    public async Task<bool> DeleteMessage(ClaimsPrincipal principal, PgRepo<DraftMessage, int> repo, int id)
    {
        var message = await repo.GetById(id);
        if (message == null) return false;
        if (message.UserId != principal.GetUserGuid()) return false;

        try
        {
            return await repo.DeleteById(id);
        }
        catch (Exception exception)
        {
            Console.WriteLine("Exception: " + exception.Message);
            return false;
        }
    }

}
