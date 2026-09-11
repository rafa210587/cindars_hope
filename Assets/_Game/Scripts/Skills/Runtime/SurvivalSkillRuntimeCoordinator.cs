using System;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Player.Conditions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Skills.Runtime
{
    [DisallowMultipleComponent]
    public sealed class SurvivalSkillRuntimeCoordinator : MonoBehaviour,
        IDirectionalMobilityModifierRuntime, INaturalSurvivalRateModifierRuntime,
        ICavebornCapstoneModifierRuntime
    {
        private const float EncounterEscapeQuietSeconds = 8f;

        private static SurvivalSkillRuntimeCoordinator _instance;
        private SurvivalSkillState _state;
        private int _previousHp;
        private int _previousMaxHp;
        private bool _hasHpBaseline;
        private float _retreatUntil;
        private float _retreatCostReduction;
        private float _retreatSpeedBonus;
        private int _damageSequence;
        private float _stationarySeconds;
        private Vector2 _previousPlayerPosition;
        private bool _hasPlayerPosition;
        private int _currentStamina;
        private int _maxStamina;
        private bool _isExhausted;

        public static SurvivalSkillRuntimeCoordinator Instance => _instance;
        public SurvivalSkillState State => _state;
        public int DamageSequence => _damageSequence;
        public bool IsRetreatActive => Time.time < _retreatUntil;
        public bool IsStationaryFor(float seconds) => _stationarySeconds >= Mathf.Max(0f, seconds);
        public bool HasEncounterThreat => TryResolvePrimaryThreat(out _);
        public bool IsActive => _state != null && _state.IsCavebornActive;
        public float CostMultiplier => IsActive
            ? CavebornCapstoneResolver.CostMultiplier(_state.ActiveCavebornRank)
            : 1f;
        public float DodgeDistanceBonus => IsActive
            ? CavebornCapstoneResolver.DodgeDistanceBonus
            : 0f;
        public float HealthRegenMultiplier => IsActive && _state.CavebornRegenDoubled
            ? CavebornCapstoneResolver.RankThreeHealthRegenMultiplier
            : 1f;

        public static SurvivalSkillRuntimeCoordinator Install(Transform owner)
        {
            if (_instance != null) return _instance;
            var go = new GameObject("SurvivalSkillRuntimeCoordinator");
            if (owner != null) go.transform.SetParent(owner, false);
            else if (Application.isPlaying) DontDestroyOnLoad(go);
            var runtime = go.AddComponent<SurvivalSkillRuntimeCoordinator>();
            runtime._state = DomainManagerRegistry.Get<SurvivalSkillState>()
                ?? new SurvivalSkillState();
            if (DomainManagerRegistry.Get<SurvivalSkillState>() == null)
                DomainManagerRegistry.Register(runtime._state);
            return runtime;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _state ??= DomainManagerRegistry.Get<SurvivalSkillState>()
                ?? new SurvivalSkillState();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<EnemyAggroStartedEvent>(OnEnemyAggroStarted);
            GameEventBus.Subscribe<EnemyPackAlertedEvent>(OnEnemyPackAlerted);
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Subscribe<EnemyKilledByEnemyEvent>(OnEnemyKilledByEnemy);
            GameEventBus.Subscribe<EnemyPackLeashCompletedEvent>(OnPackLeashCompleted);
            GameEventBus.Subscribe<EnemyLeashCompletedEvent>(OnEnemyLeashCompleted);
            GameEventBus.Subscribe<CaveRunLevelChangedEvent>(OnRunLevelChanged);
            GameEventBus.Subscribe<CaveRunIdentityStartedEvent>(OnRunStarted);
            GameEventBus.Subscribe<CaveRunIdentityEndedEvent>(OnRunEnded);
            GameEventBus.Subscribe<HPChangedEvent>(OnHpChanged);
            GameEventBus.Subscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Subscribe<PlayerFatigueChangedEvent>(OnFatigueChanged);
            GameEventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            GameEventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            GameEventBus.Subscribe<CavePlayerDefeatedEvent>(OnCavePlayerDefeated);
            GameEventBus.Subscribe<PlayerOffensiveActionCommittedEvent>(OnOffensiveActionCommitted);
            GameEventBus.Subscribe<SkillTreeRespecCompletedEvent>(OnSkillTreeRespecCompleted);
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            DirectionalMobilityModifierProvider.Source = this;
            NaturalSurvivalRateModifierProvider.Source = this;
            CavebornCapstoneProvider.Source = this;
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EnemyAggroStartedEvent>(OnEnemyAggroStarted);
            GameEventBus.Unsubscribe<EnemyPackAlertedEvent>(OnEnemyPackAlerted);
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Unsubscribe<EnemyKilledByEnemyEvent>(OnEnemyKilledByEnemy);
            GameEventBus.Unsubscribe<EnemyPackLeashCompletedEvent>(OnPackLeashCompleted);
            GameEventBus.Unsubscribe<EnemyLeashCompletedEvent>(OnEnemyLeashCompleted);
            GameEventBus.Unsubscribe<CaveRunLevelChangedEvent>(OnRunLevelChanged);
            GameEventBus.Unsubscribe<CaveRunIdentityStartedEvent>(OnRunStarted);
            GameEventBus.Unsubscribe<CaveRunIdentityEndedEvent>(OnRunEnded);
            GameEventBus.Unsubscribe<HPChangedEvent>(OnHpChanged);
            GameEventBus.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged);
            GameEventBus.Unsubscribe<PlayerFatigueChangedEvent>(OnFatigueChanged);
            GameEventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            GameEventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            GameEventBus.Unsubscribe<CavePlayerDefeatedEvent>(OnCavePlayerDefeated);
            GameEventBus.Unsubscribe<PlayerOffensiveActionCommittedEvent>(OnOffensiveActionCommitted);
            GameEventBus.Unsubscribe<SkillTreeRespecCompletedEvent>(OnSkillTreeRespecCompleted);
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            if (ReferenceEquals(DirectionalMobilityModifierProvider.Source, this))
                DirectionalMobilityModifierProvider.Source = null;
            if (ReferenceEquals(NaturalSurvivalRateModifierProvider.Source, this))
                NaturalSurvivalRateModifierProvider.Source = null;
            if (ReferenceEquals(CavebornCapstoneProvider.Source, this))
                CavebornCapstoneProvider.Source = null;
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            if (ReferenceEquals(DomainManagerRegistry.Get<SurvivalSkillState>(), _state))
                DomainManagerRegistry.Unregister(_state);
            _instance = null;
        }

        private void Update()
        {
            UpdateStationaryTime();
            _state?.AdvanceTime(Time.deltaTime);
            if (_state != null && _state.IsCavebornActive)
            {
                if (ResolveCavebornRank() <= 0) _state.ClearActiveCaveborn();
                else _state.ObserveCavebornCombatState(IsInCombat());
            }
            if (_retreatUntil > 0f && Time.time >= _retreatUntil)
                ClearRetreat();
            if (_state != null && _state.AdvanceEncounterQuietTime(
                Time.deltaTime, out string escapedEncounterId))
            {
                ClearRetreat();
                GameEventBus.Publish(new SurvivalEncounterResolvedEvent(
                    escapedEncounterId, "permanent_escape"));
            }
        }

        private void UpdateStationaryTime()
        {
            var player = PlayerController.ActiveInstance;
            if (player == null || Time.deltaTime <= 0f)
            {
                _stationarySeconds = 0f;
                _hasPlayerPosition = false;
                return;
            }
            var position = (Vector2)player.transform.position;
            if (!_hasPlayerPosition)
            {
                _previousPlayerPosition = position;
                _hasPlayerPosition = true;
                return;
            }
            float speed = Vector2.Distance(position, _previousPlayerPosition) / Time.deltaTime;
            _previousPlayerPosition = position;
            _stationarySeconds = speed <= .05f && player.MoveInput.sqrMagnitude <= .0001f
                ? _stationarySeconds + Time.deltaTime
                : 0f;
        }

        public bool TryActivateRetreat(float durationSeconds, float costReduction,
            float speedBonus)
        {
            if (_state == null || !_state.HasActiveEncounter ||
                !TryResolvePrimaryThreat(out _))
                return false;
            _retreatUntil = Time.time + Mathf.Max(0f, durationSeconds);
            _retreatCostReduction = Mathf.Clamp01(costReduction);
            _retreatSpeedBonus = Mathf.Max(0f, speedBonus);
            return _retreatUntil > Time.time;
        }

        public void ClearRetreat()
        {
            _retreatUntil = 0f;
            _retreatCostReduction = 0f;
            _retreatSpeedBonus = 0f;
        }

        public DirectionalMobilityModifier Resolve(MobilityActionKind actionKind,
            float directionX, float directionY)
        {
            float cost = CostMultiplier;
            float speed = 1f;
            if (IsRetreatActive && TryResolvePrimaryThreat(out var threat))
            {
                var player = PlayerController.ActiveInstance;
                if (player != null && SurvivalSkillActionRules.IsMovingAway(
                    player.transform.position, threat.transform.position,
                    new Vector2(directionX, directionY)))
                {
                    cost *= 1f - _retreatCostReduction;
                    speed = 1f + _retreatSpeedBonus;
                }
            }
            return new DirectionalMobilityModifier(cost, speed);
        }

        public float ResolveMultiplier(NaturalSurvivalRateChannel channel,
            float worldX, float worldY)
        {
            float multiplier = channel == NaturalSurvivalRateChannel.HealthRegen
                ? HealthRegenMultiplier
                : 1f;
            if (_state == null || _state.ActiveCampRemainingSeconds <= 0f ||
                !SurvivalSkillActionRules.IsInsideInclusiveRadius(
                    new Vector2(worldX, worldY),
                    new Vector2(_state.ActiveCampPositionX, _state.ActiveCampPositionY), 2.5f))
                return multiplier;
            float campMultiplier = channel == NaturalSurvivalRateChannel.HungerDrain ||
                   channel == NaturalSurvivalRateChannel.FatigueGain
                ? .5f
                : 1.5f;
            return multiplier * campMultiplier;
        }

        public int ResolveIncomingDamage(int rawDamage, DamageType damageType)
            => IsActive
                ? CavebornCapstoneResolver.ResolveIncomingDamage(
                    rawDamage, damageType, _state.ActiveCavebornRank)
                : rawDamage;

        public float ResolveStatusDuration(float durationSeconds, CavebornStatusFamily family)
            => IsActive
                ? CavebornCapstoneResolver.ResolveStatusDuration(
                    durationSeconds, family, _state.ActiveCavebornRank)
                : Mathf.Max(0f, durationSeconds);

        public bool TryConsumeLastBreath() =>
            _state != null && _state.LastBreathArmedRemainingSeconds > 0f &&
            _state.TryConsumeLastBreathForActiveEncounter();

        public bool CanUseLastBreath => _state != null &&
            _state.LastBreathArmedRemainingSeconds > 0f &&
            !_state.IsLastBreathConsumed(_state.ActiveEncounterId);

        public bool CanUseCamp(string runId) => _state != null &&
            !string.IsNullOrWhiteSpace(runId) &&
            !string.Equals(_state.CampUsedRunId, runId, StringComparison.Ordinal);

        public bool TryActivateCamp(string runId, int caveLevel, float duration,
            Vector2 position)
        {
            if (_state == null || !_state.TryMarkCampUsed(runId)) return false;
            _state.ActivateCamp(runId, caveLevel, duration, position.x, position.y);
            return _state.ActiveCampRemainingSeconds > 0f;
        }

        private void OnEnemyAggroStarted(EnemyAggroStartedEvent evt)
        {
            BeginFromAggro(evt.EnemyInstanceId);
        }

        private void OnEnemyPackAlerted(EnemyPackAlertedEvent evt)
        {
            if (_state == null || !_state.HasActiveEncounter || evt.MemberInstanceIds == null)
                return;

            for (int i = 0; i < evt.MemberInstanceIds.Length; i++)
            {
                string enemyInstanceId = evt.MemberInstanceIds[i];
                if (!IsLiveEnemyInstance(enemyInstanceId))
                    continue;
                if (_state.JoinActiveEncounter(enemyInstanceId))
                    GameEventBus.Publish(new SurvivalEncounterEnemyJoinedEvent(
                        _state.ActiveEncounterId, enemyInstanceId));
            }
        }

        private static bool IsLiveEnemyInstance(string enemyInstanceId)
        {
            if (string.IsNullOrWhiteSpace(enemyInstanceId)) return false;
            var active = EnemyHealth.ActiveInstances;
            for (int i = 0; i < active.Count; i++)
            {
                var enemy = active[i];
                if (enemy != null && !enemy.IsDead && string.Equals(
                    enemy.EnemyInstanceId, enemyInstanceId, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private void BeginFromAggro(string enemyInstanceId)
        {
            if (_state == null || string.IsNullOrWhiteSpace(enemyInstanceId) ||
                !TryGetEncounterScope(out string scopeId, out int caveLevel))
                return;

            if (_state.HasActiveEncounter && !_state.IsActiveEncounterScope(scopeId, caveLevel))
                ResolveEncounter("scope_transition");

            var change = _state.RegisterEnemyAggro(scopeId, caveLevel, enemyInstanceId);
            if (change == SurvivalSkillState.EncounterMembershipChange.Started)
                GameEventBus.Publish(new SurvivalEncounterStartedEvent(
                    _state.ActiveEncounterId, scopeId, caveLevel, enemyInstanceId));
            else if (change == SurvivalSkillState.EncounterMembershipChange.Joined)
                GameEventBus.Publish(new SurvivalEncounterEnemyJoinedEvent(
                    _state.ActiveEncounterId, enemyInstanceId));
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
            => RemoveEncounterEnemy(evt.EnemyInstanceId);

        private void OnEnemyKilledByEnemy(EnemyKilledByEnemyEvent evt)
            => RemoveEncounterEnemy(evt.VictimInstanceId);

        private void RemoveEncounterEnemy(string enemyInstanceId)
        {
            if (_state != null && _state.RemoveEnemyAndResolveIfEmpty(
                enemyInstanceId, EncounterEscapeQuietSeconds, out string resolved))
            {
                ClearRetreat();
                GameEventBus.Publish(new SurvivalEncounterResolvedEvent(resolved, "all_dead"));
            }
        }

        private void OnPackLeashCompleted(EnemyPackLeashCompletedEvent evt)
        {
            _state?.MarkEnemiesLeashed(evt.MemberInstanceIds, EncounterEscapeQuietSeconds);
        }

        private void OnEnemyLeashCompleted(EnemyLeashCompletedEvent evt)
        {
            _state?.MarkEnemyLeashed(evt.EnemyInstanceId, EncounterEscapeQuietSeconds);
        }

        private void OnRunLevelChanged(CaveRunLevelChangedEvent evt)
        {
            ResolveEncounter("level_transition");
            _state?.DissolveCamp();
        }

        private void OnRunStarted(CaveRunIdentityStartedEvent evt)
        {
            var conditions = PlayerConditionService.Instance;
            _isExhausted = conditions != null &&
                conditions.CurrentThreshold >= FatigueThreshold.Exhausted;
            TryActivateCaveborn();
        }

        private void OnRunEnded(CaveRunIdentityEndedEvent evt)
        {
            ResolveEncounter(evt.Reason);
            _state?.DissolveCamp();
            if (_state != null && string.Equals(
                _state.ActiveCavebornRunId, evt.RunId, StringComparison.Ordinal))
                _state.ClearActiveCaveborn();
        }

        private void ResolveEncounter(string reason)
        {
            if (_state == null || !_state.HasActiveEncounter) return;
            string resolved = _state.ResolveActiveEncounter();
            ClearRetreat();
            GameEventBus.Publish(new SurvivalEncounterResolvedEvent(resolved, reason));
        }

        private void OnHpChanged(HPChangedEvent evt)
        {
            if (_state == null) return;
            int previousHp = _previousHp;
            int previousMaxHp = _previousMaxHp;
            bool hadBaseline = _hasHpBaseline;
            _previousHp = evt.CurrentHP;
            _previousMaxHp = evt.MaxHP;
            TryActivateCaveborn();
            if (_state.IsRestoreInProgress || !hadBaseline)
            {
                _hasHpBaseline = true;
                return;
            }
            bool crossed = evt.Delta < 0 && previousMaxHp > 0 && evt.MaxHP > 0 &&
                previousHp / (float)previousMaxHp >= .25f &&
                evt.CurrentHP / (float)evt.MaxHP < .25f;
            if (crossed) _state.TryArmLastBreath(5f);
        }

        private void OnStaminaChanged(StaminaChangedEvent evt)
        {
            _currentStamina = evt.CurrentStamina;
            _maxStamina = evt.MaxStamina;
            TryActivateCaveborn();
        }

        private void OnFatigueChanged(PlayerFatigueChangedEvent evt)
        {
            _isExhausted = evt.Threshold >= (int)FatigueThreshold.Exhausted;
            TryActivateCaveborn();
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            if (evt == null || evt.DamageAmount <= 0) return;
            _damageSequence++;
            _state?.CancelEncounterQuietResolution();
            _state?.DissolveCamp();
        }

        private void OnOffensiveActionCommitted(PlayerOffensiveActionCommittedEvent evt)
        {
            _state?.CancelEncounterQuietResolution();
            ClearRetreat();
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            ResolveEncounter("player_defeated");
            _state?.ClearActiveCaveborn();
        }

        private void OnCavePlayerDefeated(CavePlayerDefeatedEvent evt)
        {
            ResolveEncounter("player_defeated");
            _state?.ClearActiveCaveborn();
        }

        private void OnSkillTreeRespecCompleted(SkillTreeRespecCompletedEvent evt)
            => _state?.OnCavebornRespec();

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            ResolveEncounter("scene_transition");
            _state?.DissolveCamp();
        }

        private static bool TryGetEncounterScope(out string scopeId, out int caveLevel)
        {
            var cave = DomainManagerRegistry.Get<ICaveRunContext>();
            if (cave != null && !string.IsNullOrWhiteSpace(cave.CaveRunId) &&
                cave.CurrentCaveLevel > 0)
            {
                scopeId = cave.CaveRunId;
                caveLevel = cave.CurrentCaveLevel;
                return true;
            }

            Scene scene = SceneManager.GetActiveScene();
            scopeId = ComposeSceneScopeId(scene.path, scene.name);
            caveLevel = 0;
            return !string.IsNullOrWhiteSpace(scopeId);
        }

        public static string ComposeSceneScopeId(string scenePath, string sceneName)
        {
            string stableSceneId = !string.IsNullOrWhiteSpace(scenePath)
                ? scenePath.Replace('\\', '/')
                : sceneName;
            return string.IsNullOrWhiteSpace(stableSceneId)
                ? string.Empty
                : $"scene:{stableSceneId}";
        }

        private void TryActivateCaveborn()
        {
            if (_state == null || _state.IsRestoreInProgress ||
                !TryGetCaveRun(out string runId)) return;

            var player = DomainManagerRegistry.Get<IPlayerRuntime>();
            int currentHp = player?.CurrentHP ?? _previousHp;
            int maxHp = player?.MaxHP ?? _previousMaxHp;
            var stamina = DomainManagerRegistry.Get<IStaminaRuntime>();
            int currentStamina = stamina?.CurrentStamina ?? _currentStamina;
            int maxStamina = stamina?.MaxStamina ?? _maxStamina;
            int rank = ResolveCavebornRank();
            bool consumed = string.Equals(_state.CavebornConsumedRunId,
                runId, StringComparison.Ordinal);
            if (!CavebornCapstoneResolver.ShouldActivate(rank, runId, consumed,
                    currentHp, maxHp, currentStamina, maxStamina, _isExhausted) ||
                !_state.TryActivateCaveborn(runId, rank, IsInCombat())) return;

            GameEventBus.Publish(new CavebornCapstoneActivatedEvent(
                runId, rank, _state.ActiveCavebornRemainingSeconds));
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Nascido da Caverna ativado."));
        }

        private static bool TryGetCaveRun(out string runId)
        {
            var cave = DomainManagerRegistry.Get<ICaveRunContext>();
            runId = cave?.CaveRunId ?? string.Empty;
            return !string.IsNullOrWhiteSpace(runId) && cave.CurrentCaveLevel > 0;
        }

        private static int ResolveCavebornRank()
            => CavebornCapstoneResolver.ClampRank(
                DomainManagerRegistry.Get<ISkillTreeRuntime>()?.GetRank(
                    CavebornCapstoneResolver.NodeId) ?? 0);

        private static bool IsInCombat()
            => CombatStateProvider.IsInCombat?.Invoke() == true;

        private bool TryResolvePrimaryThreat(out EnemyHealth threat)
        {
            threat = null;
            if (_state == null || !_state.HasActiveEncounter) return false;
            var player = PlayerController.ActiveInstance;
            if (player == null) return false;
            float bestDistance = float.MaxValue;
            string bestId = null;
            var active = EnemyHealth.ActiveInstances;
            for (int i = 0; i < active.Count; i++)
            {
                var candidate = active[i];
                if (candidate == null || candidate.IsDead ||
                    !ContainsEnemy(_state, candidate.EnemyInstanceId)) continue;
                float distance = ((Vector2)candidate.transform.position -
                    (Vector2)player.transform.position).sqrMagnitude;
                if (threat == null || SurvivalSkillActionRules.CompareTargets(
                    distance, candidate.EnemyInstanceId, bestDistance, bestId) < 0)
                {
                    threat = candidate;
                    bestDistance = distance;
                    bestId = candidate.EnemyInstanceId;
                }
            }
            return threat != null;
        }

        private static bool ContainsEnemy(SurvivalSkillState state, string enemyId)
        {
            foreach (var id in state.ActiveEnemyInstanceIds)
                if (string.Equals(id, enemyId, StringComparison.Ordinal)) return true;
            return false;
        }
    }
}
