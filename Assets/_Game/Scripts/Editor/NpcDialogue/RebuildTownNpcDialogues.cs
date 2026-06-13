using CindarsHope.NPC;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.NpcDialogue
{
    /// <summary>
    /// Writes the expanded TownNpcDialogueLibrary content into the existing DialogueTreeSO
    /// assets referenced by each NpcDataSO (creating the tree asset when missing).
    /// Uses AssetDatabase APIs only — never raw YAML edits (unity-yaml-editing-policy).
    ///
    /// Human action: run "CindarsHope/NPCs/Rebuild Town NPC Dialogues (Expanded)" once in
    /// the Unity Editor after pulling this change.
    /// </summary>
    public static class RebuildTownNpcDialogues
    {
        private const string NpcDataFolder = "Assets/_Game/Data/NPCs";
        private const string DialogueFolder = "Assets/_Game/Data/NPCs/Dialogues";

        [MenuItem("CindarsHope/NPCs/Rebuild Town NPC Dialogues (Expanded)", priority = 40)]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("RebuildTownNpcDialogues: exit Play Mode first.");
                return;
            }

            int updated = 0;
            int created = 0;
            int missingNpcData = 0;

            foreach (var content in TownNpcDialogueLibrary.AllContent)
            {
                var npcData = FindNpcData(content.NpcId);
                if (npcData == null)
                {
                    Debug.LogWarning($"RebuildTownNpcDialogues: NpcDataSO not found for '{content.NpcId}'. Skipped.");
                    missingNpcData++;
                    continue;
                }

                var tree = npcData.DialogueTree;
                if (tree == null)
                {
                    EnsureFolder();
                    tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
                    tree.Id = content.DialogueSetId;
                    AssetDatabase.CreateAsset(tree, $"{DialogueFolder}/DialogueTree_{content.NpcId}.asset");
                    npcData.DialogueTree = tree;
                    EditorUtility.SetDirty(npcData);
                    created++;
                }

                tree.Id = content.DialogueSetId;
                tree.StartNodeId = "node_greeting";
                tree.Nodes = TownNpcDialogueLibrary.BuildNodes(content);
                EditorUtility.SetDirty(tree);
                updated++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"RebuildTownNpcDialogues: {updated} dialogue trees updated ({created} created, {missingNpcData} NPCs without data asset). Nodes per NPC: {TownNpcDialogueLibrary.NodesPerNpc}.");
        }

        private static NpcDataSO FindNpcData(string npcId)
        {
            var guids = AssetDatabase.FindAssets("t:NpcDataSO", new[] { NpcDataFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>(path);
                if (npcData != null && npcData.NpcId == npcId)
                {
                    return npcData;
                }
            }

            return null;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(DialogueFolder))
            {
                AssetDatabase.CreateFolder(NpcDataFolder, "Dialogues");
            }
        }
    }
}
