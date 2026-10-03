using System.Security.Claims;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.DraftMessages;

public class DraftMessageMutations
{
    /// <summary>
    /// Creates or overwrites the current user's draft for a channel/thread (matched by channelId + parentId).
    /// Requires ChannelCreateMessage on the channel.
    /// </summary>
    [Authorize]
    public async Task<DraftMessage?> UpdateDraftMessage(
        AppRepos repos,
        IPermissionResolver resolver,
        ClaimsPrincipal principal,
        DraftMessageDto messageDto
    )
    {
        var userId = principal.GetUserGuid();
        var channel = await repos.Channels.GetById(messageDto.ChannelId);

        if (channel == null)
        {
            throw new InvalidOperationException("Channel or user is invalid");
        }

        var permissions = await resolver.ForChannel(channel);
        permissions.Require(Permission.ChannelCreateMessage);

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

    /// <summary>Deletes one of the current user's drafts. Requires ChannelReadChannel on the draft's channel.</summary>
    /// <param name="id">Draft id.</param>
    /// <returns>False if the draft doesn't exist or belongs to another user.</returns>
    [Authorize]
    public async Task<bool> DeleteMessage(
        ClaimsPrincipal principal,
        AppRepos repos,
        IPermissionResolver resolver,
        int id)
    {
        var userId = principal.GetUserGuid();
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

        var permissions = await resolver.ForChannel(channel);
        permissions.Require(Permission.ChannelReadChannel);

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
