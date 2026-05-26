using System;
using CindarsHope.Skills;
using UnityEngine;

namespace CindarsHope.Save.Migrations
{
    public sealed class SaveV4ToV5Migration : ISaveMigration
    {
        public string MigrationId => "save_v4_to_v5_skill_tree";
        public int SourceSchemaVersion => 4;
        public int TargetSchemaVersion => 5;

        public bool TryMigrate(SaveMigrationContext context, out string migratedJson, out string errorMessage)
        {
            migratedJson = string.Empty;
            errorMessage = string.Empty;

            if (context == null || string.IsNullOrWhiteSpace(context.RawJson))
            {
                errorMessage = "Migration context or raw JSON is empty.";
                return false;
            }

            GameSaveData saveData;
            try
            {
                saveData = JsonUtility.FromJson<GameSaveData>(context.RawJson);
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to deserialize save data: {ex.Message}";
                return false;
            }

            if (saveData == null)
            {
                errorMessage = "Unity JsonUtility returned null save data.";
                return false;
            }

            saveData.SchemaVersion = TargetSchemaVersion;
            InitializeSkillTree(saveData);

            try
            {
                migratedJson = JsonUtility.ToJson(saveData, true);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to serialize migrated save data: {ex.Message}";
                return false;
            }
        }

        private void InitializeSkillTree(GameSaveData saveData)
        {
            if (saveData.SkillTree == null)
                saveData.SkillTree = new SkillTreeSaveData();
        }
    }
}
