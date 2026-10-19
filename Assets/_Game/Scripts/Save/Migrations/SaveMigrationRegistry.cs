using System.Collections.Generic;
using System.Linq;

namespace CindarsHope.Save.Migrations
{
    public sealed class SaveMigrationRegistry
    {
        private readonly Dictionary<int, ISaveMigration> _migrationsBySourceVersion = new Dictionary<int, ISaveMigration>();

        public SaveMigrationRegistry(IEnumerable<ISaveMigration> migrations = null)
        {
            if (migrations == null)
            {
                return;
            }

            foreach (var migration in migrations)
            {
                Register(migration);
            }
        }

        public void Register(ISaveMigration migration)
        {
            if (migration == null)
            {
                return;
            }

            _migrationsBySourceVersion[migration.SourceSchemaVersion] = migration;
        }

        public bool CanMigrate(int sourceVersion, int targetVersion)
        {
            if (sourceVersion == targetVersion)
            {
                return true;
            }

            if (sourceVersion > targetVersion)
            {
                return false;
            }

            var version = sourceVersion;
            while (version < targetVersion)
            {
                if (!_migrationsBySourceVersion.TryGetValue(version, out var migration))
                {
                    return false;
                }

                if (migration.TargetSchemaVersion != version + 1)
                {
                    return false;
                }

                version = migration.TargetSchemaVersion;
            }

            return version == targetVersion;
        }

        public bool TryMigrate(SaveMigrationContext context, out SaveMigrationResult result)
        {
            result = SaveMigrationResult.Failed(
                context != null ? context.SourceSchemaVersion : 0,
                context != null ? context.TargetSchemaVersion : 0,
                "Migration context is null.");

            if (context == null)
            {
                return false;
            }

            if (!CanMigrate(context.SourceSchemaVersion, context.TargetSchemaVersion))
            {
                result = SaveMigrationResult.Failed(
                    context.SourceSchemaVersion,
                    context.TargetSchemaVersion,
                    $"No complete migration path from v{context.SourceSchemaVersion} to v{context.TargetSchemaVersion}.");
                return false;
            }

            var currentVersion = context.SourceSchemaVersion;
            var currentJson = context.RawJson;
            var appliedMigrationIds = new List<string>();

            while (currentVersion < context.TargetSchemaVersion)
            {
                var migration = _migrationsBySourceVersion[currentVersion];
                var stepContext = new SaveMigrationContext(
                    context.SaveFilePath,
                    context.BackupFilePath,
                    currentVersion,
                    migration.TargetSchemaVersion,
                    currentJson);

                if (!migration.TryMigrate(stepContext, out var migratedJson, out var errorMessage))
                {
                    result = SaveMigrationResult.Failed(
                        currentVersion,
                        migration.TargetSchemaVersion,
                        $"Migration {migration.MigrationId} failed: {errorMessage}");
                    result.BackupFilePath = context.BackupFilePath;
                    result.AppliedMigrationIds = appliedMigrationIds;
                    return false;
                }

                currentJson = migratedJson;
                currentVersion = migration.TargetSchemaVersion;
                appliedMigrationIds.Add(migration.MigrationId);
            }

            result = new SaveMigrationResult
            {
                Success = true,
                SourceSchemaVersion = context.SourceSchemaVersion,
                TargetSchemaVersion = context.TargetSchemaVersion,
                BackupFilePath = context.BackupFilePath,
                MigratedJson = currentJson,
                AppliedMigrationIds = appliedMigrationIds.ToList()
            };
            return true;
        }
    }
}
