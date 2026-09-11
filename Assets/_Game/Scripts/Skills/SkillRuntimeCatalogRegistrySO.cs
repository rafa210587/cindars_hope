using UnityEngine;

namespace CindarsHope.Skills
{
    [CreateAssetMenu(fileName = "SkillRuntimeCatalogRegistry", menuName = "CindarsHope/Skills/Runtime Catalog Registry")]
    public sealed class SkillRuntimeCatalogRegistrySO : ScriptableObject
    {
        [SerializeField] private SkillTreeRegistrySO _treeRegistry;
        [SerializeField] private SkillNodeDatabaseSO _nodeDatabase;
        [SerializeField] private SkillActionDatabaseSO _actionDatabase;

        public SkillTreeRegistrySO TreeRegistry => _treeRegistry;
        public SkillNodeDatabaseSO NodeDatabase => _nodeDatabase;
        public SkillActionDatabaseSO ActionDatabase => _actionDatabase;

#if UNITY_EDITOR
        public void Configure(SkillTreeRegistrySO trees, SkillNodeDatabaseSO nodes, SkillActionDatabaseSO actions)
        {
            _treeRegistry = trees;
            _nodeDatabase = nodes;
            _actionDatabase = actions;
        }
#endif
    }
}
