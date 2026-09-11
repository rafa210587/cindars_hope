using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Player;
using CindarsHope.Player.Movement;
using System.Collections.Generic;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>
    /// Real melee skill executor: hits enemies in an arc (or full circle) around the caster,
    /// optionally lunging forward first (leap/charge skills). Spends stamina via StaminaManager.
    /// One configured instance per EffectId (strategy pattern over the skill effect registry).
    /// </summary>
    public sealed class MeleeStrikeSkillEffectExecutor : ISkillEffectExecutor, IPreparableSkillEffectExecutor
    {
        private static long s_nextMeleeSkillActionToken;
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly int _baseDamage;
        private readonly float _range;
        private readonly float _arcDegrees;
        private readonly int _staminaCost;
        private readonly float _lungeDistance;
        private readonly float _knockbackForce;
        private readonly float _cooldownSeconds;
        // F02: multiplicador de dano de posture (quebra-guarda usa 3x).
        private readonly float _postureDamageMultiplier;
        private readonly SkillActionSO _defaultActionData;
        private readonly MeleeEquipmentGate _equipmentGate;
        private readonly Physics2DOverlapBuffer _overlapBuffer = new Physics2DOverlapBuffer();
        private readonly HashSet<EnemyHealth> _hitTargets = new HashSet<EnemyHealth>();
        private readonly List<EnemyHealth> _orderedTargets = new List<EnemyHealth>();
        private static readonly ContactFilter2D EnemyContactFilter = CreateEnemyContactFilter();
        private readonly MeleeMovementProfileResolver _movementProfileResolver = new MeleeMovementProfileResolver();
        // Same duration as the existing base dash; rank/tuning data migration is separate.
        private const float LungeDurationSeconds = 0.22f;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public MeleeStrikeSkillEffectExecutor(string effectId, string displayName)
            : this(effectId, displayName, 0, 0, 0, 0)
        {
        }

        public MeleeStrikeSkillEffectExecutor(string effectId, string displayName, SkillActionSO defaultActionData)
            : this(effectId, displayName, 0, 0, 0, 0)
        {
            _defaultActionData = defaultActionData;
        }

        public MeleeStrikeSkillEffectExecutor(
            string effectId,
            string displayName,
            SkillActionSO defaultActionData,
            MeleeEquipmentGate equipmentGate)
            : this(effectId, displayName, defaultActionData)
        {
            _equipmentGate = equipmentGate;
        }

        public MeleeStrikeSkillEffectExecutor(
            string effectId,
            string displayName,
            int baseDamage,
            float range,
            float arcDegrees,
            int staminaCost,
            float lungeDistance = 0f,
            float knockbackForce = 3f,
            float cooldownSeconds = 3f,
            float postureDamageMultiplier = 1f)
        {
            _equipmentGate = new MeleeEquipmentGate(null, null);
            _postureDamageMultiplier = Mathf.Max(0f, postureDamageMultiplier);
            _effectId = effectId;
            _displayName = displayName;
            _baseDamage = baseDamage;
            _range = range;
            _arcDegrees = arcDegrees;
            _staminaCost = staminaCost;
            _lungeDistance = lungeDistance;
            _knockbackForce = knockbackForce;
            _cooldownSeconds = cooldownSeconds;
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var readiness = Validate(context);
            if (!readiness.Success)
                return readiness;

            var action = context.ActionData != null ? context.ActionData : _defaultActionData;
            var rankData = action != null ? action.ResolveRank(context.Rank) : null;
            int damage = rankData != null ? rankData.Damage : _baseDamage;
            float range = rankData != null ? rankData.Range : _range;
            float arcDegrees = action != null && action.ArcDegrees > 0 ? action.ArcDegrees : _arcDegrees;
            int staminaCost = rankData != null ? Mathf.RoundToInt(rankData.StaminaCost) : _staminaCost;
            float knockbackForce = action != null ? action.KnockbackForce : _knockbackForce;
            float cooldownSeconds = rankData != null ? rankData.CooldownSeconds : _cooldownSeconds;
            float postureMultiplier = action != null ? action.PostureDamageMultiplier : _postureDamageMultiplier;
            int maxTargets = action != null ? action.MaxTargets : 0;
            int fullDamageTargets = action != null ? action.FullDamageTargetCount : 0;
            float additionalTargetMultiplier = action != null ? action.AdditionalTargetDamageMultiplier : 1f;

            string actionId = !string.IsNullOrWhiteSpace(context.SkillActionId)
                ? context.SkillActionId
                : action != null ? action.SkillActionId : string.Empty;
            string actionToken = actionId + ":" + System.Threading.Interlocked.Increment(
                ref s_nextMeleeSkillActionToken).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var equipmentReadiness = _equipmentGate.Evaluate(context.Caster, actionId);
            if (!equipmentReadiness.CanExecute)
                return SkillEffectResult.Failed(equipmentReadiness.FailureKey, "Equipe uma arma melee leve e íntegra na mão secundária.");

            var staminaManager = GameBootstrap.Instance?.StaminaManager as CindarsHope.Player.StaminaManager;
            if (staminaCost > 0 && staminaManager == null)
                return SkillEffectResult.Failed("MissingStamina", "Stamina indisponível.");

            var playerController = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            Vector2 facing = playerController != null ? playerController.LastFacingDirection : Vector2.right;
            if (facing.sqrMagnitude < 0.01f)
                facing = Vector2.right;

            Transform bodyTransform = playerController != null ? playerController.transform : context.Caster.transform;
            var movement = default(MeleeMovementResolution);
            if (action != null && !_movementProfileResolver.TryResolveDestination(
                context, action, facing, out movement, out var movementFailure))
                return SkillEffectResult.Failed(movementFailure, "Destino de movimento indisponível.");
            if (action == null && _lungeDistance > 0f)
                movement = new MeleeMovementResolution(facing.normalized, _lungeDistance, true);
            var displacement = movement.Distance > 0f
                ? bodyTransform.GetComponent<PlayerMovementDisplacementResolver>() : null;
            if (staminaCost > 0 && !staminaManager.TrySpendStamina(staminaCost))
                return SkillEffectResult.Failed("InsufficientStamina", $"Stamina insuficiente ({staminaCost}).");

            if (movement.Distance > 0f)
            {
                System.Func<Collider2D, bool> collisionBypass = actionId == MeleeMovementProfileResolver.BattleDashActionId
                    ? ShouldBattleDashIgnoreCollider
                    : null;
                if (!displacement.TryDisplace(movement.Direction, movement.Distance, LungeDurationSeconds,
                    collisionBypass,
                    () => { if (bodyTransform != null && bodyTransform.gameObject.activeInHierarchy) ApplyStrike(bodyTransform, facing, damage, range, arcDegrees, knockbackForce, postureMultiplier, maxTargets, fullDamageTargets, additionalTargetMultiplier, actionId, actionToken, context.Rank); }))
                {
                    return SkillEffectResult.Succeeded($"{_displayName}: movimento interrompido após o compromisso.",
                        costSpent: staminaCost > 0, cooldownStarted: true, cooldownSeconds: cooldownSeconds);
                }
                return SkillEffectResult.Succeeded($"{_displayName}: iniciado.", costSpent: staminaCost > 0,
                    cooldownStarted: true, cooldownSeconds: cooldownSeconds);
            }

            int hits = ApplyStrike(bodyTransform, facing, damage, range, arcDegrees, knockbackForce,
                postureMultiplier, maxTargets, fullDamageTargets, additionalTargetMultiplier, actionId, actionToken, context.Rank);
            return SkillEffectResult.Succeeded($"{_displayName}: {hits} alvo(s).", costSpent: staminaCost > 0,
                cooldownStarted: true, cooldownSeconds: cooldownSeconds);
        }

        public SkillEffectResult Validate(SkillEffectContext context)
        {
            if (context?.Caster == null || !context.Caster.activeInHierarchy)
                return SkillEffectResult.Failed("NoCaster", "Jogador nao encontrado.");

            var action = context.ActionData != null ? context.ActionData : _defaultActionData;
            var rankData = action != null ? action.ResolveRank(context.Rank) : null;
            int staminaCost = rankData != null ? Mathf.RoundToInt(rankData.StaminaCost) : _staminaCost;
            string actionId = !string.IsNullOrWhiteSpace(context.SkillActionId)
                ? context.SkillActionId
                : action != null ? action.SkillActionId : string.Empty;

            var equipmentReadiness = _equipmentGate.Evaluate(context.Caster, actionId);
            if (!equipmentReadiness.CanExecute)
                return SkillEffectResult.Failed(equipmentReadiness.FailureKey, "Equipe uma arma melee leve e íntegra na mão secundária.");

            var staminaManager = GameBootstrap.Instance?.StaminaManager as StaminaManager;
            if (staminaCost > 0 && staminaManager == null)
                return SkillEffectResult.Failed("MissingStamina", "Stamina indisponível.");
            if (staminaCost > 0 && staminaManager.CurrentStamina < staminaCost)
                return SkillEffectResult.Failed("InsufficientStamina", $"Stamina insuficiente ({staminaCost}).");

            var playerController = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            Vector2 facing = playerController != null ? playerController.LastFacingDirection : Vector2.right;
            if (facing.sqrMagnitude < .01f) facing = Vector2.right;
            if (action != null && !_movementProfileResolver.TryResolveDestination(
                context, action, facing, out _, out var movementFailure))
            {
                return SkillEffectResult.Failed(movementFailure, "Destino de movimento indisponível.");
            }
            if (action == null && _lungeDistance > 0f)
            {
                Transform bodyTransform = playerController != null ? playerController.transform : context.Caster.transform;
                var displacement = bodyTransform.GetComponent<PlayerMovementDisplacementResolver>();
                if (displacement == null || !displacement.isActiveAndEnabled || displacement.IsDisplacing)
                    return SkillEffectResult.Failed("DisplacementUnavailable", "Movimento indisponível.");
            }

            return SkillEffectResult.Succeeded(string.Empty);
        }

        private int ApplyStrike(Transform bodyTransform, Vector2 facing, int damage, float range,
            float arcDegrees, float knockbackForce, float postureMultiplier, int maxTargets,
            int fullDamageTargets, float additionalTargetMultiplier, string actionId,
            string actionToken, int rank)
        {
            _hitTargets.Clear();
            _orderedTargets.Clear();
            // Range is authored from the caster origin. Offsetting the overlap circle would extend
            // frontal reach beyond the data value and make 360-degree strikes asymmetric.
            Vector2 attackCenter = bodyTransform.position;
            int colliderCount = _overlapBuffer.QueryCircle(
                attackCenter,
                range,
                EnemyContactFilter);
            float halfArc = arcDegrees * 0.5f;

            for (int i = 0; i < colliderCount; i++)
            {
                Collider2D collider = _overlapBuffer[i];
                var enemyHealth = collider.GetComponentInParent<EnemyHealth>() ?? collider.GetComponent<EnemyHealth>();
                if (enemyHealth == null || enemyHealth.IsDead)
                    continue;

                if (arcDegrees < 360f)
                {
                    Vector2 toEnemy = ((Vector2)collider.transform.position - (Vector2)bodyTransform.position).normalized;
                    if (Vector2.Angle(facing, toEnemy) > halfArc)
                        continue;
                }

                if (_hitTargets.Add(enemyHealth)) _orderedTargets.Add(enemyHealth);
            }

            _orderedTargets.Sort((a, b) =>
            {
                float aDistance = ((Vector2)a.transform.position - (Vector2)bodyTransform.position).sqrMagnitude;
                float bDistance = ((Vector2)b.transform.position - (Vector2)bodyTransform.position).sqrMagnitude;
                int byDistance = aDistance.CompareTo(bDistance);
                if (byDistance != 0) return byDistance;
                int byInstance = string.CompareOrdinal(a.EnemyInstanceId, b.EnemyInstanceId);
                if (byInstance != 0) return byInstance;
                int bySpecies = string.CompareOrdinal(a.EnemyId, b.EnemyId);
                if (bySpecies != 0) return bySpecies;
                int byX = a.transform.position.x.CompareTo(b.transform.position.x);
                if (byX != 0) return byX;
                int byY = a.transform.position.y.CompareTo(b.transform.position.y);
                if (byY != 0) return byY;
                return string.CompareOrdinal(a.GetEntityId().ToString(), b.GetEntityId().ToString());
            });

            int targetLimit = maxTargets > 0 ? Mathf.Min(maxTargets, _orderedTargets.Count) : _orderedTargets.Count;
            for (int i = 0; i < targetLimit; i++)
            {
                var enemyHealth = _orderedTargets[i];
                int appliedDamage = ResolveTargetDamage(damage, i, fullDamageTargets, additionalTargetMultiplier);
                appliedDamage = Mathf.Max(1, Mathf.RoundToInt(appliedDamage *
                    CombatCapstoneModifierProvider.ResolveDirectMeleeDamageMultiplier()));
                var request = new DamageRequest(enemyHealth.EnemyId, appliedDamage)
                {
                    DamageType = DamageType.Physical,
                    SourcePosition = bodyTransform.position,
                    KnockbackForce = knockbackForce,
                    SourceKind = DamageSourceKind.PlayerMelee,
                    SourceInstanceId = bodyTransform.GetEntityId().ToString(),
                    TargetInstanceId = enemyHealth.EnemyInstanceId,
                    ActionToken = actionToken,
                    IsCritical = false,
                    IsPrimaryDamage = true,
                    CanTriggerCapstones = true
                };
                enemyHealth.TakeDamage(request);

                var reaction = EnemySkillReactionAdapter.GetOrCreate(enemyHealth.gameObject);
                if (actionId == MeleeMovementProfileResolver.ChallengeShoutActionId && reaction != null && !enemyHealth.IsDead)
                    reaction.ApplyTaunt(bodyTransform.position, Mathf.Max(1, rank) + 2f, Time.time);

                // F02: dano de posture (skills aplicam 1x; quebra-guarda 3x).
                if (postureMultiplier > 0f && reaction != null
                    && (actionId != MeleeMovementProfileResolver.ChallengeShoutActionId || reaction.CanReceiveBossControl))
                {
                    var posture = enemyHealth.GetComponent<EnemyPostureState>();
                    if (posture != null)
                    {
                        float resolvedMultiplier = reaction.ResolvePostureMultiplier(actionId, postureMultiplier);
                        posture.ApplyPostureDamage(
                            appliedDamage * resolvedMultiplier,
                            actionId,
                            bodyTransform.GetEntityId().ToString(),
                            true,
                            actionToken + ":" + enemyHealth.EnemyInstanceId);
                    }
                }
            }

            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: SkillMeleeStrike. EffectId={_effectId}, Hits={targetLimit}, Range={range:F1}, Arc={arcDegrees:F0}");
            return targetLimit;
        }

        private static int ResolveTargetDamage(int damage, int targetIndex, int fullDamageTargets, float multiplier)
            => fullDamageTargets > 0 && targetIndex >= fullDamageTargets
                ? Mathf.RoundToInt(damage * Mathf.Clamp01(multiplier))
                : damage;

        private static ContactFilter2D CreateEnemyContactFilter()
        {
            var filter = ContactFilter2D.noFilter;
            filter.useTriggers = true;
            return filter;
        }

        private static bool ShouldBattleDashIgnoreCollider(Collider2D collider)
        {
            if (collider == null) return false;
            var brain = collider.GetComponentInParent<IEnemySkillReactionRuntime>();
            return brain != null
                && !brain.HasEliteClassification
                && brain.Difficulty <= EnemyDifficulty.Normal;
        }
    }
}
