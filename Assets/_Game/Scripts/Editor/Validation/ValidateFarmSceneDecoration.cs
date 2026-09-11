using CindarsHope.Editor.Art;
using CindarsHope.Farm.Scene;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Validates the deterministic decoration plan, independent of a regenerated scene.</summary>
    public static class ValidateFarmSceneDecoration
    {
        [MenuItem("CindarsHope/Validar Decoracao FarmScene")]
        public static void ValidateFromMenu()
        {
            var errors = ValidateConfiguration(out var validBiomes, out var forbiddenPlacements);
            Debug.Log("ValidateFarmSceneDecoration: Forbidden decoration placements: " + forbiddenPlacements);
            Debug.Log("ValidateFarmSceneDecoration: Decoration biomes valid: " + validBiomes + "/6");
            Debug.Log("ValidateFarmSceneDecoration: " + errors + " error(s)");
        }

        public static int ValidateConfiguration(out int validBiomes, out int forbiddenPlacements)
        {
            var plan = FarmDecorationPlanner.Plan();
            var counts = new int[6];
            forbiddenPlacements = 0;
            for (var i = 0; i < plan.Count; i++)
            {
                if (FarmDecorationPlanner.IsForbiddenCell(plan[i].Position)) forbiddenPlacements++;
                counts[(int)plan[i].Biome]++;
            }

            var errors = forbiddenPlacements;
            validBiomes = 0;
            foreach (FarmDecorationBiome biome in System.Enum.GetValues(typeof(FarmDecorationBiome)))
            {
                if (FarmDecorationPlanner.IsCountInDeclaredRange(biome, counts[(int)biome])) { validBiomes++; continue; }
                FarmDecorationPlanner.GetDeclaredRange(biome, out var minimum, out var maximum);
                Debug.LogError("ValidateFarmSceneDecoration: " + biome + " count=" + counts[(int)biome] + " expected=" + minimum + ".." + maximum);
                errors++;
            }

            return errors;
        }
    }
}
