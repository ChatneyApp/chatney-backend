using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202610130001)]
public class _202610130001_CreatePasswordResetRequests : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            CREATE TABLE password_reset_requests (
                code uuid PRIMARY KEY,
                user_id uuid NOT NULL,
                created_at timestamptz NOT NULL,
                valid_until timestamptz NOT NULL,
                CONSTRAINT fk_password_reset_requests_user_id
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
            );

            CREATE INDEX ix_password_reset_requests_user_id
                ON password_reset_requests(user_id);
            CREATE INDEX ix_password_reset_requests_valid_until
                ON password_reset_requests(valid_until);
            """);
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS password_reset_requests;");
    }
}
