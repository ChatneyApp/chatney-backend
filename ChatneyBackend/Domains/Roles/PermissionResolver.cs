using System.Collections.Frozen;
using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Permissions;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Roles;

/// <summary>
/// A single object's resolved permission set (allow-list, additive across the object hierarchy).
/// </summary>
public sealed class EffectivePermissions
{
    public static readonly EffectivePermissions Empty = new(FrozenSet<Permission>.Empty);

    public IReadOnlySet<Permission> Permissions { get; }

    public EffectivePermissions(IReadOnlySet<Permission> permissions)
    {
        Permissions = permissions;
    }

    /// <summary>With zero arguments, vacuously true (there is nothing to deny) - matches <see cref="Enumerable.All{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/> semantics.</summary>
    public bool Can(params Permission[] permissions) =>
        permissions.All(Permissions.Contains);

    public void Require(params Permission[] permissions)
    {
        if (!Can(permissions))
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowForbidden();
        }
    }
}

public sealed record ObjectPermissions(int Id, Permission[] Permissions);

/// <summary>
/// The frontend-facing shape of "everything the acting user can do", grouped by object type.
/// Objects with an empty resolved set are omitted (empty means "not visible").
/// </summary>
public sealed class MyPermissions
{
    public required Permission[] Global { get; init; }
    public required ObjectPermissions[] Workspaces { get; init; }
    public required ObjectPermissions[] ChannelTypes { get; init; }
    public required ObjectPermissions[] Channels { get; init; }
}

/// <summary>
/// A per-request snapshot of everything needed to resolve permissions for one acting user,
/// built from at most six whole-table/whole-user reads (five when the user has no roles, since
/// the role-ACL read is skipped entirely - see <see cref="AclSnapshotLoader"/>). Resolved
/// permission sets are cached per chain (list of sec_obj_ids) so repeated checks against the
/// same object are free.
/// </summary>
public sealed class AclSnapshot
{
    private readonly Dictionary<int, HashSet<Permission>> _userAcls;
    private readonly Dictionary<int, HashSet<Permission>> _roleAcls;
    private readonly Dictionary<int, int> _workspaceSecObjIds;
    private readonly Dictionary<int, int> _channelTypeSecObjIds;
    private readonly Dictionary<string, FrozenSet<Permission>> _chainCache = [];
    private readonly object _chainCacheLock = new();

    public IReadOnlyList<Channel> Channels { get; }

    public IReadOnlyList<Workspace> Workspaces { get; }

    public AclSnapshot(
        Dictionary<int, HashSet<Permission>> userAcls,
        Dictionary<int, HashSet<Permission>> roleAcls,
        Dictionary<int, int> workspaceSecObjIds,
        Dictionary<int, int> channelTypeSecObjIds,
        List<Channel> channels,
        List<Workspace> workspaces)
    {
        _userAcls = userAcls;
        _roleAcls = roleAcls;
        _workspaceSecObjIds = workspaceSecObjIds;
        _channelTypeSecObjIds = channelTypeSecObjIds;
        Channels = channels;
        Workspaces = workspaces;
    }

    public int? WorkspaceSecObjId(int? workspaceId) =>
        workspaceId is int id && _workspaceSecObjIds.TryGetValue(id, out var secObjId) ? secObjId : null;

    public int? ChannelTypeSecObjId(int channelTypeId) =>
        _channelTypeSecObjIds.TryGetValue(channelTypeId, out var secObjId) ? secObjId : null;

    public IEnumerable<(int WorkspaceId, int SecObjId)> WorkspaceEntries =>
        _workspaceSecObjIds.Select(entry => (WorkspaceId: entry.Key, SecObjId: entry.Value));

    public IEnumerable<(int ChannelTypeId, int SecObjId)> ChannelTypeEntries =>
        _channelTypeSecObjIds.Select(entry => (ChannelTypeId: entry.Key, SecObjId: entry.Value));

