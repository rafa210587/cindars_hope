using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Craft.Data;
using CindarsHope.Crafting;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Craft
{
    [DisallowMultipleComponent]
    public class CraftingManager : MonoBehaviour
    {
        // arch: Core|Craft (spec_arch_core_craft_cycle_reduction_v32) — self-registro estatico,
        // molde Audio/AudioManager.cs; GameBootstrap nao segura mais [SerializeField] deste manager.
        private static CraftingManager _instance;
        public static CraftingManager Instance => _instance;

        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private RecipeDatabaseSO _recipeDatabase;

        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            IsInitialized = false;
        }

        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            if (inventoryManager == null)
            {
                Debug.LogWarning("CraftingManager received null InventoryManager for rebind.", this);
                return;
            }

            _inventoryManager = inventoryManager;
        }

        public bool CanCraft(string recipeId)
        {
            return TryGetRecipe(recipeId, out var recipe)
                && HasRequiredIngredients(recipe)
                && HasOutputCapacity(recipe);
        }

        public bool TryCraft(string recipeId)
        {
            if (!TryGetRecipe(recipeId, out var recipe))
            {
                return false;
            }

            if (!HasRequiredIngredients(recipe))
            {
                Debug.Log($"Crafting failed for '{recipeId}': insufficient ingredients.", this);
                return false;
            }

            if (!HasOutputCapacity(recipe))
            {
                Debug.Log($"Crafting failed for '{recipeId}': no inventory capacity for output '{recipe.OutputItemId}'.", this);
                return false;
            }

            if (!RemoveIngredients(recipe))
            {
                Debug.LogWarning($"Crafting failed for '{recipeId}': ingredients could not be removed after validation.", this);
                return false;
            }

            if (!_inventoryManager.AddItem(recipe.OutputItemId, recipe.OutputAmount))
            {
                RestoreIngredients(recipe);
                Debug.LogWarning($"Crafting failed for '{recipeId}': output could not be added. Ingredients were restored.", this);
                return false;
            }

            GameEventBus.Publish(new ItemCraftedEvent(recipe.Id, recipe.OutputItemId, recipe.OutputAmount));
            Debug.Log($"Crafted '{recipe.OutputItemId}' x{recipe.OutputAmount} using recipe '{recipe.Id}'.", this);
            return true;
        }

        public List<RecipeIngredient> GetMissingIngredients(string recipeId)
        {
            var missing = new List<RecipeIngredient>();
            if (!TryGetRecipe(recipeId, out var recipe) || recipe.Ingredients == null)
            {
                return missing;
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                if (string.IsNullOrWhiteSpace(ingredient.ItemId) || ingredient.Amount <= 0)
                {
                    continue;
                }

                var currentAmount = _inventoryManager != null ? _inventoryManager.GetAmount(ingredient.ItemId) : 0;
                if (currentAmount < ingredient.Amount)
                {
                    missing.Add(new RecipeIngredient(ingredient.ItemId, ingredient.Amount - currentAmount));
                }
            }

            return missing;
        }

        private bool TryGetRecipe(string recipeId, out RecipeDataSO recipe)
        {
            recipe = null;
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                Debug.LogWarning("CraftingManager rejected empty recipe id.", this);
                return false;
            }

            if (_inventoryManager == null)
            {
                Debug.LogWarning("CraftingManager cannot craft because InventoryManager is missing.", this);
                return false;
            }

            if (_recipeDatabase == null)
            {
                Debug.LogWarning("CraftingManager cannot craft because RecipeDatabaseSO is missing.", this);
                return false;
            }

            if (!_recipeDatabase.TryGetById(recipeId, out recipe) || recipe == null)
            {
                Debug.LogWarning($"CraftingManager could not resolve recipe id '{recipeId}'.", this);
                return false;
            }

            if (recipe.RequiredStationType != WorkshopType.None)
            {
                Debug.LogWarning($"CraftingManager rejected station recipe '{recipeId}' outside a physical workstation.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(recipe.OutputItemId) || recipe.OutputAmount <= 0)
            {
                Debug.LogWarning($"CraftingManager rejected invalid output data for recipe '{recipeId}'.", this);
                return false;
            }

            // fable_49: gating por receita aprendida. Receita com RequiredRecipeUnlockId só é aceita se o
            // jogador a aprendeu (first-kill do boss de gate). Slug vazio = sem gating (receitas atuais ok).
            if (!string.IsNullOrWhiteSpace(recipe.RequiredRecipeUnlockId)
                && !CraftingRecipeGate.IsRecipeUnlocked(recipe.RequiredRecipeUnlockId))
            {
                Debug.Log($"CraftingManager rejected locked recipe '{recipeId}': requires unlock '{recipe.RequiredRecipeUnlockId}'.", this);
                return false;
            }

            return true;
        }

        private bool HasRequiredIngredients(RecipeDataSO recipe)
        {
            if (recipe.Ingredients == null || recipe.Ingredients.Length == 0)
            {
                Debug.LogWarning($"CraftingManager recipe '{recipe.Id}' has no ingredients.", this);
                return false;
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                if (string.IsNullOrWhiteSpace(ingredient.ItemId) || ingredient.Amount <= 0)
                {
                    Debug.LogWarning($"CraftingManager recipe '{recipe.Id}' has an invalid ingredient.", this);
                    return false;
                }

                if (!_inventoryManager.HasItem(ingredient.ItemId, ingredient.Amount))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasOutputCapacity(RecipeDataSO recipe)
        {
            if (!_inventoryManager.TryGetItemData(recipe.OutputItemId, out var outputItem) || outputItem == null)
            {
                Debug.LogWarning($"CraftingManager output item id '{recipe.OutputItemId}' is unknown.", this);
                return false;
            }

            var projectedOutputAmount = _inventoryManager.GetAmount(recipe.OutputItemId) + recipe.OutputAmount;
            if (recipe.Ingredients != null)
            {
                foreach (var ingredient in recipe.Ingredients)
                {
                    if (ingredient.ItemId == recipe.OutputItemId)
                    {
                        projectedOutputAmount -= ingredient.Amount;
                    }
                }
            }

            return projectedOutputAmount <= outputItem.MaxStack;
        }

        private bool RemoveIngredients(RecipeDataSO recipe)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                if (!_inventoryManager.RemoveItem(ingredient.ItemId, ingredient.Amount))
                {
                    return false;
                }
            }

            return true;
        }

        private void RestoreIngredients(RecipeDataSO recipe)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                _inventoryManager.AddItem(ingredient.ItemId, ingredient.Amount);
            }
        }
    }
}
