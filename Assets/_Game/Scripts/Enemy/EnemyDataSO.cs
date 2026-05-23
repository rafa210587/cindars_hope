using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [CreateAssetMenu(fileName = "Enemy_", menuName = "CindarsHope/Enemy/EnemyData")]
    public class EnemyDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public int Level;
        public int MaxHP;
        public int Damage;
        public int Defense;
        public int XpReward;
        public float MovementSpeed = 3f;
        public float DetectionRange = 15f;
        public int Strength = 1;
        public int Dexterity = 1;
        public int Constitution = 1;
        public string AIBehaviorId;
        public string LootTableId;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            Level = Mathf.Max(1, Level);
            MaxHP = Mathf.Max(1, MaxHP);
            Damage = Mathf.Max(0, Damage);
            Defense = Mathf.Max(0, Defense);
            XpReward = Mathf.Max(0, XpReward);
            MovementSpeed = Mathf.Max(0.5f, MovementSpeed);
            DetectionRange = Mathf.Max(1f, DetectionRange);
        }
    }
}
