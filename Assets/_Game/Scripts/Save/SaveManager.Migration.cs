using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Craft;
using CindarsHope.Enemy;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.NPC;
using CindarsHope.Player;
using CindarsHope.Player.Data;
using CindarsHope.Player.Death;
using CindarsHope.Player.Progression;
using CindarsHope.Save.Migrations;
using CindarsHope.Save.Providers;
using CindarsHope.Skills;
using CindarsHope.World;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace CindarsHope.Save
{
    public partial class SaveManager
    {
        private GameSaveData TryReadExistingValidSave()
        {
            var savePath = SaveFilePath;
            if (!File.Exists(savePath))
            {
                return null;
            }

            try
            {
                return TryReadSaveWithMigration(savePath, true, out var saveData, out _)
                    ? saveData
                    : null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Error reading existing save: {exception.Message}", this);
                return null;
            }
        }

        private bool TryReadSaveWithMigration(string savePath, bool allowWriteBack, out GameSaveData saveData, out SaveMigrationResult result)
        {
            saveData = null;
            result = SaveMigrationResult.Failed(0, CurrentSchemaVersion, "Unknown save migration failure.");

            if (string.IsNullOrWhiteSpace(savePath) || !File.Exists(savePath))
            {
                result = SaveMigrationResult.Failed(0, CurrentSchemaVersion, $"Save file not found at {savePath}.");
                return false;
            }

            string rawJson;
            try
            {
                rawJson = File.ReadAllText(savePath);
            }
            catch (Exception exception)
            {
                result = SaveMigrationResult.Failed(0, CurrentSchemaVersion, $"Could not read save file: {exception.Message}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                result = SaveMigrationResult.Failed(0, CurrentSchemaVersion, "Save file is empty.");
                return false;
            }

            if (!TryDeserializeSave(rawJson, out var parsedSaveData, out var parseError))
            {
                result = SaveMigrationResult.Failed(0, CurrentSchemaVersion, parseError);
                return false;
            }

            var sourceVersion = DetectSourceSchemaVersion(parsedSaveData);
            if (sourceVersion > CurrentSchemaVersion)
            {
                result = SaveMigrationResult.Failed(
                    sourceVersion,
                    CurrentSchemaVersion,
                    $"Save schema version {sourceVersion} is newer than supported version {CurrentSchemaVersion}.");
                return false;
            }

            if (sourceVersion == CurrentSchemaVersion)
            {
                parsedSaveData.SchemaVersion = CurrentSchemaVersion;
                if (!ValidateAndNormalizeSave(parsedSaveData, out var validationError))
                {
                    result = SaveMigrationResult.Failed(sourceVersion, CurrentSchemaVersion, validationError);
                    return false;
                }

                saveData = parsedSaveData;
                result = SaveMigrationResult.NotRequired(CurrentSchemaVersion);
                return true;
            }

            if (!_migrationRegistry.CanMigrate(sourceVersion, CurrentSchemaVersion))
            {
                result = SaveMigrationResult.Failed(
                    sourceVersion,
                    CurrentSchemaVersion,
                    $"No complete migration path from v{sourceVersion} to v{CurrentSchemaVersion}.");
                return false;
            }

            var backupFilePath = string.Empty;
            if (allowWriteBack && !SaveBackupService.TryCreateBackup(savePath, out backupFilePath, out var backupError))
            {
                result = SaveMigrationResult.Failed(sourceVersion, CurrentSchemaVersion, $"Could not create save backup: {backupError}");
                return false;
            }

            var context = new SaveMigrationContext(savePath, backupFilePath, sourceVersion, CurrentSchemaVersion, rawJson);
            if (!_migrationRegistry.TryMigrate(context, out result))
            {
                Debug.LogWarning(result.ErrorMessage, this);
                return false;
            }

            if (!TryDeserializeSave(result.MigratedJson, out var migratedSaveData, out var migratedParseError))
            {
                result.Success = false;
                result.ErrorMessage = migratedParseError;
                return false;
            }

            migratedSaveData.SchemaVersion = CurrentSchemaVersion;
            if (!ValidateAndNormalizeSave(migratedSaveData, out var migratedValidationError))
            {
                result.Success = false;
                result.ErrorMessage = migratedValidationError;
                return false;
            }

            if (allowWriteBack)
            {
                try
                {
                    WriteTextSafely(savePath, JsonUtility.ToJson(migratedSaveData, true));
                }
                catch (Exception exception)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Could not write migrated save: {exception.Message}";
                    return false;
                }
            }

            saveData = migratedSaveData;
            Debug.Log($"Save migrated from schema v{sourceVersion} to v{CurrentSchemaVersion}. Backup: {backupFilePath}", this);
            return true;
        }

        private static bool TryDeserializeSave(string json, out GameSaveData saveData, out string errorMessage)
        {
            saveData = null;
            errorMessage = string.Empty;

            try
            {
                saveData = JsonUtility.FromJson<GameSaveData>(json);
                if (saveData == null)
                {
                    errorMessage = "Save file could not be parsed.";
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"Save file could not be parsed: {exception.Message}";
                return false;
            }
        }

        private static int DetectSourceSchemaVersion(GameSaveData saveData)
        {
            if (saveData == null)
            {
                return 0;
            }

            if (saveData.SchemaVersion > 0)
            {
                return saveData.SchemaVersion;
            }

            return IsLegacyV1Candidate(saveData) ? 1 : 0;
        }

        private static bool IsLegacyV1Candidate(GameSaveData saveData)
        {
            if (saveData == null)
            {
                return false;
            }

            return saveData.Player != null
                || saveData.Inventory != null
                || saveData.Farm != null
                || saveData.World != null
                || saveData.Cave != null
                || saveData.CurrentDay > 0;
        }

        private static bool ValidateAndNormalizeSave(GameSaveData saveData, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (saveData == null)
            {
                errorMessage = "Save data is null.";
                return false;
            }

            if (saveData.SchemaVersion != CurrentSchemaVersion)
            {
                errorMessage = $"Unsupported save schema version {saveData.SchemaVersion}. Expected {CurrentSchemaVersion}.";
                return false;
            }

            if (saveData.Player == null)
            {
                errorMessage = "Save data is missing Player section.";
                return false;
            }

            if (saveData.CurrentDay <= 0)
            {
                saveData.CurrentDay = 1;
            }

            saveData.Inventory ??= new InventorySaveData();
            saveData.Equipment ??= new EquipmentSaveData();
            saveData.Hotbar ??= new HotbarSaveData();
            saveData.Progression ??= new PlayerProgressionSaveData();
            saveData.Farm ??= new FarmSaveData();
            saveData.World ??= new WorldSaveData();
            saveData.Cave ??= new CaveSaveData();
            saveData.Npcs ??= new NpcManagerSaveData();
            saveData.Bestiary ??= new BestiarySaveData();

            saveData.Inventory.Items ??= new List<InventoryItemSaveData>();
            saveData.Inventory.Slots ??= new List<InventorySlotSaveData>();
            if (saveData.Inventory.Capacity <= 0)
            {
                saveData.Inventory.Capacity = InventoryManager.DefaultCapacity;
            }

            saveData.Farm.Plots ??= new List<FarmPlotSaveData>();
            saveData.Farm.Trees ??= new List<TreeSaveData>();
            // fable_55: save legado sem o campo aditivo carrega com seção de processamento vazia.
            saveData.Farm.Processing ??= new Farm.Processing.FarmProcessingSaveData();
            saveData.Farm.Processing.Jobs ??= new List<Farm.Processing.FarmProcessingJobSaveData>();
            saveData.World.Pickups ??= new List<ItemPickupSaveData>();
            saveData.World.Trees ??= new List<TreeSaveData>();
            saveData.Npcs.Npcs ??= new List<NpcSaveData>();
            saveData.Bestiary.Entries ??= new List<BestiaryEntrySaveData>();

            saveData.Death ??= new DeathSaveData();
            saveData.Death.DeathStats ??= new DeathStatsSaveData();
            if (saveData.Death.ActiveCorpse != null)
            {
                saveData.Death.ActiveCorpse.LostInventoryItems ??= new List<InventorySlotSaveData>();
                saveData.Death.ActiveCorpse.LostEquipmentItems ??= new List<InventorySlotSaveData>();
            }

            saveData.Quests ??= new QuestStateSectionSaveData();
            saveData.Quests.QuestStates ??= new List<QuestStateSaveData>();
            saveData.Quests.GlobalKnownHints ??= new List<string>();
            foreach (var qr in saveData.Quests.QuestStates)
            {
                if (qr == null) continue;
                qr.CompletedStepIds ??= new List<string>();
                qr.FailedStepIds ??= new List<string>();
                qr.ObjectiveStates ??= new List<QuestObjectiveStateSaveData>();
                qr.KnownObjectiveIds ??= new List<string>();
                qr.KnownHints ??= new List<string>();
                qr.GrantedRewardIds ??= new List<string>();
                qr.GrantedFlagIds ??= new List<string>();
            }

            return true;
        }

        /// <summary>
        /// Escreve <paramref name="contents"/> em <paramref name="path"/> sem deixar uma janela
        /// onde o arquivo final esta ausente. Se ja existir um arquivo em <paramref name="path"/>,
        /// usa <see cref="File.Replace(string, string, string)"/> (atomico no NTFS/Mono/IL2CPP:
        /// substitui o destino e move o conteudo antigo para <c>path + ".backup"</c> em uma unica
        /// chamada - nunca ha um instante em que <paramref name="path"/> nao exista). Se
        /// <paramref name="path"/> ainda nao existir, nao ha arquivo antigo a perder: basta mover
        /// o temporario.
        /// </summary>
        private static void WriteTextSafely(string path, string contents)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = $"{path}.tmp";
            File.WriteAllText(tempPath, contents);

            if (!File.Exists(tempPath))
            {
                throw new IOException($"Temporary save file was not written: {tempPath}");
            }

            if (!File.Exists(path))
            {
                // Sem arquivo antigo em `path`: nada a perder, basta mover o tmp.
                File.Move(tempPath, path);
                return;
            }

            var backupPath = $"{path}{SaveBackupSuffix}";
            try
            {
                // Atomico: substitui `path` por `tempPath` e move o `path` antigo para
                // `backupPath` em uma unica operacao do SO. Nao ha instante intermediario em
                // que `path` esteja ausente.
                File.Replace(tempPath, path, backupPath, true);
            }
            catch (PlatformNotSupportedException)
            {
                // Fallback manual seguro: cria backup do arquivo antigo ANTES de qualquer delete.
                // So prossegue com delete+move se o backup foi confirmado - nunca perde o
                // original sem garantir que uma copia ja existe em outro lugar.
                if (!SaveBackupService.TryCreateBackup(path, out _, out var backupError))
                {
                    throw new IOException($"Could not create safety backup before replacing save: {backupError}");
                }

                File.Delete(path);
                File.Move(tempPath, path);
            }
        }

        private const string SaveBackupSuffix = ".backup";

        /// <summary>
        /// Se <paramref name="path"/> estiver ausente ou seu conteudo nao puder ser
        /// deserializado como <see cref="GameSaveData"/> valido, tenta recuperar de
        /// <c>path + ".backup"</c> (criado por <see cref="WriteTextSafely"/> ou por
        /// <see cref="SaveBackupService"/>). Retorna true e loga a recuperacao se um backup valido
        /// foi restaurado; retorna false se nao havia backup ou se ele tambem e invalido - nesse
        /// caso o chamador deve seguir o comportamento existente (sem save = novo jogo / erro).
        /// </summary>
        private static bool TryRecoverFromBackupIfNeeded(string path)
        {
            var backupPath = $"{path}{SaveBackupSuffix}";

            var mainIsUsable = File.Exists(path) && IsReadableValidSave(path);
            if (mainIsUsable)
            {
                return false;
            }

            if (!File.Exists(backupPath) || !IsReadableValidSave(backupPath))
            {
                return false;
            }

            if (!SaveBackupService.TryRestoreBackup(path, backupPath, out var restoreError))
            {
                Debug.LogWarning($"Save file at {path} was missing/corrupted and backup restore failed: {restoreError}", null);
                return false;
            }

            Debug.LogWarning($"Save file at {path} was missing or corrupted. Recovered from backup at {backupPath}.", null);
            return true;
        }

        private static bool IsReadableValidSave(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return !string.IsNullOrWhiteSpace(json) && TryDeserializeSave(json, out _, out _);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
