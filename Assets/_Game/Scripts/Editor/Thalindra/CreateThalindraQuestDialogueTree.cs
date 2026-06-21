using System.Collections.Generic;
using CindarsHope.NPC;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    public static class CreateThalindraQuestDialogueTree
    {
        private const string AssetPath =
            "Assets/_Game/Data/NPC/Thalindra/DialogueTree_Thalindra_QuestOffer.asset";

        public static void Create()
        {
            System.IO.Directory.CreateDirectory("Assets/_Game/Data/NPC/Thalindra");

            var tree = ScriptableObject.CreateInstance<DialogueTreeSO>();
            tree.StartNodeId = "node_greeting";

            var greeting = new DialogueNode
            {
                NodeId = "node_greeting",
                Text   = "Cindar, há madeira e pedras espalhadas pelas estradas ao redor da cidade. Poderia trazer materiais para mim?",
                Choices = new List<DialogueChoice>
                {
                    new DialogueChoice
                    {
                        Label         = "Qual é a tarefa?",
                        NextNodeId    = "",
                        ActionType    = DialogueActionType.OfferQuest,
                        ActionPayload = "quest_first_supplies_for_cindar"
                    },
                    new DialogueChoice
                    {
                        Label      = "Não tenho tempo agora.",
                        NextNodeId = "",
                        ActionType = DialogueActionType.CloseDialogue
                    }
                }
            };

            tree.Nodes = new List<DialogueNode> { greeting };

            if (System.IO.File.Exists(AssetPath))
                AssetDatabase.DeleteAsset(AssetPath);

            AssetDatabase.CreateAsset(tree, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CreateThalindraQuestDialogueTree] Created: {AssetPath}");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = tree;
        }
    }
}
