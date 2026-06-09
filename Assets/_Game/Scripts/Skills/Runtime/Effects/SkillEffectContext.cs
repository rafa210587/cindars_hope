using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11: Carries all data needed by an executor to apply a skill effect.
    // No Unity object references are saved; this is a transient runtime-only struct.
    public sealed class SkillEffectContext
    {
        public string SkillActionId { get; set; }
        public string EffectId { get; set; }
        public int ActiveSlotIndex { get; set; }

        // Caster GameObject — resolved at runtime from player, never serialized.
        public GameObject Caster { get; set; }

        // Target — null if no target or self-targeting effect.
        public GameObject Target { get; set; }

        // World position for area effects.
        public Vector2 WorldPosition { get; set; }

        // Scene name at time of execution.
        public string SceneName { get; set; }

        // Game time (seconds since start).
        public float Time { get; set; }
    }
}
