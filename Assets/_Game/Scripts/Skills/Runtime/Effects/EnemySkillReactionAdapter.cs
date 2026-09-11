using CindarsHope.Combat;
using CindarsHope.Foundation;
using CindarsHope.Skills.Runtime;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    [DisallowMultipleComponent]
    public sealed class EnemySkillReactionAdapter : MonoBehaviour
    {
        public const float EliteDurationMultiplier = .6f;
        public const float BossDurationMultiplier = .3f;

        private readonly ControlDiminishingReturnsState _controlDr = new ControlDiminishingReturnsState();
        private IEnemySkillReactionRuntime _brain;
        private EnemyPostureState _posture;
        private IEnemyVulnerabilityWindow _vulnerability;

        public bool IsInRecovery
        {
            get { ResolveComponents(); return _brain != null && _brain.IsInAttackRecovery; }
        }
        public bool IsBlocking
        {
            get { ResolveComponents(); return _brain != null && _brain.IsGuardHold; }
        }
        public ControlDiminishingReturnsState ControlDr => _controlDr;
        public bool CanReceiveBossControl
        {
            get
            {
                ResolveComponents();
                return Difficulty != EnemyDifficulty.Boss || HasExplicitControlWindow;
            }
        }

        private EnemyDifficulty Difficulty
        {
            get
            {
                ResolveComponents();
                return _posture != null ? _posture.Difficulty
                    : _brain != null ? _brain.Difficulty : EnemyDifficulty.Normal;
            }
        }

        private void Awake()
        {
            ResolveComponents();
        }

        private void ResolveComponents()
        {
            if (_brain == null) _brain = GetComponent<IEnemySkillReactionRuntime>();
            if (_posture == null) _posture = GetComponent<EnemyPostureState>();
            if (_vulnerability == null) _vulnerability = GetComponent<IEnemyVulnerabilityWindow>();
        }

        private bool HasExplicitControlWindow => _vulnerability != null && _vulnerability.IsVulnerable;

        public static EnemySkillReactionAdapter GetOrCreate(GameObject target)
        {
            if (target == null) return null;
            var brain = target.GetComponent<IEnemySkillReactionRuntime>();
            var posture = target.GetComponent<EnemyPostureState>();
            if (posture == null && brain != null)
            {
                posture = target.AddComponent<EnemyPostureState>();
                posture.Configure(brain.Difficulty);
            }
            return target.GetComponent<EnemySkillReactionAdapter>()
                ?? target.AddComponent<EnemySkillReactionAdapter>();
        }

        public ControlDiminishingReturnsResult ApplyTaunt(Vector2 casterPosition,
            float baseDurationSeconds, float now)
        {
            ResolveComponents();
            if (_brain == null)
                return new ControlDiminishingReturnsResult(false, 0f, 0f, 0f);

            var difficulty = Difficulty;
            float resistance = 1f;
            if (difficulty == EnemyDifficulty.Boss)
            {
                if (!HasExplicitControlWindow)
                    return new ControlDiminishingReturnsResult(false, 0f, 0f, 0f);
                resistance = BossDurationMultiplier;
            }
            else if ((_brain != null && _brain.HasEliteClassification)
                || difficulty == EnemyDifficulty.Elite || difficulty == EnemyDifficulty.MiniBoss)
            {
                resistance = EliteDurationMultiplier;
            }

            var result = _controlDr.TryApply(baseDurationSeconds * resistance, now);
            if (result.CanApply)
                _brain.ApplyTemporaryTargetPriority(
                    casterPosition.x, casterPosition.y, result.EffectiveDurationSeconds);
            return result;
        }

        public float ApplyLure(Vector2 lurePosition, float normalDurationSeconds,
            float eliteDurationSeconds)
        {
            ResolveComponents();
            if (_brain == null)
                return 0f;

            float duration = SurvivalSkillActionRules.ResolveLureDuration(
                Difficulty, _brain.HasEliteClassification,
                normalDurationSeconds, eliteDurationSeconds);
            if (duration > 0f)
                _brain.ApplyTemporaryAttraction(lurePosition.x, lurePosition.y, duration);
            return duration;
        }

        public void CancelLure()
        {
            ResolveComponents();
            _brain?.CancelTemporaryAttraction();
        }

        public float ResolvePostureMultiplier(string skillActionId, float authoredMultiplier)
        {
            float multiplier = Mathf.Max(0f, authoredMultiplier);
            if (skillActionId == MeleeMovementProfileResolver.SteelAdvanceActionId && IsInRecovery)
                multiplier += .25f;
            if (skillActionId == MeleeMovementProfileResolver.GuardBreakChargeActionId && IsBlocking)
                multiplier += .25f;
            return multiplier;
        }
    }
}
