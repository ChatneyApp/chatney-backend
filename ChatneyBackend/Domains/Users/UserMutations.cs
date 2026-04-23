using ChatneyBackend.Utils;
using ChatneyBackend.Infra;
using RolesDomainSettings = ChatneyBackend.Domains.Roles.DomainSettings;

namespace ChatneyBackend.Domains.Users;

public class UserMutations
{
    public async Task<User> CreateUser(AppConfig appConfig, AppRepos repos, CreateUserDto userDto)
    {
        var user = userDto.ToModel();
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        user.Id = await repos.Users.InsertOne(user);
        return user;
    }

    public async Task<User> Register(AppConfig appConfig, AppRepos repos, UserRegisterDto userDto)
    {
        var user = userDto.ToModel();
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        var userRole = await repos.Roles.GetOne(r => r.Name == RolesDomainSettings.BaseRoleName);
        if (userRole == null)
        {
            throw new Exception("User role not found");
        }
        user.RoleId = userRole.Id;

        user.Id = await repos.Users.InsertOne(user);
        return user;
    }

    public Task<bool> DeleteUser(AppRepos repos, Guid id) => repos.Users.DeleteById(id);

    public async Task<UserLoginResponse?> Login(AppConfig appConfig, AppRepos repos, string login, string password)
    {
        var passwordHash = Helpers.GetMd5Hash(password + appConfig.UserPasswordSalt);
        var user = await repos.Users.GetOne(u => (u.Email == login || u.Name == login) && u.Password == passwordHash);

        if (user == null)
        {
            return null;
        }
        return new UserLoginResponse
        {
            Id = user.Id.ToString(),
            Token = JwtHelpers.GetJwtToken(user.Email, user.Id.ToString(), appConfig.JwtSecret)
        };
    }
}
