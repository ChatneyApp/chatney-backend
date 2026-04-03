using ChatneyBackend.Domains.Channels;
using ChatneyBackend.Domains.Messages;
using ChatneyBackend.Domains.Roles;
using ChatneyBackend.Domains.Users;
using ChatneyBackend.Domains.Workspaces;
using FluentMigrator;
using Npgsql;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202603180001)]
public class _202603180001_CreateRolesTable : Migration
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
                CREATE TABLE roles (
                    id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    name varchar(255) NOT NULL,
                    is_base bool NOT NULL DEFAULT false,
                    permissions text[] NOT NULL DEFAULT '{}',
                    created_at timestamptz NOT NULL DEFAULT NOW(),
                    updated_at timestamptz NOT NULL DEFAULT NOW()
                );

                CREATE OR REPLACE FUNCTION set_updated_at()
                RETURNS trigger AS $$
                BEGIN
                    NEW.updated_at = NOW();
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                   
                CREATE TRIGGER trg_roles_set_updated_at
                BEFORE UPDATE ON roles
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
            """;
            command.Parameters.AddRange(new NpgsqlParameter[]
            {
                new NpgsqlParameter<string>("name", ChatneyBackend.Domains.Roles.DomainSettings.BaseRoleName),
                new NpgsqlParameter<string[]>("permissions", [
                    MessagePermissions.CreateMessage,
                    MessagePermissions.DeleteMessage,
                    MessagePermissions.EditMessage,
                    MessagePermissions.ReadMessage,
                    UserPermissions.ReadUser,
                    UserPermissions.EditUser,
                    ChannelPermissions.CreateMessage,
                    ChannelPermissions.DeleteMessage,
                    ChannelPermissions.EditMessage,
                    ChannelPermissions.ReadChannel,
                    ChannelPermissions.ReadMessage,
                    WorkspacePermissions.ReadWorkspace
                ])
                {
                    NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text
                }
            });

            command.ExecuteNonQuery();
        });
    }

    public override void Down()
    {
        var sql = """
            DROP TRIGGER IF EXISTS trg_roles_set_updated_at ON roles;
            DROP TABLE IF EXISTS roles;
            DROP FUNCTION IF EXISTS set_updated_at();
        """;

        Execute.Sql(sql);
    }
}
