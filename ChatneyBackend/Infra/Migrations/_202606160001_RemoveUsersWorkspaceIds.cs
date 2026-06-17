using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202606160001)]
public class _202606160001_RemoveUsersWorkspaceIds : Migration
{
    public override void Up()
    {
        Execute.Sql("ALTER TABLE users DROP COLUMN IF EXISTS workspace_ids;");
    }

    public override void Down()
    {
        Execute.Sql("ALTER TABLE users ADD COLUMN workspace_ids int[] NOT NULL DEFAULT '{}';");
    }
}
