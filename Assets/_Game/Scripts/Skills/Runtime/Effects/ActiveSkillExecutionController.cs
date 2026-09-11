using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Combat;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Interaction;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Skills.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInput = UnityEngine.Input;

namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11: Central controller that bridges active skill slots to effect execution.
    // Input: numeric keys 1-4 map to active slots 0-3.
    // Pipeline: input -> resolve skill action -> validate unlocked -> resolve effect id ->
    //           resolve target -> execute -> apply cooldown -> publish feedback.
    // Does not own active slot state or the skill tree. Resource costs are enforced by concrete
    // executors; mapping ownership lives in the pure SkillActionEffectCatalog.
    [DisallowMultipleComponent]
    public sealed class ActiveSkillExecutionController : MonoBehaviour
    {
        // Numeric keys 1-4 map to active slot indices 0-3
        private static readonly KeyCode[] SlotInputKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
        private static ActiveSkillExecutionController _instance;

        // fable_71: singleton de leitura para a HUD ler cooldown e disparar uso por clique
        // (mesma rota das teclas 1-4). Nao e global search — e o instance pattern ja usado aqui.
        public static ActiveSkillExecutionController Instance => _instance;

        [SerializeField] private SkillTargetResolver _targetResolver;

        private SkillEffectRegistry _registry;
        private readonly SkillCooldownTracker _cooldowns = new SkillCooldownTracker();
        private bool _bootstrapped;
        private GameObject _avatar;
        private readonly SkillCastTimeline _timeline = new SkillCastTimeline();
        private PendingSkillCast _activeCast;
        private PendingSkillCast _chargedCast;
        private readonly ChargedSkillCastState _chargeState = new ChargedSkillCastState();
        private int _chargedSlotIndex = -1;
        private bool _timelineEventsBound;

        public GameObject BoundAvatar => _avatar;
        public SkillCastPhase CurrentCastPhase => _timeline.Phase;
        public bool HasActiveCast => _activeCast != null;
        public bool IsChargingSkill => _chargedCast != null;

        public void BindAvatar(GameObject avatar, InteractionSystem interactionSystem)
        {
            _avatar = avatar != null && avatar.activeInHierarchy ? avatar : null;
            _targetResolver?.SetInteractionSystem(_avatar != null ? interactionSystem : null);
        }

        public static bool TryGetEffectIdForValidation(string skillActionId, out string effectId)
            => SkillActionEffectCatalog.TryGetEffectId(skillActionId, out effectId);

        // ── fable_71: API read-only de cooldown + uso por clique (convergem com as teclas 1-4) ──
        public float GetSlotCooldownRemaining(int slotIndex)
            => _cooldowns.Remaining(GetSlotActionId(slotIndex), Time.time);

        public float GetSlotCooldownTotal(int slotIndex)
            => _cooldowns.Total(GetSlotActionId(slotIndex));

        private static string GetSlotActionId(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotInputKeys.Length) return null;
            var manager = SkillTreeManager.Instance;
            var raw = manager?.State?.GetActiveSlotSkillActionId(slotIndex);
            if (string.IsNullOrEmpty(raw)) return null;
            return manager.NodeIndex.TryGetValue(raw, out var node) ? node.UnlockedSkillActionId : raw;
        }

        // Ponto unico de uso de slot: a HUD (clique) e o Update (teclas 1-4) chamam isto.
        public void TryUseSlot(int slotIndex) => TryExecuteSlot(slotIndex, false);
        public void BeginChargedSlot(int slotIndex) => TryExecuteSlot(slotIndex, true);
        public void ReleaseChargedSlot() => ReleaseChargedCast();

        public static ActiveSkillExecutionController Install()
        {
            if (_instance != null)
                return _instance;

            var go = new GameObject("ActiveSkillExecutionController");
            if (Application.isPlaying) DontDestroyOnLoad(go);
            var controller = go.AddComponent<ActiveSkillExecutionController>();
            controller.InitializeAsSingleton();
            return _instance;
        }

        private void Bootstrap()
        {
            if (_bootstrapped)
                return;

            // Attach target resolver
            if (_targetResolver == null)
            {
                var resolverGo = new GameObject("SkillTargetResolver");
                resolverGo.transform.SetParent(transform);
                _targetResolver = resolverGo.AddComponent<SkillTargetResolver>();
            }

            Debug.Log("[ActiveSkillExecutionController] Bootstrapped. Registered effect: farm.crop.water_skill + balance patch feedback executors.");
            _bootstrapped = true;
        }

        private void Awake()
        {
            InitializeAsSingleton();
        }

        private void InitializeAsSingleton()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            if (!_timelineEventsBound)
            {
                _timeline.PhaseChanged += HandleTimelinePhaseChanged;
                _timelineEventsBound = true;
            }

            // Ensure resolver is present if created via inspector
            if (_targetResolver == null)
            {
                _targetResolver = GetComponentInChildren<SkillTargetResolver>();
                if (_targetResolver == null)
                {
                    var resolverGo = new GameObject("SkillTargetResolver");
                    resolverGo.transform.SetParent(transform);
                    _targetResolver = resolverGo.AddComponent<SkillTargetResolver>();
                }
            }

            Bootstrap();
        }

        private void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
                // A replacement may have been installed while this component was disabled.
                enabled = false;
                return;
            }
            _instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            GameEventBus.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
            GameEventBus.Subscribe<PlayerDamagedEvent>(HandlePlayerDamaged);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            GameEventBus.Unsubscribe<PlayerDiedEvent>(HandlePlayerDied);
            GameEventBus.Unsubscribe<PlayerDamagedEvent>(HandlePlayerDamaged);
            CancelActiveCast();
            BindAvatar(null, null);
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CancelActiveCast();
            _registry = null;
            // Re-wire InteractionSystem when scene changes
            WireInteractionSystem();
            EnsureRegistry();
        }

        private void EnsureRegistry()
        {
            if (_registry != null)
                return;
            var bootstrap = GameBootstrap.Instance;
            _registry = ActiveSkillExecutorCatalog.CreateRegistry(
                EquipmentManager.Instance,
                bootstrap?.ItemDatabase as ItemDatabaseSO,
                bootstrap?.WeaponDatabase,
                bootstrap?.SpellDatabase);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            CancelActiveCast();
            if (_avatar == null || _avatar.scene == scene) BindAvatar(null, null);
        }

        private void HandlePlayerDied(PlayerDiedEvent evt) => CancelActiveCast();

        private void HandlePlayerDamaged(PlayerDamagedEvent evt)
        {
            if (evt != null && evt.DamageAmount > 0)
                CancelActiveCast();
        }

        private void Start()
        {
            Bootstrap();
            WireInteractionSystem();
        }

        private void WireInteractionSystem()
        {
            if (_targetResolver == null)
                return;

            var playerObject = PlayerController.ActiveInstance != null
                ? PlayerController.ActiveInstance.gameObject : null;
            var interactionSystem = playerObject != null
                ? playerObject.GetComponentInChildren<InteractionSystem>()
                : null;
            BindAvatar(playerObject, interactionSystem);
        }

        private void Update()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
                return;

            if (ActionBlockProvider.IsActionBlocked?.Invoke() == true)
            {
                if (_chargedCast != null || (_activeCast != null && !_timeline.HasCommitted))
                    PublishFeedback("A habilidade foi interrompida.", "ActionBlocked");
                CancelActiveCast();
                return;
            }

            if (bootstrap.ModalManager != null && bootstrap.ModalManager.HasActiveModal)
            {
                CancelActiveCast();
                return;
            }

            if (_chargedCast != null)
            {
                if (GetSlotActionId(_chargedSlotIndex) != _chargedCast.SkillActionId)
                {
                    CancelActiveCast();
                    return;
                }
                _chargeState.Tick(Time.deltaTime);
                if (UnityInput.GetKeyUp(SlotInputKeys[_chargedSlotIndex]))
                    ReleaseChargedCast();
                return;
            }

            TickActiveCast(Time.deltaTime);
            if (_activeCast != null)
                return;

            // Check numeric key input 1-4
            for (int slotIndex = 0; slotIndex < SlotInputKeys.Length; slotIndex++)
            {
                if (UnityInput.GetKeyDown(SlotInputKeys[slotIndex]))
                {
                    TryExecuteSlot(slotIndex, true);
                    break;
                }
            }
        }

        private void TryExecuteSlot(int slotIndex, bool beginHeldCharge)
        {
            if (slotIndex < 0 || slotIndex >= SlotInputKeys.Length) return;
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
            {
                CancelActiveCast();
                return;
            }
            if (_activeCast != null || _chargedCast != null)
            {
                PublishFeedback("A habilidade atual ainda está em execução.");
                return;
            }
            WireInteractionSystem();
            EnsureRegistry();
            if (_avatar == null)
            {
                PublishFeedback("Jogador não disponível para usar habilidade.");
                return;
            }
            // Resolve equipped skill action ID from SkillTreeManager state
            var skillTreeManager = SkillTreeManager.Instance;
            if (skillTreeManager == null)
            {
                PublishFeedback("SkillTreeManager nao disponivel.");
                return;
            }

            var rawSlotValue = skillTreeManager.State.GetActiveSlotSkillActionId(slotIndex);
            if (!TryResolveEquippedSkillAction(skillTreeManager, rawSlotValue, out var skillActionId, out var nodeId, out var resolveMessage))
            {
                PublishFeedback(string.IsNullOrWhiteSpace(resolveMessage)
                    ? $"Slot {slotIndex + 1} vazio. Equipe uma skill na skill tree (U)."
                    : resolveMessage);
                Debug.Log($"[ActiveSkillExecutionController] Slot blocked. Slot={slotIndex}, RawSlotValue={rawSlotValue ?? "<empty>"}, Reason={resolveMessage}", this);
                return;
            }

            // Dormancy and authored action data are readiness gates. They run before cooldown,
            // executor resolution or resource spending so a dormant slot cannot commit anything.
            if (!skillTreeManager.TryResolveActionData(nodeId, out var actionData, out var rank, out var readinessFailure))
            {
                PublishFeedback(readinessFailure == "DormantAction"
                    ? "Esta habilidade ainda está dormente."
                    : $"Dados da habilidade '{skillActionId}' indisponíveis.");
                return;
            }

            if (GetSlotCooldownRemaining(slotIndex) > 0f)
            {
                float remaining = GetSlotCooldownRemaining(slotIndex);
                PublishFeedback($"Slot {slotIndex + 1} em cooldown ({remaining:F1}s).");
                return;
            }

            // Resolve EffectId
            if (!SkillActionEffectCatalog.TryGetEffectId(skillActionId, out var effectId))
            {
                // TODO_INTEGRATION_NOT_FINAL: No effect defined for this skill action.
                PublishFeedback($"Skill '{skillActionId}' sem efeito implementado. (Deferred)");
                Debug.Log($"[ActiveSkillExecutionController] No effectId. Slot={slotIndex}, RawSlotValue={rawSlotValue}, ResolvedSkillActionId={skillActionId}, NodeId={nodeId}. Deferred.", this);
                return;
            }

            // Resolve executor
            var executor = _registry.Resolve(effectId);
            if (executor == null)
            {
                // TODO_INTEGRATION_NOT_FINAL: Executor not registered for this effectId.
                PublishFeedback($"Efeito '{effectId}' sem executor. (Deferred)");
                Debug.Log($"[ActiveSkillExecutionController] No executor. Slot={slotIndex}, RawSlotValue={rawSlotValue}, ResolvedSkillActionId={skillActionId}, NodeId={nodeId}, EffectId={effectId}. Deferred.", this);
                return;
            }

            // Build context
            var playerGo = _avatar;
            var context = new SkillEffectContext
            {
                SkillActionId = skillActionId,
                EffectId = effectId,
                ActiveSlotIndex = slotIndex,
                NodeId = nodeId,
                Rank = rank,
                VariantId = skillTreeManager.State.GetChosenVariant(nodeId),
                ActionData = actionData,
                Caster = playerGo,
                WorldPosition = playerGo != null ? (Vector2)playerGo.transform.position : Vector2.zero,
                SceneName = SceneManager.GetActiveScene().name,
                Time = Time.time
            };

            // Resolve target
            context.Target = _targetResolver != null
                ? _targetResolver.Resolve(executor.TargetType, context.Caster)
                : null;

            if (executor is IPreparableSkillEffectExecutor preparable)
            {
                var validation = preparable.Validate(context);
                if (!validation.Success)
                {
                    PublishFeedback(validation.FeedbackMessage, validation.FailureReason);
                    return;
                }
            }

            var pending = new PendingSkillCast(rawSlotValue, skillActionId, nodeId, effectId, executor, context);
            if (skillActionId == SkillActionEffectCatalog.RangedChargedShotActionId)
            {
                if (beginHeldCharge)
                {
                    _chargedCast = pending;
                    _chargedSlotIndex = slotIndex;
                    _chargeState.Begin(BuildChargeProfile(actionData));
                    GameEventBus.Publish(new SkillCastPhaseChangedEvent(skillActionId, "Charge",
                        actionData.ChargeTimeSeconds));
                    return;
                }

                _chargeState.Begin(BuildChargeProfile(actionData));
                _chargeState.Tick(actionData.ChargeMinimumHoldSeconds);
                _chargedCast = pending;
                _chargedSlotIndex = slotIndex;
                ReleaseChargedCast();
                return;
            }

            _activeCast = pending;
            if (pending.Executor is ISkillCastLifecycleExecutor lifecycle)
                lifecycle.OnCastStarted(pending.Context);
            float windup = pending.Executor is ISkillCastTimingResolver timingResolver
                ? timingResolver.ResolveWindupSeconds(pending.Context, actionData.WindupSeconds)
                : actionData.WindupSeconds;
            HandleTimelineResult(_timeline.Begin(windup, actionData.ActiveSeconds, actionData.RecoverySeconds));
        }

        private void ReleaseChargedCast()
        {
            if (_chargedCast == null)
                return;

            var resolution = _chargeState.Release();
            if (!resolution.CanCommit)
            {
                PublishFeedback($"Sustente o disparo por pelo menos {_chargedCast.Context.ActionData.ChargeMinimumHoldSeconds:0.00} s.",
                    "ChargeTooShort");
                ReleasePendingCast(ref _chargedCast);
                _chargedSlotIndex = -1;
                return;
            }

            var original = _chargedCast.Context.ActionData;
            var chargedAction = Instantiate(original);
            chargedAction.BaseDamage = resolution.Damage;
            chargedAction.DamagePerRank = 0;
            chargedAction.Range = resolution.Range;
            chargedAction.StaminaCost = resolution.StaminaCost;
            chargedAction.PostureDamageMultiplier = resolution.PostureMultiplier;
            _chargedCast.Context.ActionData = chargedAction;
            _chargedCast.OwnedActionData = chargedAction;
            _activeCast = _chargedCast;
            _chargedCast = null;
            _chargedSlotIndex = -1;
            HandleTimelineResult(_timeline.Begin(chargedAction.WindupSeconds,
                chargedAction.ActiveSeconds, chargedAction.RecoverySeconds));
        }

        private static ChargedSkillProfile BuildChargeProfile(SkillActionSO action)
        {
            return new ChargedSkillProfile(
                action.ChargeMinimumHoldSeconds,
                action.ChargeTimeSeconds,
                action.BaseDamage,
                action.ChargeMaximumDamage,
                action.Range,
                action.ChargeMaximumRange,
                action.PostureDamageMultiplier,
                action.ChargeMaximumPostureDamageMultiplier,
                action.StaminaCost,
                action.ChargeMaximumStaminaCost);
        }

        private void TickActiveCast(float scaledDeltaSeconds)
        {
            if (_activeCast == null)
                return;

            if (!_timeline.HasCommitted && _activeCast.Executor is ISkillCastLifecycleExecutor lifecycle)
            {
                var validation = lifecycle.TickBeforeCommit(_activeCast.Context, scaledDeltaSeconds);
                if (!validation.Success)
                {
                    PublishFeedback(validation.FeedbackMessage, validation.FailureReason);
                    CancelActiveCast();
                    return;
                }
            }

            HandleTimelineResult(_timeline.Tick(scaledDeltaSeconds));
        }

        private void HandleTimelineResult(SkillCastTimelineResult timelineResult)
        {
            if (_activeCast == null)
                return;

            if (timelineResult.ShouldCommit && !CommitActiveCast())
            {
                CancelActiveCast();
                return;
            }

            if (timelineResult.ShouldCommit)
                timelineResult = _timeline.ContinueAfterCommit();

            if (timelineResult.Completed)
            {
                ReleasePendingCast(ref _activeCast);
                _timeline.Reset();
            }
        }

        private bool CommitActiveCast()
        {
            var cast = _activeCast;
            if (cast == null)
                return false;

            if (cast.Executor is IPreparableSkillEffectExecutor preparable)
            {
                var validation = preparable.Validate(cast.Context);
                if (!validation.Success)
                {
                    PublishFeedback(validation.FeedbackMessage, validation.FailureReason);
                    return false;
                }
            }

            bool offensive = IsOffensive(cast.Context.ActionData);
            if (offensive)
            {
                // Commit listeners must remove directional utility bonuses before the
                // offensive executor samples movement, costs or displacement.
                GameEventBus.Publish(new PlayerOffensiveActionCommittedEvent(
                    cast.SkillActionId, "ActiveSkill"));
            }

            var result = cast.Executor.Execute(cast.Context);
            if (!result.Success)
            {
                PublishFeedback(result.FeedbackMessage, result.FailureReason);
                Debug.Log($"[ActiveSkillExecutionController] Skill failed at commit. RawSlotValue={cast.RawSlotValue}, ResolvedSkillActionId={cast.SkillActionId}, NodeId={cast.NodeId}, EffectId={cast.EffectId}, Executor={cast.Executor.GetType().Name}, Reason={result.FailureReason}, Feedback={result.FeedbackMessage}", this);
                return false;
            }

            float cooldownSeconds = cast.Context.ActionData != null
                ? cast.Context.ActionData.ResolveRank(cast.Context.Rank).CooldownSeconds
                : Mathf.Max(0f, result.CooldownSeconds);
            _cooldowns.Start(cast.SkillActionId, Time.time, cooldownSeconds);
            PublishFeedback(result.FeedbackMessage);
            Debug.Log($"[ActiveSkillExecutionController] Skill committed. RawSlotValue={cast.RawSlotValue}, ResolvedSkillActionId={cast.SkillActionId}, NodeId={cast.NodeId}, EffectId={cast.EffectId}, Executor={cast.Executor.GetType().Name}", this);
            return true;
        }

        private static bool IsOffensive(SkillActionSO action)
        {
            if (action == null) return false;
            switch (action.SkillActionType)
            {
                case SkillActionType.DamageSkill:
                case SkillActionType.ProjectileSkill:
                case SkillActionType.AreaSkill:
                case SkillActionType.LeapSkill:
                    return true;
                default:
                    return false;
            }
        }

        private void CancelActiveCast()
        {
            if (_activeCast == null && _chargedCast == null)
                return;

            if (_activeCast != null)
            {
                if (_activeCast.Executor is ISkillCastLifecycleExecutor lifecycle)
                    lifecycle.OnCastCancelled(_activeCast.Context);
                _timeline.Cancel();
            }
            ReleasePendingCast(ref _activeCast);
            ReleasePendingCast(ref _chargedCast);
            _chargeState.Cancel();
            _chargedSlotIndex = -1;
            _timeline.Reset();
        }

        private static void ReleasePendingCast(ref PendingSkillCast cast)
        {
            if (cast?.OwnedActionData != null)
                Destroy(cast.OwnedActionData);
            cast = null;
        }

        private void HandleTimelinePhaseChanged(SkillCastPhase phase, float durationSeconds)
        {
            if (_activeCast == null)
                return;

            GameEventBus.Publish(new SkillCastPhaseChangedEvent(
                _activeCast.SkillActionId,
                phase.ToString(),
                durationSeconds));
        }

        private static bool TryResolveEquippedSkillAction(
            SkillTreeManager skillTreeManager,
            string rawSlotValue,
            out string skillActionId,
            out string nodeId,
            out string message)
        {
            skillActionId = string.Empty;
            nodeId = string.Empty;
            message = string.Empty;

            if (skillTreeManager == null)
            {
                message = "SkillTreeManager nao disponivel.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(rawSlotValue))
            {
                message = string.Empty;
                return false;
            }

            if (skillTreeManager.NodeIndex.TryGetValue(rawSlotValue, out var directNode))
            {
                nodeId = directNode.SkillNodeId;
                if (!ResolvePurchasedEquippableNode(skillTreeManager, directNode, out skillActionId, out message))
                {
                    return false;
                }

                return true;
            }

            foreach (var candidate in skillTreeManager.NodeIndex.Values)
            {
                if (candidate == null || candidate.UnlockedSkillActionId != rawSlotValue)
                {
                    continue;
                }

                nodeId = candidate.SkillNodeId;
                if (!ResolvePurchasedEquippableNode(skillTreeManager, candidate, out skillActionId, out message))
                {
                    return false;
                }

                return true;
            }

            message = $"Slot contem skill desconhecida '{rawSlotValue}'.";
            return false;
        }

        private static bool ResolvePurchasedEquippableNode(
            SkillTreeManager skillTreeManager,
            SkillNodeDataSO node,
            out string skillActionId,
            out string message)
        {
            skillActionId = string.Empty;
            message = string.Empty;

            if (node == null)
            {
                message = "Skill node nao encontrado.";
                return false;
            }

            if (node.SkillCategory != SkillCategory.EquippableSkill)
            {
                message = $"Skill '{node.SkillNodeId}' nao e equipavel.";
                return false;
            }

            if (!skillTreeManager.IsNodePurchased(node.SkillNodeId))
            {
                message = $"Skill '{node.SkillNodeId}' nao esta comprada.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(node.UnlockedSkillActionId))
            {
                message = $"Skill '{node.SkillNodeId}' nao tem SkillActionId.";
                return false;
            }

            skillActionId = node.UnlockedSkillActionId;
            return true;
        }

        private void PublishFeedback(string message, string failureKey = "")
        {
            if (!string.IsNullOrEmpty(message) || !string.IsNullOrEmpty(failureKey))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(message, 2f, failureKey));
            }
        }

        private sealed class PendingSkillCast
        {
            public string RawSlotValue { get; }
            public string SkillActionId { get; }
            public string NodeId { get; }
            public string EffectId { get; }
            public ISkillEffectExecutor Executor { get; }
            public SkillEffectContext Context { get; }
            public SkillActionSO OwnedActionData { get; set; }

            public PendingSkillCast(string rawSlotValue, string skillActionId, string nodeId,
                string effectId, ISkillEffectExecutor executor, SkillEffectContext context)
            {
                RawSlotValue = rawSlotValue;
                SkillActionId = skillActionId;
                NodeId = nodeId;
                EffectId = effectId;
                Executor = executor;
                Context = context;
            }
        }
    }
}
