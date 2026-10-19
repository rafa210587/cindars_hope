using System.Collections.Generic;

namespace CindarsHope.UI.Crafting
{
    /// <summary>
    /// SPEC 04: Material requirement projection for recipe detail.
    /// Shows owned vs required quantities for display and validation.
    /// </summary>
    public class CraftingMaterialRequirementViewModel
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int Required { get; set; }
        public int Owned { get; set; }

        public bool IsSatisfied => Owned >= Required;
        public int Missing => System.Math.Max(0, Required - Owned);

        public string DisplayText => $"{ItemName}: {Owned}/{Required}";
    }

    /// <summary>
    /// Craft quantity calculator - validates material totals for craft-many.
    /// </summary>
    public static class CraftQuantityCalculator
    {
        /// <summary>
        /// Calculate maximum number of times a recipe can be crafted.
        /// Backend owns the actual validation and consumption.
        /// </summary>
        public static int CalculateMaxCrafts(
            List<CraftingMaterialRequirementViewModel> requirements)
        {
            if (requirements == null || requirements.Count == 0)
                return 1;

            int maxCrafts = int.MaxValue;
            foreach (var req in requirements)
            {
                if (req.Required > 0)
                {
                    int craftsFromThisItem = req.Owned / req.Required;
                    maxCrafts = System.Math.Min(maxCrafts, craftsFromThisItem);
                }
            }

            return maxCrafts > 0 ? maxCrafts : 0;
        }

        /// <summary>
        /// Calculate total materials needed for N crafts.
        /// Returns new projection without modifying actual state.
        /// </summary>
        public static List<CraftingMaterialRequirementViewModel> ProjectCraftMany(
            List<CraftingMaterialRequirementViewModel> singleCraftRequirements,
            int craftCount)
        {
            var projected = new List<CraftingMaterialRequirementViewModel>();
            foreach (var req in singleCraftRequirements)
            {
                projected.Add(new CraftingMaterialRequirementViewModel
                {
                    ItemId = req.ItemId,
                    ItemName = req.ItemName,
                    Required = req.Required * craftCount,
                    Owned = req.Owned
                });
            }
            return projected;
        }
    }
}
