using System.Security.Claims;
using Amazon.S3;
using Amazon.S3.Model;
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
    /// <summary>
    /// Creates a user as an admin. Requires UserCreateUser. Assigns roleIds, or the configured default
    /// role if none are given. ROLE_NOT_FOUND if any role id doesn't exist.
    /// </summary>
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

    /// <summary>Self-registration (no auth). Creates an active user with the configured default role.</summary>
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

    /// <summary>Deletes a user with their role assignments and ACLs. Requires UserDeleteUser.</summary>
    /// <returns>True if the user existed and was deleted.</returns>
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

    /// <summary>
    /// Updates a user as an admin and replaces their role assignments with roleIds. Requires UserEditUser.
    /// NOT_FOUND if the user doesn't exist, ROLE_NOT_FOUND if any role id doesn't exist.
    /// </summary>
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
        var nicknameChanged = nickname != user.Nickname;

        if (nicknameChanged)
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

        if (nicknameChanged)
        {
            await webSocketConnector.SendUpdatedUserProfileAsync(WebsocketUserProfilePayload.FromUser(user));
        }

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

    /// <summary>Updates the current user's own profile. Only non-null fields are applied.</summary>
    [Authorize]
    public async Task<User> UpdateMyProfile(
        AppConfig appConfig,
        AppRepos repos,
        ClaimsPrincipal principal,
        UpdateMyProfileDto profileDto,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);
        var oldNickname = user.Nickname;
        var oldAvatarUrl = user.AvatarUrl;

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

        if (profileDto.Bio != null)
        {
            user.Bio = profileDto.Bio.Trim();
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

        if (user.Nickname != oldNickname || user.AvatarUrl != oldAvatarUrl)
        {
            await webSocketConnector.SendUpdatedUserProfileAsync(WebsocketUserProfilePayload.FromUser(user));
        }

        return user;
    }

    /// <summary>
    /// Uploads the current user's avatar to storage and sets avatarUrl to its storage path
    /// ("/avatars/{userId}.{ext}"), replacing any previous avatar. INVALID_AVATAR unless the file is
    /// a GIF, JPG or PNG of exactly 128x128 pixels and at most 2 MB.
    /// </summary>
    /// <param name="file">The image (multipart upload).</param>
    [Authorize]
    public async Task<User> UploadAvatar(
        AppConfig appConfig,
        AppRepos repos,
        ClaimsPrincipal principal,
        IAmazonS3 s3Client,
        IFile file,
        WebSocketConnector webSocketConnector)
    {
        if (file.Length is null or 0 or > DomainSettings.AvatarMaxFileSize)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowInvalidAvatar();
        }

        using var content = new MemoryStream();

        await using (var fileStream = file.OpenReadStream())
        {
            await fileStream.CopyToAsync(content);
        }

        var image = AvatarImage.Parse(content.GetBuffer().AsSpan(0, (int)content.Length));

        if (image == null
            || image.Width != DomainSettings.AvatarSideSize
            || image.Height != DomainSettings.AvatarSideSize)
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowInvalidAvatar();
        }

        var user = await principal.GetRequiredUser(repos);
        var s3Key = $"{DomainSettings.AvatarS3Folder}/{user.Id}.{image.Extension}";
        content.Position = 0;

        await s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = appConfig.S3Bucket,
            Key = s3Key,
            InputStream = content,
            ContentType = image.ContentType,
        });

        // The extension is part of the key, so a previous avatar of another format would be left behind.
        foreach (var extension in AvatarImage.Extensions.Where(extension => extension != image.Extension))
        {
            await s3Client.DeleteObjectAsync(
                appConfig.S3Bucket,
                $"{DomainSettings.AvatarS3Folder}/{user.Id}.{extension}");
        }

        user.AvatarUrl = $"/{s3Key}";
        await repos.Users.UpdateOne(user);
        await webSocketConnector.SendUpdatedUserProfileAsync(WebsocketUserProfilePayload.FromUser(user));

        return user;
    }

    /// <summary>
    /// Removes the current user's avatar: sets avatarUrl to null and deletes the uploaded file from storage.
    /// Does nothing if the user has no avatar.
    /// </summary>
    [Authorize]
    public async Task<User> ClearAvatar(
        AppConfig appConfig,
        AppRepos repos,
        ClaimsPrincipal principal,
        IAmazonS3 s3Client,
        WebSocketConnector webSocketConnector)
    {
        var user = await principal.GetRequiredUser(repos);

        if (user.AvatarUrl == null)
        {
            return user;
        }

        // avatarUrl can also be set freely through updateMyProfile, so only the user's own uploaded file is deleted.
        if (user.AvatarUrl.StartsWith($"/{DomainSettings.AvatarS3Folder}/{user.Id}."))
        {
            try
            {
                await s3Client.DeleteObjectAsync(appConfig.S3Bucket, user.AvatarUrl[1..]);
            }
            catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // The file is already gone.
            }
        }

        user.AvatarUrl = null;
        await repos.Users.UpdateOne(user);
        await webSocketConnector.SendUpdatedUserProfileAsync(WebsocketUserProfilePayload.FromUser(user));

        return user;
    }

    /// <summary>Exchanges credentials for a JWT. No auth required.</summary>
    /// <param name="login">Email or nickname, case-insensitive.</param>
    /// <param name="password">Plain-text password.</param>
    /// <returns>The user id and token, or null if the credentials are wrong.</returns>
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

    /// <summary>
    /// Sends a one-hour password-reset link when the email or nickname exists.
    /// Always returns true so callers cannot discover registered accounts.
    /// </summary>
    public async Task<bool> RequestPasswordReset(
        AppRepos repos,
        IPasswordResetEmailSender emailSender,
        ILogger<UserMutations> logger,
        string login,
        CancellationToken cancellationToken)
    {
        var user = await UserLookup.FindByLogin(repos, login);
        if (user == null)
        {
            return true;
        }

        await repos.PasswordResetRequests.Delete(request => request.UserId == user.Id);

        var now = DateTime.UtcNow;
        var request = new PasswordResetRequest
        {
            Code = Guid.NewGuid(),
            UserId = user.Id,
            CreatedAt = now,
            ValidUntil = now.AddHours(1),
        };
        await repos.PasswordResetRequests.InsertOne(request);

        try
        {
            await emailSender.SendAsync(user, request.Code, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to send password-reset email for user {UserId}",
                user.Id);
        }

        return true;
    }

    /// <summary>Sets a new password when the reset code exists, is unexpired, and has not been used.</summary>
    public async Task<bool> ResetPassword(
        AppConfig appConfig,
        AppRepos repos,
        Guid code,
        string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var request = await repos.PasswordResetRequests.GetById(code);
        if (request == null)
        {
            return false;
        }

        if (request.ValidUntil <= DateTime.UtcNow)
        {
            await repos.PasswordResetRequests.DeleteById(code);
            return false;
        }

        // Delete first so two simultaneous submissions cannot both consume the same code.
        if (!await repos.PasswordResetRequests.DeleteById(code))
        {
            return false;
        }

        var user = await repos.Users.GetById(request.UserId);
        if (user == null)
        {
            return false;
        }

        user.Password = Helpers.GetMd5Hash(password + appConfig.UserPasswordSalt);
        await repos.Users.UpdateOne(user);
        await repos.PasswordResetRequests.Delete(other => other.UserId == user.Id);
        return true;
    }
}
