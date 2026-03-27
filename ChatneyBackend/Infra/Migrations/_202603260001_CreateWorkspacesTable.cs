using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202603260001)]
public class _202603260001_CreateWorkspacesTable : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TABLE workspaces (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW()
            );

            CREATE TRIGGER trg_workspaces_set_updated_at
            BEFORE UPDATE ON workspaces
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TRIGGER IF EXISTS trg_workspaces_set_updated_at ON workspaces;
            DROP TABLE IF EXISTS workspaces;
        """;

        Execute.Sql(sql);
    }
}
