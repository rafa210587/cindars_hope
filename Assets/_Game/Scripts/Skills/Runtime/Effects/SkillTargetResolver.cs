using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11: Resolves a skill's target based on SkillEffectTargetType.
    // Uses serialized InteractionSystem reference — no runtime global search.
    public sealed class SkillTargetResolver : MonoBehaviour
    {
        [SerializeField] private InteractionSystem _interactionSystem;

        // Wired by ActiveSkillExecutionController or GameBootstrap during runtime init.
        public void SetInteractionSystem(InteractionSystem interactionSystem)
        {
            _interactionSystem = interactionSystem;
        }

        // Resolve target GameObject given target type and caster.
        // Returns null if target cannot be resolved (executor will handle null check).
        public GameObject Resolve(SkillEffectTargetType targetType, GameObject caster)
        {
            switch (targetType)
            {
                case SkillEffectTargetType.Self:
                    return caster;

                case SkillEffectTargetType.CurrentInteractable:
                case SkillEffectTargetType.CropPlot:
                case SkillEffectTargetType.ResourceNode:
                case SkillEffectTargetType.NearestInteractable:
                    return ResolveCurrentInteractable();

                case SkillEffectTargetType.None:
                case SkillEffectTargetType.DebugFixed:
                    return null;

                default:
                    Debug.LogWarning($"[SkillTargetResolver] Unhandled target type: {targetType}");
                    return null;
            }
        }

        private GameObject ResolveCurrentInteractable()
        {
            if (_interactionSystem == null)
            {
                Debug.LogWarning("[SkillTargetResolver] InteractionSystem not set. Cannot resolve interactable target.");
                return null;
            }

            var interactable = _interactionSystem.GetCurrentInteractable();
            if (interactable == null)
                return null;

            if (interactable is Component comp)
                return comp.gameObject;

            return null;
        }
    }
}
