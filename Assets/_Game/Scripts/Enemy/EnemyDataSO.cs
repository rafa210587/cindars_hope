using System;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// Legacy compatibility wrapper kept only to avoid breaking references during stabilization.
    /// The official runtime enemy model is CindarsHope.Combat.EnemyDataSO.
    /// Do not create new assets with this type.
    /// </summary>
    [Obsolete("Use CindarsHope.Combat.EnemyDataSO as the official enemy data model.")]
    public class LegacyEnemyDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public int Level = 1;
        public int MaxHP = 1;
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
            Strength = Mathf.Max(1, Strength);
            Dexterity = Mathf.Max(1, Dexterity);
            Constitution = Mathf.Max(1, Constitution);
        }
    }
}