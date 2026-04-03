using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Channels;

public class ChannelQueries
{
    public async Task<Channel?> GetChannelById(PgRepo<Channel, int> repo, int id) => await repo.GetById(id);

    public async Task<Channel?> GetChannelByName(PgRepo<Channel, int> repo, string name) =>
        await repo.GetOne(channel => channel.Name == name);

    public async Task<List<Channel>> GetWorkspaceChannelList(PgRepo<Channel, int> repo, int workspaceId) =>
        await repo.GetList(channel => channel.WorkspaceId == workspaceId);

    public async Task<List<ChannelType>> GetChannelTypeList(PgRepo<ChannelType, int> repo) => await repo.GetList();

    public async Task<List<ChannelGroup>> GetWorkspaceChannelGroupList(PgRepo<ChannelGroup, int> repo, int workspaceId) =>
        await repo.GetList(group => group.WorkspaceId == workspaceId);
}
