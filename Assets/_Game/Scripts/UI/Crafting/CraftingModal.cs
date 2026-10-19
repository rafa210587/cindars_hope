using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Craft.Events;
using CindarsHope.Foundation;
using CindarsHope.Skills.Runtime;
using CindarsHope.UI.Modal;
using UnityEngine;
using CindarsHope.Localization;
using CindarsHope.Core.Bootstrap;
using CindarsHope.UI.Shop;

namespace CindarsHope.UI.Crafting
{
    [DisallowMultipleComponent]
    public sealed class CraftingModal : MonoBehaviour, ICraftingStationModal
    {
        [SerializeField] private CraftingRuntime _runtime;
        [SerializeField] private ModalManager _modalManager;
        [SerializeField] private KeyCode _pocketCraftKey = KeyCode.C;

        private readonly List<RecipeDataSO> _recipes = new List<RecipeDataSO>();
        private CraftingStation _station;
        private int _selectedRecipeIndex;
        private int _selectedActionIndex;
        private bool _isOpen;
        private string _feedback = string.Empty;
        private LivingForgeBenefitChoice _livingForgeChoice;
        private int _selectedCommonMaterialIndex;
        private Vector2 _recipeScroll;

        private static readonly string[] Actions = { "Craft", "Cancel", "Collect", "Close" };
        private static readonly KeyCode[] NavigationKeys =
        {
            KeyCode.Escape,
            KeyCode.W, KeyCode.UpArrow,
            KeyCode.S, KeyCode.DownArrow,
            KeyCode.A, KeyCode.LeftArrow,
            KeyCode.D, KeyCode.RightArrow,
            KeyCode.Q, KeyCode.R,
            KeyCode.E, KeyCode.Return, KeyCode.Space
        };

        public bool IsOpen => _isOpen;

        private void OnEnable()
        {
            GameEventBus.Subscribe<OpenCraftingStationRequestedEvent>(HandleOpenStationRequested);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<OpenCraftingStationRequestedEvent>(HandleOpenStationRequested);
            Close();
        }

        // Estação física pediu para abrir o craft (apertou E numa forja/alambique/tear…). Resolve a estação
        // pelo id+tipo no runtime e abre o craft filtrado por aquele WorkshopType.
        private void HandleOpenStationRequested(OpenCraftingStationRequestedEvent evt)
        {
            if (_runtime == null || _isOpen)
            {
                return;
            }

            Open(_runtime.GetOrCreateStation(evt.StationInstanceId, evt.WorkshopType));
        }

        private void Update()
        {
            if (!_isOpen)
            {
                if (global::UnityEngine.Input.GetKeyDown(_pocketCraftKey) && (_modalManager == null || !_modalManager.HasActiveModal))
                {
                    Open(_runtime != null ? _runtime.GetPocketStation() : null);
                }

                return;
            }

            var key = ReadNavigationKey();
            if (key == KeyCode.Escape)
            {
                Close();
                return;
            }

            if (key == KeyCode.W || key == KeyCode.UpArrow)
            {
                MoveSelection(-1);
            }
            else if (key == KeyCode.S || key == KeyCode.DownArrow)
            {
                MoveSelection(1);
            }
            else if (key == KeyCode.A || key == KeyCode.LeftArrow)
            {
                _selectedActionIndex = (_selectedActionIndex + Actions.Length - 1) % Actions.Length;
            }
            else if (key == KeyCode.D || key == KeyCode.RightArrow)
            {
                _selectedActionIndex = (_selectedActionIndex + 1) % Actions.Length;
            }
            else if (key == KeyCode.Q)
            {
                CycleLivingForgeChoice();
            }
            else if (key == KeyCode.R)
            {
                CycleLivingForgeMaterial();
            }
            else if (key == KeyCode.E || key == KeyCode.Return || key == KeyCode.Space)
            {
                ExecuteSelectedAction();
            }
        }

        private static KeyCode ReadNavigationKey()
        {
            foreach (var key in NavigationKeys)
            {
                if (global::UnityEngine.Input.GetKeyDown(key)) return key;
            }

            return KeyCode.None;
        }

