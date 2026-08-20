using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202607310001)]
public class _202607310001_CreateRolesAndPermissionsTables : Migration
{
    public override void Up()
    {
        var sql = """
            CREATE TYPE permission AS ENUM (
                'attachment.delete',
                'attachment.read',
                'attachment.upload',
                'channel.addChannelGroup',
                'channel.createChannel',
                'channel.createMessage',
                'channel.deleteChannel',
                'channel.deleteChannelGroup',
                'channel.deleteChannelType',
                'channel.deleteMessage',
                'channel.deleteOwnMessage',
                'channel.editChannel',
                'channel.editChannelGroup',
                'channel.editMessage',
                'channel.editOwnMessage',
                'channel.readChannel',
                'channel.readMessage',
                'config.readValue',
                'config.updateValue',
                'role.createRole',
                'role.deleteRole',
                'role.editRole',
                'user.createUser',
                'user.deleteUser',
                'user.editUser',
                'user.readUser',
                'workspace.createWorkspace',
                'workspace.deleteWorkspace',
                'workspace.readWorkspace',
                'workspace.updateWorkspace'
            );

            CREATE TABLE roles (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name varchar(255) NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                updated_at timestamptz NOT NULL DEFAULT NOW()
            );

            CREATE TABLE secure_objects (
                id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                description jsonb NULL
            );

            CREATE TABLE role_acls (
                role_id int NOT NULL,
                sec_obj_id int NOT NULL,
                permissions permission[] NOT NULL DEFAULT '{}',
                CONSTRAINT role_acls_pkey PRIMARY KEY (role_id, sec_obj_id),
                CONSTRAINT fk_role_acls_role_id
                    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
                CONSTRAINT fk_role_acls_sec_obj_id
                    FOREIGN KEY (sec_obj_id) REFERENCES secure_objects(id) ON DELETE CASCADE
            );
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TABLE IF EXISTS role_acls;
            DROP TABLE IF EXISTS secure_objects;
            DROP TABLE IF EXISTS roles;
            DROP TYPE IF EXISTS permission;
        """;

        Execute.Sql(sql);
    }
}
