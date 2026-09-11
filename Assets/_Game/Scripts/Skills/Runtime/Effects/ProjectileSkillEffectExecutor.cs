using System;
using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Player;
using UnityEngine;
using CindarsHope.Foundation;
using CindarsHope.Combat.StatusEffect;
using System.Collections.Generic;
using System.Threading;
using CindarsHope.Core.Random;

namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>
    /// Real ranged/magic skill executor: fires one or more runtime-built projectiles in the
    /// facing direction (fan spread for multishot, pierce for line skills). Spends mana for
    /// magic damage types and stamina for physical ones.
    /// </summary>
    public sealed class ProjectileSkillEffectExecutor : ISkillEffectExecutor, IPreparableSkillEffectExecutor
    {
        private static int s_actionSequence;
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly int _baseDamage;
        private readonly float _speed;
        private readonly float _range;
        private readonly DamageType _damageType;
        private readonly int _projectileCount;
        private readonly float _spreadDegrees;
        private readonly int _maxHitsPerProjectile;
        private readonly int _resourceCost;
        private readonly float _cooldownSeconds;
        private readonly string _statusEffectId;
        private readonly float _statusApplyChance;
        private readonly SkillActionSO _defaultActionData;
        private readonly Func<ProjectileSpawnRequest, ProjectileSpawnResult> _spawnProjectile;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public ProjectileSkillEffectExecutor(string effectId, string displayName)
            : this(effectId, displayName, 0, 0, 0, DamageType.Physical, 0)
        {
        }

        public ProjectileSkillEffectExecutor(string effectId, string displayName,
            SkillActionSO defaultActionData,
            Func<ProjectileSpawnRequest, ProjectileSpawnResult> spawnProjectile = null)
            : this(effectId, displayName, 0, 0, 0, DamageType.Physical, 0,
                spawnProjectile: spawnProjectile)
        {
            _defaultActionData = defaultActionData;
        }

        public ProjectileSkillEffectExecutor(
            string effectId,
            string displayName,
            int baseDamage,
            float speed,
            float range,
            DamageType damageType,
            int resourceCost,
            int projectileCount = 1,
            float spreadDegrees = 0f,
            int maxHitsPerProjectile = 1,
            float cooldownSeconds = 4f,
            string statusEffectId = null,
            float statusApplyChance = 0f,
            Func<ProjectileSpawnRequest, ProjectileSpawnResult> spawnProjectile = null)
        {
            _effectId = effectId;
            _displayName = displayName;
            _baseDamage = baseDamage;
            _speed = speed;
            _range = range;
            _damageType = damageType;
            _resourceCost = resourceCost;
            _projectileCount = Mathf.Max(1, projectileCount);
            _spreadDegrees = spreadDegrees;
            _maxHitsPerProjectile = Mathf.Max(1, maxHitsPerProjectile);
            _cooldownSeconds = cooldownSeconds;
            _statusEffectId = statusEffectId;
            _statusApplyChance = statusApplyChance;
            _spawnProjectile = spawnProjectile ?? ProjectileSpawnService.SpawnProjectile;
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var readiness = Validate(context);
            if (!readiness.Success) return readiness;

            var action = context.ActionData != null ? context.ActionData : _defaultActionData;
            var rankData = action != null ? action.ResolveRank(context.Rank) : null;
            int baseDamage = rankData != null ? rankData.Damage : _baseDamage;
            float speed = action != null ? action.ProjectileSpeed : _speed;
            float range = rankData != null ? rankData.Range : _range;
            DamageType damageType = action != null ? action.DamageType : _damageType;
            int projectileCount = action != null ? Mathf.Max(1, action.ProjectileCount) : _projectileCount;
            float spreadDegrees = action != null ? action.ProjectileSpreadDegrees : _spreadDegrees;
            int maxHits = action != null ? Mathf.Max(1, action.LinePierceCount) : _maxHitsPerProjectile;
            bool usesStamina = rankData != null
                ? rankData.StaminaCost > 0f
                : damageType == DamageType.Physical;
            int resourceCost = rankData != null
                ? Mathf.RoundToInt(usesStamina ? rankData.StaminaCost : rankData.ManaCost)
                : _resourceCost;
            float cooldownSeconds = rankData != null ? rankData.CooldownSeconds : _cooldownSeconds;
            string statusEffectId = action != null ? action.StatusEffectId : _statusEffectId;
            float statusApplyChance = action != null ? action.StatusApplyChance : _statusApplyChance;
            string actionId = !string.IsNullOrWhiteSpace(context.SkillActionId)
                ? context.SkillActionId
                : action != null ? action.SkillActionId : string.Empty;

            MagicSkillRuntimeUtility.ActiveMagicCast magicCast = null;
            CindarsHope.Player.StaminaManager staminaManager = null;
            if (usesStamina)
            {
                var lunarLaunch = RangedLunarModifierProvider.ResolveLaunchModifier();
                range *= lunarLaunch.RangeMultiplier;
                speed = Mathf.Max(.1f, speed * lunarLaunch.ProjectileSpeedMultiplier);
            }

            var statusEffect = ResolveStatusEffect(statusEffectId);
            if (!string.IsNullOrEmpty(statusEffectId) && (statusEffect == null || statusApplyChance <= 0f))
                return SkillEffectResult.Failed("InvalidStatus", "Efeito de status indisponível.");

            // The authored resource fields decide the channel. Damage type cannot decide this:
            // crafted elemental projectiles such as Bomba Improvisada still spend stamina.
            if (usesStamina)
            {
                staminaManager = GameBootstrap.Instance?.StaminaManager as CindarsHope.Player.StaminaManager;
                if (resourceCost > 0 && staminaManager == null)
                    return SkillEffectResult.Failed("MissingStamina", "Stamina indisponível.");
                if (staminaManager != null && !staminaManager.TrySpendStamina(resourceCost))
                    return SkillEffectResult.Failed("InsufficientStamina", $"Stamina insuficiente ({resourceCost}).");
            }
            else
            {
                if (!MagicSkillRuntimeUtility.TryBeginCast(context, action, out magicCast))
                    return SkillEffectResult.Failed("InsufficientMana", $"Mana insuficiente ({resourceCost}).");
            }

            var playerController = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            Vector2 facing = playerController != null ? playerController.LastFacingDirection : Vector2.right;
            if (facing.sqrMagnitude < 0.01f)
                facing = Vector2.right;

            Vector2 origin = playerController != null
                ? (Vector2)playerController.transform.position
                : context.WorldPosition;

            int spawned = 0;
            var hitPolicy = CreateHitPolicy(actionId);
            bool hasMagicControl = actionId == SkillActionEffectCatalog.MagicIceBindActionId
                || actionId == SkillActionEffectCatalog.MagicIceVolleyActionId;
            var magicControlPolicy = hasMagicControl
                ? new MagicProjectileControlPolicy(actionId, context.Rank, action, statusEffect)
                : null;
            float baseAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            float step = projectileCount > 1 ? spreadDegrees / (projectileCount - 1) : 0f;
            float startAngle = baseAngle - spreadDegrees * 0.5f;
            int casterRuntimeId = context.Caster.GetEntityId().GetHashCode();
            string actionToken = usesStamina
                ? $"{actionId}:{Interlocked.Increment(ref s_actionSequence)}"
                : magicCast.Preparation.Request.ActionToken;

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = projectileCount > 1 ? startAngle + step * i : baseAngle;
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                var request = new ProjectileSpawnRequest(
                    prefab: null,
                    sourcePosition: origin,
                    direction: direction,
                    speed: speed,
                    range: range,
                    baseDamage: baseDamage,
                    damageType: damageType,
                    knockbackForce: 2.5f,
                    spawnOffset: 0.5f,
                    statusEffect: hasMagicControl ? null : statusEffect,
                    statusApplyChance: statusApplyChance);
                request.VisualStyle = ProjectileVisualStyle.SkillBolt;
                request.MaxHits = maxHits;
                request.HitPolicy = hitPolicy;
                request.PostureDamageMultiplier = action != null ? action.PostureDamageMultiplier : 0f;
                request.SourceCasterRuntimeId = casterRuntimeId;
                request.StopOnSolidObstacle = actionId == SkillActionEffectCatalog.RangedLinePiercerActionId;
                if (usesStamina)
                {
                    request.SourceKind = DamageSourceKind.PlayerRanged;
                    request.SourceInstanceId = casterRuntimeId.ToString(
                        System.Globalization.CultureInfo.InvariantCulture);
                    request.ActionToken = actionToken;
                    request.CanTriggerCapstones = true;
                    request.TargetedImpactDamageResolver = impact =>
                    {
                        var lunarImpact = RangedLunarModifierProvider.ResolveImpactModifier(
                            impact.TargetInstanceId, impact.TargetPositionX,
                            impact.TargetPositionY);
                        int damage = RangedLunarRules.ApplyCritical(baseDamage,
                            impact.GuaranteedCritical,
                            UnityGameplayRandomSource.Shared.NextFloat(), lunarImpact,
                            out bool isCritical);
                        return new ProjectileImpactDamage(damage, isCritical);
                    };
                }
                else
                {
                    request.SourceKind = DamageSourceKind.PlayerMagic;
                    request.SpellDiscipline = magicCast.Preparation.Request.Discipline;
                    request.ActionToken = actionToken;
                    request.CanTriggerCapstones =
                        magicCast.Preparation.Request.Discipline == SpellDiscipline.Offensive;
                    request.TargetedImpactDamageResolver = impact =>
                    {
                        int damage = MagicSkillRuntimeUtility.ResolveDirectDamage(
                            magicCast, baseDamage, out bool critical);
                        return new ProjectileImpactDamage(damage, critical);
                    };
                }

                var result = _spawnProjectile(request);
                if (result.Success)
                {
                    if (magicControlPolicy != null)
                        result.Projectile.AddComponent<MagicProjectileControlImpact>()
                            .Configure(magicControlPolicy);
                    spawned++;
                }
            }

            if (spawned == 0)
            {
                if (usesStamina)
                    staminaManager?.AddStamina(resourceCost);
                else
                    MagicSkillRuntimeUtility.Cancel(magicCast);
                return SkillEffectResult.Failed("ProjectileSpawnFailed",
                    $"{_displayName}: nenhum projétil foi materializado.");
            }

            if (!usesStamina)
                MagicSkillRuntimeUtility.Commit(magicCast);

            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: SkillProjectileFired. EffectId={_effectId}, Count={spawned}, Type={damageType}, Pierce={maxHits}");
            return SkillEffectResult.Succeeded($"{_displayName}!",
                costSpent: usesStamina ? resourceCost > 0 : magicCast.Preparation.ManaCost > 0,
                cooldownStarted: true, cooldownSeconds: cooldownSeconds);
        }

        public SkillEffectResult Validate(SkillEffectContext context)
        {
            if (context?.Caster == null || !context.Caster.activeInHierarchy)
                return SkillEffectResult.Failed("NoCaster", "Jogador nao encontrado.");
            var action = context.ActionData != null ? context.ActionData : _defaultActionData;
            var rankData = action != null ? action.ResolveRank(context.Rank) : null;
            DamageType damageType = action != null ? action.DamageType : _damageType;
            bool usesStamina = rankData != null ? rankData.StaminaCost > 0f : damageType == DamageType.Physical;
            int resourceCost = rankData != null
                ? Mathf.RoundToInt(usesStamina ? rankData.StaminaCost : rankData.ManaCost)
                : _resourceCost;
            string statusEffectId = action != null ? action.StatusEffectId : _statusEffectId;
            float statusApplyChance = action != null ? action.StatusApplyChance : _statusApplyChance;
            if (!string.IsNullOrEmpty(statusEffectId)
                && (ResolveStatusEffect(statusEffectId) == null || statusApplyChance <= 0f))
                return SkillEffectResult.Failed("InvalidStatus", "Efeito de status indisponível.");
            if (usesStamina)
            {
                var stamina = GameBootstrap.Instance?.StaminaManager as StaminaManager;
                if (resourceCost > 0 && stamina == null)
                    return SkillEffectResult.Failed("MissingStamina", "Stamina indisponível.");
                if (resourceCost > 0 && stamina.CurrentStamina < resourceCost)
                    return SkillEffectResult.Failed("InsufficientStamina", $"Stamina insuficiente ({resourceCost}).");
            }
            else
            {
                if (action != null)
                {
                    var magicReadiness = MagicSkillRuntimeUtility.Validate(context, action, false);
                    if (!magicReadiness.Success) return magicReadiness;
                    return SkillEffectResult.Succeeded(string.Empty);
                }
                var mana = GameBootstrap.Instance?.ManaManager as ManaManager;
                if (resourceCost > 0 && mana == null)
                    return SkillEffectResult.Failed("MissingMana", "Mana indisponível.");
                if (resourceCost > 0 && mana.CurrentMana < resourceCost)
                    return SkillEffectResult.Failed("InsufficientMana", $"Mana insuficiente ({resourceCost}).");
            }
            return SkillEffectResult.Succeeded(string.Empty);
        }

        private static RangedProjectileHitPolicy CreateHitPolicy(string actionId)
        {
            if (actionId == SkillActionEffectCatalog.RangedLinePiercerActionId)
                return new RangedProjectileHitPolicy(RangedProjectilePolicyKind.LinePiercer);
            if (actionId == SkillActionEffectCatalog.RangedMultishotFanActionId)
                return new RangedProjectileHitPolicy(RangedProjectilePolicyKind.TripleFan);
            if (actionId == SkillActionEffectCatalog.MagicIceVolleyActionId)
                return new RangedProjectileHitPolicy(RangedProjectilePolicyKind.TripleFan);
            return new RangedProjectileHitPolicy(RangedProjectilePolicyKind.Single);
        }

        private CindarsHope.Combat.StatusEffect.StatusEffectSO ResolveStatusEffect(string statusEffectId)
        {
            if (string.IsNullOrEmpty(statusEffectId))
                return null;

            var database = GameBootstrap.Instance?.StatusEffectDatabase;
            if (database != null && database.TryGetById(statusEffectId, out var effect))
                return effect;

            return null;
        }
    }

    internal sealed class MagicProjectileControlPolicy
    {
        private static int s_castSequence;
        private readonly HashSet<int> _chilledTargets = new HashSet<int>();
        private bool _strongSlowApplied;

        public string ActionId { get; }
        public int Rank { get; }
        public SkillActionSO Action { get; }
        public StatusEffectSO ChillStatus { get; }
        public string SourceId { get; }

        public MagicProjectileControlPolicy(string actionId, int rank, SkillActionSO action,
            StatusEffectSO chillStatus)
        {
            ActionId = actionId;
            Rank = Mathf.Max(1, rank);
            Action = action;
            ChillStatus = chillStatus;
            SourceId = $"{actionId}:{Interlocked.Increment(ref s_castSequence)}";
        }

        public void Apply(EnemyHealth target)
        {
            if (target == null || Action == null || ChillStatus == null) return;
            int targetId = target.GetEntityId().GetHashCode();
            if (!_chilledTargets.Add(targetId)) return;
            var slowState = MagicSlowState.GetOrCreate(target.gameObject);
            bool alreadyChilled = target.StatusEffects.HasStatusEffect(ChillStatus.Id)
                || slowState.HasActiveStatus(ChillStatus.Id, Time.time);
            if (!MagicControlRules.TryResolveLightDuration(target.gameObject,
                    Action.ResolveEffectDuration(Rank), ChillStatus.Id,
                    out float chillDuration, out bool bossBound)) return;
            string chillSourceId = SourceId + ":chill";
            MagicSkillRuntimeUtility.RefreshStatus(target, ChillStatus, chillDuration, chillSourceId);
            slowState.Apply(chillSourceId, Action.ResolveControlStrength(Rank),
                chillDuration, bossBound, Time.time, ChillStatus.Id);

            if (ActionId != SkillActionEffectCatalog.MagicIceBindActionId
                || !alreadyChilled || _strongSlowApplied) return;
            if (!MagicControlRules.TryResolveStrongSlow(target.gameObject,
                    Action.StrongSlowDurationSeconds, ChillStatus.Id, Time.time,
                    out float strongDuration, out bossBound)) return;
            _strongSlowApplied = true;
            slowState.Apply(
                SourceId + ":strong", Action.StrongSlowFraction, strongDuration, bossBound, Time.time);
        }
    }

    [DisallowMultipleComponent]
    internal sealed class MagicProjectileControlImpact : MonoBehaviour
    {
        private MagicProjectileControlPolicy _policy;
        public void Configure(MagicProjectileControlPolicy policy) => _policy = policy;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var target = collision.GetComponentInParent<EnemyHealth>()
                ?? collision.GetComponent<EnemyHealth>();
            _policy?.Apply(target);
        }
    }
}
