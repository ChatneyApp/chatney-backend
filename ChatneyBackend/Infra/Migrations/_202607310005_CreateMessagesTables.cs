using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202607310005)]
public class _202607310005_CreateMessagesTables : Migration
{
    public override void Up()
    {
        var sql = """
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
                reply_to int NULL,
                CONSTRAINT fk_messages_channel_id
                    FOREIGN KEY (channel_id) REFERENCES channels(id) ON DELETE CASCADE,
                CONSTRAINT fk_messages_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                CONSTRAINT fk_messages_parent_id
                    FOREIGN KEY (parent_id) REFERENCES messages(id) ON DELETE CASCADE,
                CONSTRAINT fk_messages_reply_to
                    FOREIGN KEY (reply_to) REFERENCES messages(id) ON DELETE SET NULL
            );

            CREATE INDEX IF NOT EXISTS ix_messages_channel_id ON messages (channel_id);
            CREATE INDEX IF NOT EXISTS ix_messages_parent_id ON messages (parent_id);
            CREATE INDEX IF NOT EXISTS ix_messages_reply_to ON messages (reply_to);
            CREATE INDEX IF NOT EXISTS ix_messages_user_id ON messages (user_id);

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
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP INDEX IF EXISTS ux_draft_messages_user_channel_parent;
            DROP TABLE IF EXISTS draft_messages;

            DROP INDEX IF EXISTS ix_message_reactions_message_id;
            DROP INDEX IF EXISTS ix_message_reactions_user_id;
            DROP TABLE IF EXISTS message_reactions;

            DROP INDEX IF EXISTS ix_messages_channel_id;
            DROP INDEX IF EXISTS ix_messages_parent_id;
            DROP INDEX IF EXISTS ix_messages_reply_to;
            DROP INDEX IF EXISTS ix_messages_user_id;
            DROP TABLE IF EXISTS messages;

            DROP INDEX IF EXISTS ix_url_previews_url;
            DROP TABLE IF EXISTS url_previews;
        """;

        Execute.Sql(sql);
    }
}
