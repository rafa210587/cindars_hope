using System.Collections.Generic;
using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class ResourceNodeRegistry : MonoBehaviour
    {
        [SerializeField] private ResourceNode[] _resourceNodes = System.Array.Empty<ResourceNode>();

        public List<ResourceNode> Nodes => new List<ResourceNode>(_resourceNodes);

        public void CaptureSaveData(List<ResourceNodeSaveData> outSaveData)
        {
            outSaveData.Clear();
            foreach (var node in _resourceNodes)
            {
                if (node != null)
                {
                    outSaveData.Add(new ResourceNodeSaveData
                    {
                        NodeId = node.NodeId,
                        IsCollected = node.IsCollected
                    });
                }
            }
        }

        public void RestoreFromSaveData(List<ResourceNodeSaveData> saveData)
        {
            if (saveData == null)
            {
                return;
            }

            foreach (var node in _resourceNodes)
            {
                if (node == null)
                {
                    continue;
                }

                var nodeSaveData = saveData.Find(sd => sd.NodeId == node.NodeId);
                if (nodeSaveData != null && nodeSaveData.IsCollected)
                {
                    node.SetCollected(true);
                }
            }
        }
    }

    [System.Serializable]
    public class ResourceNodeSaveData
    {
        public string NodeId;
        public bool IsCollected;
    }
}
