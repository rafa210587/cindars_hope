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
            InitializeStaminaSaveData(saveData);
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

            // Se já for uma List, não precisa fazer nada (pode ser um old-style Dictionary serializado)
            // Se for Dictionary (que não é possível em v2 atual pois JsonUtility não serializa), criar nova lista vazia
            if (oldDurabilities == null || (oldDurabilities is List<DurabilityEntryData> && ((List<DurabilityEntryData>)oldDurabilities).Count == 0))
            {
                saveData.EquipmentDurability.EquipmentDurabilities = new List<DurabilityEntryData>();
            }
            // Se chegou aqui como List, já está no formato correto de v3
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

        private void InitializeStaminaSaveData(GameSaveData saveData)
        {
            if (saveData.Stamina == null)
            {
                saveData.Stamina = new StaminaSaveData
                {
                    MaxStamina = 100,
                    CurrentStamina = 100
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
