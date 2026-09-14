using System;
using System.Collections.Generic;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Player.Death;
using UnityEngine;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do estado de morte do jogador (estatísticas de morte, cadáver ativo).
    /// Fonte do estado: <see cref="CorpseRecoveryManager"/> injetado via constructor.
    /// Dados de morte são preservados do save existente (seção aditiva/cumulativa).
    /// Fallback: seção de morte vazia em saves legados.
    /// </summary>
    public class DeathSectionProvider : ISaveSectionProvider
    {
        private readonly CorpseRecoveryManager _corpseRecoveryManager;

        public DeathSectionProvider(CorpseRecoveryManager corpseRecoveryManager)
        {
            _corpseRecoveryManager = corpseRecoveryManager;
        }

        public string ProviderId => "death";

        public object Capture(GameSaveData existingSaveData)
        {
            var deathData = new DeathSaveData();
            if (existingSaveData?.Death != null)
            {
                deathData.DeathStats = existingSaveData.Death.DeathStats ?? new DeathStatsSaveData();
            }
            else
            {
                deathData.DeathStats = new DeathStatsSaveData();
            }
            var liveCorpse = _corpseRecoveryManager?.ActiveCorpse;
            if (liveCorpse != null)
                deathData.ActiveCorpse = CaptureCorpse(liveCorpse);
            else if (_corpseRecoveryManager == null)
                deathData.ActiveCorpse = existingSaveData?.Death?.ActiveCorpse;
            return deathData;
        }

        private static PlayerCorpseRecoverySaveData CaptureCorpse(Corpse corpse)
        {
            var data = new PlayerCorpseRecoverySaveData
            {
                CorpseId = corpse.CorpseId,
                CorpseStatusValue = (int)corpse.Status,
                RunId = corpse.RunId,
                CaveSeed = corpse.CaveSeed,
                CaveLevel = corpse.CaveLevel,
                SceneName = corpse.SceneName,
                Position = corpse.Position,
                GoldAmount = corpse.GoldAmount,
                CreatedAtGameDay = corpse.CreatedAtGameDay,
                CreatedAtGameTime = corpse.CreatedAtGameTime,
                RecoveredAtGameDay = corpse.RecoveredAtGameDay,
                ReplacedByCorpseId = corpse.ReplacedByCorpseId,
                SnapshotLayoutHash = corpse.SnapshotLayoutHash,
                SafeAnchorId = corpse.SafeAnchorId,
                OriginSchemaVersion = 1
            };
            foreach (var item in corpse.InventoryItems)
            {
                var saved = CaptureItem(item);
                if (item.SourceSlotType > (int)EquipmentSlot.None
                    && Enum.IsDefined(typeof(EquipmentSlot), item.SourceSlotType))
                    saved.EquipmentBindingId = item.SourceSlotType.ToString();
                data.LostInventoryItems.Add(saved);
            }
            foreach (var item in corpse.EquipmentItems)
            {
                var saved = CaptureItem(item);
                saved.EquipmentBindingId = item.SourceSlotType.ToString();
                data.LostEquipmentItems.Add(saved);
            }
            return data;
        }

        private static InventorySlotSaveData CaptureItem(CorpseItem item)
        {
            var data = new InventorySlotSaveData
            {
                SlotIndex = item.SourceSlotIndex,
                ItemId = item.ItemId,
                ItemInstanceId = item.ItemInstanceId,
                Amount = item.Amount,
                OriginLots = new List<ItemOriginLotSaveData>()
            };
            if (item.OriginLots != null)
                foreach (var lot in item.OriginLots)
                    if (lot != null) data.OriginLots.Add(new ItemOriginLotSaveData
                    {
                        Kind = lot.Kind,
                        Quantity = lot.Quantity,
                        UnitPaidBasisGold = lot.UnitPaidBasisGold,
                        UnitResaleCapGold = lot.UnitResaleCapGold
                    });
            return data;
        }

        public void Restore(object sectionData)
        {
            var deathData = sectionData as DeathSaveData;
            if (deathData == null)
            {
                return;
            }

            if (_corpseRecoveryManager != null && deathData.ActiveCorpse != null)
            {
                var corpseData = deathData.ActiveCorpse;
                var corpse = new Corpse(corpseData.CorpseId)
                {
                    Status = (CorpseStatus)corpseData.CorpseStatusValue,
                    RunId = corpseData.RunId,
                    CaveSeed = corpseData.CaveSeed,
                    CaveLevel = corpseData.CaveLevel,
                    SceneName = corpseData.SceneName,
                    Position = corpseData.Position,
                    GoldAmount = corpseData.GoldAmount,
                    CreatedAtGameDay = corpseData.CreatedAtGameDay,
                    CreatedAtGameTime = corpseData.CreatedAtGameTime,
                    RecoveredAtGameDay = corpseData.RecoveredAtGameDay,
                    ReplacedByCorpseId = corpseData.ReplacedByCorpseId,
                    SnapshotLayoutHash = corpseData.SnapshotLayoutHash,
                    SafeAnchorId = corpseData.SafeAnchorId
                };

                if (corpseData.LostInventoryItems != null)
                {
                    foreach (var slotData in corpseData.LostInventoryItems)
                    {
                        if (slotData == null || string.IsNullOrWhiteSpace(slotData.ItemId))
                            continue;

                        var corpseItem = RestoreItem(slotData, corpseData.OriginSchemaVersion);
                        if (int.TryParse(slotData.EquipmentBindingId, out var stackBinding)
                            && stackBinding > (int)EquipmentSlot.None
                            && Enum.IsDefined(typeof(EquipmentSlot), stackBinding))
                            corpseItem.SourceSlotType = stackBinding;
                        corpse.InventoryItems.Add(corpseItem);
                    }
                }

                if (corpseData.LostEquipmentItems != null)
                {
                    foreach (var slotData in corpseData.LostEquipmentItems)
                    {
                        if (slotData == null || string.IsNullOrWhiteSpace(slotData.ItemId))
                            continue;

                        var corpseItem = RestoreItem(slotData, corpseData.OriginSchemaVersion);
                        if (int.TryParse(slotData.EquipmentBindingId, out var sourceSlotType)
                            && sourceSlotType != (int)EquipmentSlot.None
                            && Enum.IsDefined(typeof(EquipmentSlot), sourceSlotType))
                        {
                            corpseItem.SourceSlotType = sourceSlotType;
                            corpse.EquipmentItems.Add(corpseItem);
                        }
                        else
                        {
                            // A v5 slot index is an inventory position, never an equipment binding.
                            corpseItem.SourceSlotType = -1;
                            corpse.InventoryItems.Add(corpseItem);
                        }
                    }
                }

                _corpseRecoveryManager.SetActiveCorpse(corpse);
                Debug.Log($"[DeathSectionProvider] Restaurado cadáver ativo {corpse.CorpseId} com {corpse.InventoryItems.Count} itens de inventário e {corpse.EquipmentItems.Count} itens de equipamento.");
            }
            else if (_corpseRecoveryManager == null && deathData.ActiveCorpse != null)
            {
                Debug.LogWarning("[DeathSectionProvider] CorpseRecoveryManager não injetado; cadáver ativo do save não será restaurado.");
            }
            else if (_corpseRecoveryManager != null && deathData.ActiveCorpse == null)
                _corpseRecoveryManager.SetActiveCorpse(null);
        }

        private static CorpseItem RestoreItem(InventorySlotSaveData data, int schemaVersion)
        {
            var lots = schemaVersion == 1
                ? ItemOriginLot.FromSaveData(data.OriginLots, data.Amount)
                : ItemOriginLot.Invalid(data.Amount);
            var item = new CorpseItem(data.ItemId, data.Amount)
            {
                ItemInstanceId = data.ItemInstanceId ?? string.Empty,
                SourceSlotIndex = data.SlotIndex,
                OriginSchemaVersion = 1,
                OriginLots = new List<ItemOriginLotSaveData>()
            };
            foreach (var lot in lots) item.OriginLots.Add(lot.ToSaveData());
            return item;
        }
    }
}
