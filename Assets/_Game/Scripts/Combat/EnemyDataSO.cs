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
        public string dropItemId = "item_wood";
        public int dropAmount = 1;

        [Header("Progression")]
        public int enemyLevel = 1;
        public EnemyDifficulty baseDifficulty = EnemyDifficulty.Easy;
        public int xpRewardOverride;

        private void OnValidate()
        {
            maxHp = Mathf.Max(1, maxHp);
            contactDamage = Mathf.Max(0, contactDamage);
            contactDamageCooldownSeconds = Mathf.Max(0.01f, contactDamageCooldownSeconds);
            receivedKnockbackMultiplier = Mathf.Max(0f, receivedKnockbackMultiplier);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            detectionRadius = Mathf.Max(0f, detectionRadius);
            stopDistance = Mathf.Max(0f, stopDistance);
            hitFlashDuration = Mathf.Max(0.01f, hitFlashDuration);
            dropAmount = Mathf.Max(0, dropAmount);
            enemyLevel = Mathf.Max(1, enemyLevel);
            xpRewardOverride = Mathf.Max(0, xpRewardOverride);
        }
    }

    public enum EnemyDifficulty
    {
        VeryEasy,
        Easy,
        Normal,
        Hard,
        Elite,
        MiniBoss,
        Boss
    }
}
