using System;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class SurvivalSkillEffectExecutor : ISkillEffectExecutor,
        IPreparableSkillEffectExecutor, ISkillCastLifecycleExecutor
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly SkillActionSO _defaultAction;
        private Vector2 _channelPreviousPosition;
        private int _channelDamageSequence;
        private bool _channelActive;
        private float _channelMovingSeconds;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Utility;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public SurvivalSkillEffectExecutor(string effectId, string displayName,
            SkillActionSO defaultAction)
        {
            _effectId = effectId;
            _displayName = displayName;
            _defaultAction = defaultAction;
        }

        public SkillEffectResult Validate(SkillEffectContext context)
        {
            var action = context?.ActionData != null ? context.ActionData : _defaultAction;
            var runtime = SurvivalSkillRuntimeCoordinator.Instance;
            if (context?.Caster == null || action == null || runtime == null)
                return Failed("RuntimeUnavailable", "Sistemas de sobrevivência indisponíveis.");

            switch (_effectId)
            {
                case "survival.last_breath":
                    return runtime.CanUseLastBreath
                        ? Ready() : Failed("LastBreathWindowClosed", "Último Fôlego não está armado.");
                case "survival.sinal_retirada":
                    if (!runtime.HasEncounterThreat)
                        return Failed("NoEncounterThreat", "Nenhuma ameaça ativa para orientar a retirada.");
                    return HasStamina(action.ResolveRank(context.Rank).StaminaCost)
                        ? Ready() : Failed("InsufficientStamina", "Stamina insuficiente.");
                case "survival.isca_improvisada":
                    return ValidateLure(context, action);
                case "survival.kit_emergencia":
                    return ValidateKit(context);
                case "survival.instinto_sobrevivencia":
                    return Ready();
                case "survival.campo_seguro":
                    return ValidateCamp(context);
                default:
                    return Failed("UnknownSurvivalEffect", "Efeito de sobrevivência desconhecido.");
            }
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var validation = Validate(context);
            if (!validation.Success) return validation;
            var action = context.ActionData ?? _defaultAction;
            switch (_effectId)
            {
                case "survival.last_breath": return ExecuteLastBreath(context, action);
                case "survival.sinal_retirada": return ExecuteRetreat(context, action);
                case "survival.isca_improvisada": return ExecuteLure(context, action);
                case "survival.kit_emergencia": return ExecuteKit(context, action);
                case "survival.instinto_sobrevivencia": return ExecuteInstinct(context, action);
                case "survival.campo_seguro": return ExecuteCamp(context, action);
                default: return Failed("UnknownSurvivalEffect", "Efeito de sobrevivência desconhecido.");
            }
        }

        public void OnCastStarted(SkillEffectContext context)
        {
            if (_effectId != "survival.kit_emergencia" || context?.Caster == null) return;
            _channelActive = true;
            _channelPreviousPosition = context.Caster.transform.position;
            _channelDamageSequence = SurvivalSkillRuntimeCoordinator.Instance?.DamageSequence ?? 0;
            _channelMovingSeconds = 0f;
            SurvivalSkillRuntimeCoordinator.Instance?.State?.BeginTransientChannel(context.SkillActionId);
        }

        public SkillEffectResult TickBeforeCommit(SkillEffectContext context, float deltaSeconds)
        {
            if (!_channelActive || _effectId != "survival.kit_emergencia") return Ready();
            if (context?.Caster == null) return Failed("CasterUnavailable", "Canal interrompido.");
            var current = (Vector2)context.Caster.transform.position;
            float speed = deltaSeconds > 0f
                ? Vector2.Distance(current, _channelPreviousPosition) / deltaSeconds : 0f;
            _channelPreviousPosition = current;
            var player = context.Caster.GetComponent<PlayerController>();
            bool moving = speed > .05f ||
                (player != null && player.MoveInput.sqrMagnitude > .0001f);
            _channelMovingSeconds = moving ? _channelMovingSeconds + Mathf.Max(0f, deltaSeconds) : 0f;
            if (_channelMovingSeconds > .2f)
                return Failed("MovedDuringChannel", "Movimento interrompeu o Kit de Emergência.");
            if ((SurvivalSkillRuntimeCoordinator.Instance?.DamageSequence ?? 0) != _channelDamageSequence)
                return Failed("DamagedDuringChannel", "Dano interrompeu o Kit de Emergência.");
            return Ready();
        }

        public void OnCastCancelled(SkillEffectContext context)
        {
            _channelActive = false;
            _channelMovingSeconds = 0f;
            SurvivalSkillRuntimeCoordinator.Instance?.State?.CancelTransientChannel();
        }

        private SkillEffectResult ExecuteLastBreath(SkillEffectContext context, SkillActionSO action)
        {
            var runtime = SurvivalSkillRuntimeCoordinator.Instance;
            var player = PlayerManager();
            if (player == null || !runtime.TryConsumeLastBreath())
                return Failed("LastBreathUnavailable", "Último Fôlego não está disponível.");
            int heal = SurvivalSkillActionRules.ResolvePercentHeal(
                player.MaxHP, action.ResolveEffectMagnitude(context.Rank));
            player.RestoreHP(heal);
            return Success($"{_displayName}: +{heal} HP.");
        }

        private SkillEffectResult ExecuteRetreat(SkillEffectContext context, SkillActionSO action)
        {
            var stamina = Stamina();
            int cost = Mathf.CeilToInt(action.ResolveRank(context.Rank).StaminaCost);
            if (stamina != null && !stamina.TrySpendStamina(cost))
                return Failed("InsufficientStamina", "Stamina insuficiente.");
            if (!SurvivalSkillRuntimeCoordinator.Instance.TryActivateRetreat(
                action.ResolveSecondaryDuration(context.Rank),
                action.ResolveEffectMagnitude(context.Rank),
                action.ResolveSecondaryMagnitude(context.Rank)))
            {
                stamina?.AddStamina(cost);
                return Failed("NoEncounterThreat", "Nenhuma ameaça ativa para orientar a retirada.");
            }
            return Success($"{_displayName}: recue da ameaça.", cost > 0);
        }

        private SkillEffectResult ExecuteLure(SkillEffectContext context, SkillActionSO action)
        {
            if (!TryResolveLurePoint(context, action, out var lurePoint))
                return Failed("InvalidPlacement", "Não há chão livre para lançar a isca.");
            var commit = CommitItem(SurvivalSkillItemIds.ImprovisedLure);
            if (!commit.Success) return ItemFailure(commit);

            var candidates = new List<EnemyHealth>();
            var active = EnemyHealth.ActiveInstances;
            for (int i = 0; i < active.Count; i++)
            {
                var enemy = active[i];
                if (enemy != null && !enemy.IsDead &&
                    SurvivalSkillActionRules.IsInsideInclusiveRadius(
                        enemy.transform.position, lurePoint, action.EffectRadius))
                    candidates.Add(enemy);
            }
            candidates.Sort((left, right) => SurvivalSkillActionRules.CompareTargets(
                ((Vector2)left.transform.position - lurePoint).sqrMagnitude, left.EnemyInstanceId,
                ((Vector2)right.transform.position - lurePoint).sqrMagnitude, right.EnemyInstanceId));
            int affected = 0;
            int count = Mathf.Min(action.MaxTargets, candidates.Count);
            for (int i = 0; i < count; i++)
            {
                var reaction = EnemySkillReactionAdapter.GetOrCreate(candidates[i].gameObject);
                if (reaction != null && reaction.ApplyLure(lurePoint,
                    action.ResolveEffectMagnitude(context.Rank),
                    action.ResolveSecondaryDuration(context.Rank)) > 0f) affected++;
            }
            return Success($"{_displayName}: {affected} alvo(s) distraído(s).", true);
        }

        private SkillEffectResult ExecuteKit(SkillEffectContext context, SkillActionSO action)
        {
            var commit = CommitItem(SurvivalSkillItemIds.FieldDressing);
            if (!commit.Success) return ItemFailure(commit);
            var player = PlayerManager();
            int heal = SurvivalSkillActionRules.ResolvePercentHeal(
                player.MaxHP, action.ResolveEffectMagnitude(context.Rank));
            _channelActive = false;
            _channelMovingSeconds = 0f;
            SurvivalSkillRuntimeCoordinator.Instance.State.CancelTransientChannel();
            player.RestoreHP(heal);
            return Success($"{_displayName}: +{heal} HP.", true);
        }

        private SkillEffectResult ExecuteInstinct(SkillEffectContext context, SkillActionSO action)
        {
            int count = TemporaryRevealRegistryProvider.Registry.RevealEligible(
                context.Caster.transform.position.x, context.Caster.transform.position.y,
                action.EffectRadius, context.SkillActionId,
                Time.time + action.ResolveSecondaryDuration(context.Rank));
            return Success($"{_displayName}: {count} pista(s) revelada(s).");
        }

        private SkillEffectResult ExecuteCamp(SkillEffectContext context, SkillActionSO action)
        {
            var cave = DomainManagerRegistry.Get<ICaveRunContext>();
            var commit = CommitItem(SurvivalSkillItemIds.CampSupply);
            if (!commit.Success) return ItemFailure(commit);
            if (!SurvivalSkillRuntimeCoordinator.Instance.TryActivateCamp(
                cave.CaveRunId, cave.CurrentCaveLevel,
                action.ResolveSecondaryDuration(context.Rank), context.Caster.transform.position))
            {
                Inventory()?.AddItem(SurvivalSkillItemIds.CampSupply, 1);
                return Failed("CampAlreadyUsed", "Campo Seguro já foi usado nesta expedição.");
            }
            return Success($"{_displayName}: zona preparada.", true);
        }

        private SkillEffectResult ValidateLure(SkillEffectContext context, SkillActionSO action)
        {
            if (!HasItem(SurvivalSkillItemIds.ImprovisedLure))
                return Failed("MissingItem", "Você não possui uma Isca Improvisada.");
            return TryResolveLurePoint(context, action, out _)
                ? Ready() : Failed("InvalidPlacement", "Não há chão livre para lançar a isca.");
        }

        private SkillEffectResult ValidateKit(SkillEffectContext context)
        {
            var player = PlayerManager();
            if (player == null || player.CurrentHP >= player.MaxHP)
                return Failed("FullHealth", "O Kit exige HP incompleto.");
            if (CombatStateProvider.IsInCombat?.Invoke() == true)
                return Failed("InCombat", "O Kit só pode ser usado fora de combate.");
            var runtime = SurvivalSkillRuntimeCoordinator.Instance;
            if (_channelActive && runtime.DamageSequence != _channelDamageSequence)
                return Failed("DamagedDuringChannel", "Dano interrompeu o Kit de Emergência.");
            return HasItem(SurvivalSkillItemIds.FieldDressing)
                ? Ready() : Failed("MissingItem", "Você não possui Curativo de Campo.");
        }

        private SkillEffectResult ValidateCamp(SkillEffectContext context)
        {
            if (CombatStateProvider.IsInCombat?.Invoke() == true)
                return Failed("InCombat", "Campo Seguro exige uma área fora de combate.");
            var cave = DomainManagerRegistry.Get<ICaveRunContext>();
            if (cave == null || string.IsNullOrWhiteSpace(cave.CaveRunId))
                return Failed("NoActiveRun", "Campo Seguro exige uma expedição ativa.");
            if (!SurvivalSkillRuntimeCoordinator.Instance.CanUseCamp(cave.CaveRunId))
                return Failed("CampAlreadyUsed", "Campo Seguro já foi usado nesta expedição.");
            return HasItem(SurvivalSkillItemIds.CampSupply)
                ? Ready() : Failed("MissingItem", "Você não possui Provisão de Acampamento.");
        }

        private static bool TryResolveLurePoint(SkillEffectContext context,
            SkillActionSO action, out Vector2 point)
        {
            point = context.Caster.transform.position;
            var player = context.Caster.GetComponent<PlayerController>();
            Vector2 direction = player != null && player.LastFacingDirection.sqrMagnitude > .001f
                ? player.LastFacingDirection.normalized : Vector2.right;
            Vector2 origin = context.Caster.transform.position;
            for (float distance = action.Range; distance >= .5f; distance -= .5f)
            {
                var candidate = origin + direction * distance;
                if (SkillWorldPlacementProvider.CanPlace(
                    origin.x, origin.y, candidate.x, candidate.y, action.Range))
                {
                    point = candidate;
                    return true;
                }
            }
            return false;
        }

        private static PlayerManager PlayerManager() =>
            GameBootstrap.Instance?.PlayerManager as PlayerManager;

        private static StaminaManager Stamina() =>
            GameBootstrap.Instance?.StaminaManager as StaminaManager;

        private static InventoryManager Inventory() =>
            GameBootstrap.Instance?.InventoryManager as InventoryManager;

        private static bool HasStamina(float amount)
        {
            var stamina = Stamina();
            return stamina == null || stamina.CurrentStamina >= Mathf.CeilToInt(amount);
        }

        private static bool HasItem(string itemId) => Inventory()?.GetAmount(itemId) > 0;

        private static SkillItemCommitResult CommitItem(string itemId) =>
            new SkillItemTransaction(new SkillItemInventoryAdapter(Inventory())).TryCommit(itemId);

        private static SkillEffectResult ItemFailure(SkillItemCommitResult result) =>
            Failed(result.Failure.ToString(), "Consumível necessário indisponível.");

        private static SkillEffectResult Ready() => SkillEffectResult.Succeeded("Pronto.");
        private static SkillEffectResult Failed(string reason, string message) =>
            SkillEffectResult.Failed(reason, message);
        private static SkillEffectResult Success(string message, bool cost = false) =>
            SkillEffectResult.Succeeded(message, cost, true);
    }
}
