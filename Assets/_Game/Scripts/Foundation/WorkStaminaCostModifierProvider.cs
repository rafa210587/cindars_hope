using System;

namespace CindarsHope.Foundation
{
    public enum WorkStaminaChannel
    {
        Agricultural = 0,
        Crafting = 1
    }

    public interface IWorkStaminaCostModifierRuntime
    {
        int PreviewCost(WorkStaminaChannel channel, int baseCost,
            float worldX, float worldY, string stationInstanceId = "");

        void CommitSpend(WorkStaminaChannel channel, int baseCost, int chargedCost,
            float worldX, float worldY, string stationInstanceId = "");
    }

    /// <summary>
    /// Shared seam for agricultural and workshop stamina costs. Preview is side-effect free;
    /// fractional credit advances only after the real stamina transaction succeeds.
    /// </summary>
    public static class WorkStaminaCostModifierProvider
    {
        public static IWorkStaminaCostModifierRuntime Source;

        public static int PreviewCost(WorkStaminaChannel channel, int baseCost,
            float worldX, float worldY, string stationInstanceId = "")
        {
            int safeBaseCost = Math.Max(0, baseCost);
            if (Source == null) return safeBaseCost;
            return Math.Max(0, Math.Min(safeBaseCost,
                Source.PreviewCost(channel, safeBaseCost, worldX, worldY,
                    stationInstanceId ?? string.Empty)));
        }

        public static void CommitSpend(WorkStaminaChannel channel, int baseCost,
            int chargedCost, float worldX, float worldY, string stationInstanceId = "")
        {
            Source?.CommitSpend(channel, Math.Max(0, baseCost), Math.Max(0, chargedCost),
                worldX, worldY, stationInstanceId ?? string.Empty);
        }
    }

    /// <summary>Deterministic integer charge with a residual fractional discount.</summary>
    public sealed class FractionalWorkStaminaCredit
    {
        private const double Epsilon = 0.000001d;
        private double _credit;

        public float Credit => (float)_credit;

        public int PreviewCost(int baseCost, float reductionFraction)
        {
            int safeBaseCost = Math.Max(0, baseCost);
            double reduction = Math.Max(0d, Math.Min(1d, reductionFraction));
            int discount = Math.Min(safeBaseCost,
                (int)Math.Floor(_credit + safeBaseCost * reduction + Epsilon));
            return safeBaseCost - discount;
        }

        public void CommitSpend(int baseCost, int chargedCost, float reductionFraction)
        {
            int safeBaseCost = Math.Max(0, baseCost);
            int safeChargedCost = Math.Max(0, Math.Min(safeBaseCost, chargedCost));
            double reduction = Math.Max(0d, Math.Min(1d, reductionFraction));
            _credit = Math.Max(0d, _credit + safeBaseCost * reduction -
                (safeBaseCost - safeChargedCost));
            _credit -= Math.Floor(_credit + Epsilon);
            if (_credit < Epsilon) _credit = 0d;
        }

        public void Reset() => _credit = 0d;
    }
}
