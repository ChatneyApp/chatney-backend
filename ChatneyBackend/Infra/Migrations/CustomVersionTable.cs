namespace ChatneyBackend.Infra.Migrations
{
    using FluentMigrator.Runner.VersionTableInfo;

    [VersionTableMetaData]
    public class CustomVersionTable : IVersionTableMetaData
    {
        public bool OwnsSchema => true;

        public string SchemaName => "dbo"; // Specify your desired schema

        public string TableName => "migrations"; // Specify your desired table name

        public string ColumnName => "version";
        public string DescriptionColumnName => "description";
        public string AppliedOnColumnName => "applied_on";
        public string UniqueIndexName => "uc_version";
        public bool CreateWithPrimaryKey => false;
    }
}
