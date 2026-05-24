using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat.Weapon
{
    [CreateAssetMenu(fileName = "UnarmedAttack_", menuName = "CindarsHope/Combat/Unarmed Attack")]
    public class UnarmedAttackDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id = "unarmed_default";
        public string DisplayName = "Punch";
        [TextArea] public string Description = "A basic unarmed attack.";
        public int BaseDamage = 3;
        public float BaseCooldownSeconds = 0.4f;
        public float StaminaCost = 10f;
        public float Range = 0.5f;
        public float ArcDegrees = 120f;
        public DamageType DamageType = DamageType.Physical;

        string IIdentifiedData.Id => Id;
    }
}
