using System;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Skills.Runtime
{
    public enum LivingForgeBenefitChoice
    {
        None = 0,
        Quality = 1,
        SaveCommonMaterial = 2
    }

    public readonly struct LivingForgeCraftSelection
    {
        public LivingForgeCraftSelection(
            LivingForgeBenefitChoice choice,
            string commonIngredientItemId = "")
        {
            Choice = choice;
            CommonIngredientItemId = commonIngredientItemId ?? string.Empty;
        }

        public LivingForgeBenefitChoice Choice { get; }
        public string CommonIngredientItemId { get; }
        public bool IsSelected => Choice != LivingForgeBenefitChoice.None;
    }

    public readonly struct LivingForgePreparation
    {
        public LivingForgePreparation(
            LivingForgeBenefitChoice choice,
            string outputItemId,
            int outputAmount,
            string savedCommonIngredientItemId,
            float durabilityMultiplier)
        {
            Choice = choice;
            OutputItemId = outputItemId ?? string.Empty;
            OutputAmount = outputAmount;
            SavedCommonIngredientItemId = savedCommonIngredientItemId ?? string.Empty;
            DurabilityMultiplier = durabilityMultiplier;
        }

        public LivingForgeBenefitChoice Choice { get; }
        public string OutputItemId { get; }
        public int OutputAmount { get; }
        public string SavedCommonIngredientItemId { get; }
        public float DurabilityMultiplier { get; }
    }

    /// <summary>Pure eligibility and single-benefit policy for Thoren's Living Forge capstone.</summary>
    public static class LivingForgeCapstoneResolver
    {
        public const string NodeId = "crafting_capstone_master_artisan";
        public const float RankThreeDurabilityMultiplier = 1.08f;

        public static int ClampRank(int rank) => Math.Max(0, Math.Min(3, rank));

        public static bool TryPrepare(
            int rank,
            LivingForgeCraftSelection selection,
            ItemDataSO outputItem,
            int outputAmount,
            int outputBaseDurability,
            out LivingForgePreparation preparation,
            out string failureReason)
        {
            preparation = default;
            rank = ClampRank(rank);
            if (rank <= 0)
            {
                failureReason = "Living Forge is not unlocked.";
                return false;
            }

            if (!selection.IsSelected)
            {
                failureReason = "Choose one Living Forge benefit before crafting.";
                return false;
            }

            if (outputItem == null || string.IsNullOrWhiteSpace(outputItem.Id) || outputAmount <= 0)
            {
                failureReason = "Craft output is unavailable for Living Forge.";
                return false;
            }

            switch (selection.Choice)
            {
                case LivingForgeBenefitChoice.Quality:
                    float qualityPayloadMultiplier = rank == 1
                        ? LivingForgeOutputVariantCatalog.Quality1PayloadMultiplier
                        : LivingForgeOutputVariantCatalog.Quality2PayloadMultiplier;
                    bool qualityGear = LivingForgeOutputVariantCatalog.IsEligibleEquipment(
                        outputItem, outputAmount, outputBaseDurability);
                    bool qualityConsumable = LivingForgeOutputVariantCatalog.IsEligibleConsumable(
                        outputItem, outputAmount, qualityPayloadMultiplier);
                    if (!qualityGear && !qualityConsumable)
                    {
                        failureReason = "Living Forge quality requires durable non-stackable gear or a consumable payload batch of at most five.";
                        return false;
                    }

                    if (!TryResolveQuality(rank, outputItem.Id, out var qualityVariant))
                    {
                        failureReason = "Craft output cannot gain another quality stage.";
                        return false;
                    }

                    if (rank == 3 && !TryApplyRankThreeExtra(
                            outputItem, outputAmount, outputBaseDurability,
                            qualityVariant.ItemId, qualitySelected: true,
                            out preparation, out failureReason))
                        return false;

                    if (rank == 3)
                    {
                        failureReason = string.Empty;
                        return true;
                    }

                    preparation = new LivingForgePreparation(selection.Choice,
                        qualityVariant.ItemId, outputAmount, string.Empty, 1f);
                    failureReason = string.Empty;
                    return true;

                case LivingForgeBenefitChoice.SaveCommonMaterial when rank >= 2:
                    if (string.IsNullOrWhiteSpace(selection.CommonIngredientItemId))
                    {
                        failureReason = "Choose one common ingredient to save.";
                        return false;
                    }

                    if (rank == 3 && !TryApplyRankThreeExtra(
                            outputItem, outputAmount, outputBaseDurability,
                            outputItem.Id, qualitySelected: false,
                            out preparation, out failureReason,
                            selection.CommonIngredientItemId))
                        return false;

                    if (rank == 3)
                    {
                        failureReason = string.Empty;
                        return true;
                    }

                    preparation = new LivingForgePreparation(selection.Choice, outputItem.Id,
                        outputAmount, selection.CommonIngredientItemId, 1f);
                    failureReason = string.Empty;
                    return true;

                default:
                    failureReason = "The selected Living Forge benefit is unavailable at this rank.";
                    return false;
            }
        }

        private static bool TryResolveQuality(
            int rank,
            string currentItemId,
            out LivingForgeOutputVariant variant)
        {
            variant = default;
            if (rank == 1)
                return LivingForgeOutputVariantCatalog.TryResolveQualityUpgrade(
                    currentItemId, out variant);

            if (!LivingForgeOutputVariantCatalog.TryDescribe(currentItemId, out var current))
            {
                variant = LivingForgeOutputVariantCatalog.Describe(
                    currentItemId, LivingForgeOutputStage.Quality2);
                return true;
            }

            if (current.Stage != LivingForgeOutputStage.Quality1)
                return false;

            variant = LivingForgeOutputVariantCatalog.Describe(
                current.BaseItemId, LivingForgeOutputStage.Quality2);
            return true;
        }

        private static bool TryApplyRankThreeExtra(
            ItemDataSO outputItem,
            int outputAmount,
            int outputBaseDurability,
            string qualityOutputItemId,
            bool qualitySelected,
            out LivingForgePreparation preparation,
            out string failureReason,
            string savedCommonIngredientItemId = "")
        {
            var choice = qualitySelected
                ? LivingForgeBenefitChoice.Quality
                : LivingForgeBenefitChoice.SaveCommonMaterial;
            if (LivingForgeOutputVariantCatalog.IsEligibleEquipment(
                    outputItem, outputAmount, outputBaseDurability))
            {
                preparation = new LivingForgePreparation(choice, qualityOutputItemId,
                    outputAmount, savedCommonIngredientItemId,
                    RankThreeDurabilityMultiplier);
                failureReason = string.Empty;
                return true;
            }

            LivingForgeOutputVariant potencyVariant;
            bool resolved = qualitySelected
                ? LivingForgeOutputVariantCatalog.TryResolveQuality2PotencyVariant(
                    outputItem.Id, outputItem, outputAmount, out potencyVariant)
                : LivingForgeOutputVariantCatalog.TryResolvePotencyVariant(
                    outputItem.Id, outputItem, outputAmount, out potencyVariant);
            if (resolved)
            {
                preparation = new LivingForgePreparation(choice, potencyVariant.ItemId,
                    outputAmount, savedCommonIngredientItemId, 1f);
                failureReason = string.Empty;
                return true;
            }

            if (!qualitySelected)
            {
                preparation = new LivingForgePreparation(choice, outputItem.Id,
                    outputAmount, savedCommonIngredientItemId, 1f);
                failureReason = string.Empty;
                return true;
            }

            preparation = default;
            failureReason = "Rank three quality requires durable gear or a scalable consumable payload batch of at most five.";
            return false;
        }
    }
}
