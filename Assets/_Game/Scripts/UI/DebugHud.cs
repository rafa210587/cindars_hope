using CindarsHope.Inventory;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.UI
{
    [DisallowMultipleComponent]
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private HungerManager _hungerManager;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 360f, Screen.height - 24f), GUI.skin.box);
            GUILayout.Label("Cindar's Hope - Debug HUD");
            DrawPlayerState();
            DrawHungerState();
            DrawInventory();
            DrawCommands();
            GUILayout.EndArea();
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

        private static void DrawCommands()
        {
            GUILayout.Space(8f);
            GUILayout.Label("Commands:");
            GUILayout.Label("WASD/arrows: mover");
            GUILayout.Label("E: interagir");
            GUILayout.Label("Tab: avancar dia");
            GUILayout.Label("H: consumir comida");
        }
    }
}
