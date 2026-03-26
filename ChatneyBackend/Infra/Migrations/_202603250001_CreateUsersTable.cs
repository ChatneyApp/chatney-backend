using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202603250001)]
public class _202603250001_CreateUsersTable : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TABLE users (
                id uuid PRIMARY KEY,
                name varchar(255) NOT NULL,
                active boolean NOT NULL DEFAULT false,
                verified boolean NOT NULL DEFAULT false,
                banned boolean NOT NULL DEFAULT false,
                muted boolean NOT NULL DEFAULT false,
                email varchar(255) NOT NULL,
                avatar_url text NULL,
                role_id int NOT NULL,
                workspaces text[] NOT NULL DEFAULT '{}',
                password text NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW()
            );

            ALTER TABLE users
            ADD CONSTRAINT fk_users_global_role_id
                FOREIGN KEY (role_id) REFERENCES roles(id);

            CREATE UNIQUE INDEX IF NOT EXISTS ix_users_email ON users (email);
            CREATE UNIQUE INDEX IF NOT EXISTS ix_users_name ON users (name);
        
            CREATE TRIGGER trg_users_set_updated_at
            BEFORE UPDATE ON users
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at();

            CREATE TYPE user_role_type AS ENUM ('workspace', 'channel', 'channel_type');

            CREATE TABLE user_roles (
                user_id uuid NOT NULL,
                type user_role_type NOT NULL,
                item_id text NOT NULL,
                role_id int NOT NULL,
                PRIMARY KEY (user_id, type, item_id),
                CONSTRAINT fk_user_roles_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                CONSTRAINT fk_user_roles_role_id
                    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
            );

            CREATE TABLE user_channel_settings (
                user_id uuid NOT NULL,
                channel_id uuid NOT NULL,
                last_seen_message timestamptz NOT NULL,
                muted boolean NOT NULL DEFAULT false,
                PRIMARY KEY (user_id, channel_id),
                CONSTRAINT fk_user_channel_settings_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );
        """;
        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TRIGGER IF EXISTS trg_users_set_updated_at ON users;
            DROP INDEX IF EXISTS ix_users_email;
            DROP INDEX IF EXISTS ix_users_name;
            ALTER TABLE IF EXISTS users DROP CONSTRAINT IF EXISTS fk_users_global_role_id;
            DROP TABLE IF EXISTS user_channel_settings;
            DROP TABLE IF EXISTS user_roles;
            DROP TABLE IF EXISTS users;
            DROP TYPE IF EXISTS user_role_type;
        """;

        Execute.Sql(sql);
    }
}
