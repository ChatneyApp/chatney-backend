using System.Security.Claims;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Infra;
using ChatneyBackend.Tests.Support;
using HotChocolate;
using ChannelSettings = ChatneyBackend.Domains.Channels.DomainSettings;

namespace ChatneyBackend.Tests.Domains.Channels;

public class DirectMessageTests
{
    private static readonly ChannelQueries Queries = new();
    private static readonly ChannelMutations Mutations = new();
    private static readonly MessageMutations MessageMutations = new();

    private static ClaimsPrincipal PrincipalFor(User user) =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Sid, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        ],
        "TestAuth"));

    private static ChannelType SeedDmChannelType(PermissionResolverTestContext ctx)
    {
        var dmType = new ChannelType
        {
            Id = 3,
            Name = ChannelSettings.DmChannelTypeName,
            Key = ChannelSettings.DmChannelTypeKey,
            SecObjId = 41,
        };
        ctx.ChannelTypesRepo.Seed(dmType);
        return dmType;
    }

    [Fact]
    public async Task OpenDirectMessage_IsIdempotent_AndIndependentOfUserOrder()
    {
        var ctx = new PermissionResolverTestContext();
        SeedDmChannelType(ctx);
        var webSocket = new RecordingWebSocketConnector();
        var resolverA = ctx.CreateResolver(ctx.UserWithRoleA);

        var first = await Mutations.OpenDirectMessage(
            ctx.Repos, resolverA, PrincipalFor(ctx.UserWithRoleA), webSocket, [ctx.UserWithRoleB.Id]);
        var second = await Mutations.OpenDirectMessage(
            ctx.Repos, resolverA, PrincipalFor(ctx.UserWithRoleA), webSocket, [ctx.UserWithRoleB.Id]);
        var fromB = await Mutations.OpenDirectMessage(
            ctx.Repos,
            ctx.CreateResolver(ctx.UserWithRoleB),
            PrincipalFor(ctx.UserWithRoleB),
            webSocket,
            [ctx.UserWithRoleA.Id]);

        Assert.Equal(first.Channel.Id, second.Channel.Id);
        Assert.Equal(first.Channel.Id, fromB.Channel.Id);
        Assert.Equal(ctx.UserWithRoleB.Id, Assert.Single(first.OtherUsers).Id);
        Assert.Equal(ctx.UserWithRoleA.Id, Assert.Single(fromB.OtherUsers).Id);
        Assert.Single(ctx.ChannelsRepo.Items, channel => channel.IsDm);
        Assert.Equal(2, ctx.ChannelMembersRepo.Items.Count(member => member.ChannelId == first.Channel.Id));
    }

    [Fact]
    public async Task OpenDirectMessage_RejectsSelfAndEmpty()
    {
        var ctx = new PermissionResolverTestContext();
        SeedDmChannelType(ctx);
        var webSocket = new RecordingWebSocketConnector();
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        var self = await Assert.ThrowsAsync<GraphQLException>(() =>
            Mutations.OpenDirectMessage(
                ctx.Repos, resolver, PrincipalFor(ctx.UserWithRoleA), webSocket, [ctx.UserWithRoleA.Id]));
        var empty = await Assert.ThrowsAsync<GraphQLException>(() =>
            Mutations.OpenDirectMessage(
                ctx.Repos, resolver, PrincipalFor(ctx.UserWithRoleA), webSocket, []));

        Assert.Equal(ChatneyBackend.Infra.ErrorCodes.ForbiddenAction, Assert.Single(self.Errors).Code);
        Assert.Equal(ChatneyBackend.Infra.ErrorCodes.ForbiddenAction, Assert.Single(empty.Errors).Code);
        Assert.DoesNotContain(ctx.ChannelsRepo.Items, channel => channel.IsDm);
    }

    [Fact]
    public async Task OpenDirectMessage_GroupOfThree_IsDistinctFromPair()
    {
        var ctx = new PermissionResolverTestContext();
        SeedDmChannelType(ctx);
        var webSocket = new RecordingWebSocketConnector();
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        var pair = await Mutations.OpenDirectMessage(
            ctx.Repos, resolver, PrincipalFor(ctx.UserWithRoleA), webSocket, [ctx.UserWithRoleB.Id]);
        var group = await Mutations.OpenDirectMessage(
            ctx.Repos,
            resolver,
            PrincipalFor(ctx.UserWithRoleA),
            webSocket,
            [ctx.UserWithRoleB.Id, ctx.UserWithNoRoles.Id]);
        var groupAgain = await Mutations.OpenDirectMessage(
            ctx.Repos,
            resolver,
            PrincipalFor(ctx.UserWithRoleA),
            webSocket,
            [ctx.UserWithNoRoles.Id, ctx.UserWithRoleB.Id]);

        Assert.NotEqual(pair.Channel.Id, group.Channel.Id);
        Assert.Equal(group.Channel.Id, groupAgain.Channel.Id);
        Assert.Equal(2, group.OtherUsers.Count);
        Assert.Equal(3, ctx.ChannelMembersRepo.Items.Count(member => member.ChannelId == group.Channel.Id));
    }

    [Fact]
    public async Task OpenDirectMessage_GrantsParticipantUserAcls_WithoutAdminRoleAcl()
    {
        var ctx = new PermissionResolverTestContext();
        SeedDmChannelType(ctx);
        ctx.RolesRepo.Seed(new Role
        {
            Id = 100,
            Name = ChatneyBackend.Domains.Roles.DomainSettings.AdminRoleName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        var webSocket = new RecordingWebSocketConnector();
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        var result = await Mutations.OpenDirectMessage(
            ctx.Repos, resolver, PrincipalFor(ctx.UserWithRoleA), webSocket, [ctx.UserWithRoleB.Id]);

        Assert.Equal(
            ChannelSettings.DirectMessageParticipantPermissions.ToHashSet(),
            ctx.UserAclsRepo.Items.Single(acl =>
                acl.UserId == ctx.UserWithRoleA.Id && acl.SecObjId == result.Channel.SecObjId)
                .Permissions.ToHashSet());
        Assert.Equal(
            ChannelSettings.DirectMessageParticipantPermissions.ToHashSet(),
            ctx.UserAclsRepo.Items.Single(acl =>
                acl.UserId == ctx.UserWithRoleB.Id && acl.SecObjId == result.Channel.SecObjId)
                .Permissions.ToHashSet());
        Assert.DoesNotContain(
            ctx.RoleAclsRepo.Items,
            acl => acl.SecObjId == result.Channel.SecObjId);
        Assert.Single(webSocket.NewChannels);
        Assert.Equal(2, webSocket.NewChannels[0].MemberUserIds.Length);
    }

    [Fact]
    public async Task DirectMessageList_ReturnsOnlyActorsDms()
    {
        var ctx = new PermissionResolverTestContext();
        SeedDmChannelType(ctx);
        var webSocket = new RecordingWebSocketConnector();

        await Mutations.OpenDirectMessage(
            ctx.Repos,
            ctx.CreateResolver(ctx.UserWithRoleA),
            PrincipalFor(ctx.UserWithRoleA),
            webSocket,
            [ctx.UserWithRoleB.Id]);
        await Mutations.OpenDirectMessage(
            ctx.Repos,
            ctx.CreateResolver(ctx.UserWithRoleA),
            PrincipalFor(ctx.UserWithRoleA),
            webSocket,
            [ctx.UserWithNoRoles.Id]);

        var forA = await Queries.GetDirectMessageList(ctx.Repos, PrincipalFor(ctx.UserWithRoleA));
        var forB = await Queries.GetDirectMessageList(ctx.Repos, PrincipalFor(ctx.UserWithRoleB));
        var forC = await Queries.GetDirectMessageList(ctx.Repos, PrincipalFor(ctx.UserWithRolesAAndB));

        Assert.Equal(2, forA.Count);
        Assert.Single(forB);
        Assert.Equal(ctx.UserWithRoleA.Id, Assert.Single(forB[0].OtherUsers).Id);
        Assert.Empty(forC);
    }

    [Fact]
    public async Task WorkspaceChannelList_DoesNotIncludeDirectMessages()
    {
        var ctx = new PermissionResolverTestContext();
        SeedDmChannelType(ctx);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadChannel);
        var webSocket = new RecordingWebSocketConnector();
        await Mutations.OpenDirectMessage(
            ctx.Repos,
            ctx.CreateResolver(ctx.UserWithRoleA),
            PrincipalFor(ctx.UserWithRoleA),
            webSocket,
            [ctx.UserWithRoleB.Id]);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var workspaceChannels = await Queries.GetWorkspaceChannelList(ctx.Repos, resolver, ctx.W1.Id);

        Assert.Contains(workspaceChannels, channel => channel.Id == ctx.C1.Id);
        Assert.DoesNotContain(workspaceChannels, channel => channel.IsDm);
    }

    [Fact]
    public async Task AddChannel_RejectsDmChannelType()
    {
        var ctx = new PermissionResolverTestContext();
        var dmType = SeedDmChannelType(ctx);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.ChannelCreateChannel);
        var webSocket = new RecordingWebSocketConnector();
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
            Mutations.AddChannel(
                ctx.Repos,
                resolver,
                new ChannelDto
                {
                    Name = "should fail",
                    ChannelTypeId = dmType.Id,
                    WorkspaceId = ctx.W1.Id,
                },
                webSocket));

        Assert.Equal(ChatneyBackend.Infra.ErrorCodes.ForbiddenAction, Assert.Single(exception.Errors).Code);
    }

    [Fact]
    public async Task NonParticipant_CannotReadOrMessageDirectMessage()
    {
        var ctx = new PermissionResolverTestContext();
        SeedDmChannelType(ctx);
        var webSocket = new RecordingWebSocketConnector();
        var opened = await Mutations.OpenDirectMessage(
            ctx.Repos,
            ctx.CreateResolver(ctx.UserWithRoleA),
            PrincipalFor(ctx.UserWithRoleA),
            webSocket,
            [ctx.UserWithRoleB.Id]);

        var outsiderResolver = ctx.CreateResolver(ctx.UserWithRolesAAndB);
        await Assert.ThrowsAsync<GraphQLException>(() =>
            Queries.GetChannelById(ctx.Repos, outsiderResolver, opened.Channel.Id));

        var outsiderPrincipal = PrincipalFor(ctx.UserWithRolesAAndB);
        await Assert.ThrowsAsync<GraphQLException>(() =>
            MessageMutations.AddMessage(
                ctx.Repos,
                outsiderResolver,
                outsiderPrincipal,
                new MessageDto { ChannelId = opened.Channel.Id, Content = "nope" },
                webSocket));
    }

    [Fact]
    public async Task Participant_CanSendAndReadMessages()
    {
        var ctx = new PermissionResolverTestContext();
        SeedDmChannelType(ctx);
        var webSocket = new RecordingWebSocketConnector();
        var opened = await Mutations.OpenDirectMessage(
            ctx.Repos,
            ctx.CreateResolver(ctx.UserWithRoleA),
            PrincipalFor(ctx.UserWithRoleA),
            webSocket,
            [ctx.UserWithRoleB.Id]);

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var channel = await Queries.GetChannelById(ctx.Repos, resolver, opened.Channel.Id);
        Assert.NotNull(channel);

        var message = await MessageMutations.AddMessage(
            ctx.Repos,
            resolver,
            PrincipalFor(ctx.UserWithRoleA),
            new MessageDto { ChannelId = opened.Channel.Id, Content = "hello" },
            webSocket);

        Assert.NotNull(message);
        Assert.Equal("hello", message.Content);

        var listed = await new MessageQueries().GetListChannelMessages(
            ctx.Repos, resolver, PrincipalFor(ctx.UserWithRoleA), opened.Channel.Id);
        Assert.Contains(listed.Messages, item => item.Id == message.Id);
    }

    [Fact]
    public async Task PermissionChain_ForDirectMessage_SkipsWorkspaceGrants()
    {
        var ctx = new PermissionResolverTestContext();
        var dmType = SeedDmChannelType(ctx);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.ChannelReadMessage);

        var dm = new Channel
        {
            Id = 50,
            Name = ChannelSettings.DmChannelName,
            ChannelTypeId = dmType.Id,
            WorkspaceId = null,
            IsDm = true,
            SecObjId = 500,
        };
        ctx.ChannelsRepo.Seed(dm);
        ctx.ChannelMembersRepo.Seed(
            new ChannelMember { ChannelId = dm.Id, UserId = ctx.UserWithRoleA.Id },
            new ChannelMember { ChannelId = dm.Id, UserId = ctx.UserWithRoleB.Id });

        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);
        var permissions = await resolver.ForChannel(dm);

        Assert.False(permissions.Can(Permission.ChannelReadMessage));
        Assert.False(permissions.Can(Permission.ChannelReadChannel));
    }
}
