using System.Collections.Generic;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Player.Progression;
using CindarsHope.Skills;
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

        private void OnDisable()
        {
            if (_isOpen)
            {
                Close();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
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
            foreach (var treeId in TreeOrder)
            {
                if (!manager.TreeIndex.TryGetValue(treeId, out var tree))
                {
                    continue;
                }

                if (GUILayout.Button(tree.DisplayName))
                {
                    _selectedTreeId = treeId;
                    _scroll = Vector2.zero;
                }
                GUILayout.Label(tree.Description);
                GUILayout.Space(4f);
            }
        }

        private void DrawTreeDetail(SkillTreeManager manager, PlayerProgressionManager progression)
        {
            if (!manager.TreeIndex.TryGetValue(_selectedTreeId, out var tree))
            {
                _selectedTreeId = string.Empty;
                return;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Voltar", GUILayout.Width(90f)))
            {
                _selectedTreeId = string.Empty;
                return;
            }
            GUILayout.Label($"Skill Tree - {tree.DisplayName}");
            GUILayout.EndHorizontal();
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(390f));
            foreach (var node in tree.Nodes)
            {
                DrawNode(manager, progression, node);
            }
            GUILayout.EndScrollView();
            DrawActiveSlots(manager);
        }

        private void DrawNode(SkillTreeManager manager, PlayerProgressionManager progression, SkillNodeDataSO node)
        {
            var purchased = manager.IsNodePurchased(node.SkillNodeId);
            var requirementsMet = RequirementsMet(manager, node, progression?.Level ?? 1);
            var hasPoints = (progression?.UnspentSkillPoints ?? 0) >= node.SkillPointCost;
            var status = purchased ? "Comprado" : requirementsMet ? hasPoints ? "Disponível" : "Sem pontos" : "Bloqueado";

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"{node.DisplayName} [{status}] - Custo: {node.SkillPointCost} SP");
            GUILayout.Label(node.Description);
            GUILayout.Label($"Requisitos: {FormatRequirements(node)}");
            GUI.enabled = !purchased && requirementsMet && hasPoints;
            if (GUILayout.Button("Comprar"))
            {
                manager.TryPurchaseNode(node.SkillNodeId, progression?.Level ?? 1, out _feedback);
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
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
            _feedback = string.Empty;
        }

        private void Close()
        {
            _isOpen = false;
            GameBootstrap.Instance?.ModalManager?.TryPopModal(ModalType.SkillTree, out _);
        }
    }
}
