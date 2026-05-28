using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "EnemyActionSet_", menuName = "CindarsHope/Combat/Enemy Action Set")]
    public class EnemyActionSetSO : ScriptableObject, IIdentifiedData
    {
        public string ActionSetId;
        public string DisplayName;
        [SerializeField] public string[] ActionIds = new string[0];
        public string FallbackActionId;
        public string[] RoleTags = new string[0];
        [Multiline(2)] public string Notes;

        string IIdentifiedData.Id => ActionSetId;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(ActionSetId))
                ActionSetId = "actionset_" + name.ToLower();
        }
    }
}
