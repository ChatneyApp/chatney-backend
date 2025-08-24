using System.Diagnostics;
using System.Security.Claims;
using HotChocolate.Authorization;
using MongoDB.Driver;

namespace ChatneyBackend.Domains.Users;

public class UserQueries
{
    [Authorize]
    public User GetUserById(ClaimsPrincipal user, IMongoDatabase mongoDatabase, string id)
    {
        Console.WriteLine(user);
        return mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName).Find(u => u.Id == id).First();
    }

    public User GetUserByName(IMongoDatabase mongoDatabase, string name)
        => mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName).Find(u => u.Name == name).First();

    public List<User> GetList(IMongoDatabase mongoDatabase)
        => mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName).Find(Builders<User>.Filter.Empty).ToList();
}
