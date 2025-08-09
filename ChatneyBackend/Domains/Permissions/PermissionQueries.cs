using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Setup;

namespace ChatneyBackend.Domains.Permissions;

public class PermissionQueries
{
    public string[] GetList(ApplicationDbContext dbContext)
        => ChannelPermissions.All
            .Select(k => $"channel.{k}")
            .ToArray();
}
