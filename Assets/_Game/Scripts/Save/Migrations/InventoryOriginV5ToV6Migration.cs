using System;
using System.Collections.Generic;
using CindarsHope.Craft;
using CindarsHope.Economy;
using CindarsHope.Farm.Processing;
using CindarsHope.Farm.Shipping;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Save.Migrations
{
    /// <summary>Finite legacy exception. Only a genuine v5 header may create LegacyUntracked lots.</summary>
    public sealed class InventoryOriginV5ToV6Migration : ISaveMigration
    {
        public string MigrationId => "save_v5_to_v6_item_origin";
        public int SourceSchemaVersion => 5;
        public int TargetSchemaVersion => 6;

        public bool TryMigrate(SaveMigrationContext context, out string migratedJson,
            out string errorMessage)
        {
            migratedJson = string.Empty;
            errorMessage = string.Empty;
            if (context == null || context.SourceSchemaVersion != 5
                || context.TargetSchemaVersion != 6 || string.IsNullOrWhiteSpace(context.RawJson))
            { errorMessage = "Only a v5→v6 migration context is supported."; return false; }

            GameSaveData save;
            try { save = JsonUtility.FromJson<GameSaveData>(context.RawJson); }
            catch (Exception exception)
            { errorMessage = $"Invalid v5 JSON: {exception.Message}"; return false; }
            if (save == null || save.SchemaVersion != 5)
            { errorMessage = "Source save does not have a genuine v5 header."; return false; }

            try
            {
                MigrateInventory(save);
                MigratePickups(save);
                MigrateShipping(save);
                MigrateCorpse(save);
                MigrateCrafting(save);
                MigrateProcessing(save);
                MigrateInfusions(save);
                MigrateBookOrders(save);
                save.SchemaVersion = 6;
                migratedJson = JsonUtility.ToJson(save, true);
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"v5 origin migration rejected impossible data: {exception.Message}";
                return false;
            }
        }

        private static void MigrateInventory(GameSaveData save)
        {
            if (save.Inventory == null) save.Inventory = new InventorySaveData();
            var inventory = save.Inventory;
            if (inventory.Slots == null) inventory.Slots = new List<InventorySlotSaveData>();
            if (inventory.Slots.Count == 0 && inventory.Items != null && inventory.Items.Count > 0)
                throw new InvalidOperationException("Inventory items exist without v5 slots.");
            var occupied = new HashSet<int>();
            foreach (var slot in inventory.Slots)
            {
                if (slot == null || slot.SlotIndex < 0 || slot.SlotIndex >= InventoryManager.MaxCapacity
                    || !occupied.Add(slot.SlotIndex))
                    throw new InvalidOperationException("Invalid or duplicate inventory slot.");
                MigrateSlot(slot, inventory.OriginSchemaVersion);
            }
            inventory.OriginSchemaVersion = 1;
        }

        private static void MigratePickups(GameSaveData save)
        {
            if (save.World?.Pickups == null) return;
            var indices = new HashSet<int>();
            var dynamicIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var pickup in save.World.Pickups)
            {
                if (pickup == null || string.IsNullOrWhiteSpace(pickup.ItemId)
                    || pickup.Amount <= 0 || pickup.PickupIndex < 0
                    || !indices.Add(pickup.PickupIndex))
                    throw new InvalidOperationException("Invalid world pickup.");
                // v5 dynamic drops used indices >=10000, before IsDynamic/SceneName existed.
                // Authored FarmScene pickups use index 0 (confirmed in scene YAML).
                if (pickup.PickupIndex >= 10000 && !pickup.IsDynamic)
                    pickup.IsDynamic = true;
                if (pickup.IsDynamic)
                {
                    if (string.IsNullOrWhiteSpace(pickup.SceneName))
                        pickup.SceneName = ResolveLegacyDropScene(save);
                    if (string.IsNullOrWhiteSpace(pickup.RuntimeGeneratedPickupId))
                        pickup.RuntimeGeneratedPickupId = $"legacy-drop-{pickup.PickupIndex}";
                    if (!dynamicIds.Add(pickup.RuntimeGeneratedPickupId))
                        throw new InvalidOperationException("Duplicate dynamic pickup ID.");
                }
                pickup.OriginLots = PreserveOrLegacy(pickup.OriginSchemaVersion,
                    pickup.OriginLots, pickup.Amount);
                pickup.OriginSchemaVersion = 1;
            }
        }

        private static string ResolveLegacyDropScene(GameSaveData save)
        {
            if (!string.IsNullOrWhiteSpace(save.CurrentSceneName)) return save.CurrentSceneName;
            if (!string.IsNullOrWhiteSpace(save.CurrentScenePath))
            {
                var path = save.CurrentScenePath.Replace('\\', '/');
                var name = path.Substring(path.LastIndexOf('/') + 1);
                if (name.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                    name = name.Substring(0, name.Length - 6);
                if (!string.IsNullOrWhiteSpace(name)) return name;
            }
            // Old saves without scene metadata came from the farm baseline.
            return "FarmScene";
        }

        private static void MigrateShipping(GameSaveData save)
        {
            if (save.Farm?.PendingShipping?.Entries == null) return;
            var actualIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in save.Farm.PendingShipping.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.EntryId)
                    || !actualIds.Add(entry.EntryId))
                    throw new InvalidOperationException("Missing or duplicate shipping entry ID.");
            }
            var splitIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in save.Farm.PendingShipping.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId)
                    || entry.Quantity <= 0)
                    throw new InvalidOperationException("Invalid pending shipping entry.");
                entry.OriginLots = PreserveOrLegacy(entry.OriginSchemaVersion,
                    entry.OriginLots, entry.Quantity);
                if (entry.State == (int)ShippingEntryState.Pending)
                {
                    var lots = ItemOriginLot.FromSaveData(entry.OriginLots, entry.Quantity);
                    var hasValid = false;
                    var hasInvalid = false;
                    foreach (var lot in lots)
                    {
                        if (lot.Kind == ItemOriginKind.Invalid) hasInvalid = true;
                        else hasValid = true;
                    }
                    if (hasValid && hasInvalid)
                    {
                        var splitId = entry.EntryId + "_invalid";
                        if (actualIds.Contains(splitId) || !splitIds.Add(splitId))
                            throw new InvalidOperationException("Shipping split ID collision.");
                    }
                }
                entry.OriginSchemaVersion = 1;
            }
        }

        private static void MigrateCorpse(GameSaveData save)
        {
            var corpse = save.Death?.ActiveCorpse;
            if (corpse == null) return;
            MigrateSlots(corpse.LostInventoryItems, corpse.OriginSchemaVersion);
            MigrateSlots(corpse.LostEquipmentItems, corpse.OriginSchemaVersion);
            corpse.OriginSchemaVersion = 1;
        }

        private static void MigrateSlots(IReadOnlyList<InventorySlotSaveData> slots, int schema)
        {
            if (slots == null) return;
            foreach (var slot in slots) MigrateSlot(slot, schema);
        }

        private static void MigrateSlot(InventorySlotSaveData slot, int schema)
        {
            if (slot == null || string.IsNullOrWhiteSpace(slot.ItemId) || slot.Amount <= 0)
                throw new InvalidOperationException("Invalid saved item slot.");
            slot.OriginLots = PreserveOrLegacy(schema, slot.OriginLots, slot.Amount);
        }

        private static void MigrateCrafting(GameSaveData save)
        {
            if (save.Crafting?.Stations == null) return;
            var stationIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var station in save.Crafting.Stations)
            {
                if (station == null || string.IsNullOrWhiteSpace(station.StationInstanceId)
                    || !stationIds.Add(station.StationInstanceId))
                    throw new InvalidOperationException("Missing or duplicate crafting station ID.");
                var job = station.Job;
                if (job == null) continue;
                if (string.IsNullOrWhiteSpace(job.RecipeId) || string.IsNullOrWhiteSpace(job.OutputItemId)
                    || job.OutputAmount <= 0)
                    throw new InvalidOperationException("Invalid crafting job.");
                job.OutputOriginLots = PreserveOrLegacy(job.OriginSchemaVersion,
                    job.OutputOriginLots, job.OutputAmount);
                if (job.IngredientsConsumed == null)
                    throw new InvalidOperationException("Crafting job has no consumed ingredients.");
                foreach (var ingredient in job.IngredientsConsumed)
                {
                    if (ingredient == null || string.IsNullOrWhiteSpace(ingredient.ItemId)
                        || ingredient.Amount <= 0)
                        throw new InvalidOperationException("Invalid crafting ingredient.");
                    ingredient.OriginLots = PreserveOrLegacy(job.OriginSchemaVersion,
                        ingredient.OriginLots, ingredient.Amount);
                }
                job.OriginSchemaVersion = 1;
            }
        }

        private static void MigrateProcessing(GameSaveData save)
        {
            if (save.Farm?.Processing?.Jobs == null) return;
            var stationIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var job in save.Farm.Processing.Jobs)
            {
                if (job == null || !ProcessingRecipeCatalog.TryGetById(job.RecipeId, out var recipe)
                    || recipe == null || job.StationId != recipe.StationId
                    || !stationIds.Add(job.StationId))
                    throw new InvalidOperationException("Unknown processing job or station.");
                var outputQuantity = job.OutputQuantity > 0 ? job.OutputQuantity : recipe.OutputQuantity;
                if (outputQuantity <= 0) throw new InvalidOperationException("Invalid processing output.");
                job.OutputQuantity = outputQuantity;
                job.InputOriginLots = PreserveOrLegacy(job.OriginSchemaVersion,
                    job.InputOriginLots, recipe.InputQuantity);
                job.OutputOriginLots = PreserveOrLegacy(job.OriginSchemaVersion,
                    job.OutputOriginLots, outputQuantity);
                job.OriginSchemaVersion = 1;
            }
        }

        private static void MigrateInfusions(GameSaveData save)
        {
            // Equipment remains the single owner of historical and current infusion state.
            var source = save.Equipment?.Infusions;
            if (source == null) return;
            var instanceIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var infusion in source)
            {
                if (infusion == null || !TemperingCanon.IsValidTier(infusion.InfusionTier)
                    || TemperingCanon.ParseElement(infusion.InfusionElement) == TemperingElement.None
                    || string.IsNullOrWhiteSpace(infusion.ItemInstanceId)
                    || !instanceIds.Add(infusion.ItemInstanceId))
                    throw new InvalidOperationException("Invalid infusion.");
                var quantity = TemperingCanon.ElementEssenceCostForTier(infusion.InfusionTier);
                infusion.ConsumedEssenceLots = PreserveOrLegacy(
                    infusion.ConsumedEssenceOriginSchemaVersion,
                    infusion.ConsumedEssenceLots, quantity);
                infusion.ConsumedEssenceOriginSchemaVersion = 1;
            }
        }

        private static void MigrateBookOrders(GameSaveData save)
        {
            if (save.NpcServices?.PendingBookOrders == null) return;
            var usedOrderIds = new HashSet<int>();
            foreach (var order in save.NpcServices.PendingBookOrders)
            {
                if (order == null) throw new InvalidOperationException("Null paid order.");
                if (order.OrderId > 0 && !usedOrderIds.Add(order.OrderId))
                    throw new InvalidOperationException("Duplicate paid order ID.");
            }
            var nextOrderId = 1;
            foreach (var order in save.NpcServices.PendingBookOrders)
            {
                if (string.IsNullOrWhiteSpace(order.DeliveredBookItemId)
                    || order.PaidGold < 0)
                    throw new InvalidOperationException("Invalid pending paid order.");
                if (order.OrderId <= 0)
                {
                    while (usedOrderIds.Contains(nextOrderId))
                    {
                        if (nextOrderId == int.MaxValue)
                            throw new InvalidOperationException("No available paid order ID.");
                        nextOrderId++;
                    }
                    order.OrderId = nextOrderId;
                    usedOrderIds.Add(nextOrderId);
                }
                order.OriginLots = PreserveOrLegacy(order.OriginSchemaVersion,
                    order.OriginLots, 1);
                // v5 did not record historical cost; never invent it from today's catalog price.
                if (order.OriginSchemaVersion != 1) order.PaidGold = 0;
                order.OriginSchemaVersion = 1;
            }
        }

        private static List<ItemOriginLotSaveData> PreserveOrLegacy(int schema,
            IReadOnlyList<ItemOriginLotSaveData> saved, int quantity)
        {
            if (quantity <= 0) throw new InvalidOperationException("Nonpositive origin quantity.");
            if (schema != 1) return new List<ItemOriginLotSaveData>
                { new ItemOriginLotSaveData { Kind = (int)ItemOriginKind.LegacyUntracked,
                    Quantity = quantity } };
            var lots = ItemOriginLot.FromSaveData(saved, quantity);
            var result = new List<ItemOriginLotSaveData>();
            foreach (var lot in lots) result.Add(lot.ToSaveData());
            return result;
        }
    }
}
