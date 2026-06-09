using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player.Progression;
using CindarsHope.Skills;
using CindarsHope.UI.Routing;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.UI.Skills
{
    [DisallowMultipleComponent]
    public sealed class SkillTreeGameplayPanelController : MonoBehaviour
    {
        private static readonly string[] TreeOrder = { "melee", "ranged", "magic", "survival", "crafting" };
        private static readonly string[] SlotKeys = { "R", "T", "Y", "G" };
        private static SkillTreeGameplayPanelController _instance;

        private bool _isOpen;
        private string _selectedTreeId = string.Empty;
        private string _feedback = string.Empty;
        private Vector2 _scroll;
        // Keyboard navigation indices
        private int _selectedHomeIndex;
        private int _selectedNodeIndex;
        // Cache for available tree count and node count in current tree
        private int _availableTreeCount;
        private int _currentTreeNodeCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("SkillTreeGameplayPanelController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SkillTreeGameplayPanelController>();
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

        private void OnEnable()
        {
            // WAVE_INTEGRATION_10: subscribe so GameplayInputRouter's U-key event opens this panel.
            GameEventBus.Subscribe<SkillTreeOpenedEvent>(OnSkillTreeOpenedEvent);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<SkillTreeOpenedEvent>(OnSkillTreeOpenedEvent);
            if (_isOpen)
            {
                Close();
            }
        }

        private void OnSkillTreeOpenedEvent(SkillTreeOpenedEvent _)
        {
            // Fired by GameplayInputRouter when it is active. Guard against double-open.
            if (!_isOpen)
            {
                Toggle();
            }
        }

        private void Update()
        {
            // When GameplayInputRouter is active it publishes SkillTreeOpenedEvent on U,
            // so only skip the direct U toggle to avoid double-open. Navigation must still run.
            if (!GameplayInputRouter.IsActive && global::UnityEngine.Input.GetKeyDown(KeyCode.U))
            {
                Toggle();
            }

            if (!_isOpen)
            {
                return;
            }

            if (global::UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                if (!string.IsNullOrEmpty(_selectedTreeId))
                {
                    _selectedTreeId = string.Empty;
                    _selectedNodeIndex = 0;
                }
                else
                {
                    Close();
                }
                return;
            }

            UpdateKeyboardNavigation();
        }

        private void UpdateKeyboardNavigation()
        {
            if (string.IsNullOrEmpty(_selectedTreeId))
            {
                // Home view: W/S navigate trees, A/D also navigate, E enters tree
                if (global::UnityEngine.Input.GetKeyDown(KeyCode.W) || global::UnityEngine.Input.GetKeyDown(KeyCode.UpArrow)
                    || global::UnityEngine.Input.GetKeyDown(KeyCode.A) || global::UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    _selectedHomeIndex = Mathf.Max(0, _selectedHomeIndex - 1);
                }
                else if (global::UnityEngine.Input.GetKeyDown(KeyCode.S) || global::UnityEngine.Input.GetKeyDown(KeyCode.DownArrow)
                    || global::UnityEngine.Input.GetKeyDown(KeyCode.D) || global::UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
                {
                    var maxIndex = Mathf.Max(0, _availableTreeCount - 1);
                    _selectedHomeIndex = Mathf.Min(maxIndex, _selectedHomeIndex + 1);
                }
                else if (global::UnityEngine.Input.GetKeyDown(KeyCode.E) || global::UnityEngine.Input.GetKeyDown(KeyCode.Return))
                {
                    EnterSelectedTree();
                }
            }
            else
            {
                // Detail view: W/S navigate nodes, A/D navigate trees, E buys node
                if (global::UnityEngine.Input.GetKeyDown(KeyCode.W) || global::UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
                {
                    _selectedNodeIndex = Mathf.Max(0, _selectedNodeIndex - 1);
                }
                else if (global::UnityEngine.Input.GetKeyDown(KeyCode.S) || global::UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
                {
                    var maxNode = Mathf.Max(0, _currentTreeNodeCount - 1);
                    _selectedNodeIndex = Mathf.Min(maxNode, _selectedNodeIndex + 1);
                }
                else if (global::UnityEngine.Input.GetKeyDown(KeyCode.A) || global::UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    NavigateToAdjacentTree(-1);
                }
                else if (global::UnityEngine.Input.GetKeyDown(KeyCode.D) || global::UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
                {
                    NavigateToAdjacentTree(1);
                }
                else if (global::UnityEngine.Input.GetKeyDown(KeyCode.E) || global::UnityEngine.Input.GetKeyDown(KeyCode.Return))
                {
                    BuySelectedNode();
                }
                else if (global::UnityEngine.Input.GetKeyDown(KeyCode.R)) EquipSelectedNodeToSlot(0);
                else if (global::UnityEngine.Input.GetKeyDown(KeyCode.T)) EquipSelectedNodeToSlot(1);
                else if (global::UnityEngine.Input.GetKeyDown(KeyCode.Y)) EquipSelectedNodeToSlot(2);
                else if (global::UnityEngine.Input.GetKeyDown(KeyCode.G)) EquipSelectedNodeToSlot(3);
            }
        }

        private void EquipSelectedNodeToSlot(int slotIndex)
        {
            var manager = GameBootstrap.Instance?.SkillTreeManager;
            if (manager == null || string.IsNullOrEmpty(_selectedTreeId)) return;
            if (!manager.TreeIndex.TryGetValue(_selectedTreeId, out var tree)) return;
            if (_selectedNodeIndex < 0 || _selectedNodeIndex >= tree.Nodes.Count) return;
            var node = tree.Nodes[_selectedNodeIndex];
            if (!manager.IsNodePurchased(node.SkillNodeId)) return;
            if (node.SkillCategory != SkillCategory.EquippableSkill) return;
            if (string.IsNullOrEmpty(node.UnlockedSkillActionId)) return;
            _feedback = manager.TryAssignActiveSlot(slotIndex, node.UnlockedSkillActionId)
                ? $"Skill equipada em {SlotKeys[slotIndex]}."
                : "Não foi possível equipar neste slot.";
        }

        private void EnterSelectedTree()
        {
            var manager = GameBootstrap.Instance?.SkillTreeManager;
            if (manager == null)
            {
                return;
            }

            var count = 0;
            foreach (var treeId in TreeOrder)
            {
                if (!manager.TreeIndex.ContainsKey(treeId))
                {
                    continue;
                }

                if (count == _selectedHomeIndex)
                {
                    _selectedTreeId = treeId;
                    _selectedNodeIndex = 0;
                    _scroll = Vector2.zero;
                    return;
                }

                count++;
            }
        }

        private void NavigateToAdjacentTree(int direction)
        {
            var manager = GameBootstrap.Instance?.SkillTreeManager;
            if (manager == null)
            {
                return;
            }

            // Build list of available trees
            var available = new System.Collections.Generic.List<string>();
            foreach (var treeId in TreeOrder)
            {
                if (manager.TreeIndex.ContainsKey(treeId))
                {
                    available.Add(treeId);
                }
            }

            var currentIdx = available.IndexOf(_selectedTreeId);
            if (currentIdx < 0)
            {
                return;
            }

            var newIdx = Mathf.Clamp(currentIdx + direction, 0, available.Count - 1);
            if (newIdx != currentIdx)
            {
                _selectedTreeId = available[newIdx];
                _selectedNodeIndex = 0;
                _scroll = Vector2.zero;
                _selectedHomeIndex = newIdx;
            }
        }

        private void BuySelectedNode()
        {
            var manager = GameBootstrap.Instance?.SkillTreeManager;
            var progression = GameBootstrap.Instance?.PlayerProgressionManager;
            if (manager == null || string.IsNullOrEmpty(_selectedTreeId))
            {
                return;
            }

            if (!manager.TreeIndex.TryGetValue(_selectedTreeId, out var tree))
            {
                return;
            }

            if (_selectedNodeIndex < 0 || _selectedNodeIndex >= tree.Nodes.Count)
            {
                return;
            }

            var node = tree.Nodes[_selectedNodeIndex];
            manager.TryPurchaseNode(node.SkillNodeId, progression?.Level ?? 1, out _feedback);
        }

        private void OnGUI()
        {
            if (!_isOpen)
            {
                return;
            }

            var manager = GameBootstrap.Instance?.SkillTreeManager;
            var progression = GameBootstrap.Instance?.PlayerProgressionManager;
            manager?.RebindProgressionManager(progression);
            var rect = new Rect((Screen.width - 580f) * 0.5f, (Screen.height - 590f) * 0.5f, 580f, 590f);
            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label("Skill Trees");
            GUILayout.Label($"Skill Points disponíveis: {progression?.UnspentSkillPoints ?? 0}");

            if (manager == null)
            {
                GUILayout.Label("SkillTreeManager não disponível.");
            }
            else if (string.IsNullOrEmpty(_selectedTreeId))
            {
                DrawTreeHome(manager);
            }
            else
            {
                DrawTreeDetail(manager, progression);
            }

            if (!string.IsNullOrWhiteSpace(_feedback))
            {
                GUILayout.Label($"Feedback: {_feedback}");
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Fechar (Esc)"))
            {
                Close();
            }
            GUILayout.EndArea();
        }

        private void DrawTreeHome(SkillTreeManager manager)
        {
            GUILayout.Label("[W/S ou A/D] navegar  [E] entrar na arvore");
            var rowIndex = 0;
            foreach (var treeId in TreeOrder)
            {
                if (!manager.TreeIndex.TryGetValue(treeId, out var tree))
                {
                    continue;
                }

                var prevColor = GUI.backgroundColor;
                if (rowIndex == _selectedHomeIndex)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                if (GUILayout.Button(tree.DisplayName))
                {
                    _selectedHomeIndex = rowIndex;
                    _selectedTreeId = treeId;
                    _selectedNodeIndex = 0;
                    _scroll = Vector2.zero;
                }
                GUI.backgroundColor = prevColor;
                GUILayout.Label(tree.Description);
                GUILayout.Space(4f);
                rowIndex++;
            }
            _availableTreeCount = rowIndex;
        }

        private void DrawTreeDetail(SkillTreeManager manager, PlayerProgressionManager progression)
        {
            if (!manager.TreeIndex.TryGetValue(_selectedTreeId, out var tree))
            {
                _selectedTreeId = string.Empty;
                return;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Voltar (Esc)", GUILayout.Width(110f)))
            {
                _selectedTreeId = string.Empty;
                _selectedNodeIndex = 0;
                return;
            }
            GUILayout.Label($"Skill Tree - {tree.DisplayName}  [A/D] trocar arvore");
            GUILayout.EndHorizontal();
            GUILayout.Label("[W/S] navegar  [E] comprar  [R/T/Y/G] equipar em slot");
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(390f));
            _currentTreeNodeCount = tree.Nodes.Count;
            for (var nodeIdx = 0; nodeIdx < tree.Nodes.Count; nodeIdx++)
            {
                DrawNode(manager, progression, tree.Nodes[nodeIdx], nodeIdx);
            }
            GUILayout.EndScrollView();
            DrawActiveSlots(manager);
        }

        private void DrawNode(SkillTreeManager manager, PlayerProgressionManager progression, SkillNodeDataSO node, int nodeIdx)
        {
            var purchased = manager.IsNodePurchased(node.SkillNodeId);
            var requirementsMet = RequirementsMet(manager, node, progression?.Level ?? 1);
            var hasPoints = (progression?.UnspentSkillPoints ?? 0) >= node.SkillPointCost;
            var status = purchased ? "Comprado" : requirementsMet ? hasPoints ? "Disponível" : "Sem pontos" : "Bloqueado";

            var prevColor = GUI.backgroundColor;
            if (nodeIdx == _selectedNodeIndex)
            {
                GUI.backgroundColor = Color.yellow;
            }
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"{node.DisplayName} [{status}] - Custo: {node.SkillPointCost} SP");
            GUILayout.Label(node.Description);
            GUILayout.Label($"Requisitos: {FormatRequirements(node)}");
            GUI.enabled = !purchased && requirementsMet && hasPoints;
            if (GUILayout.Button("Comprar"))
            {
                _selectedNodeIndex = nodeIdx;
                manager.TryPurchaseNode(node.SkillNodeId, progression?.Level ?? 1, out _feedback);
            }
            GUI.enabled = true;

            if (purchased && node.SkillCategory == SkillCategory.EquippableSkill
                && !string.IsNullOrEmpty(node.UnlockedSkillActionId))
            {
                GUILayout.Label("Equipar em slot:");
                GUILayout.BeginHorizontal();
                for (var i = 0; i < SlotKeys.Length; i++)
                {
                    var occupied = manager.State.GetActiveSlotSkillActionId(i) == node.UnlockedSkillActionId;
                    var slotLabel = occupied ? $"{SlotKeys[i]} [OK]" : SlotKeys[i];
                    if (GUILayout.Button(slotLabel, GUILayout.Width(65f)))
                    {
                        _selectedNodeIndex = nodeIdx;
                        if (manager.TryAssignActiveSlot(i, node.UnlockedSkillActionId))
                            _feedback = $"Skill equipada em {SlotKeys[i]}.";
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUI.backgroundColor = prevColor;
        }

        private static bool RequirementsMet(SkillTreeManager manager, SkillNodeDataSO node, int level)
        {
            if (level < node.MinimumPlayerLevel)
            {
                return false;
            }

            foreach (var prerequisite in node.PrerequisiteNodeIds)
            {
                if (!manager.IsNodePurchased(prerequisite))
                {
                    return false;
                }
            }

            return node.RequiredPurchasedNodesInTree <= 0
                || manager.State.CountPurchasedInTree(node.TreeId) >= node.RequiredPurchasedNodesInTree;
        }

        private static string FormatRequirements(SkillNodeDataSO node)
        {
            if (node.PrerequisiteNodeIds.Count == 0 && node.MinimumPlayerLevel <= 1)
            {
                return "nenhum";
            }

            var entries = new List<string>(node.PrerequisiteNodeIds);
            if (node.MinimumPlayerLevel > 1)
            {
                entries.Add($"level {node.MinimumPlayerLevel}");
            }
            if (node.RequiredPurchasedNodesInTree > 0)
            {
                entries.Add($"{node.RequiredPurchasedNodesInTree} nodes na árvore");
            }
            return string.Join(", ", entries);
        }

        private static void DrawActiveSlots(SkillTreeManager manager)
        {
            GUILayout.Label("Active slots:");
            GUILayout.BeginHorizontal();
            for (var index = 0; index < SlotKeys.Length; index++)
            {
                var actionId = manager.State.GetActiveSlotSkillActionId(index);
                GUILayout.Label($"{SlotKeys[index]}: {(string.IsNullOrWhiteSpace(actionId) ? "vazio" : actionId)}");
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
            if (modal != null && !modal.PushModal(ModalType.SkillTree))
            {
                return;
            }

            _isOpen = true;
            _selectedTreeId = string.Empty;
            _selectedHomeIndex = 0;
            _selectedNodeIndex = 0;
            _feedback = string.Empty;
        }

        private void Close()
        {
            _isOpen = false;
            GameBootstrap.Instance?.ModalManager?.TryPopModal(ModalType.SkillTree, out _);
        }
    }
}
