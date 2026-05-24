using CindarsHope.Core.Bootstrap;
using CindarsHope.Inventory;
using CindarsHope.UI.Modal;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.UI
{
    [DisallowMultipleComponent]
    public class InventoryPanelController : MonoBehaviour
    {
        private enum PanelMode
        {
            Slots,
            Actions,
            DestroyConfirm
        }

        private static InventoryPanelController _instance;

        private InventoryManager _inventoryManager;
        private ModalManager _modalManager;
        private PanelMode _mode;
        private bool _isOpen;
        private int _selectedSlotIndex;
        private int _selectedActionIndex;
        private string _message = string.Empty;
        private readonly string[] _actions = { "Use", "Equip", "Drop", "Destroy", "Split", "Cancel" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("InventoryPanelController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<InventoryPanelController>();
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
                ClosePanel();
            }
        }

        private void Update()
        {
            ResolveInventoryManager();

            if (Input.GetKeyDown(KeyCode.I))
            {
                Toggle();
            }

            if (!_isOpen)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseOrBack();
                return;
            }

            switch (_mode)
            {
                case PanelMode.Slots:
                    UpdateSlotNavigation();
                    break;
                case PanelMode.Actions:
                    UpdateActionNavigation();
                    break;
                case PanelMode.DestroyConfirm:
                    UpdateDestroyConfirmation();
                    break;
            }
        }

        private void OnGUI()
        {
            if (!_isOpen)
            {
                return;
            }

            ResolveInventoryManager();
            var width = Mathf.Min(620f, Screen.width - 32f);
            var height = Mathf.Min(520f, Screen.height - 32f);
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label($"Inventory ({GetFilledSlotCount()}/{GetCapacity()})");
            DrawSlots();
            DrawSelectedDetails();

            if (_mode == PanelMode.Actions)
            {
                DrawActions();
            }
            else if (_mode == PanelMode.DestroyConfirm)
            {
                DrawDestroyConfirmation();
            }

            if (!string.IsNullOrWhiteSpace(_message))
            {
                GUILayout.Space(6f);
                GUILayout.Label(_message);
            }

            GUILayout.EndArea();
        }

        private void Toggle()
        {
            if (_isOpen)
            {
                ClosePanel();
                return;
            }

            ResolveModalManager();
            if (_modalManager != null && !_modalManager.PushModal(ModalType.Inventory))
            {
                return;
            }

            _isOpen = true;
            _mode = PanelMode.Slots;
            _selectedActionIndex = 0;
            _message = string.Empty;
        }

        private void CloseOrBack()
        {
            if (_mode == PanelMode.Slots)
            {
                ClosePanel();
                return;
            }

            _mode = PanelMode.Slots;
            _selectedActionIndex = 0;
        }

        private void UpdateSlotNavigation()
        {
            var capacity = GetCapacity();
            if (capacity <= 0)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                _selectedSlotIndex = Mathf.Max(0, _selectedSlotIndex - 1);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                _selectedSlotIndex = Mathf.Min(capacity - 1, _selectedSlotIndex + 1);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                _selectedSlotIndex = Mathf.Max(0, _selectedSlotIndex - 6);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                _selectedSlotIndex = Mathf.Min(capacity - 1, _selectedSlotIndex + 6);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            {
                _mode = PanelMode.Actions;
                _selectedActionIndex = 0;
            }
        }

        private void UpdateActionNavigation()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                _selectedActionIndex = Mathf.Max(0, _selectedActionIndex - 1);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                _selectedActionIndex = Mathf.Min(_actions.Length - 1, _selectedActionIndex + 1);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            {
                ExecuteSelectedAction();
            }
        }

        private void UpdateDestroyConfirmation()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            {
                if (_inventoryManager != null && _inventoryManager.DestroySlot(_selectedSlotIndex))
                {
                    _message = "Item destroyed.";
                }

                _mode = PanelMode.Slots;
            }
        }

        private void ExecuteSelectedAction()
        {
            if (_inventoryManager == null || !_inventoryManager.TryGetSlot(_selectedSlotIndex, out var slot) || slot.IsEmpty)
            {
                _message = "Slot is empty.";
                _mode = PanelMode.Slots;
                return;
            }

            switch (_actions[_selectedActionIndex])
            {
                case "Use":
                    ExecuteUse();
                    break;
                case "Equip":
                    _message = _inventoryManager.MarkSlotEquipped(_selectedSlotIndex, "manual")
                        ? "Item marked as equipped."
                        : "Item is not equippable.";
                    _mode = PanelMode.Slots;
                    break;
                case "Drop":
                    ExecuteDrop();
                    break;
                case "Destroy":
                    _mode = PanelMode.DestroyConfirm;
                    break;
                case "Split":
                    _message = _inventoryManager.SplitSlot(_selectedSlotIndex)
                        ? "Stack split."
                        : "Stack cannot be split.";
                    _mode = PanelMode.Slots;
                    break;
                default:
                    _mode = PanelMode.Slots;
                    break;
            }
        }

        private void ExecuteUse()
        {
            if (_inventoryManager == null || !_inventoryManager.TryGetSlot(_selectedSlotIndex, out var slot) || slot.IsEmpty)
            {
                _message = "Slot is empty.";
                _mode = PanelMode.Slots;
                return;
            }

            var useManager = ItemUseManager.Instance;
            if (useManager == null)
            {
                _message = "Item use system not available.";
                _mode = PanelMode.Slots;
                return;
            }

            if (!useManager.CanUseItem(slot.ItemId))
            {
                _message = "This item cannot be used.";
                _mode = PanelMode.Slots;
                return;
            }

            var player = GameObject.FindWithTag("Player");
            if (useManager.TryUseItem(slot.ItemId, player))
            {
                _message = $"Used {slot.ItemId}.";
            }
            else
            {
                _message = $"Failed to use {slot.ItemId}.";
            }

            _mode = PanelMode.Slots;
        }

        private void ExecuteDrop()
        {
            if (_inventoryManager == null || !_inventoryManager.TryGetSlot(_selectedSlotIndex, out var slot) || slot.IsEmpty)
            {
                _message = "Slot is empty.";
                _mode = PanelMode.Slots;
                return;
            }

            var player = GameObject.FindWithTag("Player");
            var dropPosition = player != null ? player.transform.position + Vector3.right * 0.5f : Vector3.zero;

            if (_inventoryManager.DropItem(_selectedSlotIndex, dropPosition))
            {
                _message = "Item dropped.";
            }
            else
            {
                _message = "Failed to drop item.";
            }

            _mode = PanelMode.Slots;
        }

        private void DrawSlots()
        {
            if (_inventoryManager == null)
            {
                GUILayout.Label("InventoryManager not available.");
                return;
            }

            const int columns = 6;
            var capacity = GetCapacity();
            for (var row = 0; row < 5; row++)
            {
                GUILayout.BeginHorizontal();
                for (var column = 0; column < columns; column++)
                {
                    var slotIndex = row * columns + column;
                    if (slotIndex >= capacity)
                    {
                        continue;
                    }

                    _inventoryManager.TryGetSlot(slotIndex, out var slot);
                    var label = FormatSlotLabel(slotIndex, slot);
                    var previousColor = GUI.color;
                    if (slotIndex == _selectedSlotIndex)
                    {
                        GUI.color = Color.yellow;
                    }

                    GUILayout.Box(label, GUILayout.Width(92f), GUILayout.Height(44f));
                    GUI.color = previousColor;
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawSelectedDetails()
        {
            GUILayout.Space(8f);
            if (_inventoryManager == null || !_inventoryManager.TryGetSlot(_selectedSlotIndex, out var slot) || slot.IsEmpty)
            {
                GUILayout.Label("Selected: empty");
                return;
            }

            var equipped = slot.IsEquipped ? $" [{slot.EquipmentBindingId}]" : string.Empty;
            GUILayout.Label($"Selected: {slot.ItemId} x{slot.Amount}{equipped}");
        }

        private void DrawActions()
        {
            GUILayout.Space(8f);
            GUILayout.BeginVertical(GUI.skin.box);
            for (var index = 0; index < _actions.Length; index++)
            {
                GUILayout.Label(index == _selectedActionIndex ? $"> {_actions[index]}" : $"  {_actions[index]}");
            }

            GUILayout.EndVertical();
        }

        private void DrawDestroyConfirmation()
        {
            GUILayout.Space(8f);
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Destroy selected stack?");
            GUILayout.Label("Enter/E: confirm, Esc: cancel");
            GUILayout.EndVertical();
        }

        private void ResolveInventoryManager()
        {
            if (_inventoryManager == null && GameBootstrap.Instance != null)
            {
                _inventoryManager = GameBootstrap.Instance.InventoryManager;
            }
        }

        private void ResolveModalManager()
        {
            if (_modalManager == null && GameBootstrap.Instance != null)
            {
                _modalManager = GameBootstrap.Instance.ModalManager;
            }
        }

        private void ClosePanel()
        {
            _isOpen = false;
            _modalManager?.TryPopModal(ModalType.Inventory, out _);
        }

        private int GetCapacity()
        {
            return _inventoryManager != null ? _inventoryManager.Capacity : InventoryManager.DefaultCapacity;
        }

        private int GetFilledSlotCount()
        {
            if (_inventoryManager == null)
            {
                return 0;
            }

            var count = 0;
            foreach (var slot in _inventoryManager.Slots)
            {
                if (slot != null && !slot.IsEmpty)
                {
                    count++;
                }
            }

            return count;
        }

        private static string FormatSlotLabel(int slotIndex, InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                return $"{slotIndex + 1}\n-";
            }

            return $"{slotIndex + 1}\n{slot.ItemId}\nx{slot.Amount}";
        }
    }
}
