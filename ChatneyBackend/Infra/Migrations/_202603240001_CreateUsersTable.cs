using FluentMigrator;
using Npgsql;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202603240001)]
public class _202603240001_CreateUsersTable : Migration
{
    public override void Up()
    {
        Execute.WithConnection((connection, _) =>
        {
            using var command = new NpgsqlCommand
            {
                Connection = (NpgsqlConnection)connection
            };
            command.CommandText = """
                CREATE EXTENSION IF NOT EXISTS "pgcrypto";

                CREATE TABLE IF NOT EXISTS users (
                    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                    name varchar(255) NOT NULL,
                    email varchar(255) NOT NULL,
                    password varchar(255) NOT NULL,
                    active bool NOT NULL DEFAULT false,
                    verified bool NOT NULL DEFAULT false,
                    banned bool NOT NULL DEFAULT false,
                    muted bool NOT NULL DEFAULT false,
                    avatar_url varchar(255),
                    global_role_id int NOT NULL DEFAULT 0,
                    workspace_roles jsonb NOT NULL DEFAULT '{}',
                    channel_roles jsonb NOT NULL DEFAULT '{}',
                    channel_type_roles jsonb NOT NULL DEFAULT '{}',
                    channels_settings jsonb NOT NULL DEFAULT '{}',
                    workspaces text[] NOT NULL DEFAULT '{}',
                    created_at timestamptz NOT NULL DEFAULT NOW(),
                    updated_at timestamptz NOT NULL DEFAULT NOW()
                );

                CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users (email);

                DROP TRIGGER IF EXISTS trg_users_set_updated_at ON users;

                CREATE TRIGGER trg_users_set_updated_at
                BEFORE UPDATE ON users
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
            """;
            command.ExecuteNonQuery();
        });
    }

    public override void Down()
    {
        Execute.Sql("""
            DROP TRIGGER IF EXISTS trg_users_set_updated_at ON users;
            DROP INDEX IF EXISTS idx_users_email;
            DROP TABLE IF EXISTS users;
        """);
    }
}
