using CindarsHope.Player;
using CindarsHope.Player.Movement;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public readonly struct MeleeMovementResolution
    {
        public Vector2 Direction { get; }
        public float Distance { get; }
        public bool ImpactAfterMovement { get; }

        public MeleeMovementResolution(Vector2 direction, float distance, bool impactAfterMovement)
        {
            Direction = direction;
            Distance = distance;
            ImpactAfterMovement = impactAfterMovement;
        }
    }

    /// <summary>Resolves authored melee displacement without moving or spending resources.</summary>
    public sealed class MeleeMovementProfileResolver
    {
        public const string SteelAdvanceActionId = "skill_melee_avanco_aco";
        public const string BattleDashActionId = "skill_melee_battle_dash";
        public const string LeapAttackActionId = "skill_melee_leap_attack";
        public const string ChallengeShoutActionId = "skill_melee_grito_desafio";
        public const string GuardBreakChargeActionId = "skill_melee_investida_quebra_guarda";

        private readonly Collider2D[] _landingBuffer = new Collider2D[12];
        private static readonly ContactFilter2D LandingFilter = ContactFilter2D.noFilter;

        public bool TryResolveDestination(SkillEffectContext context, SkillActionSO action,
            Vector2 facing, out MeleeMovementResolution resolution, out string failureReason)
        {
            resolution = default;
            failureReason = string.Empty;
            if (context?.Caster == null || action == null)
            {
                failureReason = "InvalidMovementContext";
                return false;
            }

            var player = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            Transform body = player != null ? player.transform : context.Caster.transform;
            var displacement = body.GetComponent<PlayerMovementDisplacementResolver>();
            float authoredDistance = Mathf.Max(action.DashDistance, action.LeapDistance);
            if (authoredDistance <= 0f)
                return true;
            if (displacement == null || !displacement.isActiveAndEnabled || displacement.IsDisplacing
                || body.GetComponent<Collider2D>() == null || body.GetComponent<Rigidbody2D>() == null
                || (player != null && player.IsBeingDisplaced))
            {
                failureReason = "DisplacementUnavailable";
                return false;
            }

            Vector2 origin = body.position;
            Vector2 safeFacing = facing.sqrMagnitude > .001f ? facing.normalized : Vector2.right;
            bool isLeap = action.SkillActionId == LeapAttackActionId || action.LeapDistance > 0f;
            Vector2 requested = origin + safeFacing * authoredDistance;
            if (isLeap && ((Vector2)context.WorldPosition - origin).sqrMagnitude > .01f)
            {
                Vector2 offset = (Vector2)context.WorldPosition - origin;
                requested = origin + Vector2.ClampMagnitude(offset, authoredDistance);
            }

            Vector2 delta = requested - origin;
            float distance = delta.magnitude;
            if (distance <= .01f)
            {
                failureReason = "InvalidLandingPoint";
                return false;
            }

            if (isLeap && !IsLandingClear(body, requested))
            {
                failureReason = "InvalidLandingPoint";
                return false;
            }

            resolution = new MeleeMovementResolution(delta / distance, distance, true);
            return true;
        }

        private bool IsLandingClear(Transform body, Vector2 destination)
        {
            var ownCollider = body.GetComponent<Collider2D>();
            float radius = ownCollider != null
                ? Mathf.Max(.05f, Mathf.Min(ownCollider.bounds.extents.x, ownCollider.bounds.extents.y) * .9f)
                : .2f;
            int count = Physics2D.OverlapCircle(destination, radius, LandingFilter, _landingBuffer);
            for (int i = 0; i < count; i++)
            {
                var hit = _landingBuffer[i];
                if (hit == null || hit.isTrigger || hit.transform == body || hit.transform.IsChildOf(body)
                    || body.IsChildOf(hit.transform))
                    continue;
                return false;
            }
            return true;
        }
    }
}
