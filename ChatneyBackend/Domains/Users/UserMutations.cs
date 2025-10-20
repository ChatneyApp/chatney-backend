using ChatneyBackend.Utils;
using ChatneyBackend.Domains.Roles;

namespace ChatneyBackend.Domains.Users;

public class UserMutations
{
    public async Task<User> CreateUser(AppConfig appConfig, Repo<User> repo, CreateUserDTO userDto)
    {
        var user = userDto.ToModel();
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        await repo.InsertOne(user);
        return user;
    }

    public async Task<User> Register(AppConfig appConfig, Repo<User> repo, Repo<Role> rolesRepo, UserRegisterDTO userDto)
    {
        var user = userDto.ToModel();
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        // find a base role
        var userRole = await rolesRepo.GetOne(r => r.Name == Roles.DomainSettings.BaseRoleName);
        if (userRole == null)
        {
            throw new Exception("User role not found");
        }
        user.Roles.Global = userRole.Id;

        await repo.InsertOne(user);
        return user;
    }

    public Task<bool> DeleteUser(Repo<User> repo, string id) => repo.DeleteById(id);

    public async Task<UserLoginResponse?> Login(AppConfig appConfig, Repo<User> repo, string login, string password)
    {
        var passwordHash = Helpers.GetMd5Hash(password + appConfig.UserPasswordSalt);
        var user = await repo.GetOne(u => (u.Email == login || u.Name == login) && u.Password == passwordHash);

        if (user == null)
        {
            return null;
        }
        return new UserLoginResponse
        {
            Id = user.Id,
            Token = JwtHelpers.GetJwtToken(user.Email, user.Id, appConfig.JwtSecret)
        };
    }
}
