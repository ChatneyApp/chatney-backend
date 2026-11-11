using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202611100001)]
public class _202611100001_AddUserBio : Migration
{
    public override void Up()
    {
        var sql = """
            ALTER TABLE users ADD COLUMN bio text NOT NULL DEFAULT '';
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            ALTER TABLE users DROP COLUMN bio;
        """;

        Execute.Sql(sql);
    }
}
