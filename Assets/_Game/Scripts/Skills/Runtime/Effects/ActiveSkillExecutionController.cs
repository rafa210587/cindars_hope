using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
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

        private readonly SkillEffectRegistry _registry = ActiveSkillExecutorCatalog.CreateRegistry();
        private readonly float[] _slotCooldowns = new float[4];
        // fable_71: cooldown total (no momento do disparo) para a HUD calcular o fill radial.
        private readonly float[] _slotCooldownTotals = new float[4];
        private bool _bootstrapped;

        public static bool TryGetEffectIdForValidation(string skillActionId, out string effectId)
            => SkillActionEffectCatalog.TryGetEffectId(skillActionId, out effectId);

        // ── fable_71: API read-only de cooldown + uso por clique (convergem com as teclas 1-4) ──
        public float GetSlotCooldownRemaining(int slotIndex)
            => (slotIndex >= 0 && slotIndex < _slotCooldowns.Length) ? Mathf.Max(0f, _slotCooldowns[slotIndex]) : 0f;

        public float GetSlotCooldownTotal(int slotIndex)
            => (slotIndex >= 0 && slotIndex < _slotCooldownTotals.Length) ? Mathf.Max(0f, _slotCooldownTotals[slotIndex]) : 0f;

        // Ponto unico de uso de slot: a HUD (clique) e o Update (teclas 1-4) chamam isto.
        public void TryUseSlot(int slotIndex) => TryExecuteSlot(slotIndex);

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Re-wire InteractionSystem when scene changes
            WireInteractionSystem();
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

            var playerObject = GameBootstrap.Instance?.PlayerManager != null
                ? GameBootstrap.Instance.PlayerManager.gameObject
                : null;
            var interactionSystem = playerObject != null
                ? playerObject.GetComponentInChildren<InteractionSystem>()
                : null;
            if (interactionSystem != null)
            {
                _targetResolver.SetInteractionSystem(interactionSystem);
                Debug.Log("[ActiveSkillExecutionController] Wired InteractionSystem to SkillTargetResolver.");
            }
        }

        private void Update()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
                return;

            if (bootstrap.ModalManager != null && bootstrap.ModalManager.HasActiveModal)
                return;

            // Update cooldowns
            for (int i = 0; i < _slotCooldowns.Length; i++)
            {
                if (_slotCooldowns[i] > 0f)
                    _slotCooldowns[i] -= Time.deltaTime;
            }

            // Check numeric key input 1-4
            for (int slotIndex = 0; slotIndex < SlotInputKeys.Length; slotIndex++)
            {
                if (UnityInput.GetKeyDown(SlotInputKeys[slotIndex]))
                {
                    TryExecuteSlot(slotIndex);
                    break;
                }
            }
        }

        private void TryExecuteSlot(int slotIndex)
        {
            // Validate cooldown
            if (_slotCooldowns[slotIndex] > 0f)
            {
                float remaining = _slotCooldowns[slotIndex];
                PublishFeedback($"Slot {slotIndex + 1} em cooldown ({remaining:F1}s).");
                return;
            }

            // Resolve equipped skill action ID from SkillTreeManager state
            var skillTreeManager = GameBootstrap.Instance?.SkillTreeManager;
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
            var playerGo = GameBootstrap.Instance?.PlayerManager != null
                ? GameBootstrap.Instance.PlayerManager.gameObject
                : null;
            var context = new SkillEffectContext
            {
                SkillActionId = skillActionId,
                EffectId = effectId,
                ActiveSlotIndex = slotIndex,
                Caster = playerGo,
                WorldPosition = playerGo != null ? (Vector2)playerGo.transform.position : Vector2.zero,
                SceneName = SceneManager.GetActiveScene().name,
                Time = Time.time
            };

            // Resolve target
            context.Target = _targetResolver != null
                ? _targetResolver.Resolve(executor.TargetType, context.Caster)
                : null;

            // Execute
            var result = executor.Execute(context);

            if (result.Success)
            {
                // Apply cooldown on success — executors suggest their own balance cooldown;
                // fall back to a short default for executors that do not.
                _slotCooldowns[slotIndex] = result.CooldownSeconds > 0f ? result.CooldownSeconds : 1.5f;
                _slotCooldownTotals[slotIndex] = _slotCooldowns[slotIndex]; // fable_71: base do fill da HUD
                PublishFeedback(result.FeedbackMessage);
                Debug.Log($"[ActiveSkillExecutionController] Skill executed. Slot={slotIndex}, RawSlotValue={rawSlotValue}, ResolvedSkillActionId={skillActionId}, NodeId={nodeId}, EffectId={effectId}, Executor={executor.GetType().Name}", this);
            }
            else
            {
                PublishFeedback(result.FeedbackMessage);
                Debug.Log($"[ActiveSkillExecutionController] Skill failed. Slot={slotIndex}, RawSlotValue={rawSlotValue}, ResolvedSkillActionId={skillActionId}, NodeId={nodeId}, EffectId={effectId}, Executor={executor.GetType().Name}, Reason={result.FailureReason}, Feedback={result.FeedbackMessage}", this);
            }
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

        private void PublishFeedback(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(message));
            }
        }
    }
}
