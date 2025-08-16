using MongoDB.Driver;

namespace ChatneyBackend.Domains.Channels;

public class ChannelQueries
{
    public async Task<Channel?> GetChannelById(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Channel>(DomainSettings.ChannelCollectionName);
        var records = (await collection.FindAsync(u => u.Id == id)).ToList();
        return records.Count > 0
            ? records[0]
            : null;
    }

    public async Task<Channel?> GetChannelByName(IMongoDatabase mongoDatabase, string name)
    {
        var collection = mongoDatabase.GetCollection<Channel>(DomainSettings.ChannelCollectionName);
        var records = (await collection.FindAsync(u => u.Name == name)).ToList();
        return records.Count > 0
            ? records[0]
            : null;
    }

    public async Task<List<Channel>> GetWorkspaceChannelList(IMongoDatabase mongoDatabase, string workspaceId)
    {
        var collection = mongoDatabase.GetCollection<Channel>(DomainSettings.ChannelCollectionName);
        var records = await collection.FindAsync(channel => channel.WorkspaceId == workspaceId);
        return records.ToList();
    }

    public async Task<List<ChannelType>> GetChannelTypeList(IMongoDatabase mongoDatabase)
    {
        var collection = mongoDatabase.GetCollection<ChannelType>(DomainSettings.ChannelTypeCollectionName);
        var records = await collection.FindAsync(Builders<ChannelType>.Filter.Empty);
        return records.ToList();
    }

    public async Task<List<ChannelGroup>> GetWorkspaceChannelGroupList(IMongoDatabase mongoDatabase, string workspaceId)
    {
        var collection = mongoDatabase.GetCollection<ChannelGroup>(DomainSettings.ChannelGroupCollectionName);
        var records = await collection.FindAsync(channel => channel.WorkspaceId == workspaceId);
        return records.ToList();
    }
}
