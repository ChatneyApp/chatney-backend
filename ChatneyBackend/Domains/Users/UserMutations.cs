using System.Text;
using ChatneyBackend.Utils;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace ChatneyBackend.Domains.Users;

public class UserMutations
{
    public async Task<User> CreateUser(AppConfig appConfig, Repo<User> repo, CreateUserDTO userDTO)
    {
        var user = userDTO.ToModel();
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        await repo.InsertOne(user);
        return user;
    }

    public async Task<User> Register(AppConfig appConfig, Repo<User> repo, UserRegisterDTO userDTO)
    {
        var user = userDTO.ToModel();
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        await repo.InsertOne(user);
        return user;
    }

    public Task<bool> DeleteUser(Repo<User> repo, string id) => repo.DeleteById(id);

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
                Token = JwtHelpers.GetJwtToken(users[0].Email, users[0].Id, appConfig.JwtSecret)
            };
        }
    }
}
