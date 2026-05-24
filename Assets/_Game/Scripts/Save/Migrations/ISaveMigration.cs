namespace CindarsHope.Save.Migrations
{
    public interface ISaveMigration
    {
        string MigrationId { get; }
        int SourceSchemaVersion { get; }
        int TargetSchemaVersion { get; }
        bool TryMigrate(SaveMigrationContext context, out string migratedJson, out string errorMessage);
    }
}
