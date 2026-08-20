using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202607310008)]
public class _202607310008_AddDirectMessageChannels : Migration
{
    public override void Up()
    {
        var sql = """
            ALTER TABLE channels
                ALTER COLUMN workspace_id DROP NOT NULL;

            ALTER TABLE channels
                ADD COLUMN is_dm boolean NOT NULL DEFAULT false;

            CREATE TABLE channel_members (
                channel_id int NOT NULL,
                user_id uuid NOT NULL,
                created_at timestamptz NOT NULL DEFAULT NOW(),
                CONSTRAINT channel_members_pkey PRIMARY KEY (channel_id, user_id),
                CONSTRAINT fk_channel_members_channel_id
                    FOREIGN KEY (channel_id) REFERENCES channels(id) ON DELETE CASCADE,
                CONSTRAINT fk_channel_members_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );

            CREATE INDEX ix_channel_members_user_id ON channel_members (user_id);
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DROP TABLE IF EXISTS channel_members;
            ALTER TABLE channels DROP COLUMN IF EXISTS is_dm;
            ALTER TABLE channels ALTER COLUMN workspace_id SET NOT NULL;
        """;

        Execute.Sql(sql);
    }
}
