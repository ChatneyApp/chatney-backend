using ChatneyBackend.Utils;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Users;

public class UserMutations
{
    public async Task<User> CreateUser(AppConfig appConfig, PgRepo<User, Guid> repo, CreateUserDTO userDto)
    {
        var user = userDto.ToModel();
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        await repo.InsertOne(user);
        return user;
    }

    public async Task<User> Register(AppConfig appConfig, PgRepo<User, Guid> repo, PgRepo<Role, int> rolesRepo, UserRegisterDTO userDto)
    {
        var user = userDto.ToModel();
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        var userRole = await rolesRepo.GetOne(r => r.Name == Roles.DomainSettings.BaseRoleName);
        if (userRole == null)
        {
            throw new Exception("User role not found");
        }
        user.GlobalRoleId = userRole.Id;
    
        var id = await repo.InsertOne(user);
        user.Id = id;
        return user;
    }

    public Task<bool> DeleteUser(PgRepo<User, Guid> repo, Guid id) => repo.DeleteById(id);

    public async Task<UserLoginResponse?> Login(AppConfig appConfig, PgRepo<User, Guid> repo, string login, string password)
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
            Token = JwtHelpers.GetJwtToken(user.Email, user.Id.ToString(), appConfig.JwtSecret)
        };
    }
}
