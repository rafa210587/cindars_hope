using System;
using System.IO;
using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using CindarsHope.Enemy;
using CindarsHope.Farm;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.NPC;
using CindarsHope.Player.Progression;
using CindarsHope.Save.Migrations;
using CindarsHope.World;
using UnityEngine;

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
                    SaveBackupService.WriteTextSafely(savePath, JsonUtility.ToJson(migratedSaveData, true));
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

        private bool ValidateOriginCarriers(GameSaveData saveData, out string errorMessage)
        {
            errorMessage = string.Empty;
            bool Known(string itemId) => !string.IsNullOrWhiteSpace(itemId)
                && _inventoryManager != null && _inventoryManager.TryGetItemData(itemId, out _);

            if (saveData.World?.Pickups != null)
            {
                var pickupIndices = new HashSet<int>();
                var dynamicPickupIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var pickup in saveData.World.Pickups)
                    if (pickup == null || pickup.Amount <= 0 || pickup.PickupIndex < 0
                        || !pickupIndices.Add(pickup.PickupIndex) || !Known(pickup.ItemId)
                        || (pickup.PickupIndex >= 10000 && !pickup.IsDynamic)
                        || (pickup.IsDynamic
                            && (string.IsNullOrWhiteSpace(pickup.SceneName)
                                || string.IsNullOrWhiteSpace(pickup.RuntimeGeneratedPickupId)
                                || !dynamicPickupIds.Add(pickup.RuntimeGeneratedPickupId)))
                        || (!string.IsNullOrWhiteSpace(pickup.ItemInstanceId)
                            && (!_inventoryManager.TryGetItemData(pickup.ItemId, out var pickupData)
                                || pickupData.MaxStack != 1 || pickup.Amount != 1
                                || !ItemInstanceIdUtility.IsForItem(
                                    pickup.ItemId, pickup.ItemInstanceId))))
                    { errorMessage = "World pickup has unknown item or impossible identity."; return false; }
            }

            if (saveData.Farm?.PendingShipping?.Entries != null)
            {
                var shippingIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var entry in saveData.Farm.PendingShipping.Entries)
                    if (entry == null || entry.Quantity <= 0 || !Known(entry.ItemId)
                        || string.IsNullOrWhiteSpace(entry.EntryId)
                        || !shippingIds.Add(entry.EntryId))
                    { errorMessage = "Pending shipping has an unknown item or duplicate identity."; return false; }
                foreach (var entry in saveData.Farm.PendingShipping.Entries)
                {
                    if (entry.State != (int)Farm.Shipping.ShippingEntryState.Pending
                        || entry.OriginSchemaVersion != 1) continue;
                    var lots = ItemOriginLot.FromSaveData(entry.OriginLots, entry.Quantity);
                    var hasValid = false;
                    var hasInvalid = false;
                    foreach (var lot in lots)
                    {
                        hasInvalid |= lot.Kind == ItemOriginKind.Invalid;
                        hasValid |= lot.Kind != ItemOriginKind.Invalid;
                    }
                    if (hasValid && hasInvalid && !shippingIds.Add(entry.EntryId + "_invalid"))
                    { errorMessage = "Pending shipping has a reserved identity collision."; return false; }
                }
            }

            if (saveData.Equipment?.Slots != null)
            {
                var equippedSlots = new HashSet<EquipmentSlot>();
                var equippedInstances = new HashSet<string>(StringComparer.Ordinal);
                foreach (var slot in saveData.Equipment.Slots)
                {
                    var itemId = ItemInstanceIdUtility.GetItemId(slot?.ItemInstanceId);
                    if (slot == null || slot.SlotType == EquipmentSlot.None
                        || !Enum.IsDefined(typeof(EquipmentSlot), slot.SlotType)
                        || !equippedSlots.Add(slot.SlotType)
                        || string.IsNullOrWhiteSpace(slot.ItemInstanceId)
                        || !equippedInstances.Add(slot.ItemInstanceId) || !Known(itemId))
                    { errorMessage = "Equipment has an unknown item or impossible slot."; return false; }
                }
            }

            var corpse = saveData.Death?.ActiveCorpse;
            if (corpse != null)
            {
                var corpseInstances = new HashSet<string>(StringComparer.Ordinal);
                if (!ValidateCorpseSlots(corpse.LostInventoryItems, Known,
                        new HashSet<int>(), corpseInstances)
                    || !ValidateCorpseSlots(corpse.LostEquipmentItems, Known,
                        new HashSet<int>(), corpseInstances))
                { errorMessage = "Corpse has an unknown item or impossible slot."; return false; }
            }

            if (saveData.Crafting?.Stations != null)
            {
                var craftingStations = new HashSet<string>(StringComparer.Ordinal);
                foreach (var station in saveData.Crafting.Stations)
                {
                    var job = station?.Job;
                    if (station == null || string.IsNullOrWhiteSpace(station.StationInstanceId)
                        || !craftingStations.Add(station.StationInstanceId)
                        || (job != null && (job.OutputAmount <= 0
                        || !Known(job.OutputItemId))))
                    { errorMessage = "Crafting job has an unknown output."; return false; }
                    if (job?.IngredientsConsumed == null) continue;
                    foreach (var ingredient in job.IngredientsConsumed)
                        if (ingredient == null || ingredient.Amount <= 0 || !Known(ingredient.ItemId))
                        { errorMessage = "Crafting job has an unknown ingredient."; return false; }
                }
            }

            if (saveData.Farm?.Processing?.Jobs != null)
            {
                var processingStations = new HashSet<string>(StringComparer.Ordinal);
                foreach (var job in saveData.Farm.Processing.Jobs)
                    if (job == null || job.OutputQuantity <= 0 || !Known(job.OutputItemId)
                        || !Farm.Processing.ProcessingRecipeCatalog.TryGetById(job.RecipeId,
                            out var recipe) || recipe == null || job.StationId != recipe.StationId
                        || job.OutputItemId != recipe.OutputItemId
                        || !processingStations.Add(job.StationId))
                    { errorMessage = "Processing job has an unknown or duplicate station/recipe."; return false; }
            }

            if (saveData.NpcServices?.PendingBookOrders != null)
            {
                var orderIds = new HashSet<int>();
                foreach (var order in saveData.NpcServices.PendingBookOrders)
                    if (order == null || !Known(order.DeliveredBookItemId)
                        || order.OrderId <= 0 || !orderIds.Add(order.OrderId))
                    { errorMessage = "Pending order has an unknown item or duplicate identity."; return false; }
            }

            if (saveData.Equipment?.Infusions != null)
            {
                var infusedInstances = new HashSet<string>(StringComparer.Ordinal);
                foreach (var infusion in saveData.Equipment.Infusions)
                    if (infusion == null
                        || !Known(ItemInstanceIdUtility.GetItemId(infusion.ItemInstanceId))
                        || !infusedInstances.Add(infusion.ItemInstanceId)
                        || !CindarsHope.Economy.TemperingCanon.IsValidTier(infusion.InfusionTier)
                        || CindarsHope.Economy.TemperingCanon.ParseElement(
                            infusion.InfusionElement) == CindarsHope.Economy.TemperingElement.None)
                    { errorMessage = "Infusion has an unknown weapon, element or identity."; return false; }
            }
            return true;
        }

        private static bool ValidateCorpseSlots(IReadOnlyList<InventorySlotSaveData> slots,
            Func<string, bool> known, HashSet<int> usedSlots, HashSet<string> usedInstances)
        {
            if (slots == null) return true;
            foreach (var slot in slots)
                if (slot == null || slot.Amount <= 0 || slot.SlotIndex < 0
                    || slot.SlotIndex >= InventoryManager.MaxCapacity
                    || !usedSlots.Add(slot.SlotIndex) || !known(slot.ItemId)
                    || (!string.IsNullOrWhiteSpace(slot.ItemInstanceId)
                        && (!ItemInstanceIdUtility.IsForItem(slot.ItemId, slot.ItemInstanceId)
                            || !usedInstances.Add(slot.ItemInstanceId))))
                    return false;
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

        private bool ValidateAndNormalizeSave(GameSaveData saveData, out string errorMessage)
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

            if (!NormalizeAndValidateOriginInventory(saveData.Inventory, out errorMessage))
                return false;

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

            if (!ValidateOriginCarriers(saveData, out errorMessage))
                return false;
            return true;
        }

        private bool NormalizeAndValidateOriginInventory(InventorySaveData inventory,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (inventory.Capacity > InventoryManager.MaxCapacity)
            {
                errorMessage = "Origin-enabled inventory capacity is impossible.";
                return false;
            }

            // A v6 file without the origin marker is damaged, never a historical v5 save.
            // Keep known items, but make every affected unit unsellable.
            var missingMarker = inventory.OriginSchemaVersion != 1;
            inventory.OriginSchemaVersion = 1;
            if (inventory.Slots.Count == 0 && inventory.Items.Count > 0)
            {
                foreach (var item in inventory.Items)
                {
                    if (item == null || item.Amount <= 0 || string.IsNullOrWhiteSpace(item.ItemId)
                        || _inventoryManager == null
                        || !_inventoryManager.TryGetItemData(item.ItemId, out var itemData))
                    {
                        errorMessage = "Origin-enabled inventory has an unknown item.";
                        return false;
                    }
                    var remaining = item.Amount;
                    while (remaining > 0)
                    {
                        if (inventory.Slots.Count >= inventory.Capacity)
                        {
                            errorMessage = "Origin-enabled inventory has impossible slots.";
                            return false;
                        }
                        var quantity = Math.Min(remaining, Math.Max(1, itemData.MaxStack));
                        inventory.Slots.Add(new InventorySlotSaveData
                        {
                            SlotIndex = inventory.Slots.Count,
                            ItemId = item.ItemId,
                            Amount = quantity,
                            OriginLots = InvalidOriginLots(quantity)
                        });
                        remaining -= quantity;
                    }
                }
            }
            if (missingMarker)
            {
                foreach (var slot in inventory.Slots)
                    if (slot != null && slot.Amount > 0)
                        slot.OriginLots = InvalidOriginLots(slot.Amount);
            }

            foreach (var item in inventory.Items)
                if (item == null || item.Amount < 0 || string.IsNullOrWhiteSpace(item.ItemId)
                    || _inventoryManager == null
                    || !_inventoryManager.TryGetItemData(item.ItemId, out _))
                {
                    errorMessage = "Origin-enabled inventory has an unknown aggregate item.";
                    return false;
                }

            if (_inventoryManager == null
                || !_inventoryManager.TryValidateRestoreFromSaveData(inventory, true, out errorMessage))
            {
                if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = "Inventory catalog is unavailable for save validation.";
                return false;
            }
            return true;
        }

        private static List<ItemOriginLotSaveData> InvalidOriginLots(int quantity)
            => new List<ItemOriginLotSaveData>
            {
                new ItemOriginLotSaveData { Kind = (int)ItemOriginKind.Invalid, Quantity = quantity }
            };

        private const string SaveBackupSuffix = ".backup";

        /// <summary>
        /// Se <paramref name="path"/> estiver ausente ou seu conteudo nao puder ser
        /// deserializado como <see cref="GameSaveData"/> valido, tenta recuperar de
        /// <c>path + ".backup"</c> (criado por <see cref="SaveBackupService.WriteTextSafely"/> ou por
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
