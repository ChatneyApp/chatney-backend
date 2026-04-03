using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202603270001)]
public class _202603270001_CreateChannelsTables : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TABLE channel_types (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                key varchar(255) NOT NULL,
                base_role_id int NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW()
            );

            CREATE TRIGGER trg_channel_types_set_updated_at
            BEFORE UPDATE ON channel_types
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();
        
            CREATE TABLE channels (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                channel_type_id int NOT NULL,
                workspace_id int NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW()
            );

            CREATE TRIGGER trg_channels_set_updated_at
            BEFORE UPDATE ON channels
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();

            CREATE TABLE channel_groups (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                workspace_id int NOT NULL,
                channel_ids int[] NOT NULL DEFAULT '{}',
                "order" int NOT NULL DEFAULT 0,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW()
            );

            CREATE TRIGGER trg_channel_groups_set_updated_at
            BEFORE UPDATE ON channel_groups
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TRIGGER IF EXISTS trg_channel_groups_set_updated_at ON channel_groups;
            DROP TABLE IF EXISTS channel_groups;
            DROP TRIGGER IF EXISTS trg_channels_set_updated_at ON channels;
            DROP TABLE IF EXISTS channels;
            DROP TRIGGER IF EXISTS trg_channel_types_set_updated_at ON channel_types;
            DROP TABLE IF EXISTS channel_types;
        """;

        Execute.Sql(sql);
    }
}
