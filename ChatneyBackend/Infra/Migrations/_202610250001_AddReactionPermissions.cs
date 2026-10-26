using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202610250001)]
public class _202610250001_AddReactionPermissions : Migration
{
    // No default grants: admins assign reaction permissions manually.
    public override void Up()
    {
        var sql = """
            ALTER TYPE permission ADD VALUE IF NOT EXISTS 'channel.addReaction';
            ALTER TYPE permission ADD VALUE IF NOT EXISTS 'channel.removeReaction';
        """;

        Execute.Sql(sql);
    }

    // Postgres can't drop an enum value, so only the grants are removed.
    public override void Down()
    {
        var sql = """
            UPDATE role_acls SET permissions = array_remove(array_remove(permissions, 'channel.addReaction'), 'channel.removeReaction');
            UPDATE user_acls SET permissions = array_remove(array_remove(permissions, 'channel.addReaction'), 'channel.removeReaction');
        """;

        Execute.Sql(sql);
    }
}