    /// <summary>
    /// D1/D2/D3 resolution: for the given chain (workspace/channel-type/channel sec_obj_ids, top-down,
    /// within one object's hierarchy only), pick user ACLs if any exist anywhere in the chain, otherwise
    /// role ACLs, then union the permissions found at every level of the chain. Pure allow-list, no bypass.
    /// </summary>
    public FrozenSet<Permission> Resolve(IReadOnlyList<int> chain)
    {
        var cacheKey = string.Join(",", chain);

        lock (_chainCacheLock)
        {
            if (_chainCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }
        }

        var useUserAcls = chain.Any(_userAcls.ContainsKey);
        var source = useUserAcls ? _userAcls : _roleAcls;

        var result = new HashSet<Permission>();

        foreach (var id in chain)
        {
            if (source.TryGetValue(id, out var permissions))
            {
                result.UnionWith(permissions);
            }
        }

        var frozen = result.ToFrozenSet();

        lock (_chainCacheLock)
        {
            _chainCache[cacheKey] = frozen;
        }

        return frozen;
    }
}

public interface IAclSnapshotLoader
{
    Task<AclSnapshot> LoadAsync(AppRepos repos, Guid actorId);
}

/// <summary>
/// Builds an <see cref="AclSnapshot"/> with at most six repo reads. This is the seam a future
/// IMemoryCache-backed loader can replace (e.g. via a decorator swapped in at construction time)
/// without any change to <see cref="PermissionResolver"/>.
/// </summary>
/// <remarks>
/// <see cref="LoadAsync"/> takes <c>repos</c> rather than the loader capturing it in its own
/// constructor because <see cref="AclSnapshotLoader"/> is a process-wide singleton
/// (<see cref="Instance"/>) shared by every <see cref="PermissionResolver"/> instance, each of
/// which may be handed a different <see cref="AppRepos"/> (e.g. per-request DI scope vs. a
/// test's <c>InMemoryPgRepo</c>-backed instance) - the repos therefore cannot be fixed at
/// construction time. A future caching decorator that keys only on <c>actorId</c> must still
/// read through to the same <c>repos</c> it was given on a cache miss.
/// </remarks>
public sealed class AclSnapshotLoader : IAclSnapshotLoader
{
    public static readonly AclSnapshotLoader Instance = new();

    public async Task<AclSnapshot> LoadAsync(AppRepos repos, Guid actorId)
    {
        var userRoles = await repos.UserRoles.GetList(userRole => userRole.UserId == actorId);
        var roleIds = userRoles.Select(userRole => userRole.RoleId).ToHashSet();

        var userAclRows = await repos.UserAcls.GetList(acl => acl.UserId == actorId);
        var userAcls = new Dictionary<int, HashSet<Permission>>();

        foreach (var row in userAclRows)
        {
            if (!userAcls.TryGetValue(row.SecObjId, out var userSet))
            {
                userSet = [];
                userAcls[row.SecObjId] = userSet;
            }

            userSet.UnionWith(row.Permissions);
        }

        // A role-less user (e.g. just registered, or every role has been unassigned) must resolve
        // to nothing. Skipping the query entirely also sidesteps a RepoDb 1.13.1 pitfall: an empty
        // collection inside `.Any(...)` parses to a QueryGroup with zero fields, which
        // PostgreSqlStatementBuilder renders as a WHERE-less `SELECT * FROM role_acls` - i.e. every
        // role's ACLs on every object. `.Contains` on an empty collection degrades to `WHERE (1 = 0)`
        // instead, but we still short-circuit here as defence in depth (see B1).
        var roleAcls = new Dictionary<int, HashSet<Permission>>();

        if (roleIds.Count > 0)
        {
            var roleAclRows = await repos.RoleAcls.GetList(acl => roleIds.Contains(acl.RoleId));

            foreach (var row in roleAclRows)
            {
                if (!roleAcls.TryGetValue(row.SecObjId, out var roleSet))
                {
                    roleSet = [];
                    roleAcls[row.SecObjId] = roleSet;
                }

                roleSet.UnionWith(row.Permissions);
            }
        }

        var workspaces = await repos.Workspaces.GetList();
        var workspaceSecObjIds = workspaces.ToDictionary(ws => ws.Id, ws => ws.SecObjId);

        var channelTypes = await repos.ChannelTypes.GetList();
        var channelTypeSecObjIds = channelTypes.ToDictionary(ct => ct.Id, ct => ct.SecObjId);

        var channels = await repos.Channels.GetList();

        return new AclSnapshot(userAcls, roleAcls, workspaceSecObjIds, channelTypeSecObjIds, channels, workspaces);
    }
}

