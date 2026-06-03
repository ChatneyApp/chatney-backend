using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202606020001)]
public class _202606020001_AlterUserRolesColumns : Migration
{
    public override void Up()
    {
        var sql = """
            ALTER TABLE user_roles
            DROP CONSTRAINT IF EXISTS fk_user_roles_role_id,
            DROP COLUMN IF EXISTS type,
            DROP COLUMN IF EXISTS item_id,
            ADD COLUMN channel_id int NULL,
            ADD COLUMN channel_type_id int NULL,
            ADD COLUMN workspace_id int NULL,
            ADD CONSTRAINT fk_user_roles_channel_id
                FOREIGN KEY (channel_id) REFERENCES channels(id) ON DELETE CASCADE,
            ADD CONSTRAINT fk_user_roles_channel_type_id
                FOREIGN KEY (channel_type_id) REFERENCES channel_types(id) ON DELETE CASCADE,
            ADD CONSTRAINT fk_user_roles_workspace_id
                FOREIGN KEY (workspace_id) REFERENCES workspaces(id) ON DELETE CASCADE,
            ADD CONSTRAINT fk_user_roles_role_id
                FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
            ADD COLUMN allowlist text[] NOT NULL DEFAULT '{}',
            ADD COLUMN denylist text[] NOT NULL DEFAULT '{}';
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            ALTER TABLE user_roles
            DROP CONSTRAINT IF EXISTS fk_user_roles_channel_id,
            DROP CONSTRAINT IF EXISTS fk_user_roles_channel_type_id,
            DROP CONSTRAINT IF EXISTS fk_user_roles_workspace_id,
            DROP CONSTRAINT IF EXISTS fk_user_roles_role_id,
            DROP COLUMN IF EXISTS channel_id,
            DROP COLUMN IF EXISTS channel_type_id,
            DROP COLUMN IF EXISTS workspace_id,
            DROP COLUMN IF EXISTS allowlist,
            DROP COLUMN IF EXISTS denylist,
            ADD COLUMN type user_role_type NOT NULL DEFAULT 'workspace',
            ADD COLUMN item_id int NOT NULL DEFAULT 0,
            ADD CONSTRAINT fk_user_roles_role_id
                FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE;
        """;

        Execute.Sql(sql);
    }
}
