using System.Collections.Generic;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.NPC
{
    [CreateAssetMenu(fileName = "DialogueTree", menuName = "CindarsHope/NPC/Dialogue Tree")]
    public class DialogueTreeSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string StartNodeId;
        public List<DialogueNode> Nodes = new();

        string IIdentifiedData.Id => Id;

        public DialogueNode GetNodeById(string nodeId)
        {
            return Nodes.Find(n => n.NodeId == nodeId);
        }
    }
}
