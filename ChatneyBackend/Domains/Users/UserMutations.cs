using MongoDB.Driver;

namespace ChatneyBackend.Domains.Users;

public class UserMutations
{
    public User AddUser(IMongoDatabase mongoDatabase, User user)
    {
        var collection = mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName);
        collection.InsertOne(user);
        return user;
    }

    public bool DeleteUser(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName);
        var result = collection.DeleteOne(u => u.Id == id);
        return result.DeletedCount > 0;
    }
}
