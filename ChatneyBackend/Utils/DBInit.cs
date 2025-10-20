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
        // == Users collection initialization ==
        var usersCollection = db.GetCollection<User>(UsersDomainSettings.UserCollectionName);
        // Create unique index on email field
        var emailIndex = Builders<User>.IndexKeys.Ascending(u => u.Email);
        var emailIndexOptions = new CreateIndexOptions { Unique = true };
        var emailIndexModel = new CreateIndexModel<User>(emailIndex, emailIndexOptions);

        var nameIndex = Builders<User>.IndexKeys.Ascending(u => u.Name);
        var nameIndexOptions = new CreateIndexOptions { Unique = true };
        var nameIndexModel = new CreateIndexModel<User>(nameIndex, nameIndexOptions);
        usersCollection.Indexes.CreateMany([nameIndexModel, emailIndexModel]);


        var messageReactionsCollection = db.GetCollection<MessageReaction>(MessageDomainSettings.ReactionCollectionName);
        var indexKeys = Builders<MessageReaction>.IndexKeys
            .Ascending(r => r.MessageId)
            .Ascending(r => r.Code)
            .Ascending(r => r.UserId);

        var indexModel = new CreateIndexModel<MessageReaction>(indexKeys, new CreateIndexOptions { Unique = true });
        messageReactionsCollection.Indexes.CreateOne(indexModel);

    }
}
