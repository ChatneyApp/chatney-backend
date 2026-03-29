using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202603290001)]
public class _202603290001_CreateConfigsTable : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TABLE configs (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                value varchar(2048) NOT NULL,
                type varchar(255) NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS ix_configs_name ON configs (name);

            CREATE TRIGGER trg_configs_set_updated_at
            BEFORE UPDATE ON configs
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TRIGGER IF EXISTS trg_configs_set_updated_at ON configs;
            DROP INDEX IF EXISTS ix_configs_name;
            DROP TABLE IF EXISTS configs;
        """;

        Execute.Sql(sql);
    }
}
