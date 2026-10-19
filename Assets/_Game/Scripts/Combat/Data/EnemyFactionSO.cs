using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "EnemyFaction_", menuName = "CindarsHope/Combat/Enemy Faction")]
    public class EnemyFactionSO : ScriptableObject, IIdentifiedData
    {
        public string factionId;
        public string DisplayName;
        public Color FactionColor = Color.white;
        [TextArea] public string Description;

        string IIdentifiedData.Id => factionId;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(factionId))
                factionId = "faction_" + name.ToLower();
        }
    }
}
