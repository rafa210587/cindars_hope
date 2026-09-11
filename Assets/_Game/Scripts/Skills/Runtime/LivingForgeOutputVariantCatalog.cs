using System;
using System.Collections.Generic;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Skills.Runtime
{
    public enum LivingForgeOutputStage
    {
        Base = 0,
        Quality1 = 1,
        Quality2 = 2,
        Potency = 3,
        Quality2Potency = 4
    }

    public readonly struct LivingForgeOutputVariant
    {
        public readonly string BaseItemId;
        public readonly string ItemId;
        public readonly LivingForgeOutputStage Stage;
        public readonly float ValueMultiplier;
        public readonly float ConsumablePayloadMultiplier;
        public readonly float DurabilityMaxMultiplier;

        public LivingForgeOutputVariant(
            string baseItemId,
            string itemId,
            LivingForgeOutputStage stage,
            float valueMultiplier,
            float consumablePayloadMultiplier,
            float durabilityMaxMultiplier)
        {
            BaseItemId = baseItemId ?? string.Empty;
            ItemId = itemId ?? string.Empty;
            Stage = stage;
            ValueMultiplier = valueMultiplier;
            ConsumablePayloadMultiplier = consumablePayloadMultiplier;
            DurabilityMaxMultiplier = durabilityMaxMultiplier;
        }
    }

    /// <summary>
    /// Stable item identities and balance rules for Living Forge outputs. Variants are separate
    /// item ids, so existing inventory stacking, save, drop, pickup and consume paths retain their
    /// normal item-id semantics without per-stack metadata.
    /// </summary>
    public static class LivingForgeOutputVariantCatalog
    {
        public const string Quality1Suffix = "_living_forge_q1";
        public const string Quality2Suffix = "_living_forge_q2";
        public const string PotencySuffix = "_living_forge_potency";
        public const string Quality2PotencySuffix = "_living_forge_q2_potency";

        public const float Quality1PayloadMultiplier = 1.15f;
        public const float Quality2PayloadMultiplier = 1.35f;
        public const float Quality1DurabilityMaxMultiplier = 1.05f;
        public const float Quality2DurabilityMaxMultiplier = 1.10f;
        public const float PotencyMultiplier = 1.08f;
        public const int MaximumConsumableBatch = 5;

        public static string Quality1Id(string baseItemId) => Compose(baseItemId, Quality1Suffix);
        public static string Quality2Id(string baseItemId) => Compose(baseItemId, Quality2Suffix);
        public static string PotencyId(string baseItemId) => Compose(baseItemId, PotencySuffix);
        public static string Quality2PotencyId(string baseItemId) =>
            Compose(baseItemId, Quality2PotencySuffix);

        public static IEnumerable<string> EnumerateItemFamily(string baseItemId)
        {
            if (string.IsNullOrWhiteSpace(baseItemId) || TryDescribe(baseItemId, out _))
                yield break;

            yield return baseItemId;
            yield return Quality1Id(baseItemId);
            yield return Quality2Id(baseItemId);
            yield return PotencyId(baseItemId);
            yield return Quality2PotencyId(baseItemId);
        }

        public static bool TryDescribe(string itemId, out LivingForgeOutputVariant variant)
        {
            if (TryStripSuffix(itemId, Quality2PotencySuffix, out var baseId))
            {
                variant = Describe(baseId, LivingForgeOutputStage.Quality2Potency);
                return true;
            }

            if (TryStripSuffix(itemId, Quality1Suffix, out baseId))
            {
                variant = Describe(baseId, LivingForgeOutputStage.Quality1);
                return true;
            }

            if (TryStripSuffix(itemId, Quality2Suffix, out baseId))
            {
                variant = Describe(baseId, LivingForgeOutputStage.Quality2);
                return true;
            }

            if (TryStripSuffix(itemId, PotencySuffix, out baseId))
            {
                variant = Describe(baseId, LivingForgeOutputStage.Potency);
                return true;
            }

            variant = default;
            return false;
        }

        public static bool TryResolveQualityUpgrade(
            string currentItemId,
            out LivingForgeOutputVariant variant)
        {
            variant = default;
            if (string.IsNullOrWhiteSpace(currentItemId))
                return false;

            if (!TryDescribe(currentItemId, out var current))
            {
                variant = Describe(currentItemId, LivingForgeOutputStage.Quality1);
                return true;
            }

            if (current.Stage != LivingForgeOutputStage.Quality1)
                return false;

            variant = Describe(current.BaseItemId, LivingForgeOutputStage.Quality2);
            return true;
        }

        public static bool TryResolvePotencyVariant(
            string baseItemId,
            ItemDataSO baseItem,
            int batchAmount,
            out LivingForgeOutputVariant variant)
        {
            variant = default;
            if (baseItem == null
                || string.IsNullOrWhiteSpace(baseItemId)
                || !string.Equals(baseItem.Id, baseItemId, StringComparison.Ordinal)
                || TryDescribe(baseItemId, out _)
                || batchAmount <= 0
                || !IsEligibleConsumable(baseItem, batchAmount, PotencyMultiplier))
            {
                return false;
            }

            variant = Describe(baseItemId, LivingForgeOutputStage.Potency);
            return true;
        }

        public static bool TryResolveQuality2PotencyVariant(
            string baseItemId,
            ItemDataSO baseItem,
            int batchAmount,
            out LivingForgeOutputVariant variant)
        {
            variant = default;
            if (baseItem == null
                || string.IsNullOrWhiteSpace(baseItemId)
                || !string.Equals(baseItem.Id, baseItemId, StringComparison.Ordinal)
                || TryDescribe(baseItemId, out _)
                || batchAmount <= 0
                || !IsEligibleConsumable(baseItem, batchAmount,
                    Quality2PayloadMultiplier * PotencyMultiplier))
            {
                return false;
            }

            variant = Describe(baseItemId, LivingForgeOutputStage.Quality2Potency);
            return true;
        }

        public static LivingForgeOutputVariant Describe(string baseItemId, LivingForgeOutputStage stage)
        {
            if (string.IsNullOrWhiteSpace(baseItemId) || TryDescribe(baseItemId, out _))
                throw new ArgumentException("A base Living Forge item id must be non-empty and unmodified.", nameof(baseItemId));

            return stage switch
            {
                LivingForgeOutputStage.Quality1 => new LivingForgeOutputVariant(
                    baseItemId, Quality1Id(baseItemId), stage,
                    1f, Quality1PayloadMultiplier,
                    Quality1DurabilityMaxMultiplier),
                LivingForgeOutputStage.Quality2 => new LivingForgeOutputVariant(
                    baseItemId, Quality2Id(baseItemId), stage,
                    1f, Quality2PayloadMultiplier,
                    Quality2DurabilityMaxMultiplier),
                LivingForgeOutputStage.Potency => new LivingForgeOutputVariant(
                    baseItemId, PotencyId(baseItemId), stage,
                    1f, PotencyMultiplier, 1f),
                LivingForgeOutputStage.Quality2Potency => new LivingForgeOutputVariant(
                    baseItemId, Quality2PotencyId(baseItemId), stage,
                    1f, Quality2PayloadMultiplier * PotencyMultiplier,
                    Quality2DurabilityMaxMultiplier),
                _ => new LivingForgeOutputVariant(baseItemId, baseItemId,
                    LivingForgeOutputStage.Base, 1f, 1f, 1f)
            };
        }

        public static int ScaleInteger(int baseValue, float multiplier)
        {
            if (baseValue <= 0)
                return 0;

            decimal authoredMultiplier = Convert.ToDecimal(multiplier);
            return (int)Math.Round(baseValue * authoredMultiplier, 0,
                MidpointRounding.AwayFromZero);
        }

        public static bool IsConsumable(ItemDataSO item)
        {
            if (item == null)
                return false;

            return item.Category == ItemCategory.Food
                || item.Category == ItemCategory.Consumable
                || item.ConsumableSubtype != ConsumableSubtype.None;
        }

        public static bool IsEligibleConsumable(
            ItemDataSO item, int batchAmount, float payloadMultiplier)
        {
            if (!IsConsumable(item) || batchAmount <= 0 ||
                batchAmount > MaximumConsumableBatch || payloadMultiplier <= 1f)
                return false;

            return ScaleInteger(item.HungerRestore, payloadMultiplier) > item.HungerRestore
                || ScaleInteger(item.StaminaRestore, payloadMultiplier) > item.StaminaRestore
                || ScaleInteger(item.DurabilityRestoreAmount, payloadMultiplier) >
                   item.DurabilityRestoreAmount;
        }

        public static bool IsEligibleEquipment(
            ItemDataSO item, int outputAmount, int baseDurability)
            => item != null && item.IsEquippable && item.MaxStack == 1 &&
               outputAmount == 1 && baseDurability > 0;

        public static bool IsPotentialEquipment(ItemDataSO item)
            => item != null && item.IsEquippable && item.MaxStack == 1;

        private static string Compose(string baseItemId, string suffix)
        {
            if (string.IsNullOrWhiteSpace(baseItemId))
                return string.Empty;
            return baseItemId + suffix;
        }

        private static bool TryStripSuffix(string itemId, string suffix, out string baseItemId)
        {
            baseItemId = string.Empty;
            if (string.IsNullOrWhiteSpace(itemId)
                || !itemId.EndsWith(suffix, StringComparison.Ordinal)
                || itemId.Length == suffix.Length)
            {
                return false;
            }

            baseItemId = itemId.Substring(0, itemId.Length - suffix.Length);
            return !string.IsNullOrWhiteSpace(baseItemId);
        }
    }
}
