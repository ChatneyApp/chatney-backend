using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202607310002)]
public class _202607310002_CreateUsersTables : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TABLE users (
                id uuid PRIMARY KEY DEFAULT uuidv7(),
                nickname varchar(20) NOT NULL,
                full_name varchar(255) NULL,
                active boolean NOT NULL DEFAULT false,
                verified boolean NOT NULL DEFAULT false,
                banned boolean NOT NULL DEFAULT false,
                muted boolean NOT NULL DEFAULT false,
                email varchar(255) NOT NULL UNIQUE,
                avatar_url text NULL,
                role_id int NOT NULL,
                password text NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT fk_users_global_role_id
                    FOREIGN KEY (role_id) REFERENCES roles(id)
            );

            CREATE TABLE user_roles (
                user_id uuid NOT NULL,
                role_id int NOT NULL,
                CONSTRAINT user_roles_pkey PRIMARY KEY (user_id, role_id),
                CONSTRAINT fk_user_roles_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                CONSTRAINT fk_user_roles_role_id
                    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
            );

            CREATE TABLE user_acls (
                user_id uuid NOT NULL,
                sec_obj_id int NOT NULL,
                permissions permission[] NOT NULL DEFAULT '{}',
                CONSTRAINT user_acls_pkey PRIMARY KEY (user_id, sec_obj_id),
                CONSTRAINT fk_user_acls_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                CONSTRAINT fk_user_acls_sec_obj_id
                    FOREIGN KEY (sec_obj_id) REFERENCES secure_objects(id) ON DELETE CASCADE
            );
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TABLE IF EXISTS user_acls;
            DROP TABLE IF EXISTS user_roles;
            ALTER TABLE IF EXISTS users DROP CONSTRAINT IF EXISTS fk_users_global_role_id;
            DROP TABLE IF EXISTS users;
        """;

        Execute.Sql(sql);
    }
}
