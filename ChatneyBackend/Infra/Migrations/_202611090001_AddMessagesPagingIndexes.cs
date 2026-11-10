using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202611090001)]
public class _202611090001_AddMessagesPagingIndexes : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE INDEX IF NOT EXISTS ix_messages_channel_top_level
                ON messages (channel_id, id DESC) WHERE parent_id IS NULL;
            CREATE INDEX IF NOT EXISTS ix_messages_parent_id_id ON messages (parent_id, id DESC);
            DROP INDEX IF EXISTS ix_messages_parent_id;
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            CREATE INDEX IF NOT EXISTS ix_messages_parent_id ON messages (parent_id);
            DROP INDEX IF EXISTS ix_messages_parent_id_id;
            DROP INDEX IF EXISTS ix_messages_channel_top_level;
        """;

        Execute.Sql(sql);
    }
}
