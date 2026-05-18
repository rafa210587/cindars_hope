using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "Enemy_Slime", menuName = "CindarsHope/Combat/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        public string enemyId;
        public int maxHp = 10;
        public int contactDamage = 1;
        public float contactDamageCooldownSeconds = 1f;
        public string dropItemId = "ore_copper";
        public int dropAmount = 1;
    }
}
