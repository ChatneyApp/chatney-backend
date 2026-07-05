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

        var nickname = NicknameValidator.NormalizeAndValidate(userDto.Nickname);
        await NicknameValidator.EnsureUnique(repos, nickname);

        var user = userDto.ToModel();
        user.Nickname = nickname;
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        user.Id = await repos.Users.InsertOne(user);
        return user;
    }

    public async Task<User> Register(AppConfig appConfig, AppRepos repos, UserRegisterDto userDto)
    {
        var nickname = NicknameValidator.NormalizeAndValidate(userDto.Nickname);
        await NicknameValidator.EnsureUnique(repos, nickname);

        var user = userDto.ToModel();
        user.Nickname = nickname;
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

    [Authorize]
    public async Task<User> UpdateUser(
        AppConfig appConfig,
        AppRepos repos,
        RoleManager roleManager,
        ClaimsPrincipal principal,
        UpdateUserDto userDto)
    {
        var currentUser = await principal.GetRequiredUser(repos);
        var permissions = await roleManager.GetUserPermissions(currentUser, RoleScope.Global());
        permissions.Require(UserPermissionNames.EditUser);

        var user = await repos.Users.GetById(userDto.Id);
        if (user == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        var nickname = NicknameValidator.NormalizeAndValidate(userDto.Nickname);
        await NicknameValidator.EnsureUnique(repos, nickname, user.Id);

        user.Nickname = nickname;
        user.FullName = string.IsNullOrWhiteSpace(userDto.FullName) ? null : userDto.FullName.Trim();
        user.Email = userDto.Email.Trim();
        user.Active = userDto.Active;
        user.Verified = userDto.Verified;
        user.Banned = userDto.Banned;
        user.Muted = userDto.Muted;
        user.RoleId = userDto.RoleId;

        if (!string.IsNullOrWhiteSpace(userDto.Password))
        {
            user.Password = Helpers.GetMd5Hash(userDto.Password + appConfig.UserPasswordSalt);
        }

        await repos.Users.UpdateOne(user);
        return user;
    }

    [Authorize]
    public async Task<User> UpdateMyProfile(
        AppConfig appConfig,
        AppRepos repos,
        ClaimsPrincipal principal,
        UpdateMyProfileDto profileDto)
    {
        var user = await principal.GetRequiredUser(repos);

        if (!string.IsNullOrWhiteSpace(profileDto.Nickname))
        {
            var nickname = NicknameValidator.NormalizeAndValidate(profileDto.Nickname);
            await NicknameValidator.EnsureUnique(repos, nickname, user.Id);
            user.Nickname = nickname;
        }

        if (profileDto.FullName != null)
        {
            user.FullName = string.IsNullOrWhiteSpace(profileDto.FullName)
                ? null
                : profileDto.FullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(profileDto.Email))
        {
            user.Email = profileDto.Email.Trim();
        }

        if (profileDto.AvatarUrl != null)
        {
            user.AvatarUrl = string.IsNullOrWhiteSpace(profileDto.AvatarUrl)
                ? null
                : profileDto.AvatarUrl.Trim();
        }

        if (!string.IsNullOrWhiteSpace(profileDto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(profileDto.CurrentPassword))
            {
                ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
            }

            var currentPasswordHash = Helpers.GetMd5Hash(
                profileDto.CurrentPassword + appConfig.UserPasswordSalt);
            if (user.Password != currentPasswordHash)
            {
                ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
            }

            user.Password = Helpers.GetMd5Hash(profileDto.NewPassword + appConfig.UserPasswordSalt);
        }

        await repos.Users.UpdateOne(user);
        return user;
    }

    public async Task<UserLoginResponse?> Login(AppConfig appConfig, AppRepos repos, string login, string password)
    {
        var passwordHash = Helpers.GetMd5Hash(password + appConfig.UserPasswordSalt);
        var loginLower = login.Trim().ToLowerInvariant();
        var user = await repos.Users.GetOne(u =>
            (u.Email.ToLower() == loginLower || u.Nickname.ToLower() == loginLower) &&
            u.Password == passwordHash);

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
