using System;
using System.Collections.Generic;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Skills.Runtime
{
    public interface ICommonHarvestItemPolicy
    {
        bool IsExplicitlyCommon(string itemId);
    }

    public sealed class ExplicitCommonHarvestItemPolicy : ICommonHarvestItemPolicy
    {
        private readonly HashSet<string> _commonItemIds;

        public ExplicitCommonHarvestItemPolicy(IEnumerable<string> commonItemIds)
        {
            _commonItemIds = new HashSet<string>(StringComparer.Ordinal);
            if (commonItemIds == null)
            {
                return;
            }

            foreach (var itemId in commonItemIds)
            {
                if (!string.IsNullOrWhiteSpace(itemId))
                {
                    _commonItemIds.Add(itemId);
                }
            }
        }

        public bool IsExplicitlyCommon(string itemId) =>
            !string.IsNullOrWhiteSpace(itemId) && _commonItemIds.Contains(itemId);
    }

    public sealed class ItemDatabaseCommonHarvestItemPolicy : ICommonHarvestItemPolicy
    {
        private readonly ItemDatabaseSO _itemDatabase;

        public ItemDatabaseCommonHarvestItemPolicy(ItemDatabaseSO itemDatabase)
        {
            _itemDatabase = itemDatabase;
        }

        public bool IsExplicitlyCommon(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId)
                && _itemDatabase != null
                && _itemDatabase.TryGetById(itemId, out var item)
                && CraftingPassiveConsumers.IsCommonIngredient(item);
        }
    }

    public readonly struct MaterialEyeHarvestRoll
    {
        internal MaterialEyeHarvestRoll(string bonusItemId, CraftingPassiveRoll roll)
        {
            BonusItemId = bonusItemId;
            Roll = roll;
            IsPrepared = true;
        }

        public bool IsPrepared { get; }
        public string BonusItemId { get; }
        public CraftingPassiveRoll Roll { get; }
    }

    /// <summary>
    /// Adapta o bônus do Olho de Material ao ledger seeded. Elegibilidade é deny-by-default:
    /// somente IDs afirmados por uma policy explícita podem receber a unidade adicional.
    /// </summary>
    public sealed class MaterialEyeHarvestConsumer
    {
        public const string OperationKind = "material_eye_harvest";

        private readonly ICommonHarvestItemPolicy _commonItemPolicy;

        public MaterialEyeHarvestConsumer(ICommonHarvestItemPolicy commonItemPolicy)
        {
            _commonItemPolicy = commonItemPolicy;
        }

        public bool TryPrepare(
            CraftingPassiveRngState rngState,
            float chance01,
            string harvestInstanceId,
            IEnumerable<string> candidateItemIds,
            out MaterialEyeHarvestRoll prepared)
        {
            prepared = default;
            if (rngState == null || chance01 <= 0f || _commonItemPolicy == null ||
                string.IsNullOrWhiteSpace(harvestInstanceId) || candidateItemIds == null)
            {
                return false;
            }

            string bonusItemId = string.Empty;
            foreach (var itemId in candidateItemIds)
            {
                if (_commonItemPolicy.IsExplicitlyCommon(itemId))
                {
                    bonusItemId = itemId;
                    break;
                }
            }

            if (string.IsNullOrEmpty(bonusItemId) ||
                !rngState.TryPrepareRoll(OperationKind, harvestInstanceId, chance01, out var roll))
            {
                return false;
            }

            prepared = new MaterialEyeHarvestRoll(bonusItemId, roll);
            return true;
        }

        public bool Commit(
            CraftingPassiveRngState rngState,
            MaterialEyeHarvestRoll prepared,
            bool harvestCommitted,
            bool successfulBonusAdded)
        {
            if (rngState == null || !prepared.IsPrepared || !harvestCommitted)
            {
                return false;
            }

            // Once the base harvest commits, the attempt is consumed even if the optional
            // bonus could not fit. Reusing PlotId on a later crop must never replay a winner.
            return rngState.Commit(prepared.Roll);
        }
    }
}
