using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Tests.Support;

namespace ChatneyBackend.Tests.Domains.Channels;

/// <summary>
/// Pins a deliberate Step-5 behaviour change: GetWorkspaceChannelList now gates on
/// channel.readChannel (visibility), not channel.readMessage (content access) as the old code did.
/// </summary>
public class ChannelQueriesGetWorkspaceChannelListTests
{
    [Fact]
    public async Task PrincipalWithOnlyReadMessage_DoesNotSeeChannelInList()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        var queries = new ChannelQueries();
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        var channels = await queries.GetWorkspaceChannelList(ctx.Repos, resolver, ctx.W1.Id);

        Assert.DoesNotContain(channels, c => c.Id == ctx.C1.Id);
    }

    [Fact]
    public async Task PrincipalWithReadChannel_SeesChannelInList()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadChannel);
        var queries = new ChannelQueries();
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        var channels = await queries.GetWorkspaceChannelList(ctx.Repos, resolver, ctx.W1.Id);

        Assert.Contains(channels, c => c.Id == ctx.C1.Id);
    }
}
