using ChatneyBackend.Domains.Configs;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Tests.Support;
using ChatneyBackend.Utils;
using HotChocolate;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChatneyBackend.Tests.Domains.Users;

public class UserMutationsTests
{
    private static readonly AppConfig AppConfig = new() { UserPasswordSalt = "salt", JwtSecret = "secret" };

    private static UserMutations Mutations => new();

    [Fact]
    public async Task CreateUser_WithExplicitRoleIds_InsertsUserRolesAndInvalidatesResolver()
    {
        var context = new UserRoleMutationsTestContext([Permission.UserCreateUser]);

        var user = await Mutations.CreateUser(
            AppConfig,
            context.Repos,
            context.Resolver,
            new CreateUserDto
            {
                Nickname = "new_user",
                Email = "new_user@example.com",
                Password = "password",
                RoleIds = [context.AssignedRole.Id],
            },
            context.WebSocket);

        var stored = context.UserRolesRepo.Items.Single(userRole => userRole.UserId == user.Id);
        Assert.Equal(context.AssignedRole.Id, stored.RoleId);
        Assert.Single(context.WebSocket.NewUserRoles);
    }

    [Fact]
    public async Task CreateUser_WithUnknownRoleId_ThrowsAndInsertsNothing()
    {
        var context = new UserRoleMutationsTestContext([Permission.UserCreateUser]);
        const int unknownRoleId = 999;

        await Assert.ThrowsAsync<GraphQLException>(() => Mutations.CreateUser(
            AppConfig,
            context.Repos,
            context.Resolver,
            new CreateUserDto
            {
                Nickname = "new_user",
                Email = "new_user@example.com",
                Password = "password",
                RoleIds = [unknownRoleId],
            },
            context.WebSocket));

        Assert.DoesNotContain(context.UsersRepo.Items, u => u.Nickname == "new_user");
    }

    [Fact]
    public async Task CreateUser_WithoutRoleIds_FallsBackToConfiguredDefaultRole()
    {
        var context = new UserRoleMutationsTestContext([Permission.UserCreateUser]);
        context.ConfigsRepo.Seed(new Config
        {
            Name = ChatneyBackend.Domains.Configs.DomainSettings.NewUserDefaultRole,
            Value = context.AssignedRole.Id.ToString(),
            Type = "int",
        });

        var user = await Mutations.CreateUser(
            AppConfig,
            context.Repos,
            context.Resolver,
            new CreateUserDto
            {
                Nickname = "new_user",
                Email = "new_user@example.com",
                Password = "password",
            },
            context.WebSocket);

        var stored = context.UserRolesRepo.Items.Single(userRole => userRole.UserId == user.Id);
        Assert.Equal(context.AssignedRole.Id, stored.RoleId);
    }

