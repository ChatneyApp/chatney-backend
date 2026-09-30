using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202609290001)]
public class _202609290001_MakeChannelTypeIdNullable : Migration
{
    public override void Up()
    {
        // DM channels no longer have a channel type. Detach them before removing the legacy "dm"
        // channel type, otherwise the channel_type_id FK cascade would delete them too.
        var sql = """
            ALTER TABLE channels
                ALTER COLUMN channel_type_id DROP NOT NULL;

            UPDATE channels SET channel_type_id = NULL WHERE is_dm;

            DELETE FROM secure_objects
            WHERE id IN (SELECT sec_obj_id FROM channel_types WHERE key = 'dm');
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            ALTER TABLE channels ALTER COLUMN channel_type_id SET NOT NULL;
        """;

        Execute.Sql(sql);
    }
}
