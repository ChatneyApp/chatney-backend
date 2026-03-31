using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202603300001)]
public class _202603300001_CreateDraftMessagesTable : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TABLE draft_messages (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                channel_id int NOT NULL,
                user_id uuid NOT NULL,
                content varchar(4096) NOT NULL,
                attachment_ids int[] NOT NULL DEFAULT '{}',
                parent_id int NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT fk_draft_messages_channel_id
                    FOREIGN KEY (channel_id) REFERENCES channels(id) ON DELETE CASCADE,
                CONSTRAINT fk_draft_messages_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                CONSTRAINT fk_draft_messages_parent_id
                    FOREIGN KEY (parent_id) REFERENCES draft_messages(id) ON DELETE CASCADE
            );

            CREATE UNIQUE INDEX IF NOT EXISTS ux_draft_messages_user_channel_parent
                ON draft_messages (user_id, channel_id, parent_id);

            CREATE TRIGGER trg_draft_messages_set_updated_at
            BEFORE UPDATE ON draft_messages
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TRIGGER IF EXISTS trg_draft_messages_set_updated_at ON draft_messages;
            DROP INDEX IF EXISTS ux_draft_messages_user_channel_parent;
            DROP TABLE IF EXISTS draft_messages;
        """;

        Execute.Sql(sql);
    }
}
