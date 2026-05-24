using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.UI.Crafting
{
    [DisallowMultipleComponent]
    public sealed class CraftingModal : MonoBehaviour
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

        private static readonly string[] Actions = { "Craft", "Cancel", "Collect", "Close" };

        public bool IsOpen => _isOpen;

        private void Update()
        {
            if (!_isOpen)
            {
                if (Input.GetKeyDown(_pocketCraftKey) && (_modalManager == null || !_modalManager.HasActiveModal))
                {
                    Open(_runtime != null ? _runtime.GetPocketStation() : null);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveSelection(-1);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveSelection(1);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _selectedActionIndex = (_selectedActionIndex + Actions.Length - 1) % Actions.Length;
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                _selectedActionIndex = (_selectedActionIndex + 1) % Actions.Length;
            }
            else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                ExecuteSelectedAction();
            }
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

                    _runtime.TryStartCraft(_station, _recipes[_selectedRecipeIndex], out _feedback);
                    break;
                case "Cancel":
                    _runtime.TryCancel(_station, out _feedback);
                    break;
                case "Collect":
                    _runtime.TryCollect(_station, out _feedback);
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
        }

        private void OnGUI()
        {
            if (!_isOpen || _station == null)
            {
                return;
            }

            var rect = new Rect((Screen.width - 620f) * 0.5f, (Screen.height - 430f) * 0.5f, 620f, 430f);
            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label($"Crafting - {_station.StationType} ({_station.StationInstanceId})");
            GUILayout.Label("W/S: recipe  A/D: action  E/Enter: confirm  Esc: close  C: pocket crafting");
            GUILayout.Space(8f);

            if (_recipes.Count == 0)
            {
                GUILayout.Label("No compatible recipes available.");
            }
            else
            {
                for (var index = 0; index < _recipes.Count; index++)
                {
                    var recipe = _recipes[index];
                    var prefix = index == _selectedRecipeIndex ? "> " : "  ";
                    GUILayout.Label($"{prefix}{recipe.DisplayName} -> {recipe.OutputItemId} x{recipe.OutputAmount} ({recipe.CraftTimeSeconds:0.0}s)");
                }

                var selected = _recipes[_selectedRecipeIndex];
                GUILayout.Space(8f);
                GUILayout.Label($"Ingredients: {FormatIngredients(selected)}");
            }

            GUILayout.Space(8f);
            GUILayout.Label(_station.HasActiveJob
                ? $"Job in progress: {_station.Job.RemainingSeconds:0.0}s remaining"
                : _station.HasCompletedOutput ? "Output completed and waiting for collection." : "Station idle.");
            GUILayout.BeginHorizontal();
            for (var index = 0; index < Actions.Length; index++)
            {
                GUILayout.Label(index == _selectedActionIndex ? $"[{Actions[index]}]" : Actions[index], GUILayout.Width(120f));
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
                parts.Add($"{ingredient.ItemId} x{ingredient.Amount}");
            }

            return string.Join(", ", parts);
        }
    }
}
