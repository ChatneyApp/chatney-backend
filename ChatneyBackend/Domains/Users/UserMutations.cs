using System.Security.Claims;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using ChatneyBackend.Infra.Middleware;
using ChatneyBackend.Utils;
using HotChocolate.Authorization;

namespace ChatneyBackend.Domains.Users;

public class UserMutations
{
    [Authorize]
    public async Task<User> CreateUser(
        AppConfig appConfig,
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        CreateUserDto userDto)
    {
        var currentUser = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(currentUser, RoleScope.Global());
        permissions.Require(UserPermissionNames.CreateUser);

        var user = userDto.ToModel();
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        user.Id = await repos.Users.InsertOne(user);
        return user;
    }

    public async Task<User> Register(AppConfig appConfig, AppRepos repos, UserRegisterDto userDto)
    {
        var user = userDto.ToModel();
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        var defaultRoleId = await SystemConfigReader.GetIntByName(
            repos.Configs,
            Configs.DomainSettings.NewUserDefaultRole);
        if (defaultRoleId == null)
        {
            throw new Exception("New user default role is not configured");
        }

        var userRole = await repos.Roles.GetById(defaultRoleId.Value);
        if (userRole == null)
        {
            throw new Exception("New user default role not found");
        }
        user.RoleId = userRole.Id;

        user.Id = await repos.Users.InsertOne(user);
        return user;
    }

    [Authorize]
    public async Task<bool> DeleteUser(
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        Guid id)
    {
        var currentUser = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(currentUser, RoleScope.Global());
        permissions.Require(UserPermissionNames.DeleteUser);

        return await repos.Users.DeleteById(id);
    }

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
