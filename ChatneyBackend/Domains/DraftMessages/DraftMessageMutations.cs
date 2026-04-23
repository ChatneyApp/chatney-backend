using System.Security.Claims;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.DraftMessages;

public class DraftMessageMutations
{
    [Authorize]
    public async Task<DraftMessage?> UpdateDraftMessage(
        AppRepos repos,
        ClaimsPrincipal principal,
        DraftMessageDto messageDto
    )
    {
        var user = await repos.Users.GetById(principal.GetUserGuid());

        var channel = await repos.Channels.GetById(messageDto.ChannelId);

        if (channel == null || user == null)
        {
            throw new InvalidOperationException("Channel or user is invalid");
        }

        var existingMessage = await repos.DraftMessages.GetOne(m =>
            m.UserId == principal.GetUserGuid() &&
            m.ChannelId == messageDto.ChannelId &&
            m.ParentId == messageDto.ParentId
        );
        if (existingMessage != null)
        {
            existingMessage.Content = messageDto.Content;
            existingMessage.AttachmentIds = messageDto.AttachmentIds ?? [];
            await repos.DraftMessages.UpdateOne(existingMessage);
            return existingMessage;
        }
        DraftMessage message = DraftMessage.FromDto(messageDto, principal.GetUserGuid());
        await repos.DraftMessages.InsertOne(message);
        return message;
    }

    [Authorize]
    public async Task<bool> DeleteMessage(ClaimsPrincipal principal, AppRepos repos, int id)
    {
        var message = await repos.DraftMessages.GetById(id);
        if (message == null) return false;
        if (message.UserId != principal.GetUserGuid()) return false;

        try
        {
            return await repos.DraftMessages.DeleteById(id);
        }
        catch (Exception exception)
        {
            Console.WriteLine("Exception: " + exception.Message);
            return false;
        }
    }

}
