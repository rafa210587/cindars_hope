using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [CreateAssetMenu(fileName = "BestiaryEntry_", menuName = "CindarsHope/Enemy/BestiaryEntry")]
    public class BestiaryDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string EnemyId;
        public string CommonName;
        [TextArea(3, 5)] public string Lore;
        public int FirstEncounteredLevel;
        public string FactionId;
        public int KillCount;
        public bool IsDiscovered;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            FirstEncounteredLevel = Mathf.Max(1, FirstEncounteredLevel);
            KillCount = Mathf.Max(0, KillCount);
        }
    }
}
