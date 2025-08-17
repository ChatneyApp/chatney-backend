using System.Text;
using ChatneyBackend.Utils;
using MongoDB.Driver;
using System;
using System.Security.Claims;
using System.Text;


namespace ChatneyBackend.Domains.Users;

public class UserMutations
{
    public async Task<UserResponse> CreateUser(IMongoDatabase mongoDatabase, CreateUserDTO userDTO)
    {
        var collection = mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName);
        var user = userDTO.ToModel();
        await collection.InsertOneAsync(user);
        return user.ToResponse();
    }

    public async Task<UserResponse> Register(IMongoDatabase mongoDatabase, UserRegisterDTO userDTO)
    {
        var collection = mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName);
        var user = userDTO.ToModel();
        await collection.InsertOneAsync(user);
        return user.ToResponse();
    }

    public async Task<bool> DeleteUser(IMongoDatabase mongoDatabase, string id)
    {
        var collection = mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName);
        var result = await collection.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<UserLoginResponse?> Login(AppConfig appConfig, IMongoDatabase mongoDatabase, string login, string password)
    {
        var passwordHash = Helpers.GetMd5Hash(password + appConfig.UserPasswordSalt);
        var userStask = mongoDatabase.GetCollection<User>(DomainSettings.UserCollectionName).FindAsync(u => (u.Email == login || u.Name == login) && u.Password == passwordHash);
        var users = (await userStask).ToList();

        if (users.Count == 0)
        {
            return null;
        }
        else
        {
            return new UserLoginResponse
            {
                Id = users[0].Id,
                Token = "sfsdfds"
            };
        }
    }
}
