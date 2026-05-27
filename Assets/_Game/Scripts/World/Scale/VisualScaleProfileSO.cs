using UnityEngine;

namespace CindarsHope.World.Scale
{
    public enum EntityScaleCategory
    {
        Player,
        NPC,
        EnemyTiny,
        EnemySmall,
        EnemyMedium,
        EnemyLarge,
        EnemyHuge,
        EnemyBoss,
        TreeSmall,
        TreeMedium,
        TreeLarge,
        RockSmall,
        RockMedium,
        Pickup,
        Chest,
        Workbench,
        Forge,
        CookingStation,
        FarmObject,
        CavePortal,
        CheckpointPortal,
        Corpse,
    }

    [CreateAssetMenu(fileName = "VisualScaleProfile_", menuName = "CindarsHope/Scale/Visual Scale Profile")]
    public sealed class VisualScaleProfileSO : ScriptableObject
    {
        [Tooltip("Identifies this profile for registry lookup.")]
        public string ProfileId;
        public string DisplayName;
        public EntityScaleCategory Category;

        [Header("Visual")]
        [Tooltip("Multiplier applied to transform.localScale. Does not affect collider.")]
        public float VisualScale = 1f;

        [Header("Collider")]
        [Tooltip("Multiplier applied to the BoxCollider2D or CircleCollider2D size. 0 = keep collider as-is.")]
        public float ColliderScale = 1f;
        [Tooltip("Physical footprint size in Unity units (used by AI/pathfinding; 0 = auto from collider).")]
        public Vector2 FootprintSize = Vector2.zero;

        [Header("Interaction")]
        [Tooltip("Radius within which the player can interact with this entity. 0 = use InteractionSystem default.")]
        public float InteractionRadius = 0f;
        [Tooltip("Radius for selection highlighting. 0 = auto.")]
        public float SelectionRadius = 0f;

        [Header("Offsets")]
        [Tooltip("Nameplate/HUD label offset relative to entity pivot.")]
        public Vector2 NameplateOffset = new Vector2(0f, 1f);
        [Tooltip("Context hint offset relative to entity pivot.")]
        public Vector2 HintOffset = new Vector2(0f, 1.2f);
        [Tooltip("Damage number spawn offset relative to entity pivot.")]
        public Vector2 DamageNumberOffset = new Vector2(0f, 1f);
        [Tooltip("Shadow scale relative to VisualScale. 0 = no shadow override.")]
        public float ShadowScale = 0f;

        private void OnValidate()
        {
            VisualScale = Mathf.Max(0.1f, VisualScale);
            ColliderScale = Mathf.Max(0f, ColliderScale);
            InteractionRadius = Mathf.Max(0f, InteractionRadius);
            SelectionRadius = Mathf.Max(0f, SelectionRadius);
            FootprintSize = new Vector2(Mathf.Max(0f, FootprintSize.x), Mathf.Max(0f, FootprintSize.y));
        }
    }
}
