using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202610240002)]
public class _202610240002_GrantWorkspaceReadChannelGroup : Migration
{
    // Channel groups used to be readable with channel.readChannel on the workspace, so every global or
    // workspace grant that has it gets the new read permission too.
    public override void Up()
    {
        var sql = """
            UPDATE role_acls
            SET permissions = array_append(permissions, 'workspace.readChannelGroup')
            WHERE (sec_obj_id = 1 OR sec_obj_id IN (SELECT sec_obj_id FROM workspaces))
                AND 'channel.readChannel' = ANY(permissions)
                AND NOT 'workspace.readChannelGroup' = ANY(permissions);

            UPDATE user_acls
            SET permissions = array_append(permissions, 'workspace.readChannelGroup')
            WHERE (sec_obj_id = 1 OR sec_obj_id IN (SELECT sec_obj_id FROM workspaces))
                AND 'channel.readChannel' = ANY(permissions)
                AND NOT 'workspace.readChannelGroup' = ANY(permissions);
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            UPDATE role_acls SET permissions = array_remove(permissions, 'workspace.readChannelGroup');
            UPDATE user_acls SET permissions = array_remove(permissions, 'workspace.readChannelGroup');
        """;

        Execute.Sql(sql);
    }
}
