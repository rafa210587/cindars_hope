namespace CindarsHope.Save.Migrations
{
    public sealed class SaveMigrationContext
    {
        public SaveMigrationContext(
            string saveFilePath,
            string backupFilePath,
            int sourceSchemaVersion,
            int targetSchemaVersion,
            string rawJson)
        {
            SaveFilePath = saveFilePath;
            BackupFilePath = backupFilePath;
            SourceSchemaVersion = sourceSchemaVersion;
            TargetSchemaVersion = targetSchemaVersion;
            RawJson = rawJson;
        }

        public string SaveFilePath { get; }
        public string BackupFilePath { get; }
        public int SourceSchemaVersion { get; }
        public int TargetSchemaVersion { get; }
        public string RawJson { get; }
    }
}