        public void Configure(CraftingRuntime runtime, ModalManager modalManager)
        {
            _runtime = runtime;
            _modalManager = modalManager;
        }

        public void Open(CraftingStation station)
        {
            if (_runtime == null || station == null)
            {
                return;
            }

            if (_modalManager != null && !_modalManager.PushModal(ModalType.Crafting))
            {
                return;
            }

            _station = station;
            _recipes.Clear();
            _recipes.AddRange(_runtime.GetRecipesForStation(station.StationType));
            _selectedRecipeIndex = 0;
            _selectedActionIndex = 0;
            _feedback = string.Empty;
            ResetLivingForgeSelection();
            _isOpen = true;
            GameEventBus.Publish(new CraftingStationOpenedEvent(station.StationInstanceId));
        }

        public void Close()
        {
            if (!_isOpen)
            {
                return;
            }

            var stationId = _station != null ? _station.StationInstanceId : string.Empty;
            _isOpen = false;
            _station = null;
            _modalManager?.TryPopModal(ModalType.Crafting, out _);
            GameEventBus.Publish(new CraftingStationClosedEvent(stationId));
        }

        private void ExecuteSelectedAction()
        {
            if (_station == null || _runtime == null)
            {
                return;
            }

            switch (Actions[_selectedActionIndex])
            {
                case "Craft":
                    if (_recipes.Count == 0)
                    {
                        _feedback = "No compatible recipes.";
                        return;
                    }

                    var recipe = _recipes[_selectedRecipeIndex];
                    if (!TryBuildLivingForgeSelection(
                            _runtime.LivingForgeRank,
                            _runtime.IsLivingForgeChargeAvailable,
                            _livingForgeChoice,
                            _selectedCommonMaterialIndex,
                            recipe,
                            _runtime.IsLivingForgeCommonIngredient,
                            out var selection,
                            out _feedback))
                    {
                        return;
                    }

                    if (_runtime.TryStartCraft(_station, recipe, selection, out _feedback))
                        _feedback = _station.HasActiveJob
                            ? string.Format(LocalizationService.Get("ui.crafting.started"), ItemName(recipe.OutputItemId))
                            : string.Format(LocalizationService.Get("ui.crafting.created"), ItemName(recipe.OutputItemId), recipe.OutputAmount);
                    else _feedback = PresentFailure(_feedback);
                    break;
                case "Cancel":
                    if (_runtime.TryCancel(_station, out _feedback)) _feedback = LocalizationService.Get("ui.crafting.cancelled");
                    else _feedback = PresentFailure(_feedback);
                    break;
                case "Collect":
                    if (_runtime.TryCollect(_station, out _feedback)) _feedback = LocalizationService.Get("ui.crafting.collected");
                    else _feedback = PresentFailure(_feedback);
                    break;
                default:
                    Close();
                    break;
            }
        }

        private void MoveSelection(int direction)
        {
            if (_recipes.Count == 0)
            {
                return;
            }

            _selectedRecipeIndex = (_selectedRecipeIndex + direction + _recipes.Count) % _recipes.Count;
            ResetLivingForgeSelection();
        }

