using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Skills;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInput = UnityEngine.Input;

namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11: Central controller that bridges active skill slots to effect execution.
    // Input: numeric keys 1-4 map to active slots 0-3.
    // Pipeline: input -> resolve skill action -> validate unlocked -> resolve effect id ->
    //           resolve target -> execute -> apply cooldown -> publish feedback.
    // Does NOT manage active slot state, skill tree, or cost deduction (deferred; see debt).
    //
    // TODO_INTEGRATION_NOT_FINAL: Stamina/mana cost deduction is not enforced per skill.
    // Final design requires cost lookup from SkillDefinition and deduction from StaminaManager.
    // Blocks final acceptance: NO (farm crop vertical slice is functional)
    [DisallowMultipleComponent]
    public sealed class ActiveSkillExecutionController : MonoBehaviour
    {
        // Numeric keys 1-4 map to active slot indices 0-3
        private static readonly KeyCode[] SlotInputKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
        private static ActiveSkillExecutionController _instance;

        [SerializeField] private SkillTargetResolver _targetResolver;

        // Skill action ID -> EffectId mapping.
        // TODO_INTEGRATION_NOT_FINAL: This is a static mapping for the first vertical slice.
        // Final design should read EffectId from SkillActionSO or SkillNodeDataSO.
        private static readonly Dictionary<string, string> SkillActionToEffectId = new Dictionary<string, string>
        {
            // Farm/Utility skill effects (vertical slice)
            { "skill_survival_emergency_roll", "farm.crop.water_skill" },  // debug: survival roll maps to farm water for demo
            { "skill_crafting_field_patch", "farm.crop.water_skill" },     // debug: field patch maps to farm water for demo

            // Placeholder mappings for all equippable skills — effect not yet implemented
            { "skill_melee_offhand_cut", "combat.melee.offhand_cut" },
            { "skill_melee_guarded_block", "combat.melee.block" },
            { "skill_melee_battle_dash", "combat.melee.battle_dash" },
            { "skill_melee_leap_attack", "combat.melee.leap_attack" },
            { "skill_melee_whirl_cut", "combat.melee.whirl_cut" },
            { "skill_ranged_charged_shot", "combat.ranged.charged_shot" },
            { "skill_ranged_line_piercer", "combat.ranged.line_piercer" },
            { "skill_ranged_multishot_fan", "combat.ranged.multishot_fan" },
            { "skill_ranged_bleeding_arrow", "combat.ranged.bleeding_arrow" },
            { "skill_ranged_marked_prey", "combat.ranged.marked_prey" },
            { "skill_magic_fire_spark", "combat.magic.fire_spark" },
            { "skill_magic_ice_bind", "combat.magic.ice_bind" },
            { "skill_magic_toxic_cloud", "combat.magic.toxic_cloud" },
            { "skill_magic_lightning_chain", "combat.magic.lightning_chain" },
            { "skill_magic_elemental_ward", "combat.magic.elemental_ward" },
            { "skill_magic_slowing_sigils", "combat.magic.slowing_sigils" },

            // ── Action Skill Balance Patch — WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH ──
            // DEFERRED_RUNTIME_EFFECT: mapeamentos registrados no catálogo; executores feedback-only.
            // TODO_INTEGRATION_NOT_FINAL: substituir por executores reais quando combat/utility runtime existir.

            // Melee: novas action skills
            { "skill_melee_avanco_aco", "melee.avanco_aco" },
            { "skill_melee_grito_desafio", "melee.grito_desafio" },
            { "skill_melee_investida_quebra_guarda", "melee.investida_quebra_guarda" },

            // Magic: novas action skills
            { "skill_magic_chama_breve", "magic.chama_breve" },
            { "skill_magic_rajada_gelida", "magic.rajada_gelida" },

            // Survival: novas action skills
            { "skill_survival_sinal_retirada", "survival.sinal_retirada" },
            { "skill_survival_isca_improvisada", "survival.isca_improvisada" },
            { "skill_survival_kit_emergencia", "survival.kit_emergencia" },
            { "skill_survival_instinto_sobrevivencia", "survival.instinto_sobrevivencia" },
            { "skill_survival_campo_seguro", "survival.campo_seguro" },

            // Crafting: novas action skills (crafting_quick_repair já mapeado acima via field_patch)
            { "skill_crafting_irrigador_portatil", "crafting.irrigador_portatil" },
            { "skill_crafting_bomba_improvisada", "crafting.bomba_improvisada" },
            { "skill_crafting_mecanismo_campo", "crafting.mecanismo_campo" },
            { "skill_crafting_marca_eficiencia", "crafting.marca_eficiencia" },
        };

        private readonly SkillEffectRegistry _registry = new SkillEffectRegistry();
        private readonly float[] _slotCooldowns = new float[4];
        private bool _bootstrapped;

        public static bool TryGetEffectIdForValidation(string skillActionId, out string effectId)
            => SkillActionToEffectId.TryGetValue(skillActionId, out effectId);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            if (_instance != null)
                return;

            var go = new GameObject("ActiveSkillExecutionController");
            DontDestroyOnLoad(go);
            go.AddComponent<ActiveSkillExecutionController>();
        }

        private void Bootstrap()
        {
            if (_bootstrapped)
                return;

            // Register farm crop executor as vertical slice
            _registry.Register(new FarmCropSkillEffectExecutor());

            // Register placeholder feedback executors for action skill balance patch.
            // TODO_INTEGRATION_NOT_FINAL: substituir por executores reais quando combat/utility runtime existir.
            RegisterFeedbackExecutors();

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

        private void RegisterFeedbackExecutors()
        {
            // Melee action skills (DEFERRED_RUNTIME_EFFECT — feedback only)
            _registry.Register(new FeedbackOnlySkillEffectExecutor("melee.avanco_aco", "Avanço de Aço ativado. (Efeito de combate pendente.)", SkillEffectCategory.Combat));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("melee.grito_desafio", "Grito de Desafio ativado. (Efeito de combate pendente.)", SkillEffectCategory.Combat));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("melee.investida_quebra_guarda", "Investida Quebra-Guarda ativada. (Efeito de combate pendente.)", SkillEffectCategory.Combat));

            // Magic action skills (DEFERRED_RUNTIME_EFFECT — feedback only)
            _registry.Register(new FeedbackOnlySkillEffectExecutor("magic.chama_breve", "Chama Breve lançada. (Efeito de combate pendente.)", SkillEffectCategory.Combat));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("magic.rajada_gelida", "Rajada Gélida lançada. (Efeito de combate pendente.)", SkillEffectCategory.Combat));

            // Survival action skills (DEFERRED_RUNTIME_EFFECT — feedback only)
            _registry.Register(new FeedbackOnlySkillEffectExecutor("survival.sinal_retirada", "Sinal de Retirada ativado. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("survival.isca_improvisada", "Isca Improvisada lançada. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("survival.kit_emergencia", "Kit de Emergência usado. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("survival.instinto_sobrevivencia", "Instinto de Sobrevivência ativado. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("survival.campo_seguro", "Campo Seguro criado. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));

            // Crafting action skills (DEFERRED_RUNTIME_EFFECT — feedback only)
            // crafting.irrigador_portatil usa farm.crop.water_skill como bridge real
            _registry.Register(new FeedbackOnlySkillEffectExecutor("crafting.irrigador_portatil", "Irrigador Portátil usado. (Efeito de farm pendente.)", SkillEffectCategory.Farm));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("crafting.bomba_improvisada", "Bomba Improvisada lançada. (Efeito de combate pendente.)", SkillEffectCategory.Combat));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("crafting.mecanismo_campo", "Mecanismo de Campo ativado. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("crafting.marca_eficiencia", "Marca de Eficiência aplicada. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

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
            if (!SkillActionToEffectId.TryGetValue(skillActionId, out var effectId))
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
                // Apply cooldown on success
                _slotCooldowns[slotIndex] = 1.5f; // TODO_INTEGRATION_NOT_FINAL: hardcoded 1.5s cooldown
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
