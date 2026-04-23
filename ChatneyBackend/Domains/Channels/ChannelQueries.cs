using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Channels;

public class ChannelQueries
{
    public async Task<Channel?> GetChannelById(AppRepos repos, int id) => await repos.Channels.GetById(id);

    public async Task<Channel?> GetChannelByName(AppRepos repos, string name) =>
        await repos.Channels.GetOne(channel => channel.Name == name);

    public async Task<List<Channel>> GetWorkspaceChannelList(AppRepos repos, int workspaceId) =>
        await repos.Channels.GetList(channel => channel.WorkspaceId == workspaceId);

    public async Task<List<ChannelType>> GetChannelTypeList(AppRepos repos) => await repos.ChannelTypes.GetList();

    public async Task<List<ChannelGroup>> GetWorkspaceChannelGroupList(AppRepos repos, int workspaceId) =>
        await repos.ChannelGroups.GetList(group => group.WorkspaceId == workspaceId);
}
