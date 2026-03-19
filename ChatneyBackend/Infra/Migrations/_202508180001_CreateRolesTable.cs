using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202508180001)]
public class _202508180001_CreateRolesTable : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            CREATE TABLE IF NOT EXISTS roles (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                is_base bool NOT NULL DEFAULT false,
                permissions text[] NOT NULL DEFAULT '{}',
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW()
            );
        """);
    }

    public override void Down()
    {
        Delete.Table("roles");
    }
}
