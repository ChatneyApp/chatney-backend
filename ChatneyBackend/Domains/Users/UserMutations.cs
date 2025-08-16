using MongoDB.Driver;

namespace ChatneyBackend.Domains.Users;

public class UserMutations
{
    public async Task<User> CreateUser(IMongoDatabase mongoDatabase, CreateUserDTO userDTO)
    {
        var collection = mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName);
        var user = userDTO.ToModel();
        await collection.InsertOneAsync(user);
        return user;
    }

    public async Task<User> Register(IMongoDatabase mongoDatabase, UserRegisterDTO userDTO)
    {
        var collection = mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName);
        var user = userDTO.ToModel();
        await collection.InsertOneAsync(user);
        return user;
    }

    public async Task<bool> DeleteUser(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName);
        var result = await collection.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }
}
