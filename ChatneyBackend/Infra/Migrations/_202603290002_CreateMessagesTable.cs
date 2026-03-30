using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202603290002)]
public class _202603290002_CreateMessagesTable : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TABLE messages (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                channel_id int NOT NULL,
                user_id uuid NOT NULL,
                content varchar(4096) NOT NULL,
                attachment_ids int[] NOT NULL DEFAULT '{}',
                url_preview_ids int[] NOT NULL DEFAULT '{}',
                status varchar(50) NOT NULL,
                parent_id int NULL,
                children_count int NOT NULL DEFAULT 0,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT fk_messages_channel_id
                    FOREIGN KEY (channel_id) REFERENCES channels(id) ON DELETE CASCADE,
                CONSTRAINT fk_messages_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                CONSTRAINT fk_messages_parent_id
                    FOREIGN KEY (parent_id) REFERENCES messages(id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ix_messages_channel_id ON messages (channel_id);
            CREATE INDEX IF NOT EXISTS ix_messages_parent_id ON messages (parent_id);
            CREATE INDEX IF NOT EXISTS ix_messages_user_id ON messages (user_id);

            CREATE TRIGGER trg_messages_set_updated_at
            BEFORE UPDATE ON messages
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TRIGGER IF EXISTS trg_messages_set_updated_at ON messages;
            DROP INDEX IF EXISTS ix_messages_channel_id;
            DROP INDEX IF EXISTS ix_messages_parent_id;
            DROP INDEX IF EXISTS ix_messages_user_id;
            DROP TABLE IF EXISTS messages;
        """;

        Execute.Sql(sql);
    }
}
