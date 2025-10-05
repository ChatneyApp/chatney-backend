using ChatneyBackend.Domains.Users;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatneyBackend.Domains.Messages;

public class MessageQueries
{
    public async Task<Message?> GetMessageById(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);
        var records = (await collection.FindAsync(r => r.Id == id)).ToList();
        return records.Count > 0
            ? records[0]
            : null;
    }

    public async Task<List<MessageWithUser>> GetListChannelMessages(IMongoDatabase mongoDatabase, string channelId)
    {
        var collection = mongoDatabase.GetCollection<Message>(DomainSettings.MessageCollectionName);

        var pipeline = new[]
            {
            new BsonDocument("$match", new BsonDocument("channelId", channelId)),

            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "users" },
                { "localField", "userId" },
                { "foreignField", "_id" },
                { "as", "user" }
            }),

            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$user" },
                { "preserveNullAndEmptyArrays", true } // Optional: allows messages with missing users
            }),

            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 1 },
                { "channelId", 1 },
                { "userId", 1 },
                { "content", 1 },
                { "attachments", 1 },
                { "status", 1 },
                { "createdAt", 1 },
                { "updatedAt", 1 },
                { "reactions", 1 },
                { "parentId", 1 },
                {
                    "user", new BsonDocument
                    {
                        { "_id", 1 },
                        { "name", 1 },
                        { "avatarUrl", 1 }
                    }
                } // remove temporary 'user' field used for lookup
            }),
        };

        try
        {
            var result = await collection.AggregateAsync<MessageWithUser>(pipeline);
            return await result.ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return new List<MessageWithUser>();
        }
    }
}
