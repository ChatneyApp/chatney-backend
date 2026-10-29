using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202610200002)]
public class _202610200002_GrantRoleReadRole : Migration
{
    // Every role that can already edit roles globally (admin included) gets read access too.
    public override void Up()
    {
        var sql = """
            UPDATE role_acls
            SET permissions = array_append(permissions, 'role.readRole')
            WHERE sec_obj_id = 1
                AND 'role.editRole' = ANY(permissions)
                AND NOT 'role.readRole' = ANY(permissions);
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        Execute.Sql("UPDATE role_acls SET permissions = array_remove(permissions, 'role.readRole');");
    }
}
