using CindarsHope.Inventory;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Core.Time;
using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.UI
{
    [DisallowMultipleComponent]
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private HungerManager _hungerManager;
        [SerializeField] private InteractionSystem _interactionSystem;
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private SaveManager _saveManager;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 360f, Screen.height - 24f), GUI.skin.box);
            GUILayout.Label("Cindar's Hope - Debug HUD");
            DrawPlayerState();
            DrawHungerState();
            DrawWorldState();
            DrawInteractionState();
            DrawInventory();
            DrawCommands();
            GUILayout.EndArea();
        }

        private void DrawWorldState()
        {
            if (_timeManager == null)
            {
                GUILayout.Label("Day: not assigned");
                return;
            }

            GUILayout.Label($"Day: {_timeManager.CurrentDay}");
        }

        private void DrawInteractionState()
        {
            if (_interactionSystem == null)
            {
                GUILayout.Label("Interacao: not assigned");
                return;
            }

            GUILayout.Label(_interactionSystem.HasCandidate
                ? $"Interacao: {_interactionSystem.CurrentPrompt}"
                : "Interacao: nenhum alvo");
        }

        private void DrawPlayerState()
        {
            if (_playerManager == null)
            {
                GUILayout.Label("PlayerManager: not assigned");
                return;
            }

            GUILayout.Label($"Gold: {_playerManager.CurrentGold}");
            GUILayout.Label($"HP: {_playerManager.CurrentHP}/{_playerManager.MaxHP}");
        }

        private void DrawHungerState()
        {
            if (_hungerManager == null)
            {
                GUILayout.Label("HungerManager: not assigned");
                return;
            }

            GUILayout.Label($"Hunger: {_hungerManager.CurrentHunger}/{_hungerManager.MaxHunger}");
        }

        private void DrawInventory()
        {
            GUILayout.Space(8f);
            GUILayout.Label("Inventory:");

            if (_inventoryManager == null)
            {
                GUILayout.Label("- not assigned");
                return;
            }

            if (_inventoryManager.Items.Count == 0)
            {
                GUILayout.Label("- empty");
                return;
            }

            foreach (var item in _inventoryManager.Items)
            {
                GUILayout.Label($"- {item.Key}: {item.Value}");
            }
        }

        private void DrawCommands()
        {
            GUILayout.Space(8f);
            GUILayout.Label("Commands:");
            GUILayout.Label("WASD/arrows: mover");
            GUILayout.Label("E: interagir");
            GUILayout.Label("Tab: avancar dia");
            GUILayout.Label("H: consumir comida");
            GUILayout.Label("Loja: E compra trigo x3 por 5g");
            GUILayout.Label("Pesca: E no lago com cana");
            GUILayout.Label("Arvore: E para cortar");
            GUILayout.Label("F5: salvar");
            GUILayout.Label("F9: carregar");

            if (_saveManager != null)
            {
                GUILayout.Label($"Save: {_saveManager.SaveFilePath}");
            }
        }
    }
}
