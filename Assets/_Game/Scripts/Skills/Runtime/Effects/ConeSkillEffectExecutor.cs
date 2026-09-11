using System;
using System.Collections.Generic;
using System.Threading;
using CindarsHope.Combat;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class ConeSkillEffectExecutor : ISkillEffectExecutor, IPreparableSkillEffectExecutor
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly SkillActionSO _defaultAction;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public ConeSkillEffectExecutor(string effectId, string displayName, SkillActionSO defaultAction)
        {
            _effectId = effectId;
            _displayName = displayName;
            _defaultAction = defaultAction;
        }

        public SkillEffectResult Validate(SkillEffectContext context)
            => MagicSkillRuntimeUtility.Validate(context, MagicSkillRuntimeUtility.Action(context, _defaultAction), true);

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var action = MagicSkillRuntimeUtility.Action(context, _defaultAction);
            var readiness = MagicSkillRuntimeUtility.Validate(context, action, true);
            if (!readiness.Success) return readiness;
            var rank = action.ResolveRank(context.Rank);
            if (!MagicSkillRuntimeUtility.TryBeginCast(context, action, out var cast))
                return SkillEffectResult.Failed("InsufficientMana", $"Mana insuficiente ({Mathf.RoundToInt(rank.ManaCost)}).");

            Vector2 origin = MagicSkillRuntimeUtility.Origin(context);
            Vector2 facing = MagicSkillRuntimeUtility.Facing(context);
            float halfArc = Mathf.Max(0f, action.ArcDegrees) * .5f;
            var targets = MagicSkillRuntimeUtility.OrderedEnemies(origin, enemy =>
            {
                Vector2 delta = (Vector2)enemy.transform.position - origin;
                return delta.sqrMagnitude <= rank.Range * rank.Range
                    && delta.sqrMagnitude > .0001f
                    && Vector2.Angle(facing, delta) <= halfArc;
            });

            var status = MagicSkillRuntimeUtility.ResolveStatus(action.StatusEffectId);
            int limit = action.MaxTargets > 0 ? Mathf.Min(action.MaxTargets, targets.Count) : targets.Count;
            for (int i = 0; i < limit; i++)
            {
                int damage = action.ResolveDamageForTargetIndex(context.Rank, i);
                MagicSkillRuntimeUtility.Damage(targets[i], damage, action.DamageType, origin, cast);
                MagicSkillRuntimeUtility.RefreshStatus(targets[i], status, 0f);
            }

            MagicSkillRuntimeUtility.Commit(cast);
            return SkillEffectResult.Succeeded($"{_displayName}: {limit} alvo(s).",
                cast.Preparation.ManaCost > 0, true, rank.CooldownSeconds);
        }
    }

    internal static class MagicSkillRuntimeUtility
    {
        private static int s_actionSequence;

        internal sealed class ActiveMagicCast
        {
            public SpellCastPreparation Preparation;
            public SpellCastTransaction Transaction;
            public ManaManager Mana;
            public DamageType DamageType;
        }
        public static SkillActionSO Action(SkillEffectContext context, SkillActionSO fallback)
            => context?.ActionData != null ? context.ActionData : fallback;

        public static SkillEffectResult Validate(SkillEffectContext context, SkillActionSO action, bool requireStatus)
        {
            if (context?.Caster == null || !context.Caster.activeInHierarchy)
                return SkillEffectResult.Failed("NoCaster", "Jogador não encontrado.");
            if (action == null)
                return SkillEffectResult.Failed("MissingActionData", "Dados da habilidade indisponíveis.");
            var rank = action.ResolveRank(context.Rank);
            var mana = GameBootstrap.Instance?.ManaManager as ManaManager;
            int baseCost = Mathf.RoundToInt(Mathf.Max(0f, rank.ManaCost));
            string actionId = !string.IsNullOrWhiteSpace(context.SkillActionId)
                ? context.SkillActionId : action.SkillActionId;
            var preview = SpellCastPreparationProvider.Prepare(new SpellCastPreparationRequest(
                actionId, $"preview:{actionId}:{Interlocked.Increment(ref s_actionSequence)}",
                action.SpellDiscipline, baseCost, true));
            SpellCastPreparationProvider.Cancel(preview);
            if (preview.ManaCost > 0 && mana == null)
                return SkillEffectResult.Failed("MissingMana", "Mana indisponível.");
            if (preview.ManaCost > 0 && mana.CurrentMana < preview.ManaCost)
                return SkillEffectResult.Failed("InsufficientMana", $"Mana insuficiente ({preview.ManaCost}).");
            if (requireStatus && ResolveStatus(action.StatusEffectId) == null)
                return SkillEffectResult.Failed("InvalidStatus", "Efeito de status indisponível.");
            return SkillEffectResult.Succeeded(string.Empty);
        }

        public static bool TrySpendMana(float amount)
        {
            int cost = Mathf.RoundToInt(Mathf.Max(0f, amount));
            if (cost == 0) return true;
            var mana = GameBootstrap.Instance?.ManaManager as ManaManager;
            return mana != null && mana.TrySpendMana(cost);
        }

        public static bool TryBeginCast(SkillEffectContext context, SkillActionSO action,
            out ActiveMagicCast cast)
        {
            cast = null;
            if (action == null) return false;
            int baseCost = Mathf.RoundToInt(Mathf.Max(0f,
                action.ResolveRank(context?.Rank ?? 1).ManaCost));
            string actionId = !string.IsNullOrWhiteSpace(context?.SkillActionId)
                ? context.SkillActionId : action.SkillActionId;
            string token = $"active-magic:{actionId}:{Interlocked.Increment(ref s_actionSequence)}";
            var preparation = SpellCastPreparationProvider.Prepare(
                new SpellCastPreparationRequest(actionId, token, action.SpellDiscipline,
                    baseCost, true));
            var mana = GameBootstrap.Instance?.ManaManager as ManaManager;
            var transaction = new SpellCastTransaction(preparation);
            if (!transaction.TryReserve(cost => cost <= 0 ||
                    (mana != null && mana.TrySpendMana(cost))))
            {
                SpellCastPreparationProvider.Cancel(preparation);
                return false;
            }
            cast = new ActiveMagicCast
            {
                Preparation = preparation,
                Transaction = transaction,
                Mana = mana,
                DamageType = action.DamageType
            };
            return true;
        }

        public static void Commit(ActiveMagicCast cast)
        {
            if (cast?.Transaction == null) return;
            PlayerMagicCastCommit.TryCommit(cast.Transaction,
                cast.Mana != null ? cast.Mana.MaxMana : 0, cast.DamageType.ToString());
        }

        public static void Cancel(ActiveMagicCast cast)
        {
            if (cast?.Transaction == null) return;
            if (cast.Transaction.Cancel(amount => cast.Mana?.RestoreMana(amount)))
                SpellCastPreparationProvider.Cancel(cast.Preparation);
        }

        public static Vector2 Origin(SkillEffectContext context)
        {
            var controller = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            return controller != null ? (Vector2)controller.transform.position : context.WorldPosition;
        }

        public static Vector2 Facing(SkillEffectContext context)
        {
            var controller = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            Vector2 facing = controller != null ? controller.LastFacingDirection : Vector2.right;
            return facing.sqrMagnitude > .0001f ? facing.normalized : Vector2.right;
        }

        public static Vector2 AimPoint(SkillEffectContext context, float targetingRange)
        {
            Vector2 origin = Origin(context);
            if (context.Target != null)
            {
                Vector2 target = context.Target.transform.position;
                Vector2 delta = target - origin;
                return delta.sqrMagnitude <= targetingRange * targetingRange
                    ? target : origin + delta.normalized * targetingRange;
            }
            return origin + Facing(context) * Mathf.Max(0f, targetingRange);
        }

        public static bool HasLineOfEffect(Vector2 from, Vector2 to, GameObject source, GameObject target = null)
        {
            foreach (var hit in Physics2D.LinecastAll(from, to))
            {
                var collider = hit.collider;
                if (collider == null || collider.isTrigger) continue;
                if (source != null && collider.transform.IsChildOf(source.transform)) continue;
                if (target != null && collider.transform.IsChildOf(target.transform)) continue;
                if (collider.GetComponentInParent<EnemyHealth>() != null) continue;
                return false;
            }
            return true;
        }

        public static List<EnemyHealth> OrderedEnemies(Vector2 origin, Func<EnemyHealth, bool> predicate)
        {
            var result = new List<EnemyHealth>();
            foreach (var enemy in EnemyHealth.ActiveInstances)
                if (enemy != null && !enemy.IsDead && enemy.gameObject.activeInHierarchy
                    && (predicate == null || predicate(enemy))) result.Add(enemy);
            result.Sort((left, right) =>
            {
                float ld = ((Vector2)left.transform.position - origin).sqrMagnitude;
                float rd = ((Vector2)right.transform.position - origin).sqrMagnitude;
                int distance = ld.CompareTo(rd);
                return distance != 0 ? distance : string.CompareOrdinal(left.EnemyInstanceId, right.EnemyInstanceId);
            });
            return result;
        }

        public static int Damage(EnemyHealth target, int damage, DamageType type, Vector2 source,
            ActiveMagicCast cast = null, bool isPrimaryDamage = true,
            bool canTriggerCapstones = true, bool canTriggerStatusEffects = true,
            bool canTriggerReactions = true)
        {
            if (target == null || damage <= 0) return 0;
            int resolvedDamage = ResolveDirectDamage(cast, damage, out bool critical);
            int before = target.CurrentHp;
            target.TakeDamage(new DamageRequest(target.EnemyId, resolvedDamage)
            {
                DamageType = type,
                SourcePosition = source,
                KnockbackForce = 0f,
                SourceKind = cast != null ? DamageSourceKind.PlayerMagic : DamageSourceKind.None,
                SpellDiscipline = cast?.Preparation.Request.Discipline ?? SpellDiscipline.None,
                ActionToken = cast?.Preparation.Request.ActionToken ?? string.Empty,
                IsCritical = critical,
                IsPrimaryDamage = isPrimaryDamage,
                IsDamageOverTimeTick = !isPrimaryDamage,
                CanTriggerCapstones = canTriggerCapstones && cast != null &&
                    cast.Preparation.Request.Discipline == SpellDiscipline.Offensive,
                CanTriggerStatusEffects = canTriggerStatusEffects,
                CanTriggerReactions = canTriggerReactions
            });
            return Mathf.Max(0, before - target.CurrentHp);
        }

        public static int ResolveDirectDamage(ActiveMagicCast cast, int damage,
            out bool critical)
        {
            float criticalChanceBonus = cast?.Preparation.CriticalChanceBonus ?? 0f;
            critical = criticalChanceBonus > 0f &&
                CindarsHope.Core.Random.UnityGameplayRandomSource.Shared.NextFloat() <
                Mathf.Clamp01(criticalChanceBonus);
            float damageMultiplier = cast?.Preparation.DirectDamageMultiplier ?? 1f;
            float criticalMultiplier = critical
                ? PlayerCombatStatsProvider.CritMultiplier : 1f;
            return Mathf.Max(0, Mathf.RoundToInt(
                damage * damageMultiplier * criticalMultiplier));
        }

        public static StatusEffectSO ResolveStatus(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId)) return null;
            var database = GameBootstrap.Instance?.StatusEffectDatabase;
            return database != null && database.TryGetById(statusId, out var status) ? status : null;
        }

        public static void RefreshStatus(EnemyHealth target, StatusEffectSO status, float durationSeconds,
            string sourceId = null)
        {
            if (target == null || status == null) return;
            bool refreshed = target.StatusEffects.HasStatusEffect(status.Id);
            if (refreshed && string.IsNullOrEmpty(sourceId))
                target.StatusEffects.RemoveStatusEffect(status.Id);
            else if (!string.IsNullOrEmpty(sourceId))
                target.StatusEffects.RemoveStatusEffect(status.Id, sourceId);
            StatusEffectSO applied = status;
            if (durationSeconds > 0f)
            {
                applied = UnityEngine.Object.Instantiate(status);
                applied.DurationTurns = Mathf.Max(1, Mathf.RoundToInt(durationSeconds));
            }
            target.ApplyStatusEffect(applied, sourceId);
            if (applied != status)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(applied);
                else UnityEngine.Object.DestroyImmediate(applied);
            }
            if (refreshed)
                GameEventBus.Publish(new StatusRefreshedEvent(target.EnemyId, status.Id,
                    durationSeconds > 0f ? durationSeconds : status.DurationTurns));
            else
                GameEventBus.Publish(new StatusAppliedEvent(target.EnemyId, status.Id, string.Empty,
                    durationSeconds > 0f ? durationSeconds : status.DurationTurns));
        }
    }
}
