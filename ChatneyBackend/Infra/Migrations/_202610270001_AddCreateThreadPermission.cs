using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202610270001)]
public class _202610270001_AddCreateThreadPermission : Migration
{
    // No default grants: admins assign the thread permission manually.
    public override void Up()
    {
        var sql = """
            ALTER TYPE permission ADD VALUE IF NOT EXISTS 'channel.createThread';
        """;

        Execute.Sql(sql);
    }

    // Postgres can't drop an enum value, so only the grants are removed.
    public override void Down()
    {
        var sql = """
            UPDATE role_acls SET permissions = array_remove(permissions, 'channel.createThread');
            UPDATE user_acls SET permissions = array_remove(permissions, 'channel.createThread');
        """;

        Execute.Sql(sql);
    }
}
