using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202607050001)]
public class _202607050001_RenameRolesIsBaseToIsProtected : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            ALTER TABLE roles RENAME COLUMN is_base TO is_protected;
        """);
    }

    public override void Down()
    {
        Execute.Sql("""
            ALTER TABLE roles RENAME COLUMN is_protected TO is_base;
        """);
    }
}