public interface IPermissionResolver
{
    Task<EffectivePermissions> Global();
    Task<EffectivePermissions> ForWorkspace(Workspace ws);
    Task<EffectivePermissions> ForWorkspace(int workspaceId);
    Task<EffectivePermissions> ForChannelType(ChannelType ct);
    Task<EffectivePermissions> ForChannel(Channel channel);
    Task<MyPermissions> ResolveAll();
    Task<List<Channel>> VisibleChannels(int? workspaceId, Permission permission);
    Task<List<Workspace>> VisibleWorkspaces(Permission permission);

    /// <summary>Drops the cached snapshot; call after ACL/role mutations within the same request.</summary>
    void Invalidate();
}

/// <summary>
/// Resolves what the acting user can do, per Chatney's RBAC/ACL model (see
/// .claude/skills/chatney-rbac-acl/SKILL.md). Deliberately has no dependency on HttpContext or
/// ClaimsPrincipal so it is trivially constructible in tests from InMemoryPgRepo-backed AppRepos;
/// the ambient-user lookup lives only in the DI registration (see Program.cs).
/// </summary>
public sealed class PermissionResolver : IPermissionResolver
{
    private readonly AppRepos _repos;
    private readonly Guid? _actorId;
    private readonly IAclSnapshotLoader _snapshotLoader;
    private readonly object _snapshotLock = new();
    private Task<AclSnapshot>? _snapshotTask;

    public PermissionResolver(AppRepos repos, Guid? actorId)
        : this(repos, actorId, AclSnapshotLoader.Instance)
    {
    }

    public PermissionResolver(AppRepos repos, Guid? actorId, IAclSnapshotLoader snapshotLoader)
    {
        _repos = repos;
        _actorId = actorId;
        _snapshotLoader = snapshotLoader;
    }

    public static PermissionResolver For(AppRepos repos, User user) => new(repos, user.Id);

    public static PermissionResolver Anonymous(AppRepos repos) => new(repos, null);

    /// <summary>Drops the cached snapshot; call after ACL/role mutations within the same request.</summary>
    public void Invalidate()
    {
        lock (_snapshotLock)
        {
            _snapshotTask = null;
        }
    }

    public Task<EffectivePermissions> Global() =>
        Resolve(BuildGlobalChain());

    public Task<EffectivePermissions> ForWorkspace(Workspace ws) =>
        Resolve(BuildWorkspaceChain(ws.SecObjId));

    public async Task<EffectivePermissions> ForWorkspace(int workspaceId)
    {
        if (_actorId is null)
        {
            return EffectivePermissions.Empty;
        }

        var snapshot = await GetSnapshot();
        var secObjId = snapshot.WorkspaceSecObjId(workspaceId);

        return secObjId is null
            ? EffectivePermissions.Empty
            : new EffectivePermissions(snapshot.Resolve(BuildWorkspaceChain(secObjId.Value)));
    }

    public Task<EffectivePermissions> ForChannelType(ChannelType ct) =>
        Resolve(BuildChannelTypeChain(ct.SecObjId));

    public async Task<EffectivePermissions> ForChannel(Channel channel)
    {
        if (_actorId is null)
        {
            return EffectivePermissions.Empty;
        }

        var snapshot = await GetSnapshot();
        return new EffectivePermissions(snapshot.Resolve(BuildChannelChain(snapshot, channel)));
    }

