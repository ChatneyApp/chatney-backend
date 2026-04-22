using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202604210001)]
public class _202604210001_AddReplyToMessages : Migration
{
    public override void Up()
    {
        var sql = """
            ALTER TABLE messages
            ADD COLUMN reply_to int NULL,
            ADD CONSTRAINT fk_messages_reply_to
                FOREIGN KEY (reply_to) REFERENCES messages(id) ON DELETE SET NULL;

            CREATE INDEX IF NOT EXISTS ix_messages_reply_to ON messages (reply_to);
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP INDEX IF EXISTS ix_messages_reply_to;
            ALTER TABLE messages
            DROP CONSTRAINT IF EXISTS fk_messages_reply_to,
            DROP COLUMN IF EXISTS reply_to;
        """;

        Execute.Sql(sql);
    }
}
