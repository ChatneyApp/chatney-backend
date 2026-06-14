using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.DraftMessages;

public class DraftMessageMutations
{
    [Authorize]
    public async Task<DraftMessage?> UpdateDraftMessage(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        DraftMessageDto messageDto
    )
    {
        var user = await principal.GetRequiredUser(repos);
        var userId = user.Id;
        var channel = await repos.Channels.GetById(messageDto.ChannelId);

        if (channel == null)
        {
            throw new InvalidOperationException("Channel or user is invalid");
        }

        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel));
        permissions.Require(ChannelPermissions.CreateMessage);

        var existingMessage = await repos.DraftMessages.GetOne(m =>
            m.UserId == userId &&
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
        DraftMessage message = DraftMessage.FromDto(messageDto, userId);
        await repos.DraftMessages.InsertOne(message);
        return message;
    }

    [Authorize]
    public async Task<bool> DeleteMessage(
        ClaimsPrincipal principal,
        AppRepos repos,
        RoleManager roleManager,
        int id)
    {
        var user = await principal.GetRequiredUser(repos);
        var userId = user.Id;
        var message = await repos.DraftMessages.GetById(id);
        if (message == null)
        {
            return false;
        }
        if (message.UserId != userId)
        {
            return false;
        }

        var channel = await repos.Channels.GetById(message.ChannelId);
        if (channel == null)
        {
            return false;
        }

        var permissions = await roleManager.GetUserPermissions(user, RoleScope.FromChannel(channel));
        permissions.Require(ChannelPermissions.ReadChannel);

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