        private void OnGUI()
        {
            if (!_isOpen || _station == null)
            {
                return;
            }

            CindarsHope.Core.MenuGuiStyle.Apply();
            var rect = new Rect((Screen.width - 620f) * 0.5f, (Screen.height - 430f) * 0.5f, 620f, 430f);
            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label(LocalizationService.Get("ui.crafting.title"));
            GUILayout.Label(LocalizationService.Get("ui.crafting.controls"));
            GUILayout.Space(8f);

            if (_recipes.Count == 0)
            {
                GUILayout.Label(LocalizationService.Get("ui.crafting.empty"));
            }
            else
            {
                _recipeScroll = GUILayout.BeginScrollView(_recipeScroll, GUILayout.Height(135f));
                for (var index = 0; index < _recipes.Count; index++)
                {
                    var recipe = _recipes[index];
                    var prefix = index == _selectedRecipeIndex ? "> " : "  ";
                    if (GUILayout.Button($"{prefix}{ItemName(recipe.OutputItemId)} ×{recipe.OutputAmount} ({recipe.CraftTimeSeconds:0.0}s)"))
                    { _selectedRecipeIndex = index; ResetLivingForgeSelection(); }
                }

                GUILayout.EndScrollView();
                var selected = _recipes[_selectedRecipeIndex];
                GUILayout.Space(8f);
                GUILayout.Label(string.Format(LocalizationService.Get("ui.crafting.ingredients"), FormatIngredients(selected)));
                DrawLivingForgeSelection(selected);
            }

            GUILayout.Space(8f);
            GUILayout.Label(_station.HasActiveJob
                ? string.Format(LocalizationService.Get("ui.crafting.remaining"), _station.Job.RemainingSeconds)
                : LocalizationService.Get(_station.HasCompletedOutput ? "ui.crafting.ready" : "ui.crafting.idle"));
            GUILayout.BeginHorizontal();
            for (var index = 0; index < Actions.Length; index++)
            {
                var label = LocalizationService.Get("ui.crafting.action_" + Actions[index].ToLowerInvariant());
                if (GUILayout.Button(index == _selectedActionIndex ? $"[{label}]" : label, GUILayout.Width(140f)))
                { _selectedActionIndex = index; ExecuteSelectedAction(); }
            }

            GUILayout.EndHorizontal();
            if (!string.IsNullOrWhiteSpace(_feedback))
            {
                GUILayout.Space(8f);
                GUILayout.Label(_feedback);
            }

            GUILayout.EndArea();
        }

        private static string FormatIngredients(RecipeDataSO recipe)
        {
            if (recipe.Ingredients == null || recipe.Ingredients.Length == 0)
            {
                return "None";
            }

            var parts = new List<string>();
            foreach (var ingredient in recipe.Ingredients)
            {
                parts.Add($"{ItemName(ingredient.ItemId)} ×{ingredient.Amount}");
            }

            return string.Join(", ", parts);
        }

        private static string ItemName(string id)
        {
            var inventory = GameBootstrap.Instance?.InventoryManager as CindarsHope.Inventory.InventoryManager;
            return inventory != null && inventory.TryGetItemData(id, out var item)
                ? ItemDisplayNameFormatter.GetFullName(item) : LocalizationService.Get("ui.item.unknown");
        }

        public static string PresentFailure(string reason)
        {
            if (reason == "Only an in-progress job can be cancelled.") return LocalizationService.Get("ui.crafting.cancel_unavailable");
            if (reason == "No completed output to collect.") return LocalizationService.Get("ui.crafting.collect_unavailable");
            if (reason != null && reason.StartsWith("Missing ingredient ")) return LocalizationService.Get("ui.crafting.material_missing");
            if (reason != null && reason.StartsWith("Inventory is full")) return LocalizationService.Get("ui.crafting.inventory_full");
            if (reason == "Not enough stamina.") return LocalizationService.Get("ui.crafting.stamina_missing");
            if (reason == "Station already has a pending job or output.") return LocalizationService.Get("ui.crafting.busy");
            return LocalizationService.Get(reason);
        }

        public static bool TryBuildLivingForgeSelection(
            int rank,
            bool chargeAvailable,
            LivingForgeBenefitChoice choice,
            int selectedCommonMaterialIndex,
            RecipeDataSO recipe,
            System.Func<string, bool> isCommonIngredient,
            out LivingForgeCraftSelection selection,
            out string failureReason)
        {
            selection = default;
            if (rank <= 0 || !chargeAvailable)
            {
                failureReason = string.Empty;
                return true;
            }

            if (choice == LivingForgeBenefitChoice.None)
            {
                failureReason = string.Empty;
                return true;
            }

            if (choice == LivingForgeBenefitChoice.Quality)
            {
                selection = new LivingForgeCraftSelection(choice);
                failureReason = string.Empty;
                return true;
            }

            if (rank == 1)
            {
                failureReason = "Living Forge rank one only offers Quality.";
                return false;
            }

            var commonMaterials = CollectEligibleCommonMaterials(recipe, isCommonIngredient);
            if (commonMaterials.Count == 0)
            {
                failureReason = "This recipe has no eligible common material to save.";
                return false;
            }

            int index = Mathf.Clamp(selectedCommonMaterialIndex, 0,
                commonMaterials.Count - 1);
            selection = new LivingForgeCraftSelection(
                LivingForgeBenefitChoice.SaveCommonMaterial, commonMaterials[index]);
            failureReason = string.Empty;
            return true;
        }

