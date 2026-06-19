using System;
using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using UnityEngine;

namespace CindarsHope.Loot
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "CindarsHope/Data/Loot Table")]
    public class LootTableSO : ScriptableObject, IIdentifiedData
    {
        // fable_06 (aditivo): id estável da tabela (= EnemyDataSO.lootTableId). Permite indexar a
        // tabela num LootTableDatabaseSO sem FindObjectOfType. Vazio em assets legados (sem dano).
        public string TableId;

        public LootTableEntry[] Entries;
        public EquipmentLootEntry[] EquipmentEntries;

        string IIdentifiedData.Id => TableId;

        // fable_06 (aditivo, defaults neutros): metadados de tabela por família de inimigo.
        // FamilyId é puramente informativo/diagnóstico (ex.: "beast", "undead"). Não muda o
        // comportamento legado de TryRoll; consumido por EnemyLootResolver para logs e validator.
        [Header("Enemy Family Table (fable_06)")]
        [Tooltip("Família-base do catálogo a que esta tabela pertence (informativo). Ex.: beast, undead, construct.")]
        public string FamilyId;

        [Tooltip("Drops garantidos (rolam SEMPRE, antes das entradas ponderadas). Use para o material " +
                 "comum da família. DropChance é ignorado aqui — sempre cai dentro do MinAmount..MaxAmount.")]
        public LootTableEntry[] GuaranteedEntries = new LootTableEntry[0];

        [Tooltip("Essência elemental da banda (canon ITEM_CATALOG §10): 8% comum / +25% elite / 100% miniboss/boss. " +
                 "Vazio = sem essência nesta família.")]
        public string EssenceItemId;
        [Range(0f, 1f)] public float EssenceCommonChance = 0.08f;
        [Range(0f, 1f)] public float EssenceEliteBonusChance = 0.25f;

        public bool TryRoll(out string itemId, out int amount)
        {
            itemId = string.Empty;
            amount = 0;

            if (Entries == null || Entries.Length == 0)
            {
                return false;
            }

            var totalWeight = 0;
            foreach (var entry in Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId) || entry.Weight <= 0)
                {
                    continue;
                }

                totalWeight += entry.Weight;
            }

            if (totalWeight <= 0)
            {
                return false;
            }

            var roll = UnityEngine.Random.Range(1, totalWeight + 1);
            var cursor = 0;
            foreach (var entry in Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId) || entry.Weight <= 0)
                {
                    continue;
                }

                cursor += entry.Weight;
                if (roll > cursor)
                {
                    continue;
                }

                itemId = entry.ItemId;
                amount = UnityEngine.Random.Range(Mathf.Max(1, entry.MinAmount), Mathf.Max(entry.MinAmount, entry.MaxAmount) + 1);
                return amount > 0;
            }

            return false;
        }

        public bool TryRollEquipment(out EquipmentLootData equipmentData)
        {
            equipmentData = null;

            if (EquipmentEntries == null || EquipmentEntries.Length == 0)
            {
                return false;
            }

            var totalWeight = 0;
            foreach (var entry in EquipmentEntries)
            {
                if (entry == null || entry.EquipmentReference == null || entry.Weight <= 0)
                {
                    continue;
                }

                totalWeight += entry.Weight;
            }

            if (totalWeight <= 0)
            {
                return false;
            }

            var roll = UnityEngine.Random.Range(1, totalWeight + 1);
            var cursor = 0;
            foreach (var entry in EquipmentEntries)
            {
                if (entry == null || entry.EquipmentReference == null || entry.Weight <= 0)
                {
                    continue;
                }

                cursor += entry.Weight;
                if (roll > cursor)
                {
                    continue;
                }

                equipmentData = new EquipmentLootData
                {
                    ItemInstanceId = System.Guid.NewGuid().ToString(),
                    ItemId = entry.EquipmentReference.Id,
                    DurabilityCurrent = entry.EquipmentReference.DurabilityMax,
                    DurabilityMax = entry.EquipmentReference.DurabilityMax,
                    IsBroken = false
                };
                return true;
            }

            return false;
        }
    }

    [Serializable]
    public class LootTableEntry
    {
        public string ItemId;
        public int MinAmount = 1;
        public int MaxAmount = 1;
        public int Weight = 1;
        public string[] RequiredTags;

        // fable_06 (aditivo): chance independente de a entrada dropar quando selecionada/garantida.
        // Default 1.0 = sempre dropa (preserva o comportamento ponderado legado de TryRoll, que
        // ignora este campo). EnemyLootResolver respeita DropChance para drops raros por família.
        [Range(0f, 1f)] public float DropChance = 1f;

        // fable_06 (aditivo): marca um drop como raro (apenas diagnóstico/validator/log).
        public bool IsRare;
    }

    [Serializable]
    public class EquipmentLootEntry
    {
        public EquipmentDataSO EquipmentReference;
        public int Weight = 1;
    }

    [Serializable]
    public class EquipmentLootData
    {
        public string ItemInstanceId;
        public string ItemId;
        public int DurabilityCurrent;
        public int DurabilityMax;
        public bool IsBroken;
    }
}
