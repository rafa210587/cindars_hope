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
        private enum PanelMode
        {
            Attributes,
            Equipment
        }

        private static CharacterEquipmentPanelController _instance;
        private bool _isOpen;
        private PanelMode _mode;
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
            if (global::UnityEngine.Input.GetKeyDown(KeyCode.K))
            {
                Toggle(PanelMode.Attributes);
            }

            if (global::UnityEngine.Input.GetKeyDown(KeyCode.L))
            {
                Toggle(PanelMode.Equipment);
            }

            if (_isOpen && global::UnityEngine.Input.GetKeyDown(KeyCode.Escape))
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

            var rect = new Rect((Screen.width - 520f) * 0.5f, (Screen.height - 500f) * 0.5f, 520f, 500f);
            GUILayout.BeginArea(rect, GUI.skin.window);

            if (_mode == PanelMode.Attributes)
            {
                GUILayout.Label("Atributos / Progressao");
                DrawAttributes(GameBootstrap.Instance?.PlayerProgressionManager);
            }
            else
            {
                GUILayout.Label("Equipamento");
                DrawEquipment(GameBootstrap.Instance?.EquipmentManager, GameBootstrap.Instance?.InventoryManager);
            }

            GUILayout.Space(8f);
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

        private void DrawAttributes(PlayerProgressionManager progression)
        {
            if (progression == null)
            {
                GUILayout.Label("PlayerProgressionManager nao disponivel.");
                return;
            }

            GUILayout.Label($"Level: {progression.Level}  XP: {progression.CurrentXp} / {progression.XpToNextLevel}");
            GUILayout.Label($"Attribute Points: {progression.UnspentAttributePoints}  Skill Points: {progression.UnspentSkillPoints}");
            DrawAttribute(progression, PlayerAttributeType.Strength, "Strength", progression.Strength);
            DrawAttribute(progression, PlayerAttributeType.Dexterity, "Dexterity", progression.Dexterity);
            DrawAttribute(progression, PlayerAttributeType.Intelligence, "Intelligence", progression.Intelligence);
            DrawAttribute(progression, PlayerAttributeType.Willpower, "Willpower", progression.Willpower);
            DrawAttribute(progression, PlayerAttributeType.Constitution, "Constitution", progression.Constitution);
            DrawAttribute(progression, PlayerAttributeType.Breath, "Breath", progression.Breath);
        }

        private void DrawEquipment(EquipmentManager equipment, InventoryManager inventory)
        {
            if (equipment == null || inventory == null)
            {
                GUILayout.Label("EquipmentManager ou InventoryManager nao disponivel.");
                return;
            }

            DrawEquipmentSlot(equipment, inventory, EquipmentSlot.Chest, "Corpo + Cabeca / Armadura");
            DrawEquipmentSlot(equipment, inventory, EquipmentSlot.RightHand, "Mao direita");
            DrawEquipmentSlot(equipment, inventory, EquipmentSlot.LeftHand, "Mao esquerda");
            DrawEquipmentSlot(equipment, inventory, EquipmentSlot.Accessory, "Lateral / Acessorio");
            GUILayout.Space(10f);
            GUILayout.Label("Stats derivados: bonus aplicados pelo equipamento/skills em runtime.");
        }

        private void DrawAttribute(PlayerProgressionManager progression, PlayerAttributeType type, string label, int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {value}", GUILayout.Width(180f));
            if (GUILayout.Button("+", GUILayout.Width(40f)))
            {
                _feedback = progression.TrySpendAttributePoint(type)
                    ? $"{label} aumentado."
                    : "Sem Attribute Points disponiveis.";
            }
            GUILayout.EndHorizontal();
        }

        private void DrawEquipmentSlot(EquipmentManager equipment, InventoryManager inventory, EquipmentSlot slot, string label)
        {
            var itemId = equipment?.GetEquippedItem(slot);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {(string.IsNullOrWhiteSpace(itemId) ? "vazio" : itemId)}", GUILayout.Width(260f));
            if (GUILayout.Button(string.IsNullOrWhiteSpace(itemId) ? "Equipar" : "Trocar", GUILayout.Width(82f)))
            {
                OpenEquipmentSelection(slot, label);
            }

            var previousEnabled = GUI.enabled;
            GUI.enabled = !string.IsNullOrWhiteSpace(itemId);
            if (GUILayout.Button("Desequipar", GUILayout.Width(90f)))
            {
                equipment.UnequipSlot(slot);
                inventory.ClearEquippedBinding(slot, itemId);
                _feedback = $"{label} desequipado.";
            }
            GUI.enabled = previousEnabled;
            GUILayout.EndHorizontal();
        }

        private void OpenEquipmentSelection(EquipmentSlot slot, string label)
        {
            Close();
            if (!InventoryPanelController.OpenForEquipmentSelection(slot, HandleEquipmentSelectionClosed))
            {
                _feedback = $"Nao foi possivel abrir selecao para {label}.";
                Debug.LogError(
                    $"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(CharacterEquipmentPanelController)}' could not open inventory selection for slot '{slot}'.",
                    this);
                OpenEquipmentPanel();
            }
        }

        private void HandleEquipmentSelectionClosed(bool selected, string message)
        {
            _feedback = message;
            OpenEquipmentPanel();
        }

        private void Toggle(PanelMode mode)
        {
            if (_isOpen)
            {
                if (_mode == mode)
                {
                    Close();
                }
                else
                {
                    _mode = mode;
                    _feedback = string.Empty;
                }
                return;
            }

            var modal = GameBootstrap.Instance?.ModalManager;
            if (modal != null && !modal.PushModal(ModalType.CharacterEquipment))
            {
                return;
            }

            _feedback = string.Empty;
            _mode = mode;
            _isOpen = true;
        }

        private void OpenEquipmentPanel()
        {
            if (_isOpen)
            {
                _mode = PanelMode.Equipment;
                return;
            }

            var modal = GameBootstrap.Instance?.ModalManager;
            if (modal != null && !modal.PushModal(ModalType.CharacterEquipment))
            {
                Debug.LogError(
                    $"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(CharacterEquipmentPanelController)}' cannot reopen equipment modal after slot selection: modal stack rejected slot picker return.",
                    this);
                return;
            }

            _mode = PanelMode.Equipment;
            _isOpen = true;
        }

        private void Close()
        {
            _isOpen = false;
            GameBootstrap.Instance?.ModalManager?.TryPopModal(ModalType.CharacterEquipment, out _);
        }
    }
}
