#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using CindarsHope.Core.Data;
using CindarsHope.Craft.Data;
using CindarsHope.Crafting;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    /// <summary>
    /// fable_49 — gerador ADITIVO das receitas de craft de gear tier alto (gated por receita aprendida)
    /// e das UpgradeRecipeSO de upgrade focado. Espelha CraftingRecipeInitializer: cria/atualiza assets
    /// via AssetDatabase, idempotente (não duplica), e os adiciona ao RecipeDatabase. NÃO mexe em loja
    /// (tier alto NUNCA é vendido). Custo das receitas de craft = material-âncora da banda + ouro
    /// proporcional; saída = itens F32 (stats da matriz canônica, nunca duplicados aqui).
    ///
    /// Execução DEFERIDA (asset-gen humano no Unity Editor). O código compila em Assembly-CSharp-Editor.
    /// </summary>
    public static class HighTierGearRecipeGenerator
    {
        private const string RecipePath = "Assets/_Game/Data/Craft/Recipes";
        private const string UpgradeRecipePath = "Assets/_Game/Data/Craft/UpgradeRecipes";
        private const string RecipeDatabasePath = "Assets/_Game/Data/Registries/RecipeDatabase.asset";

        // (assetName, recipeId, displayName, outputItemId, materialBand, gateUnlockSlug)
        private static readonly (string asset, string id, string name, string output, MaterialBand band, string unlock)[] CraftRecipes =
        {
            // Mithril (gate 60 — Mithril Work)
            ("Recipe_Forge_MithrilSword", "recipe_forge_mithril_sword", "Mithril Sword", "item_weapon_sword_mithril", MaterialBand.Mithril, RecipeUnlock.MithrilWork),
            ("Recipe_Forge_MithrilDagger", "recipe_forge_mithril_dagger", "Mithril Dagger", "item_weapon_dagger_mithril", MaterialBand.Mithril, RecipeUnlock.MithrilWork),
            ("Recipe_Forge_MithrilLightArmor", "recipe_forge_mithril_light_armor", "Mithril Light Armor", "item_armor_light_mithril", MaterialBand.Mithril, RecipeUnlock.MithrilWork),
            // Bromecian (gate 75)
            ("Recipe_Forge_BromecianHammer", "recipe_forge_bromecian_hammer", "Bromecian Hammer", "item_weapon_hammer_bromecian", MaterialBand.Bromecian, RecipeUnlock.BromecianWork),
            ("Recipe_Forge_BromecianSpear", "recipe_forge_bromecian_spear", "Bromecian Spear", "item_weapon_spear_bromecian", MaterialBand.Bromecian, RecipeUnlock.BromecianWork),
            ("Recipe_Forge_BromecianMediumArmor", "recipe_forge_bromecian_medium_armor", "Bromecian Medium Armor", "item_armor_medium_bromecian", MaterialBand.Bromecian, RecipeUnlock.BromecianWork),
            ("Recipe_Forge_BromecianKiteShield", "recipe_forge_bromecian_kite_shield", "Bromecian Kite Shield", "item_shield_bromecian_kite", MaterialBand.Bromecian, RecipeUnlock.BromecianWork),
            // Blackstone / Pedra Negra (gate 90)
            ("Recipe_Forge_BlackstoneSword", "recipe_forge_blackstone_sword", "Blackstone Sword", "item_weapon_sword_blackstone", MaterialBand.Blackstone, RecipeUnlock.BlackstoneWork),
            ("Recipe_Forge_BlackstoneAxe", "recipe_forge_blackstone_axe", "Blackstone Axe", "item_weapon_axe_blackstone", MaterialBand.Blackstone, RecipeUnlock.BlackstoneWork),
            ("Recipe_Forge_BlackstoneHeavyArmor", "recipe_forge_blackstone_heavy_armor", "Blackstone Heavy Armor", "item_armor_heavy_blackstone", MaterialBand.Blackstone, RecipeUnlock.BlackstoneWork),
            // Meteoric / Meteórica (gate 100 — star_iron)
            ("Recipe_Forge_MeteoricSword", "recipe_forge_meteoric_sword", "Meteoric Sword", "item_weapon_sword_meteoric", MaterialBand.Meteoric, RecipeUnlock.MeteoricWork),
            ("Recipe_Forge_MeteoricStaff", "recipe_forge_meteoric_staff", "Meteoric Staff", "item_weapon_staff_meteoric", MaterialBand.Meteoric, RecipeUnlock.MeteoricWork),
            ("Recipe_Forge_MeteoricBow", "recipe_forge_meteoric_bow", "Meteoric Bow", "item_weapon_bow_meteoric", MaterialBand.Meteoric, RecipeUnlock.MeteoricWork),
            ("Recipe_Forge_MeteoricRobe", "recipe_forge_meteoric_robe", "Meteoric Robe", "item_armor_robe_meteoric", MaterialBand.Meteoric, RecipeUnlock.MeteoricWork),
        };

        // (assetName, upgradeId, displayName, band, focus) — um foco por linha de upgrade (§35).
        private static readonly (string asset, string id, string name, MaterialBand band, UpgradeFocus focus)[] UpgradeRecipes =
        {
            ("UpgradeRecipe_Weapon_Damage", "upgrade_weapon_damage", "Weapon Edge (+Damage)", MaterialBand.Mithril, UpgradeFocus.Damage),
            ("UpgradeRecipe_Weapon_Durability", "upgrade_weapon_durability", "Weapon Temper (+Durability)", MaterialBand.Mithril, UpgradeFocus.Durability),
            ("UpgradeRecipe_Weapon_Stamina", "upgrade_weapon_stamina", "Weapon Balance (+Stamina)", MaterialBand.Mithril, UpgradeFocus.Stamina),
            ("UpgradeRecipe_Armor_Durability", "upgrade_armor_durability", "Armor Reinforce (+Durability)", MaterialBand.Bromecian, UpgradeFocus.Durability),
            ("UpgradeRecipe_Armor_Weight", "upgrade_armor_weight", "Armor Lighten (+Weight)", MaterialBand.Bromecian, UpgradeFocus.Weight),
            ("UpgradeRecipe_Shield_Block", "upgrade_shield_block", "Shield Hone (+Block)", MaterialBand.Bromecian, UpgradeFocus.Block),
        };

        [MenuItem("CindarsHope/Crafting/Generate High-Tier Gear Recipes (fable_49)")]
        public static void GenerateHighTierRecipes()
        {
            EnsureDirectory(RecipePath);
            EnsureDirectory(UpgradeRecipePath);

            var craftAssets = new List<RecipeDataSO>();
            foreach (var r in CraftRecipes)
            {
                int bandValue = HighTierGearCanon.MaterialBandValue(r.band);
                string materialId = HighTierGearCanon.MaterialBandItemId(r.band);
                // Custo de craft proporcional à banda: 5× material-âncora + 1 componente raro implícito.
                var ingredients = new[] { new RecipeIngredient(materialId, 5) };
                craftAssets.Add(CreateOrUpdateGatedRecipe(r.asset, r.id, r.name, r.output, ingredients, r.unlock, bandValue));
            }

            var upgradeAssets = new List<UpgradeRecipeSO>();
            foreach (var u in UpgradeRecipes)
            {
                upgradeAssets.Add(CreateOrUpdateUpgradeRecipe(u.asset, u.id, u.name, u.band, u.focus));
            }

            AddRecipesToRegistry(craftAssets);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"fable_49: generated {craftAssets.Count} high-tier craft recipes (gated) + {upgradeAssets.Count} upgrade recipes.");
        }

        private static RecipeDataSO CreateOrUpdateGatedRecipe(
            string assetName, string id, string displayName, string outputItemId,
            RecipeIngredient[] ingredients, string requiredUnlockSlug, int bandValue)
        {
            var path = $"{RecipePath}/{assetName}.asset";
            var recipe = AssetDatabase.LoadAssetAtPath<RecipeDataSO>(path);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<RecipeDataSO>();
                AssetDatabase.CreateAsset(recipe, path);
            }

            recipe.SetId(id);
            recipe.DisplayName = displayName;
            recipe.Description = $"fable_49 high-tier forge recipe: {displayName}. Learned via boss first-kill.";
            recipe.RequiredStationType = WorkshopType.Forge;
            recipe.RequiredWorkshopLevel = 1;
            recipe.CraftTimeSeconds = 6f;
            recipe.OutputItemId = outputItemId;
            recipe.OutputAmount = 1;
            recipe.Ingredients = ingredients;
            recipe.IsUnlockedByDefault = false; // tier alto começa bloqueado
            recipe.RequiredRecipeUnlockId = requiredUnlockSlug;
            recipe.StaminaCost = 0;
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static UpgradeRecipeSO CreateOrUpdateUpgradeRecipe(
            string assetName, string id, string displayName, MaterialBand band, UpgradeFocus focus)
        {
            var path = $"{UpgradeRecipePath}/{assetName}.asset";
            var recipe = AssetDatabase.LoadAssetAtPath<UpgradeRecipeSO>(path);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<UpgradeRecipeSO>();
                AssetDatabase.CreateAsset(recipe, path);
            }

            recipe.SetId(id);
            recipe.DisplayName = displayName;
            recipe.Description = $"fable_49 upgrade: {displayName}. Single focus, +1/+2/+3.";
            recipe.TargetBand = band;
            recipe.Focus = focus;
            recipe.MaxLevel = HighTierGearCanon.MaxUpgradeLevel;
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static void AddRecipesToRegistry(List<RecipeDataSO> recipes)
        {
            var registry = AssetDatabase.LoadAssetAtPath<RecipeDatabaseSO>(RecipeDatabasePath);
            if (registry == null)
            {
                Debug.LogWarning($"fable_49: RecipeDatabase not found at '{RecipeDatabasePath}'. Recipes were created but not registered.");
                return;
            }

            var serialized = new SerializedObject(registry);
            var entries = serialized.FindProperty("_items");
            foreach (var recipe in recipes)
            {
                if (recipe == null || ContainsId(entries, recipe.Id))
                {
                    continue;
                }

                entries.InsertArrayElementAtIndex(entries.arraySize);
                entries.GetArrayElementAtIndex(entries.arraySize - 1).objectReferenceValue = recipe;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
        }

        private static bool ContainsId(SerializedProperty entries, string id)
        {
            for (var index = 0; index < entries.arraySize; index++)
            {
                var recipe = entries.GetArrayElementAtIndex(index).objectReferenceValue as RecipeDataSO;
                if (recipe != null && recipe.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
#endif
