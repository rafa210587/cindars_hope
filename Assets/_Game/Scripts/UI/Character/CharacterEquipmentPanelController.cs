using System;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Player.Progression;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.UI.Character
{
    [DisallowMultipleComponent]
    public sealed class CharacterEquipmentPanelController : MonoBehaviour
    {
        private static CharacterEquipmentPanelController _instance;
        private bool _isOpen;
        private string _feedback = string.Empty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("CharacterEquipmentPanelController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<CharacterEquipmentPanelController>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDisable()
        {
            if (_isOpen)
            {
                Close();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                Toggle();
            }

            if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private void OnGUI()
        {
            if (!_isOpen)
            {
                return;
            }

            var rect = new Rect((Screen.width - 520f) * 0.5f, (Screen.height - 560f) * 0.5f, 520f, 560f);
            var progression = GameBootstrap.Instance?.PlayerProgressionManager;
            var equipment = GameBootstrap.Instance?.EquipmentManager;
            var inventory = GameBootstrap.Instance?.InventoryManager;

            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label("Personagem / Equipamento");
            if (progression == null)
            {
                GUILayout.Label("PlayerProgressionManager não disponível.");
            }
            else
            {
                GUILayout.Label($"Level: {progression.Level}  XP: {progression.CurrentXp} / {progression.XpToNextLevel}");
                GUILayout.Label($"Attribute Points: {progression.UnspentAttributePoints}  Skill Points: {progression.UnspentSkillPoints}");
                DrawAttribute(progression, PlayerAttributeType.Strength, "Strength", progression.Strength);
                DrawAttribute(progression, PlayerAttributeType.Dexterity, "Dexterity", progression.Dexterity);
                DrawAttribute(progression, PlayerAttributeType.Intelligence, "Intelligence", progression.Intelligence);
                DrawAttribute(progression, PlayerAttributeType.Willpower, "Willpower", progression.Willpower);
                DrawAttribute(progression, PlayerAttributeType.Constitution, "Constitution", progression.Constitution);
                DrawAttribute(progression, PlayerAttributeType.Breath, "Breath", progression.Breath);
            }

            GUILayout.Space(10f);
            GUILayout.Label("Equipamento");
            DrawEquipmentSlot(equipment, inventory, EquipmentSlot.RightHand, "Right Hand");
            DrawEquipmentSlot(equipment, inventory, EquipmentSlot.LeftHand, "Left Hand");
            DrawEquipmentSlot(equipment, inventory, EquipmentSlot.Chest, "Armor");
            DrawEquipmentSlot(equipment, inventory, EquipmentSlot.Accessory, "Accessory");

            GUILayout.Space(8f);
            GUILayout.Label("Stats derivados: bônus aplicados pelo equipamento/skills em runtime.");
            if (!string.IsNullOrWhiteSpace(_feedback))
            {
                GUILayout.Label(_feedback);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Fechar (Esc)"))
            {
                Close();
            }
            GUILayout.EndArea();
        }

        private void DrawAttribute(PlayerProgressionManager progression, PlayerAttributeType type, string label, int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {value}", GUILayout.Width(180f));
            if (GUILayout.Button("+", GUILayout.Width(40f)))
            {
                _feedback = progression.TrySpendAttributePoint(type)
                    ? $"{label} aumentado."
                    : "Sem Attribute Points disponíveis.";
            }
            GUILayout.EndHorizontal();
        }

        private void DrawEquipmentSlot(EquipmentManager equipment, InventoryManager inventory, EquipmentSlot slot, string label)
        {
            var itemId = equipment?.GetEquippedItem(slot);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {(string.IsNullOrWhiteSpace(itemId) ? "vazio" : itemId)}", GUILayout.Width(340f));
            if (!string.IsNullOrWhiteSpace(itemId) && GUILayout.Button("Desequipar"))
            {
                equipment.UnequipSlot(slot);
                inventory?.ClearEquippedBinding(itemId);
                _feedback = $"{label} desequipado.";
            }
            GUILayout.EndHorizontal();
        }

        private void Toggle()
        {
            if (_isOpen)
            {
                Close();
                return;
            }

            var modal = GameBootstrap.Instance?.ModalManager;
            if (modal != null && !modal.PushModal(ModalType.CharacterEquipment))
            {
                return;
            }

            _feedback = string.Empty;
            _isOpen = true;
        }

        private void Close()
        {
            _isOpen = false;
            GameBootstrap.Instance?.ModalManager?.TryPopModal(ModalType.CharacterEquipment, out _);
        }
    }
}
