using System.Security.Claims;
using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Permissions;
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
        IPermissionResolver resolver,
        CreateUserDto userDto,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.UserCreateUser);

        var nickname = NicknameValidator.NormalizeAndValidate(userDto.Nickname);
        await NicknameValidator.EnsureUnique(repos, nickname);

        List<int> roleIds = userDto.RoleIds is { Count: > 0 } explicitRoleIds
            ? explicitRoleIds
            : [await GetDefaultRoleId(repos)];
        await EnsureRolesExist(repos, roleIds);

        var user = userDto.ToModel();
        user.Nickname = nickname;
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        user.Id = await repos.Users.InsertOne(user);

        foreach (var roleId in roleIds)
        {
            var userRole = new UserRole { UserId = user.Id, RoleId = roleId };
            await repos.UserRoles.InsertOne(userRole);
            await webSocketConnector.SendNewUserRoleAsync(userRole);
        }

        resolver.Invalidate();
        return user;
    }

    public async Task<User> Register(
        AppConfig appConfig,
        AppRepos repos,
        IPermissionResolver resolver,
        UserRegisterDto userDto)
    {
        var nickname = NicknameValidator.NormalizeAndValidate(userDto.Nickname);
        await NicknameValidator.EnsureUnique(repos, nickname);

        var user = userDto.ToModel();
        user.Nickname = nickname;
        user.Password = Helpers.GetMd5Hash(user.Password + appConfig.UserPasswordSalt);

        var defaultRoleId = await GetDefaultRoleId(repos);

        user.Id = await repos.Users.InsertOne(user);
        await repos.UserRoles.InsertOne(new UserRole { UserId = user.Id, RoleId = defaultRoleId });

        resolver.Invalidate();
        return user;
    }

    private static async Task<int> GetDefaultRoleId(AppRepos repos)
    {
        var defaultRoleId = await SystemConfigReader.GetIntByName(
            repos.Configs,
            Configs.DomainSettings.NewUserDefaultRole);

        if (defaultRoleId == null)
        {
            throw new Exception("New user default role is not configured");
        }

        var defaultRole = await repos.Roles.GetById(defaultRoleId.Value);

        if (defaultRole == null)
        {
            throw new Exception("New user default role not found");
        }

        return defaultRole.Id;
    }

    private static async Task EnsureRolesExist(AppRepos repos, IEnumerable<int> roleIds)
    {
        var distinctRoleIds = roleIds.ToHashSet();

        if (distinctRoleIds.Count == 0)
        {
            return;
        }

        var existingRoles = await repos.Roles.GetList(role => distinctRoleIds.Contains(role.Id));

        if (existingRoles.Count != distinctRoleIds.Count)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowRoleNotFound();
        }
    }

    [Authorize]
    public async Task<bool> DeleteUser(
        AppRepos repos,
        IPermissionResolver resolver,
        Guid id)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.UserDeleteUser);

        return await repos.Users.DeleteById(id);
    }

    [Authorize]
    public async Task<User> UpdateUser(
        AppConfig appConfig,
        AppRepos repos,
        IPermissionResolver resolver,
        UpdateUserDto userDto,
        WebSocketConnector webSocketConnector)
    {
        var permissions = await resolver.Global();
        permissions.Require(Permission.UserEditUser);

        var user = await repos.Users.GetById(userDto.Id);
        if (user == null)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNotFound();
        }

        await EnsureRolesExist(repos, userDto.RoleIds);

        var nickname = NicknameValidator.NormalizeAndValidate(userDto.Nickname);
        if (nickname != user.Nickname)
        {
            await NicknameValidator.EnsureUnique(repos, nickname, user.Id);
        }

        user.Nickname = nickname;
        user.FullName = string.IsNullOrWhiteSpace(userDto.FullName) ? null : userDto.FullName.Trim();
        user.Email = userDto.Email.Trim();
        user.Active = userDto.Active;
        user.Verified = userDto.Verified;
        user.Banned = userDto.Banned;
        user.Muted = userDto.Muted;

        if (!string.IsNullOrWhiteSpace(userDto.Password))
        {
            user.Password = Helpers.GetMd5Hash(userDto.Password + appConfig.UserPasswordSalt);
        }

        await repos.Users.UpdateOne(user);

        var existingUserRoles = await repos.UserRoles.GetList(userRole => userRole.UserId == user.Id);
        var existingRoleIds = existingUserRoles.Select(userRole => userRole.RoleId).ToHashSet();
        var desiredRoleIds = userDto.RoleIds.ToHashSet();

        foreach (var roleId in desiredRoleIds.Except(existingRoleIds))
        {
            var userRole = new UserRole { UserId = user.Id, RoleId = roleId };
            await repos.UserRoles.InsertOne(userRole);
            await webSocketConnector.SendNewUserRoleAsync(userRole);
        }

        foreach (var roleId in existingRoleIds.Except(desiredRoleIds))
        {
            var key = new UserRoleKey(user.Id, roleId);
            await repos.UserRoles.DeleteById(key);
            await webSocketConnector.SendDeletedUserRoleAsync(WebsocketUserRoleDeletedPayload.FromKey(key));
        }

        resolver.Invalidate();
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
        var user = await UserLookup.FindByLogin(repos, login, passwordHash);

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
