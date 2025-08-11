using MongoDB.Driver;

namespace ChatneyBackend.Domains.Messages;

public class MessageMutations
{
    public Message AddMessage(IMongoDatabase mongoDatabase, Message message)
    {
        var collection = mongoDatabase.GetCollection<Message>("messages");
        collection.InsertOne(message);
        return message;
    }

    public bool DeleteMessage(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<Message>("messages");
        var result = collection.DeleteOne(m => m.Id == id);
        return result.DeletedCount > 0;
    }
}
