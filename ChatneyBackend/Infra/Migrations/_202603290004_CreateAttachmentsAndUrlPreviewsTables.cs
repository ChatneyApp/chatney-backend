using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202603290004)]
public class _202603290004_CreateAttachmentsAndUrlPreviewsTables : Migration
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
                type varchar(4096) NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT fk_attachments_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS ix_attachments_user_id ON attachments (user_id);

            CREATE TRIGGER trg_attachments_set_updated_at
            BEFORE UPDATE ON attachments
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();

            CREATE TABLE url_previews (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                url varchar(4096) NOT NULL,
                title varchar(4096) NULL,
                description text NULL,
                thumbnail_url varchar(4096) NULL,
                video_thumbnail_url varchar(4096) NULL,
                site_name varchar(4096) NULL,
                fav_icon_url varchar(4096) NULL,
                type varchar(255) NULL,
                author varchar(4096) NULL,
                thumbnail_width int NULL,
                thumbnail_height int NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS ix_url_previews_url ON url_previews (url);

            CREATE TRIGGER trg_url_previews_set_updated_at
            BEFORE UPDATE ON url_previews
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TRIGGER IF EXISTS trg_url_previews_set_updated_at ON url_previews;
            DROP INDEX IF EXISTS ix_url_previews_url;
            DROP TABLE IF EXISTS url_previews;

            DROP TRIGGER IF EXISTS trg_attachments_set_updated_at ON attachments;
            DROP INDEX IF EXISTS ix_attachments_user_id;
            DROP TABLE IF EXISTS attachments;
        """;

        Execute.Sql(sql);
    }
}
