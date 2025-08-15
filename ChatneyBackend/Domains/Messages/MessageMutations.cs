using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class MessageMutations
{
    public async Task<Message> AddMessage(IMongoDatabase mongoDatabase, MessageDTO messageDto)
    {
        var collection = mongoDatabase.GetCollection<Message>("messages");
        Message message = Message.FromDTO(messageDto);
        await collection.InsertOneAsync(message);
        return message;
    }

    public async Task<Message?> UpdateMessage(IMongoDatabase mongoDatabase, Message message)
    {
        var collection = mongoDatabase.GetCollection<Message>("messages");
        var filter = Builders<Message>.Filter.Eq("_id", message.Id);
        var result = await collection.ReplaceOneAsync(filter, message);
        return result.ModifiedCount > 0 ? message : null;
    }

    public async Task<bool> DeleteMessage(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Message>("messages");
        var result = await collection.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
