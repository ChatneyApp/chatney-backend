using FluentMigrator;

namespace ChatneyBackend.Infra.Migrations;

[Migration(202607310007)]
public class _202607310007_SeedGlobalSecureObject : Migration
{
    public override void Up()
    {
        var sql = """
            INSERT INTO secure_objects (id, description)
            OVERRIDING SYSTEM VALUE
            VALUES (1, '{"kind":"global","name":"System"}'::jsonb)
            ON CONFLICT (id) DO NOTHING;

            SELECT setval('secure_objects_id_seq', GREATEST(1, (SELECT COALESCE(MAX(id), 1) FROM secure_objects)), true);
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        var sql = """
            DELETE FROM secure_objects WHERE id = 1;
            ALTER SEQUENCE secure_objects_id_seq RESTART WITH 1;
        """;

        Execute.Sql(sql);
    }
}
