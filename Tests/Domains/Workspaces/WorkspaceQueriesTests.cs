using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Tests.Support;

namespace ChatneyBackend.Tests.Domains.Workspaces;

/// <summary>
/// Pins a deliberate Step-5 behaviour change: GetList now filters on workspace.readWorkspace per
/// workspace object, rather than deriving visible workspaces transitively from readable channels.
/// </summary>
public class WorkspaceQueriesGetListTests
{
    [Fact]
    public async Task PrincipalWithReadWorkspaceOnW1Only_SeesOnlyW1()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.W1SecObjId, Permission.WorkspaceReadWorkspace);
        var queries = new WorkspaceQueries();
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        var workspaces = await queries.GetList(resolver);

        Assert.Contains(workspaces, w => w.Id == ctx.W1.Id);
        Assert.DoesNotContain(workspaces, w => w.Id == ctx.W2.Id);
    }

    /// <summary>
    /// Deliberate behaviour change: channel-level grants alone no longer imply workspace visibility.
    /// </summary>
    [Fact]
    public async Task PrincipalWithOnlyChannelLevelGrants_SeesNoWorkspaces()
    {
        var ctx = new PermissionResolverTestContext();
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadMessage);
        ctx.GrantRoleAcl(ctx.RoleA.Id, PermissionResolverTestContext.C1SecObjId, Permission.ChannelReadChannel);
        var queries = new WorkspaceQueries();
        var resolver = ctx.CreateResolver(ctx.UserWithRoleA);

        var workspaces = await queries.GetList(resolver);

        Assert.Empty(workspaces);
    }
}