    [Fact]
    public async Task UpdateUser_RoleDiffing_AddsAndRemovesRolesWithoutDuplicating()
    {
        var context = new UserRoleMutationsTestContext();
        var keptRole = new Role { Id = 3, Name = "kept", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var droppedRole = new Role { Id = 4, Name = "dropped", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.RolesRepo.Seed(keptRole, droppedRole);
        context.UserRolesRepo.Seed(
            new UserRole { UserId = context.TargetUser.Id, RoleId = keptRole.Id },
            new UserRole { UserId = context.TargetUser.Id, RoleId = droppedRole.Id });

        await Mutations.UpdateUser(
            AppConfig,
            context.Repos,
            context.Resolver,
            new UpdateUserDto
            {
                Id = context.TargetUser.Id,
                Nickname = context.TargetUser.Nickname,
                Email = context.TargetUser.Email,
                Active = true,
                Verified = true,
                Banned = false,
                Muted = false,
                RoleIds = [keptRole.Id, context.AssignedRole.Id],
            },
            context.WebSocket);

        var storedRoleIds = context.UserRolesRepo.Items
            .Where(userRole => userRole.UserId == context.TargetUser.Id)
            .Select(userRole => userRole.RoleId)
            .ToHashSet();

        Assert.Equal(new HashSet<int> { keptRole.Id, context.AssignedRole.Id }, storedRoleIds);
        Assert.Single(context.WebSocket.NewUserRoles);
        Assert.Equal(context.AssignedRole.Id, context.WebSocket.NewUserRoles[0].RoleId);
        Assert.Single(context.WebSocket.DeletedUserRoles);
        Assert.Equal(droppedRole.Id, context.WebSocket.DeletedUserRoles[0].RoleId);
    }

    [Fact]
    public async Task UpdateUser_WithNonexistentRoleId_ThrowsGraphQLErrorInsteadOfFkFailure()
    {
        var context = new UserRoleMutationsTestContext();
        const int unknownRoleId = 999;

        var exception = await Assert.ThrowsAsync<GraphQLException>(() => Mutations.UpdateUser(
            AppConfig,
            context.Repos,
            context.Resolver,
            new UpdateUserDto
            {
                Id = context.TargetUser.Id,
                Nickname = context.TargetUser.Nickname,
                Email = context.TargetUser.Email,
                Active = true,
                Verified = true,
                Banned = false,
                Muted = false,
                RoleIds = [unknownRoleId],
            },
            context.WebSocket));

        Assert.Equal(ChatneyBackend.Infra.ErrorCodes.RoleNotFound, Assert.Single(exception.Errors).Code);
        Assert.DoesNotContain(context.UserRolesRepo.Items, userRole => userRole.UserId == context.TargetUser.Id);
    }

    [Fact]
    public async Task Register_AssignsConfiguredDefaultRole()
    {
        var context = new UserRoleMutationsTestContext();
        context.ConfigsRepo.Seed(new Config
        {
            Name = ChatneyBackend.Domains.Configs.DomainSettings.NewUserDefaultRole,
            Value = context.AssignedRole.Id.ToString(),
            Type = "int",
        });

        var user = await Mutations.Register(
            AppConfig,
            context.Repos,
            context.Resolver,
            new UserRegisterDto
            {
                Nickname = "registrant",
                Email = "registrant@example.com",
                Password = "password",
            });

        var stored = context.UserRolesRepo.Items.Single(userRole => userRole.UserId == user.Id);
        Assert.Equal(context.AssignedRole.Id, stored.RoleId);
    }

    [Fact]
    public async Task Register_ThrowsWhenDefaultRoleConfigIsMissing()
    {
        var context = new UserRoleMutationsTestContext();

        await Assert.ThrowsAsync<Exception>(() => Mutations.Register(
            AppConfig,
            context.Repos,
            context.Resolver,
            new UserRegisterDto
            {
                Nickname = "registrant",
                Email = "registrant@example.com",
                Password = "password",
            }));
    }

    [Fact]
    public async Task Register_ThrowsWhenDefaultRoleConfigPointsAtNonexistentRole()
    {
        var context = new UserRoleMutationsTestContext();
        context.ConfigsRepo.Seed(new Config
        {
            Name = ChatneyBackend.Domains.Configs.DomainSettings.NewUserDefaultRole,
            Value = "999",
            Type = "int",
        });

        await Assert.ThrowsAsync<Exception>(() => Mutations.Register(
            AppConfig,
            context.Repos,
            context.Resolver,
            new UserRegisterDto
            {
                Nickname = "registrant",
                Email = "registrant@example.com",
                Password = "password",
            }));
    }

    [Fact]
    public async Task RequestPasswordReset_CreatesOneHourRequestAndSendsEmail()
    {
        var context = new UserRoleMutationsTestContext();
        var emailSender = new RecordingPasswordResetEmailSender();
        var before = DateTime.UtcNow;

        var result = await Mutations.RequestPasswordReset(
            context.Repos,
            emailSender,
            NullLogger<UserMutations>.Instance,
            context.TargetUser.Nickname.ToUpperInvariant(),
            CancellationToken.None);

        var request = Assert.Single(context.PasswordResetRequestsRepo.Items);
        Assert.True(result);
        Assert.Equal(context.TargetUser.Id, request.UserId);
        Assert.InRange(request.ValidUntil, before.AddHours(1), DateTime.UtcNow.AddHours(1));
        Assert.Equal((context.TargetUser.Email, request.Code), Assert.Single(emailSender.Messages));
    }

    [Fact]
    public async Task RequestPasswordReset_ForUnknownLogin_ReturnsSameResponseWithoutCreatingRequest()
    {
        var context = new UserRoleMutationsTestContext();
        var emailSender = new RecordingPasswordResetEmailSender();

        var result = await Mutations.RequestPasswordReset(
            context.Repos,
            emailSender,
            NullLogger<UserMutations>.Instance,
            "missing@example.com",
            CancellationToken.None);

        Assert.True(result);
        Assert.Empty(context.PasswordResetRequestsRepo.Items);
        Assert.Empty(emailSender.Messages);
    }

    [Fact]
    public async Task ResetPassword_UpdatesPasswordAndConsumesAllUserRequests()
    {
        var context = new UserRoleMutationsTestContext();
        var code = Guid.NewGuid();
        context.PasswordResetRequestsRepo.Seed(
            new PasswordResetRequest
            {
                Code = code,
                UserId = context.TargetUser.Id,
                CreatedAt = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddHours(1),
            },
            new PasswordResetRequest
            {
                Code = Guid.NewGuid(),
                UserId = context.TargetUser.Id,
                CreatedAt = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddHours(1),
            });

        var result = await Mutations.ResetPassword(
            AppConfig,
            context.Repos,
            code,
            "new-password");

        Assert.True(result);
        Assert.Equal(
            Helpers.GetMd5Hash("new-password" + AppConfig.UserPasswordSalt),
            context.TargetUser.Password);
        Assert.Empty(context.PasswordResetRequestsRepo.Items);
        Assert.False(await Mutations.ResetPassword(
            AppConfig,
            context.Repos,
            code,
            "another-password"));
    }

    [Fact]
    public async Task ResetPassword_RejectsAndDeletesExpiredRequest()
    {
        var context = new UserRoleMutationsTestContext();
        var originalPassword = context.TargetUser.Password;
        var code = Guid.NewGuid();
        context.PasswordResetRequestsRepo.Seed(new PasswordResetRequest
        {
            Code = code,
            UserId = context.TargetUser.Id,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ValidUntil = DateTime.UtcNow.AddHours(-1),
        });

        var result = await Mutations.ResetPassword(
            AppConfig,
            context.Repos,
            code,
            "new-password");

        Assert.False(result);
        Assert.Equal(originalPassword, context.TargetUser.Password);
        Assert.Empty(context.PasswordResetRequestsRepo.Items);
    }

    private sealed class RecordingPasswordResetEmailSender : IPasswordResetEmailSender
    {
        public List<(string Email, Guid Code)> Messages { get; } = [];

        public Task SendAsync(
            User user,
            Guid code,
            CancellationToken cancellationToken = default)
        {
            Messages.Add((user.Email, code));
            return Task.CompletedTask;
        }
    }
}
