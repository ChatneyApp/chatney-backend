using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202607310003)]
public class _202607310003_CreateWorkspacesAndChannelsTables : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TABLE workspaces (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                sec_obj_id int NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT fk_workspaces_sec_obj_id
                    FOREIGN KEY (sec_obj_id) REFERENCES secure_objects(id) ON DELETE CASCADE
            );

            CREATE TABLE channel_types (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                key varchar(255) NOT NULL,
                sec_obj_id int NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT fk_channel_types_sec_obj_id
                    FOREIGN KEY (sec_obj_id) REFERENCES secure_objects(id) ON DELETE CASCADE
            );

            CREATE TABLE channels (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                channel_type_id int NOT NULL,
                workspace_id int NOT NULL,
                sec_obj_id int NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT fk_channels_channel_type_id
                    FOREIGN KEY (channel_type_id) REFERENCES channel_types(id) ON DELETE CASCADE,
                CONSTRAINT fk_channels_workspace_id
                    FOREIGN KEY (workspace_id) REFERENCES workspaces(id) ON DELETE CASCADE,
                CONSTRAINT fk_channels_sec_obj_id
                    FOREIGN KEY (sec_obj_id) REFERENCES secure_objects(id) ON DELETE CASCADE
            );

            CREATE TABLE channel_groups (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                workspace_id int NOT NULL,
                channel_ids int[] NOT NULL DEFAULT '{}',
                "order" int NOT NULL DEFAULT 0,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT fk_channel_groups_workspace_id
                    FOREIGN KEY (workspace_id) REFERENCES workspaces(id) ON DELETE CASCADE
            );

            CREATE TABLE user_channel_settings (
                user_id uuid NOT NULL,
                channel_id int NOT NULL,
                last_seen_message timestamptz NOT NULL,
                CONSTRAINT user_channel_settings_pkey PRIMARY KEY (user_id, channel_id),
                CONSTRAINT fk_user_channel_settings_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TABLE IF EXISTS user_channel_settings;
            DROP TABLE IF EXISTS channel_groups;
            DROP TABLE IF EXISTS channels;
            DROP TABLE IF EXISTS channel_types;
            DROP TABLE IF EXISTS workspaces;
        """;

        Execute.Sql(sql);
    }
}
