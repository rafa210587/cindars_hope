using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Save.Migrations
{
    public sealed class SaveV2ToV3Migration : ISaveMigration
    {
        public string MigrationId => "save_v2_to_v3_equipment_durability_and_time";
        public int SourceSchemaVersion => 2;
        public int TargetSchemaVersion => 3;

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
            catch (Exception exception)
            {
                errorMessage = $"Failed to deserialize save data: {exception.Message}";
                return false;
            }

            if (saveData == null)
            {
                errorMessage = "Unity JsonUtility returned null save data.";
                return false;
            }

            saveData.SchemaVersion = TargetSchemaVersion;

            MigrateEquipmentDurability(saveData);
            InitializeGameTimeSaveData(saveData);
            InitializePlayerStatusEffects(saveData);

            try
            {
                migratedJson = JsonUtility.ToJson(saveData, true);
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"Failed to serialize migrated save data: {exception.Message}";
                return false;
            }
        }

        private void MigrateEquipmentDurability(GameSaveData saveData)
        {
            if (saveData.EquipmentDurability == null)
            {
                saveData.EquipmentDurability = new EquipmentDurabilitySaveData();
                return;
            }

            var oldDurabilities = saveData.EquipmentDurability.EquipmentDurabilities;
            var newDurabilities = new List<DurabilityEntryData>();

            if (oldDurabilities != null && oldDurabilities.Count > 0)
            {
                var index = 0;
                foreach (var kvp in oldDurabilities)
                {
                    if (kvp.Value != null)
                    {
                        newDurabilities.Add(new DurabilityEntryData
                        {
                            ItemInstanceId = kvp.Key ?? $"durability_{index}",
                            CurrentDurability = kvp.Value.CurrentDurability,
                            MaxDurability = kvp.Value.MaxDurability
                        });
                        index++;
                    }
                }
            }

            saveData.EquipmentDurability.EquipmentDurabilities = newDurabilities;
        }

        private void InitializeGameTimeSaveData(GameSaveData saveData)
        {
            if (saveData.GameTime == null)
            {
                saveData.GameTime = new GameTimeSaveData
                {
                    CurrentDay = saveData.CurrentDay > 0 ? saveData.CurrentDay : 1,
                    CurrentPhase = 0,
                    PhaseElapsedSeconds = 0f
                };
            }
        }

        private void InitializePlayerStatusEffects(GameSaveData saveData)
        {
            if (saveData.PlayerStatusEffects == null)
            {
                saveData.PlayerStatusEffects = new PlayerStatusEffectsSaveData
                {
                    ActiveEffects = new List<StatusEffectEntryData>()
                };
            }
        }
    }
}
