using System;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.UI.Modal;
using CindarsHope.UI.Routing;
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
            DestroyConfirm,
            EquipmentSelection
        }

        private static InventoryPanelController _instance;

        private InventoryManager _inventoryManager;
        private EquipmentManager _equipmentManager;
        private ModalManager _modalManager;
        private PanelMode _mode;
        private bool _isOpen;
        private int _selectedSlotIndex;
        private int _selectedActionIndex;
        private string _message = string.Empty;
        private EquipmentSlot _targetEquipmentSlot = EquipmentSlot.None;
        private Action<bool, string> _onEquipmentSelectionClosed;
        private readonly string[] _actions = { "Use", "Equip", "Drop", "Destroy", "Split", "Cancel" };

        public static void Install(Transform owner)
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("InventoryPanelController");
            go.transform.SetParent(owner);
            if (owner == null) DontDestroyOnLoad(go);
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
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<InventoryPanelOpenedEvent>(OnInventoryOpenRequested);
            GameEventBus.Subscribe<ModalCloseRequestedEvent>(OnModalCloseRequested);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<InventoryPanelOpenedEvent>(OnInventoryOpenRequested);
            GameEventBus.Unsubscribe<ModalCloseRequestedEvent>(OnModalCloseRequested);
            if (_isOpen)
            {
                ClosePanel();
            }
        }

        private void Update()
        {
            ResolveInventoryManager();

            if (!GameplayInputRouter.IsActive && global::UnityEngine.Input.GetKeyDown(KeyCode.I))
            {
                Toggle();
            }

            if (!_isOpen)
            {
                return;
            }

            if (!GameplayInputRouter.IsActive && global::UnityEngine.Input.GetKeyDown(KeyCode.Escape))
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
                case PanelMode.EquipmentSelection:
                    UpdateEquipmentSelectionNavigation();
                    break;
            }
        }

        private void OnInventoryOpenRequested(InventoryPanelOpenedEvent _)
        {
            if (!_isOpen) Toggle();
        }

        private void OnModalCloseRequested(ModalCloseRequestedEvent _)
        {
            if (_isOpen) CloseOrBack();
        }

        private void OnGUI()
        {
            if (!_isOpen)
            {
                return;
            }

            CindarsHope.Core.MenuGuiStyle.Apply();
            ResolveInventoryManager();
            ResolveEquipmentManager();
            var width = Mathf.Min(620f, Screen.width - 32f);
            var height = Mathf.Min(520f, Screen.height - 32f);
            var rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            GUILayout.BeginArea(rect, GUI.skin.window);
            var title = _mode == PanelMode.EquipmentSelection
                ? $"Selecionar para {_targetEquipmentSlot} ({GetFilledSlotCount()}/{GetCapacity()})"
                : $"Inventory ({GetFilledSlotCount()}/{GetCapacity()})";
            GUILayout.Label(title);
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

            if (GUILayout.Button(_mode == PanelMode.EquipmentSelection ? "Cancelar (Esc)" : "Fechar"))
            {
                if (_mode == PanelMode.EquipmentSelection)
                {
                    CompleteEquipmentSelection(false, "Selecao cancelada.");
                }
                else
                {
                    ClosePanel();
                }
            }

            GUILayout.EndArea();
        }

        private void Toggle()
        {
            if (_isOpen)
            {
                if (_mode == PanelMode.EquipmentSelection)
                {
                    CompleteEquipmentSelection(false, "Selecao cancelada.");
                    return;
                }

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

        public static bool OpenForEquipmentSelection(EquipmentSlot targetSlot, Action<bool, string> onClosed)
        {
            return _instance != null
                && targetSlot != EquipmentSlot.None
                && _instance.OpenEquipmentSelection(targetSlot, onClosed);
        }

        private bool OpenEquipmentSelection(EquipmentSlot targetSlot, Action<bool, string> onClosed)
        {
            ResolveInventoryManager();
            ResolveEquipmentManager();
            ResolveModalManager();
            if (_inventoryManager == null || _equipmentManager == null || _modalManager == null)
            {
                Debug.LogError(
                    $"Scene '{gameObject.scene.path}' GameObject '{gameObject.name}' component '{nameof(InventoryPanelController)}' cannot open selection for slot '{targetSlot}': required manager missing.",
                    this);
                return false;
            }

            if (_isOpen)
            {
                ClosePanel();
            }

            if (!_modalManager.PushModal(ModalType.Inventory))
            {
                return false;
            }

            _targetEquipmentSlot = targetSlot;
            _onEquipmentSelectionClosed = onClosed;
            _selectedSlotIndex = FindFirstCompatibleSlotIndex(targetSlot);
            _message = "Escolha um item compativel ou pressione Esc para voltar.";
            _mode = PanelMode.EquipmentSelection;
            _isOpen = true;
            return true;
        }

        private void CloseOrBack()
        {
            if (_mode == PanelMode.EquipmentSelection)
            {
                CompleteEquipmentSelection(false, "Selecao cancelada.");
                return;
            }

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

            if (global::UnityEngine.Input.GetKeyDown(KeyCode.A))
            {
                _selectedSlotIndex = Mathf.Max(0, _selectedSlotIndex - 1);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                _selectedSlotIndex = Mathf.Min(capacity - 1, _selectedSlotIndex + 1);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                _selectedSlotIndex = Mathf.Max(0, _selectedSlotIndex - 6);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.S))
            {
                _selectedSlotIndex = Mathf.Min(capacity - 1, _selectedSlotIndex + 6);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.Return) || global::UnityEngine.Input.GetKeyDown(KeyCode.Space) || global::UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                _mode = PanelMode.Actions;
                _selectedActionIndex = 0;
            }
        }

        private void UpdateActionNavigation()
        {
            if (global::UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                _selectedActionIndex = Mathf.Max(0, _selectedActionIndex - 1);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.S))
            {
                _selectedActionIndex = Mathf.Min(_actions.Length - 1, _selectedActionIndex + 1);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.Return) || global::UnityEngine.Input.GetKeyDown(KeyCode.Space) || global::UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                ExecuteSelectedAction();
            }
        }

        private void UpdateDestroyConfirmation()
        {
            if (global::UnityEngine.Input.GetKeyDown(KeyCode.Return) || global::UnityEngine.Input.GetKeyDown(KeyCode.Space) || global::UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                if (_inventoryManager != null && _inventoryManager.DestroySlot(_selectedSlotIndex))
                {
                    _message = "Item destroyed.";
                }

                _mode = PanelMode.Slots;
            }
        }

        private void UpdateEquipmentSelectionNavigation()
        {
            if (global::UnityEngine.Input.GetKeyDown(KeyCode.A))
            {
                MoveSelectionToCompatible(-1);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                MoveSelectionToCompatible(1);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                MoveSelectionToCompatible(-6);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.S))
            {
                MoveSelectionToCompatible(6);
            }
            else if (global::UnityEngine.Input.GetKeyDown(KeyCode.Return) || global::UnityEngine.Input.GetKeyDown(KeyCode.Space) || global::UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                SelectEquipmentSlot(_selectedSlotIndex);
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
                    ExecuteEquipOrUnequip(slot);
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

            var player = GameBootstrap.Instance?.PlayerManager?.gameObject;
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

            var player = GameBootstrap.Instance?.PlayerManager;
            var dropPosition = player != null ? player.transform.position + Vector3.right * 0.5f : transform.position;

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

            // 5 colunas (botoes mais largos) com fonte menor + wordWrap para os nomes caberem/lerem.
            const int columns = 5;
            var capacity = GetCapacity();
            var rows = Mathf.CeilToInt(capacity / (float)columns);
            var slotStyle = GetSlotButtonStyle();
            for (var row = 0; row < rows; row++)
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
                    var label = FormatSlotLabel(slot);
                    var previousColor = GUI.color;
                    var previousEnabled = GUI.enabled;
                    var selectable = _mode != PanelMode.EquipmentSelection || IsCompatibleSlot(slotIndex, _targetEquipmentSlot);
                    if (slotIndex == _selectedSlotIndex)
                    {
                        GUI.color = Color.yellow;
                    }

                    GUI.enabled = selectable;
                    if (GUILayout.Button(label, slotStyle, GUILayout.Width(112f), GUILayout.Height(52f)))
                    {
                        _selectedSlotIndex = slotIndex;
                        if (_mode == PanelMode.EquipmentSelection)
                        {
                            SelectEquipmentSlot(slotIndex);
                        }
                        else
                        {
                            _mode = PanelMode.Actions;
                        }
                    }
                    GUI.enabled = previousEnabled;
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
            GUILayout.Label($"Selected: {ResolveDisplayName(slot.ItemId)} x{slot.Amount}{equipped}");
            if (_mode == PanelMode.EquipmentSelection && !IsCompatibleSlot(_selectedSlotIndex, _targetEquipmentSlot))
            {
                GUILayout.Label($"Incompativel com {_targetEquipmentSlot}.");
            }
        }

        private void DrawActions()
        {
            GUILayout.Space(8f);
            GUILayout.BeginVertical(GUI.skin.box);
            for (var index = 0; index < _actions.Length; index++)
            {
                var label = _actions[index] == "Equip" ? GetEquipActionLabel() : _actions[index];
                if (GUILayout.Button(index == _selectedActionIndex ? $"> {label}" : label))
                {
                    _selectedActionIndex = index;
                    ExecuteSelectedAction();
                }
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

        private void ResolveEquipmentManager()
        {
            // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — EquipmentManager
            // resolvido via EquipmentManager.Instance (self-registro, molde Craft/Economy/Skills).
            if (_equipmentManager == null)
            {
                _equipmentManager = EquipmentManager.Instance;
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

        private string GetEquipActionLabel()
        {
            return _inventoryManager != null
                && _inventoryManager.TryGetSlot(_selectedSlotIndex, out var slot)
                && slot != null
                && slot.IsEquipped
                ? "Desequipar"
                : "Equipar";
        }

        private void ExecuteEquipOrUnequip(InventorySlot slot)
        {
            ResolveEquipmentManager();
            if (_equipmentManager == null || !_inventoryManager.TryGetItemData(slot.ItemId, out var item))
            {
                _message = "Item não equipável ou sem slot definido.";
                return;
            }

            if (slot.IsEquipped)
            {
                var currentSlot = EquipmentSlotRules.ResolveEquipmentSlot(item);
                if (currentSlot != EquipmentSlot.None)
                {
                    _equipmentManager.UnequipSlot(currentSlot);
                }
                _inventoryManager.ClearEquippedBindingAtSlot(_selectedSlotIndex);
                _message = "Item desequipado.";
                return;
            }

            var equipmentSlot = EquipmentSlotRules.ResolveEquipmentSlot(item);
            if (!item.IsEquippable || equipmentSlot == EquipmentSlot.None)
            {
                _message = "Item não equipável ou sem slot definido.";
                return;
            }

            ReplaceEquippedItem(equipmentSlot, _selectedSlotIndex, slot);
            _message = _inventoryManager.MarkSlotEquipped(_selectedSlotIndex, equipmentSlot)
                ? $"Item equipado em {equipmentSlot}."
                : "Não foi possível equipar o item.";
        }

        private void SelectEquipmentSlot(int slotIndex)
        {
            if (_inventoryManager == null
                || !_inventoryManager.TryGetSlot(slotIndex, out var slot)
                || slot.IsEmpty
                || !IsCompatibleSlot(slotIndex, _targetEquipmentSlot))
            {
                _message = $"Item incompativel com {_targetEquipmentSlot}.";
                return;
            }

            ReplaceEquippedItem(_targetEquipmentSlot, slotIndex, slot);
            if (!_inventoryManager.MarkSlotEquipped(slotIndex, _targetEquipmentSlot))
            {
                _message = "Nao foi possivel marcar o item equipado.";
                return;
            }

            CompleteEquipmentSelection(true, $"{slot.ItemId} equipado em {_targetEquipmentSlot}.");
        }

        private void ReplaceEquippedItem(EquipmentSlot equipmentSlot, int slotIndex, InventorySlot slot)
        {
            var previousItemId = _equipmentManager.GetEquippedItem(equipmentSlot);
            if (!string.IsNullOrWhiteSpace(previousItemId))
            {
                _inventoryManager.ClearEquippedBinding(equipmentSlot, previousItemId);
            }

            if (slot.IsEquipped)
            {
                _inventoryManager.ClearEquippedBindingAtSlot(slotIndex);
            }

            _equipmentManager.EquipItem(equipmentSlot, slot.ItemId);
        }

        private bool IsCompatibleSlot(int slotIndex, EquipmentSlot equipmentSlot)
        {
            return _inventoryManager != null
                && _inventoryManager.TryGetSlot(slotIndex, out var slot)
                && slot != null
                && !slot.IsEmpty
                && _inventoryManager.TryGetItemData(slot.ItemId, out var item)
                && item != null
                && EquipmentSlotRules.IsCompatibleWithEquipmentSlot(item, equipmentSlot);
        }

        private int FindFirstCompatibleSlotIndex(EquipmentSlot equipmentSlot)
        {
            for (var index = 0; index < GetCapacity(); index++)
            {
                if (IsCompatibleSlot(index, equipmentSlot))
                {
                    return index;
                }
            }

            return 0;
        }

        private void MoveSelectionToCompatible(int direction)
        {
            var capacity = GetCapacity();
            if (capacity <= 0)
            {
                return;
            }

            var candidate = Mathf.Clamp(_selectedSlotIndex + direction, 0, capacity - 1);
            var step = direction >= 0 ? 1 : -1;
            for (; candidate >= 0 && candidate < capacity; candidate += step)
            {
                if (IsCompatibleSlot(candidate, _targetEquipmentSlot))
                {
                    _selectedSlotIndex = candidate;
                    return;
                }
            }
        }

        private void CompleteEquipmentSelection(bool selected, string message)
        {
            var callback = _onEquipmentSelectionClosed;
            _onEquipmentSelectionClosed = null;
            _targetEquipmentSlot = EquipmentSlot.None;
            ClosePanel();
            callback?.Invoke(selected, message);
        }

        private string FormatSlotLabel(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                return "-";
            }

            // Nome legivel (wordWrap no estilo cuida de quebrar) + quantidade em linha propria.
            return $"{ResolveDisplayName(slot.ItemId)}\nx{slot.Amount}";
        }

        // Estilo dos botoes de slot: fonte menor + wordWrap para nomes longos caberem e serem
        // legiveis (sem cortar "Espada de Ferro" / "Crystal Berry Seed"). Cacheado; construido
        // dentro do OnGUI (GUI.skin so e valido durante OnGUI).
        private GUIStyle _slotButtonStyle;
        private GUIStyle GetSlotButtonStyle()
        {
            if (_slotButtonStyle == null)
            {
                _slotButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    wordWrap = true,
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(3, 3, 2, 2)
                };
            }

            return _slotButtonStyle;
        }

        // Nome legivel do item (DisplayName do ItemDataSO) em vez do Id "tipo variavel". Fallback ao
        // Id se o item nao resolver (asset ausente / DB nao injetada) para nunca mostrar vazio.
        private string ResolveDisplayName(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return "-";
            }

            if (_inventoryManager != null
                && _inventoryManager.TryGetItemData(itemId, out var item)
                && item != null
                && !string.IsNullOrWhiteSpace(item.DisplayName))
            {
                return item.DisplayName;
            }

            return itemId;
        }
    }

}
