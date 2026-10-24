using ChatneyBackend.Domains.Workspaces;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Channels;

public static class ChannelGroupMutationQueries
{
    // @Orders is a JSON object: { "<channel group id>": <order>, ... }
    public const string GetWorkspacesOfGroupsSql =
        """
        SELECT * FROM workspaces
        WHERE id IN (
            SELECT workspace_id FROM channel_groups
            WHERE id IN (SELECT key::int FROM jsonb_object_keys(CAST(@Orders AS jsonb)) AS key)
        );
        """;

    public const string UpdateOrdersSql =
        """
        UPDATE channel_groups AS g
        SET "order" = v.value::int,
            updated_at = NOW()
        FROM jsonb_each_text(CAST(@Orders AS jsonb)) AS v
        WHERE g.id = v.key::int
        RETURNING g.*;
        """;

    public static Task<List<Workspace>> GetWorkspacesOfGroupsAsync(
        IPgRepo<Workspace, int> workspaces,
        string ordersJson) =>
        workspaces.GetList(GetWorkspacesOfGroupsSql, new { Orders = ordersJson });

    public static Task<List<ChannelGroup>> UpdateOrdersAsync(
        IPgRepo<ChannelGroup, int> channelGroups,
        string ordersJson) =>
        channelGroups.GetList(UpdateOrdersSql, new { Orders = ordersJson });
}
