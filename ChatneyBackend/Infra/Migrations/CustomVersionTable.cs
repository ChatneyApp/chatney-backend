namespace ChatneyBackend.Infra.Migrations
{
    using FluentMigrator.Runner.VersionTableInfo;

    [VersionTableMetaData]
    public class CustomVersionTable : DefaultVersionTableMetaData
    {
        public override string SchemaName => "dbo"; // Specify your desired schema

        public override string TableName => "migrations"; // Specify your desired table name

        // Other properties you can optionally override:
        public override string ColumnName => "version";
        public override string DescriptionColumnName => "description";
        public override string AppliedOnColumnName => "applied_on";
        public override string UniqueIndexName => "uc_version";
    }
}
