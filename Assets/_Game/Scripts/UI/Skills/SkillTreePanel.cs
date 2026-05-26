using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Skills;
using CindarsHope.UI.Modal;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Skills
{
    // Minimal skill tree modal. Opens/closes with U. Tabs switch with Q/E inside modal.
    // W/A/S/D navigate nodes; Enter/E confirms purchase; R/T/Y/G assign to active slot.
    [DisallowMultipleComponent]
    public class SkillTreePanel : ModalBase
    {
        [Header("References")]
        [SerializeField] private Text _skillPointsText;
        [SerializeField] private Text _treeNameText;
        [SerializeField] private Text _nodeNameText;
        [SerializeField] private Text _nodeDescText;
        [SerializeField] private Text _nodeStatusText;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Button _closeButton;

        public override ModalType ModalType => ModalType.SkillTree;

        private SkillTreeManager _skillTreeManager;
        private int _currentTreeIndex;
        private int _currentNodeIndex;
        private string _feedback = string.Empty;

        private static readonly string[] TreeOrder = { "melee", "ranged", "magic", "survival", "crafting" };

        private void OnEnable()
        {
            if (_purchaseButton != null)
                _purchaseButton.onClick.AddListener(OnPurchaseClicked);
            if (_closeButton != null)
                _closeButton.onClick.AddListener(CloseModal);
        }

        private void OnDisable()
        {
            if (_purchaseButton != null)
                _purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(CloseModal);
        }

        public override void InitializeModal(ModalManager modalManager)
        {
            base.InitializeModal(modalManager);
            _skillTreeManager = GameBootstrap.Instance?.SkillTreeManager;
            _skillTreeManager?.RebindProgressionManager(GameBootstrap.Instance?.PlayerProgressionManager);
            RefreshDisplay();
            GameEventBus.Publish(new SkillTreeOpenedEvent());
        }

        public override void CloseModal()
        {
            GameEventBus.Publish(new SkillTreeClosedEvent());
            base.CloseModal();
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // Close
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.U))
            {
                CloseModal();
                return;
            }

            // Switch tree tab
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _currentTreeIndex = (_currentTreeIndex - 1 + TreeOrder.Length) % TreeOrder.Length;
                _currentNodeIndex = 0;
                RefreshDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                _currentTreeIndex = (_currentTreeIndex + 1) % TreeOrder.Length;
                _currentNodeIndex = 0;
                RefreshDisplay();
            }

            // Navigate nodes
            var nodes = GetCurrentNodes();
            if (nodes.Count == 0) return;

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                _currentNodeIndex = (_currentNodeIndex - 1 + nodes.Count) % nodes.Count;
                RefreshDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                _currentNodeIndex = (_currentNodeIndex + 1) % nodes.Count;
                RefreshDisplay();
            }

            // Purchase
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                OnPurchaseClicked();

            // Assign to active slot
            if (_skillTreeManager != null && nodes.Count > _currentNodeIndex)
            {
                var node = nodes[_currentNodeIndex];
                if (node.SkillCategory == SkillCategory.EquippableSkill && _skillTreeManager.IsNodePurchased(node.SkillNodeId))
                {
                    if (Input.GetKeyDown(KeyCode.R)) AssignSlot(0, node.UnlockedSkillActionId);
                    else if (Input.GetKeyDown(KeyCode.T)) AssignSlot(1, node.UnlockedSkillActionId);
                    else if (Input.GetKeyDown(KeyCode.Y)) AssignSlot(2, node.UnlockedSkillActionId);
                    else if (Input.GetKeyDown(KeyCode.G)) AssignSlot(3, node.UnlockedSkillActionId);
                }
            }
        }

        private void AssignSlot(int slotIndex, string skillActionId)
        {
            if (_skillTreeManager == null || string.IsNullOrEmpty(skillActionId)) return;
            _skillTreeManager.TryAssignActiveSlot(slotIndex, skillActionId);
            RefreshDisplay();
        }

        private void OnPurchaseClicked()
        {
            if (_skillTreeManager == null) return;
            var nodes = GetCurrentNodes();
            if (nodes.Count == 0 || _currentNodeIndex >= nodes.Count) return;

            var bootstrap = GameBootstrap.Instance;
            int level = bootstrap?.PlayerProgressionManager?.Level ?? 1;
            _skillTreeManager.TryPurchaseNode(nodes[_currentNodeIndex].SkillNodeId, level, out _feedback);
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (_skillTreeManager == null) return;
            var state = _skillTreeManager.State;

            if (_skillPointsText != null)
                _skillPointsText.text = $"Skill Points: {state.AvailableSkillPoints}";

            string treeId = _currentTreeIndex < TreeOrder.Length ? TreeOrder[_currentTreeIndex] : string.Empty;
            if (_treeNameText != null)
                _treeNameText.text = treeId.Length > 0 ? char.ToUpper(treeId[0]) + treeId.Substring(1) : string.Empty;

            var nodes = GetCurrentNodes();
            if (nodes.Count == 0)
            {
                ClearNodeInfo();
                return;
            }

            if (_currentNodeIndex >= nodes.Count) _currentNodeIndex = 0;
            var selected = nodes[_currentNodeIndex];

            if (_nodeNameText != null)
                _nodeNameText.text = selected.DisplayName;
            if (_nodeDescText != null)
                _nodeDescText.text = selected.Description;

            if (_nodeStatusText != null)
            {
                bool purchased = state.IsPurchased(selected.SkillNodeId);
                string status = purchased ? "[Comprado]" : $"[{selected.SkillPointCost} SP]";
                if (selected.SkillCategory == SkillCategory.EquippableSkill && purchased)
                    status += " [Equipavel: R/T/Y/G]";
                else if (selected.SkillCategory == SkillCategory.PassiveSkill && purchased)
                    status += " [Passiva ativa]";
                if (!string.IsNullOrWhiteSpace(_feedback))
                    status += $"\n{_feedback}";
                _nodeStatusText.text = status;
            }

            if (_purchaseButton != null)
                _purchaseButton.interactable = !state.IsPurchased(selected.SkillNodeId)
                    && state.AvailableSkillPoints >= selected.SkillPointCost;
        }

        private void ClearNodeInfo()
        {
            if (_nodeNameText != null) _nodeNameText.text = string.Empty;
            if (_nodeDescText != null) _nodeDescText.text = string.Empty;
            if (_nodeStatusText != null) _nodeStatusText.text = string.Empty;
        }

        private List<SkillNodeDataSO> GetCurrentNodes()
        {
            if (_skillTreeManager == null) return new List<SkillNodeDataSO>();
            string treeId = _currentTreeIndex < TreeOrder.Length ? TreeOrder[_currentTreeIndex] : string.Empty;
            if (!_skillTreeManager.TreeIndex.TryGetValue(treeId, out var tree))
                return new List<SkillNodeDataSO>();
            return tree.Nodes;
        }
    }
}