    public async Task<MyPermissions> ResolveAll()
    {
        if (_actorId is null)
        {
            return new MyPermissions { Global = [], Workspaces = [], ChannelTypes = [], Channels = [] };
        }

        var snapshot = await GetSnapshot();

        var global = snapshot.Resolve(BuildGlobalChain()).Order().ToArray();

        var workspaces = snapshot.WorkspaceEntries
            .Select(entry => new ObjectPermissions(entry.WorkspaceId, [.. snapshot.Resolve(BuildWorkspaceChain(entry.SecObjId)).Order()]))
            .Where(objectPermissions => objectPermissions.Permissions.Length > 0)
            .ToArray();

        var channelTypes = snapshot.ChannelTypeEntries
            .Select(entry => new ObjectPermissions(entry.ChannelTypeId, [.. snapshot.Resolve(BuildChannelTypeChain(entry.SecObjId)).Order()]))
            .Where(objectPermissions => objectPermissions.Permissions.Length > 0)
            .ToArray();

        var channels = snapshot.Channels
            .Select(channel => new ObjectPermissions(channel.Id, [.. snapshot.Resolve(BuildChannelChain(snapshot, channel)).Order()]))
            .Where(objectPermissions => objectPermissions.Permissions.Length > 0)
            .ToArray();

        return new MyPermissions
        {
            Global = global,
            Workspaces = workspaces,
            ChannelTypes = channelTypes,
            Channels = channels,
        };
    }

    public async Task<List<Channel>> VisibleChannels(int? workspaceId, Permission permission)
    {
        if (_actorId is null)
        {
            return [];
        }

        var snapshot = await GetSnapshot();

        return snapshot.Channels
            .Where(channel => workspaceId is null || (!channel.IsDm && channel.WorkspaceId == workspaceId))
            .Where(channel => snapshot.Resolve(BuildChannelChain(snapshot, channel)).Contains(permission))
            .ToList();
    }

    public async Task<List<Workspace>> VisibleWorkspaces(Permission permission)
    {
        if (_actorId is null)
        {
            return [];
        }

        var snapshot = await GetSnapshot();

        return snapshot.Workspaces
            .Where(workspace => snapshot.Resolve(BuildWorkspaceChain(workspace.SecObjId)).Contains(permission))
            .ToList();
    }

    private async Task<EffectivePermissions> Resolve(IReadOnlyList<int> chain)
    {
        if (_actorId is null)
        {
            return EffectivePermissions.Empty;
        }

        var snapshot = await GetSnapshot();
        return new EffectivePermissions(snapshot.Resolve(chain));
    }

    private Task<AclSnapshot> GetSnapshot()
    {
        lock (_snapshotLock)
        {
            _snapshotTask ??= _snapshotLoader.LoadAsync(_repos, _actorId!.Value);
            return _snapshotTask;
        }
    }

    /// <summary>D2: the global sec_obj (1) is its own namespace, never part of an object chain.</summary>
    private static List<int> BuildGlobalChain() => [SecureObjectIds.Global];

    private static List<int> BuildWorkspaceChain(int workspaceSecObjId) => [workspaceSecObjId];

    private static List<int> BuildChannelTypeChain(int channelTypeSecObjId) => [channelTypeSecObjId];

    private static List<int> BuildChannelChain(AclSnapshot snapshot, Channel channel)
    {
        var chain = new List<int>(3);

        // A workspace/channel-type row missing from the snapshot means the object was deleted (or
        // never existed) between snapshot load and use; skipping it is fail-closed - it simply
        // contributes no grants to the chain rather than throwing or granting anything extra.
        var workspaceSecObjId = snapshot.WorkspaceSecObjId(channel.WorkspaceId);

        if (workspaceSecObjId is not null)
        {
            chain.Add(workspaceSecObjId.Value);
        }

        var channelTypeSecObjId = snapshot.ChannelTypeSecObjId(channel.ChannelTypeId);

        if (channelTypeSecObjId is not null)
        {
            chain.Add(channelTypeSecObjId.Value);
        }

        chain.Add(channel.SecObjId);
        return chain;
    }
}
