using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202607310004)]
public class _202607310004_CreateConfigsTable : Migration
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
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP INDEX IF EXISTS ix_configs_name;
            DROP TABLE IF EXISTS configs;
        """;

        Execute.Sql(sql);
    }
}
