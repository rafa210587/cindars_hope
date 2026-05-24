using System.Collections.Generic;

namespace CindarsHope.Save.Migrations
{
    public sealed class SaveMigrationResult
    {
        public bool Success;
        public int SourceSchemaVersion;
        public int TargetSchemaVersion;
        public string BackupFilePath;
        public string MigratedJson;
        public string ErrorMessage;
        public List<string> AppliedMigrationIds = new List<string>();

        public static SaveMigrationResult NotRequired(int schemaVersion)
        {
            return new SaveMigrationResult
            {
                Success = true,
                SourceSchemaVersion = schemaVersion,
                TargetSchemaVersion = schemaVersion
            };
        }

        public static SaveMigrationResult Failed(int sourceSchemaVersion, int targetSchemaVersion, string errorMessage)
        {
            return new SaveMigrationResult
            {
                Success = false,
                SourceSchemaVersion = sourceSchemaVersion,
                TargetSchemaVersion = targetSchemaVersion,
                ErrorMessage = errorMessage
            };
        }
    }
}
