using MongoDB.Driver;

namespace ChatneyBackend.Domains.Channels;

public class ChannelQueries
{
    public Channel GetChannelById(IMongoDatabase mongoDatabase, string id)
        => mongoDatabase.GetCollection<Channel>("channels").Find(u => u.Id == id).First();

    public Channel GetChannelByName(IMongoDatabase mongoDatabase, string name)
        => mongoDatabase.GetCollection<Channel>("channels").Find(u => u.Name == name).First();

    public List<Channel> GetWorkspaceChannelList(IMongoDatabase mongoDatabase, string workspaceId)
        => mongoDatabase.GetCollection<Channel>("channels").Find(channel => channel.WorkspaceId == workspaceId).ToList();

    public List<ChannelType> GetChannelTypeList(IMongoDatabase mongoDatabase)
        => mongoDatabase.GetCollection<ChannelType>("channel_types").Find(Builders<ChannelType>.Filter.Empty).ToList();

    public List<ChannelGroup> GetWorkspaceChannelGroupList(IMongoDatabase mongoDatabase, string workspaceId)
        => mongoDatabase.GetCollection<ChannelGroup>("channel_groups").Find(channel => channel.WorkspaceId == workspaceId).ToList();
}
