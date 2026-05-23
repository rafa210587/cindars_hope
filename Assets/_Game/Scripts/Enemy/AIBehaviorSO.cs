using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Enemy.AI
{
    [CreateAssetMenu(fileName = "AIBehavior_", menuName = "CindarsHope/Enemy/AIBehavior")]
    public class AIBehaviorSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string BehaviorName;
        public AIType Type;
        public float PatrolDistance = 10f;
        public float AttackRange = 2f;
        public float ChaseDuration = 10f;
        public int ActionCooldownMs = 1000;
        public bool UsesRangedAttacks;
        public bool CastsSpells;
        public float AggressionLevel = 0.5f;
        public int MaxCombatDistance = 20;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            PatrolDistance = Mathf.Max(1f, PatrolDistance);
            AttackRange = Mathf.Max(0.5f, AttackRange);
            ChaseDuration = Mathf.Max(1f, ChaseDuration);
            ActionCooldownMs = Mathf.Max(100, ActionCooldownMs);
            AggressionLevel = Mathf.Clamp01(AggressionLevel);
            MaxCombatDistance = Mathf.Max(1, MaxCombatDistance);
        }
    }

    public enum AIType
    {
        None,
        Patrol,
        Aggressive,
        Defensive,
        Ranged,
        Support,
        Boss
    }
}
