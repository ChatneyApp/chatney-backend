using MongoDB.Driver;

namespace ChatneyBackend.Domains.Users;

public class UserQueries
{
    public User GetUserById(IMongoDatabase mongoDatabase, string id)
        => mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName).Find(u => u.Id == id).First();

    public User GetUserByName(IMongoDatabase mongoDatabase, string name)
        => mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName).Find(u => u.Name == name).First();

    public List<User> GetList(IMongoDatabase mongoDatabase)
        => mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName).Find(Builders<User>.Filter.Empty).ToList();
}
