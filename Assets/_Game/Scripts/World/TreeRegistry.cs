using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class TreeRegistry : MonoBehaviour
    {
        [SerializeField] private TreeNode[] _trees;

        public IReadOnlyList<TreeNode> Trees => _trees;

        public void Configure(TreeNode[] trees)
        {
            _trees = trees ?? new TreeNode[0];
        }

        public List<TreeSaveData> CaptureSaveData()
        {
            var trees = new List<TreeSaveData>();
            if (_trees == null)
            {
                return trees;
            }

            foreach (var tree in _trees)
            {
                if (tree == null)
                {
                    continue;
                }

                trees.Add(tree.CaptureSaveData());
            }

            return trees;
        }

        public void RestoreFromSaveData(IReadOnlyList<TreeSaveData> trees)
        {
            if (trees == null || _trees == null)
            {
                return;
            }

            foreach (var treeData in trees)
            {
                if (treeData == null)
                {
                    continue;
                }

                var tree = GetTreeByIndex(treeData.TreeIndex);
                if (tree == null)
                {
                    Debug.LogWarning($"TreeRegistry skipped saved tree index {treeData.TreeIndex} because no matching tree exists.", this);
                    continue;
                }

                tree.RestoreFromSaveData(treeData);
            }
        }

        private TreeNode GetTreeByIndex(int treeIndex)
        {
            if (_trees == null)
            {
                return null;
            }

            foreach (var tree in _trees)
            {
                if (tree == null)
                {
                    continue;
                }

                var data = tree.CaptureSaveData();
                if (data.TreeIndex == treeIndex)
                {
                    return tree;
                }
            }

            return null;
        }
    }
}
