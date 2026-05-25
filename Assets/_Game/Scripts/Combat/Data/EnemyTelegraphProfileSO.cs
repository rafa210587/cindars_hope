using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "EnemyTelegraph_", menuName = "CindarsHope/Combat/Enemy Telegraph Profile")]
    public class EnemyTelegraphProfileSO : ScriptableObject, IIdentifiedData
    {
        public string TelegraphProfileId;
        public Color BlinkColor = Color.yellow;
        public float BlinkFrequency = 0.1f;
        public float WindupSeconds = 0.5f;

        string IIdentifiedData.Id => TelegraphProfileId;

        private void OnValidate()
        {
            BlinkFrequency = Mathf.Max(0.05f, BlinkFrequency);
            WindupSeconds = Mathf.Max(0.1f, WindupSeconds);

            if (string.IsNullOrWhiteSpace(TelegraphProfileId))
                TelegraphProfileId = "telegraph_" + name.ToLower();
        }
    }
}
