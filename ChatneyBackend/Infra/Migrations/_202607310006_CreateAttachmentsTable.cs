using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202607310006)]
public class _202607310006_CreateAttachmentsTable : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TABLE attachments (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                user_id uuid NOT NULL,
                url_path varchar(4096) NOT NULL,
                original_file_name varchar(4096) NOT NULL,
                extension varchar(4096) NOT NULL,
                mime_type varchar(4096) NOT NULL,
                size bigint NOT NULL,
                width int NULL,
                height int NULL,
                duration int NULL,
                type varchar(4096) NOT NULL,
                as_file boolean NOT NULL DEFAULT false,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT fk_attachments_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ix_attachments_user_id ON attachments (user_id);
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP INDEX IF EXISTS ix_attachments_user_id;
            DROP TABLE IF EXISTS attachments;
        """;

        Execute.Sql(sql);
    }
}
