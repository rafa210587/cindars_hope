using System;
using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.Save.Migrations
{
    public sealed class SaveV3ToV4Migration : ISaveMigration
    {
        public string MigrationId => "save_v3_to_v4_mana_active_skills_cave_snapshots";
        public int SourceSchemaVersion => 3;
        public int TargetSchemaVersion => 4;

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

            InitializeMana(saveData);
            InitializeActiveSkillSlots(saveData);
            InitializeCaveRunState(saveData);

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

        private void InitializeMana(GameSaveData saveData)
        {
            if (saveData.Player == null)
            {
                return;
            }

            // Initialize mana to defaults if not present
            if (saveData.Player.MaxMana == 0)
            {
                saveData.Player.MaxMana = 100;
                saveData.Player.CurrentMana = 100;
            }
        }

        private void InitializeActiveSkillSlots(GameSaveData saveData)
        {
            if (saveData.ActiveSkillSlots == null)
            {
                saveData.ActiveSkillSlots = new ActiveSkillSlotsSaveData
                {
                    SlotRSkillActionId = string.Empty,
                    SlotTSkillActionId = string.Empty,
                    SlotYSkillActionId = string.Empty,
                    SlotGSkillActionId = string.Empty
                };
            }
        }

        private void InitializeCaveRunState(GameSaveData saveData)
        {
            if (saveData.Cave == null)
            {
                saveData.Cave = new CaveSaveData();
            }

            // Ensure all collections exist
            if (saveData.Cave.UnlockedCheckpoints == null)
            {
                saveData.Cave.UnlockedCheckpoints = new List<int>();
            }

            if (saveData.Cave.DepletedNodeIds == null)
            {
                saveData.Cave.DepletedNodeIds = new List<string>();
            }

            if (saveData.Cave.VisitedLevelSnapshots == null)
            {
                saveData.Cave.VisitedLevelSnapshots = new List<SerializedVisitedLevelSnapshot>();
            }

            if (saveData.Cave.BossDefeatStates == null)
            {
                saveData.Cave.BossDefeatStates = new List<CaveBossDefeatState>();
            }
        }
    }
}
