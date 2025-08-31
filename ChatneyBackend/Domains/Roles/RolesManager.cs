
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using MongoDB.Driver;

public sealed record RoleScope(string? WorkspaceId, string? ChannelId, string? ChannelTypeId);

public class RoleManager
{
    private readonly IMongoCollection<Role> _roles;

    public RoleManager(IMongoDatabase db)
    {
        _roles = db.GetCollection<Role>("roles");
    }

    public Role GetRelevantRole(
        User user,
        RoleScope roleScope)
    {
        if (roleScope.ChannelId != null && user.Roles.Channel.ContainsKey(roleScope.ChannelId))
        {
            var channelRole = user.Roles.Channel[roleScope.ChannelId];
            return _roles.Find(r => r.Id == channelRole.RoleId).FirstOrDefault();
        }

        if (roleScope.ChannelTypeId != null && user.Roles.ChannelTypes.ContainsKey(roleScope.ChannelTypeId))
        {
            var channelTypeRole = user.Roles.ChannelTypes[roleScope.ChannelTypeId];
            return _roles.Find(r => r.Id == channelTypeRole.RoleId).FirstOrDefault();
        }

        if (roleScope.WorkspaceId != null && user.Roles.Workspace.ContainsKey(roleScope.WorkspaceId))
        {
            var workspaceRole = user.Roles.Workspace[roleScope.WorkspaceId];
            return _roles.Find(r => r.Id == workspaceRole.RoleId).FirstOrDefault();
        }

        return _roles.Find(r => r.Id == user.Roles.Global).FirstOrDefault();
    }
}

