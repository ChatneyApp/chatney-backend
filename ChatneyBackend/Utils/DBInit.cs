using ChatneyBackend.Domains.Users;
using MongoDB.Driver;
using UsersDomainSettings = ChatneyBackend.Domains.Users.DomainSettings;
using MessageDomainSettings = ChatneyBackend.Domains.Messages.DomainSettings;

namespace ChatneyBackend.Utils;

public static class DbInit
{
    public static void Init(MongoClient mongoClient, string dbName)
    {
        var db = mongoClient.GetDatabase(dbName);
        var messageReactionsCollection = db.GetCollection<MessageReaction>(MessageDomainSettings.ReactionCollectionName);
        var indexKeys = Builders<MessageReaction>.IndexKeys
            .Ascending(r => r.MessageId)
            .Ascending(r => r.Code)
            .Ascending(r => r.UserId);

        var indexModel = new CreateIndexModel<MessageReaction>(indexKeys, new CreateIndexOptions { Unique = true });
        messageReactionsCollection.Indexes.CreateOne(indexModel);

    }
}
