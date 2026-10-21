using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202610200001)]
public class _202610200001_AddRoleReadRolePermission : Migration
{
    // The new value can't be used in the transaction that adds it, so granting it lives in the
    // next migration.
    public override void Up()
    {
        Execute.Sql("ALTER TYPE permission ADD VALUE IF NOT EXISTS 'role.readRole' AFTER 'role.editRole';");
    }

    // Postgres can't drop an enum value; the grants are removed by the next migration's Down.
    public override void Down()
    {
    }
}
