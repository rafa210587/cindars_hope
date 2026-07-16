using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Localization;
using CindarsHope.Player;
using CindarsHope.Player.Progression;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Routing;
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

        // Attribute rows: Strength, Dexterity, Intelligence, Willpower, Constitution, Breath
        private const int AttributeCount = 6;
        // Equipment slots drawn in order: Chest, RightHand, LeftHand, Accessory
        private static readonly EquipmentSlot[] EquipmentSlots =
        {
            EquipmentSlot.Chest,
            EquipmentSlot.RightHand,
            EquipmentSlot.LeftHand,
            EquipmentSlot.Accessory,
        };
        private static readonly PlayerAttributeType[] AttributeTypes =
        {
            PlayerAttributeType.Strength,
            PlayerAttributeType.Dexterity,
            PlayerAttributeType.Intelligence,
            PlayerAttributeType.Willpower,
            PlayerAttributeType.Constitution,
            PlayerAttributeType.Breath,
        };
        private static readonly string[] AttributeLabels =
        {
            "Strength",
            "Dexterity",
            "Intelligence",
            "Willpower",
            "Constitution",
            "Breath",
        };

        private static CharacterEquipmentPanelController _instance;
        private bool _isOpen;
        private PanelMode _mode;
        private string _feedback = string.Empty;
        private int _selectedIndex;

        public static void Install(Transform owner)
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("CharacterEquipmentPanelController");
            go.transform.SetParent(owner);
            if (owner == null) DontDestroyOnLoad(go);
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
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<EquipmentPanelOpenedEvent>(OnEquipmentOpenRequested);
            GameEventBus.Subscribe<ModalCloseRequestedEvent>(OnModalCloseRequested);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EquipmentPanelOpenedEvent>(OnEquipmentOpenRequested);
            GameEventBus.Unsubscribe<ModalCloseRequestedEvent>(OnModalCloseRequested);
            if (_isOpen)
            {
                Close();
            }
        }

        private void Update()
        {
            if (!GameplayInputRouter.IsActive && global::UnityEngine.Input.GetKeyDown(KeyCode.K))
            {
                Toggle(PanelMode.Attributes);
            }

            if (global::UnityEngine.Input.GetKeyDown(KeyCode.L))
            {
                Toggle(PanelMode.Equipment);
            }

            if (!_isOpen)
            {
                return;
            }

            if (!GameplayInputRouter.IsActive && global::UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }

            UpdateKeyboardNavigation();
        }

        private void OnEquipmentOpenRequested(EquipmentPanelOpenedEvent _)
        {
            if (!_isOpen) Toggle(PanelMode.Attributes);
        }

        private void OnModalCloseRequested(ModalCloseRequestedEvent _)
        {
            if (_isOpen) Close();
        }

        private void UpdateKeyboardNavigation()
        {
            var maxIndex = _mode == PanelMode.Attributes ? AttributeCount - 1 : EquipmentSlots.Length - 1;

            if (global::UnityEngine.Input.GetKeyDown(KeyCode.W) || global::UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
            {
                _selectedIndex = Mathf.Max(0, _selectedIndex - 1);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.S) || global::UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
            {
                _selectedIndex = Mathf.Min(maxIndex, _selectedIndex + 1);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.E) || global::UnityEngine.Input.GetKeyDown(KeyCode.Return))
            {
                ExecuteSelectedAction();
            }
        }

        private void ExecuteSelectedAction()
        {
            if (_mode == PanelMode.Attributes)
            {
                var progression = GameBootstrap.Instance?.PlayerProgressionManager;
                if (progression == null || _selectedIndex >= AttributeTypes.Length)
                {
                    return;
                }

                _feedback = progression.TrySpendAttributePoint(AttributeTypes[_selectedIndex])
                    ? $"{AttributeLabels[_selectedIndex]} aumentado."
                    : "Sem Attribute Points disponiveis.";
            }
            else
            {
                if (_selectedIndex >= EquipmentSlots.Length)
                {
                    return;
                }

                // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — EquipmentManager
                // resolvido via EquipmentManager.Instance (self-registro, molde Craft/Economy/Skills).
                var equipment = EquipmentManager.Instance;
                // arch: quebra do par mutuo Core|Inventory (2026-07-15) — cast local para o tipo
                // concreto (GameBootstrap.InventoryManager agora retorna a porta IInventoryRuntime).
                var inventory = GameBootstrap.Instance?.InventoryManager as InventoryManager;
                if (equipment == null || inventory == null)
                {
                    return;
                }

                var slot = EquipmentSlots[_selectedIndex];
                var itemId = equipment.GetEquippedItem(slot);
                if (!string.IsNullOrWhiteSpace(itemId))
                {
                    // Unequip if already equipped
                    equipment.UnequipSlot(slot);
                    inventory.ClearEquippedBinding(slot, itemId);
                    _feedback = $"Slot {slot} desequipado.";
                }
                else
                {
                    // Open inventory for equipment selection
                    var slotLabel = slot.ToString();
                    OpenEquipmentSelection(slot, slotLabel);
                }
            }
        }

        private void OnGUI()
        {
            if (!_isOpen)
            {
                return;
            }

            CindarsHope.Core.MenuGuiStyle.Apply();
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
                DrawEquipment(EquipmentManager.Instance, GameBootstrap.Instance?.InventoryManager as InventoryManager);
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
            // fable_39: classe INFERIDA (Q9.1) — linha aditiva read-only; titulo derivado do
            // investimento por arvore (sem estado salvo). Resolvido via LocalizationService.
            var classProfile = InferredClassRuntime.CurrentProfile;
            GUILayout.Label($"Classe: {LocalizationService.Get(classProfile.TitleId)}");
            GUILayout.Label("[W/S] navegar  [E] gastar ponto");
            DrawAttribute(progression, 0, PlayerAttributeType.Strength, "Strength", progression.Strength);
            DrawAttribute(progression, 1, PlayerAttributeType.Dexterity, "Dexterity", progression.Dexterity);
            DrawAttribute(progression, 2, PlayerAttributeType.Intelligence, "Intelligence", progression.Intelligence);
            DrawAttribute(progression, 3, PlayerAttributeType.Willpower, "Willpower", progression.Willpower);
            DrawAttribute(progression, 4, PlayerAttributeType.Constitution, "Constitution", progression.Constitution);
            DrawAttribute(progression, 5, PlayerAttributeType.Breath, "Breath", progression.Breath);
        }

        private void DrawEquipment(EquipmentManager equipment, InventoryManager inventory)
        {
            if (equipment == null || inventory == null)
            {
                GUILayout.Label("EquipmentManager ou InventoryManager nao disponivel.");
                return;
            }

            GUILayout.Label("[W/S] navegar  [E] equipar/desequipar");
            DrawEquipmentSlot(equipment, inventory, 0, EquipmentSlot.Chest, "Corpo + Cabeca / Armadura");
            DrawEquipmentSlot(equipment, inventory, 1, EquipmentSlot.RightHand, "Mao direita");
            DrawEquipmentSlot(equipment, inventory, 2, EquipmentSlot.LeftHand, "Mao esquerda");
            DrawEquipmentSlot(equipment, inventory, 3, EquipmentSlot.Accessory, "Lateral / Acessorio");
            GUILayout.Space(10f);
            GUILayout.Label("Stats derivados: bonus aplicados pelo equipamento/skills em runtime.");
        }

        private void DrawAttribute(PlayerProgressionManager progression, int rowIndex, PlayerAttributeType type, string label, int value)
        {
            var prevColor = GUI.backgroundColor;
            if (rowIndex == _selectedIndex)
            {
                GUI.backgroundColor = Color.yellow;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {value}", GUILayout.Width(180f));
            if (GUILayout.Button("+", GUILayout.Width(40f)))
            {
                _selectedIndex = rowIndex;
                _feedback = progression.TrySpendAttributePoint(type)
                    ? $"{label} aumentado."
                    : "Sem Attribute Points disponiveis.";
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = prevColor;
        }

        private void DrawEquipmentSlot(EquipmentManager equipment, InventoryManager inventory, int rowIndex, EquipmentSlot slot, string label)
        {
            var itemId = equipment?.GetEquippedItem(slot);
            var prevColor = GUI.backgroundColor;
            if (rowIndex == _selectedIndex)
            {
                GUI.backgroundColor = Color.yellow;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {(string.IsNullOrWhiteSpace(itemId) ? "vazio" : itemId)}", GUILayout.Width(260f));
            if (GUILayout.Button(string.IsNullOrWhiteSpace(itemId) ? "Equipar" : "Trocar", GUILayout.Width(82f)))
            {
                _selectedIndex = rowIndex;
                OpenEquipmentSelection(slot, label);
            }

            var previousEnabled = GUI.enabled;
            GUI.enabled = !string.IsNullOrWhiteSpace(itemId);
            if (GUILayout.Button("Desequipar", GUILayout.Width(90f)))
            {
                _selectedIndex = rowIndex;
                equipment.UnequipSlot(slot);
                inventory.ClearEquippedBinding(slot, itemId);
                _feedback = $"{label} desequipado.";
            }
            GUI.enabled = previousEnabled;
            GUILayout.EndHorizontal();
            GUI.backgroundColor = prevColor;
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
                    _selectedIndex = 0;
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
            _selectedIndex = 0;
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
            _selectedIndex = 0;
            _isOpen = true;
        }

        private void Close()
        {
            _isOpen = false;
            GameBootstrap.Instance?.ModalManager?.TryPopModal(ModalType.CharacterEquipment, out _);
        }
    }
}
