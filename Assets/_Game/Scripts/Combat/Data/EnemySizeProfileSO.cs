using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "EnemySize_", menuName = "CindarsHope/Combat/Enemy Size Profile")]
    public class EnemySizeProfileSO : ScriptableObject, IIdentifiedData
    {
        public string SizeProfileId;
        public EnemySizeClass SizeClass;

        [Header("Visual")]
        public float SpriteScale = 1f;

        [Header("Physics")]
        public float ColliderRadius = 0.5f;
        public float FootprintCells = 1f;

        [Header("Offsets")]
        public Vector2 TargetingOffset = Vector2.zero;
        public Vector2 DamageNumberOffset = Vector2.zero;

        [Header("Combat")]
        public float KnockbackMultiplier = 1f;

        [Header("Pathing")]
        public float PathingRadius = 0.5f;

        string IIdentifiedData.Id => SizeProfileId;

        private void OnValidate()
        {
            SpriteScale = Mathf.Max(0.1f, SpriteScale);
            ColliderRadius = Mathf.Max(0.1f, ColliderRadius);
            FootprintCells = Mathf.Max(1f, FootprintCells);
            KnockbackMultiplier = Mathf.Max(0f, KnockbackMultiplier);
            PathingRadius = Mathf.Max(0.1f, PathingRadius);

            if (string.IsNullOrWhiteSpace(SizeProfileId))
                SizeProfileId = "size_" + name.ToLower();
        }
    }

    public enum EnemySizeClass
    {
        Tiny,
        Small,
        Medium,
        Large,
        Huge,
        Boss
    }
}
