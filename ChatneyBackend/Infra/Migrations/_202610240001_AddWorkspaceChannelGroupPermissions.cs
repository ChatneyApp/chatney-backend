using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202610240001)]
public class _202610240001_AddWorkspaceChannelGroupPermissions : Migration
{
    // Renaming keeps existing grants in role_acls/user_acls. The new read value can't be used in the
    // transaction that adds it, so granting it lives in the next migration.
    public override void Up()
    {
        var sql = """
            ALTER TYPE permission RENAME VALUE 'channel.addChannelGroup' TO 'workspace.createChannelGroup';
            ALTER TYPE permission RENAME VALUE 'channel.editChannelGroup' TO 'workspace.updateChannelGroup';
            ALTER TYPE permission RENAME VALUE 'channel.deleteChannelGroup' TO 'workspace.deleteChannelGroup';
            ALTER TYPE permission ADD VALUE IF NOT EXISTS 'workspace.readChannelGroup';
        """;

        Execute.Sql(sql);
    }

    // Postgres can't drop an enum value; the read grants are removed by the next migration's Down.
    public override void Down()
    {
        var sql = """
            ALTER TYPE permission RENAME VALUE 'workspace.createChannelGroup' TO 'channel.addChannelGroup';
            ALTER TYPE permission RENAME VALUE 'workspace.updateChannelGroup' TO 'channel.editChannelGroup';
            ALTER TYPE permission RENAME VALUE 'workspace.deleteChannelGroup' TO 'channel.deleteChannelGroup';
        """;

        Execute.Sql(sql);
    }
}
