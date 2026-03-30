using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202603290003)]
public class _202603290003_CreateMessageReactionsTable : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TABLE message_reactions (
                message_id int NOT NULL,
                user_id uuid NOT NULL,
                code varchar(255) NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT pk_message_reactions PRIMARY KEY (message_id, user_id, code),
                CONSTRAINT fk_message_reactions_message_id
                    FOREIGN KEY (message_id) REFERENCES messages(id) ON DELETE CASCADE,
                CONSTRAINT fk_message_reactions_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ix_message_reactions_message_id
                ON message_reactions (message_id);
            CREATE INDEX IF NOT EXISTS ix_message_reactions_user_id
                ON message_reactions (user_id);

            CREATE TRIGGER trg_message_reactions_set_updated_at
            BEFORE UPDATE ON message_reactions
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TRIGGER IF EXISTS trg_message_reactions_set_updated_at ON message_reactions;
            DROP INDEX IF EXISTS ix_message_reactions_message_id;
            DROP INDEX IF EXISTS ix_message_reactions_user_id;
            DROP TABLE IF EXISTS message_reactions;
        """;

        Execute.Sql(sql);
    }
}
