using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Infra;
using HotChocolate;

namespace ChatneyBackend.Tests.Domains.Roles;

public class UserPermissionsTests
{
    [Fact]
    public void Constructor_MergesRolePermissionsWithAllowlistAndDenylist()
    {
        var permissions = new UserPermissions(
            ["message.readMessage", "message.createMessage"],
            ["channel.readChannel"],
            ["message.createMessage"]);

        Assert.Equal(2, permissions.Permissions.Count);
        Assert.Contains("message.readMessage", permissions.Permissions);
        Assert.Contains("channel.readChannel", permissions.Permissions);
        Assert.DoesNotContain("message.createMessage", permissions.Permissions);
    }

    [Fact]
    public void Can_ReturnsTrueForAnyPermissionWhenAllMightyPresent()
    {
        var permissions = new UserPermissions([RolePermissions.AllMighty], [], []);

        Assert.True(permissions.Can("message.readMessage"));
        Assert.True(permissions.Can("message.createMessage", "channel.deleteChannel"));
    }

    [Fact]
    public void Require_DoesNotThrowWhenAllMightyPresent()
    {
        var permissions = new UserPermissions([RolePermissions.AllMighty], [], []);

        permissions.Require("message.readMessage", "channel.deleteChannel");
    }

    [Fact]
    public void Can_ReturnsTrueWhenPermissionPresent()
    {
        var permissions = new UserPermissions(["message.readMessage"], [], []);

        Assert.True(permissions.Can("message.readMessage"));
        Assert.False(permissions.Can("message.createMessage"));
    }

    [Fact]
    public void Require_ThrowsWhenPermissionMissing()
    {
        var permissions = new UserPermissions([], [], []);

        var exception = Assert.Throws<GraphQLException>(() =>
            permissions.Require("message.readMessage"));

        Assert.Equal(ChatneyBackend.Infra.ErrorCodes.ForbiddenAction, exception.Message);
    }

    [Fact]
    public void Require_DoesNotThrowWhenPermissionPresent()
    {
        var permissions = new UserPermissions(["message.readMessage"], [], []);

        permissions.Require("message.readMessage");
    }
}

public class ErrorCodesTests
{
    [Fact]
    public void ThrowForbidden_ThrowsGraphQlException()
    {
        var exception = Assert.Throws<GraphQLException>(ChatneyBackend.Infra.ErrorCodes.ThrowForbidden);

        Assert.Equal(ChatneyBackend.Infra.ErrorCodes.ForbiddenAction, exception.Message);
    }
}

public class RoleScopeTests
{
    [Fact]
    public void FromChannel_BuildsScopedRoleScope()
    {
        var channel = new ChatneyBackend.Domains.Channels.Channel
        {
            Id = 10,
            Name = "general",
            WorkspaceId = 1,
            ChannelTypeId = 2,
        };

        var scope = RoleScope.FromChannel(channel);

        Assert.Equal(1, scope.WorkspaceId);
        Assert.Equal(10, scope.ChannelId);
        Assert.Equal(2, scope.ChannelTypeId);
    }

    [Fact]
    public void FromWorkspace_SetsWorkspaceOnly()
    {
        var workspace = new ChatneyBackend.Domains.Workspaces.Workspace
        {
            Id = 5,
            Name = "Main",
        };

        var scope = RoleScope.FromWorkspace(workspace);

        Assert.Equal(5, scope.WorkspaceId);
        Assert.Null(scope.ChannelId);
        Assert.Null(scope.ChannelTypeId);
    }

    [Fact]
    public void FromChannelType_SetsChannelTypeOnly()
    {
        var channelType = new ChatneyBackend.Domains.Channels.ChannelType
        {
            Id = 3,
            Name = "public",
            Key = "public",
            SecObjId = 1,
        };

        var scope = RoleScope.FromChannelType(channelType);

        Assert.Null(scope.WorkspaceId);
        Assert.Null(scope.ChannelId);
        Assert.Equal(3, scope.ChannelTypeId);
    }

    [Fact]
    public void Global_SetsAllScopeFieldsNull()
    {
        var scope = RoleScope.Global();

        Assert.Null(scope.WorkspaceId);
        Assert.Null(scope.ChannelId);
        Assert.Null(scope.ChannelTypeId);
    }
}
