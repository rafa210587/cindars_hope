using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "EnemySize_", menuName = "CindarsHope/Combat/Enemy Size Profile")]
    public class EnemySizeProfileSO : ScriptableObject, IIdentifiedData
    {
        public string SizeProfileId;
        public EnemySizeClass SizeClass;

        // fable_79: SpriteScale NÃO É MAIS AUTORITATIVO para escala visual de inimigos da caverna.
        // A escala visual agora vem de EnemyScaleResolver.ResolveVisualScale(BestiarySize, IsMiniBoss, IsBoss)
        // (player-relative). Este campo é mantido apenas por compatibilidade de assets existentes e para
        // o gerador CreateDefaultEnemyProfiles (que precisa persistir algo no SO). CaveRuntimeMaterializer
        // ignora este campo desde fable_79. Física (collider/footprint/pathing) continua nos campos abaixo.
        [Header("Visual — NAO AUTORITATIVO para escala (fable_79): ver EnemyScaleResolver.ResolveVisualScale")]
        [Tooltip("fable_79: CAMPO NAO AUTORITATIVO. Escala visual vem de EnemyScaleResolver.ResolveVisualScale " +
                 "(player-relative). Mantido apenas por compatibilidade de assets existentes. " +
                 "CaveRuntimeMaterializer ignora este campo desde fable_79.")]
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

        [Header("Room Requirements")]
        [Tooltip("Minimum room dimension (cells) required to spawn this size class.")]
        public int MinimumRoomSize = 6;

        string IIdentifiedData.Id => SizeProfileId;

        private void OnValidate()
        {
            SpriteScale = Mathf.Max(0.1f, SpriteScale);
            ColliderRadius = Mathf.Max(0.1f, ColliderRadius);
            FootprintCells = Mathf.Max(1f, FootprintCells);
            KnockbackMultiplier = Mathf.Max(0f, KnockbackMultiplier);
            PathingRadius = Mathf.Max(0.1f, PathingRadius);

            MinimumRoomSize = Mathf.Max(4, MinimumRoomSize);

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
