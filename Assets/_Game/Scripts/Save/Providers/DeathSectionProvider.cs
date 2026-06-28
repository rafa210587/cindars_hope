using System.Collections.Generic;
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
                deathData.ActiveCorpse = existingSaveData.Death.ActiveCorpse;
            }
            else
            {
                deathData.DeathStats = new DeathStatsSaveData();
            }
            return deathData;
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
                    ReplacedByCorpseId = corpseData.ReplacedByCorpseId
                };

                if (corpseData.LostInventoryItems != null)
                {
                    foreach (var slotData in corpseData.LostInventoryItems)
                    {
                        if (slotData == null || string.IsNullOrWhiteSpace(slotData.ItemId))
                            continue;

                        var corpseItem = new CorpseItem(slotData.ItemId, slotData.Amount)
                        {
                            SourceSlotIndex = slotData.SlotIndex
                        };
                        corpse.InventoryItems.Add(corpseItem);
                    }
                }

                if (corpseData.LostEquipmentItems != null)
                {
                    foreach (var slotData in corpseData.LostEquipmentItems)
                    {
                        if (slotData == null || string.IsNullOrWhiteSpace(slotData.ItemId))
                            continue;

                        var corpseItem = new CorpseItem(slotData.ItemId, slotData.Amount)
                        {
                            SourceSlotIndex = slotData.SlotIndex
                        };
                        corpse.EquipmentItems.Add(corpseItem);
                    }
                }

                _corpseRecoveryManager.SetActiveCorpse(corpse);
                Debug.Log($"[DeathSectionProvider] Restaurado cadáver ativo {corpse.CorpseId} com {corpse.InventoryItems.Count} itens de inventário e {corpse.EquipmentItems.Count} itens de equipamento.");
            }
            else if (_corpseRecoveryManager == null && deathData.ActiveCorpse != null)
            {
                Debug.LogWarning("[DeathSectionProvider] CorpseRecoveryManager não injetado; cadáver ativo do save não será restaurado.");
            }
        }
    }
}