        public static List<string> CollectEligibleCommonMaterials(
            RecipeDataSO recipe,
            System.Func<string, bool> isCommonIngredient)
        {
            var result = new List<string>();
            if (recipe?.Ingredients == null || isCommonIngredient == null)
                return result;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (string.IsNullOrWhiteSpace(ingredient.ItemId) || ingredient.Amount <= 1 ||
                    !isCommonIngredient(ingredient.ItemId) || result.Contains(ingredient.ItemId))
                    continue;
                result.Add(ingredient.ItemId);
            }

            return result;
        }

        private void CycleLivingForgeChoice()
        {
            if (_runtime == null || _runtime.LivingForgeRank <= 0 ||
                !_runtime.IsLivingForgeChargeAvailable)
                return;

            if (_runtime.LivingForgeRank == 1)
            {
                _livingForgeChoice = _livingForgeChoice == LivingForgeBenefitChoice.None
                    ? LivingForgeBenefitChoice.Quality
                    : LivingForgeBenefitChoice.None;
                _feedback = string.Empty;
                return;
            }

            _livingForgeChoice = _livingForgeChoice switch
            {
                LivingForgeBenefitChoice.None => LivingForgeBenefitChoice.Quality,
                LivingForgeBenefitChoice.Quality => LivingForgeBenefitChoice.SaveCommonMaterial,
                _ => LivingForgeBenefitChoice.None
            };
            _selectedCommonMaterialIndex = 0;
            _feedback = string.Empty;
        }

        private void CycleLivingForgeMaterial()
        {
            if (_runtime == null || _recipes.Count == 0 ||
                _livingForgeChoice != LivingForgeBenefitChoice.SaveCommonMaterial)
                return;

            var materials = CollectEligibleCommonMaterials(
                _recipes[_selectedRecipeIndex], _runtime.IsLivingForgeCommonIngredient);
            if (materials.Count > 0)
                _selectedCommonMaterialIndex =
                    (_selectedCommonMaterialIndex + 1) % materials.Count;
        }

        private void DrawLivingForgeSelection(RecipeDataSO recipe)
        {
            if (_runtime == null || _runtime.LivingForgeRank <= 0)
                return;

            if (!_runtime.IsLivingForgeChargeAvailable)
            {
                GUILayout.Label("Living Forge: daily charge already used or reserved.");
                return;
            }

            string choice = _livingForgeChoice switch
            {
                LivingForgeBenefitChoice.Quality => "Quality",
                LivingForgeBenefitChoice.SaveCommonMaterial => "Save 1 common material",
                _ => "Do not use (craft normally)"
            };
            GUILayout.Label($"Living Forge R{_runtime.LivingForgeRank}: {choice} (Q: choose)");
            if (_livingForgeChoice != LivingForgeBenefitChoice.SaveCommonMaterial)
                return;

            var materials = CollectEligibleCommonMaterials(
                recipe, _runtime.IsLivingForgeCommonIngredient);
            string material = materials.Count > 0
                ? materials[Mathf.Clamp(_selectedCommonMaterialIndex, 0, materials.Count - 1)]
                : "none eligible";
            GUILayout.Label($"Common material: {material} (R: next)");
        }

        private void ResetLivingForgeSelection()
        {
            _livingForgeChoice = LivingForgeBenefitChoice.None;
            _selectedCommonMaterialIndex = 0;
        }
    }
}
