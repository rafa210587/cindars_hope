using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "Enemy_Slime", menuName = "CindarsHope/Combat/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        public string enemyId;

        [Header("Health")]
        public int maxHp = 10;

        [Header("Damage")]
        public int contactDamage = 1;
        public float contactDamageCooldownSeconds = 1f;
        public float contactKnockbackForce = 0f;

        [Header("Knockback Resistance")]
        public float receivedKnockbackResistance = 0f;
        public float receivedKnockbackMultiplier = 1f;

        [Header("Movement")]
        public float moveSpeed = 1.2f;
        public float detectionRadius = 5f;
        public float stopDistance = 0.55f;

        [Header("Feedback")]
        public Color hitFlashColor = Color.red;
        public float hitFlashDuration = 0.12f;

        [Header("Drops")]
        public string dropItemId = "ore_copper";
        public int dropAmount = 1;
    }
}
