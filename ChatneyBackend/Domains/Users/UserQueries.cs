using MongoDB.Driver;

namespace ChatneyBackend.Domains.Users;

public class UserQueries
{
    public User GetUserById(IMongoDatabase mongoDatabase, string id)
        => mongoDatabase.GetCollection<User>("users").Find(u => u.Id == id).First();

    public User GetUserByName(IMongoDatabase mongoDatabase, string name)
        => mongoDatabase.GetCollection<User>("users").Find(u => u.Name == name).First();

    public List<User> GetList(IMongoDatabase mongoDatabase)
        => mongoDatabase.GetCollection<User>("users").Find(Builders<User>.Filter.Empty).ToList();
}
