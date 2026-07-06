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

        private readonly SkillEffectRegistry _registry = new SkillEffectRegistry();
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

            // Register farm crop executor as vertical slice
            _registry.Register(new FarmCropSkillEffectExecutor());

            // Real combat/utility executors (melee strikes, projectiles, self restores).
            RegisterCombatExecutors();

            // Remaining feedback-only placeholders for effects whose target system
            // (marking, wards, traps, efficiency buffs) does not exist yet.
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

        // Real gameplay executors. Damage/cost/cooldown values follow the balance addendum
        // (WAVE_INTEGRATION_11_ACTION_SKILL_BALANCE_PATCH) tiers: quick hits ~2-3s CD,
        // heavy hits 6-9s CD, restores 30-60s CD.
        private void RegisterCombatExecutors()
        {
            // ── Melee strikes ──
            _registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.offhand_cut", "Corte com a Mao Inversa", baseDamage: 8, range: 1.2f, arcDegrees: 140f, staminaCost: 10, cooldownSeconds: 2.5f));
            _registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.whirl_cut", "Corte Giratorio", baseDamage: 10, range: 1.7f, arcDegrees: 360f, staminaCost: 22, cooldownSeconds: 6f));
            _registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.leap_attack", "Ataque Saltante", baseDamage: 14, range: 1.4f, arcDegrees: 120f, staminaCost: 25, lungeDistance: 2.2f, cooldownSeconds: 7f));
            _registry.Register(new MeleeStrikeSkillEffectExecutor("combat.melee.battle_dash", "Avanco de Batalha", baseDamage: 8, range: 1.2f, arcDegrees: 100f, staminaCost: 20, lungeDistance: 3f, cooldownSeconds: 5f));
            _registry.Register(new MeleeStrikeSkillEffectExecutor("melee.avanco_aco", "Avanco de Aco", baseDamage: 12, range: 1.3f, arcDegrees: 110f, staminaCost: 22, lungeDistance: 2.5f, cooldownSeconds: 6f));
            _registry.Register(new MeleeStrikeSkillEffectExecutor("melee.grito_desafio", "Grito de Desafio", baseDamage: 6, range: 2.2f, arcDegrees: 360f, staminaCost: 18, knockbackForce: 6f, cooldownSeconds: 8f));
            _registry.Register(new MeleeStrikeSkillEffectExecutor("melee.investida_quebra_guarda", "Investida Quebra-Guarda", baseDamage: 16, range: 1.3f, arcDegrees: 90f, staminaCost: 26, lungeDistance: 2f, knockbackForce: 3f, cooldownSeconds: 9f, postureDamageMultiplier: 3f));

            // ── Ranged projectiles (physical → stamina) ──
            _registry.Register(new ProjectileSkillEffectExecutor("combat.ranged.charged_shot", "Tiro Carregado", baseDamage: 20, speed: 12f, range: 9f, damageType: CindarsHope.Combat.DamageType.Physical, resourceCost: 20, cooldownSeconds: 6f));
            _registry.Register(new ProjectileSkillEffectExecutor("combat.ranged.line_piercer", "Perfurador em Linha", baseDamage: 12, speed: 14f, range: 10f, damageType: CindarsHope.Combat.DamageType.Physical, resourceCost: 18, maxHitsPerProjectile: 5, cooldownSeconds: 7f));
            _registry.Register(new ProjectileSkillEffectExecutor("combat.ranged.multishot_fan", "Leque de Flechas", baseDamage: 8, speed: 11f, range: 7f, damageType: CindarsHope.Combat.DamageType.Physical, resourceCost: 24, projectileCount: 3, spreadDegrees: 28f, cooldownSeconds: 8f));
            _registry.Register(new ProjectileSkillEffectExecutor("combat.ranged.bleeding_arrow", "Flecha Lacerante", baseDamage: 14, speed: 12f, range: 8f, damageType: CindarsHope.Combat.DamageType.Physical, resourceCost: 16, cooldownSeconds: 6f, statusEffectId: "status_bleed"));

            // ── Magic projectiles (elemental → mana) ──
            _registry.Register(new ProjectileSkillEffectExecutor("combat.magic.fire_spark", "Faisca de Fogo", baseDamage: 12, speed: 10f, range: 7f, damageType: CindarsHope.Combat.DamageType.Fire, resourceCost: 10, cooldownSeconds: 3f));
            _registry.Register(new ProjectileSkillEffectExecutor("combat.magic.ice_bind", "Prisao de Gelo", baseDamage: 10, speed: 9f, range: 7f, damageType: CindarsHope.Combat.DamageType.Ice, resourceCost: 14, cooldownSeconds: 6f, statusEffectId: "status_chill"));
            _registry.Register(new ProjectileSkillEffectExecutor("combat.magic.toxic_cloud", "Nuvem Toxica", baseDamage: 8, speed: 7f, range: 6f, damageType: CindarsHope.Combat.DamageType.Toxic, resourceCost: 18, projectileCount: 3, spreadDegrees: 40f, cooldownSeconds: 8f, statusEffectId: "status_poison"));
            _registry.Register(new ProjectileSkillEffectExecutor("combat.magic.lightning_chain", "Corrente Eletrica", baseDamage: 12, speed: 16f, range: 9f, damageType: CindarsHope.Combat.DamageType.Lightning, resourceCost: 20, maxHitsPerProjectile: 4, cooldownSeconds: 8f));
            _registry.Register(new ProjectileSkillEffectExecutor("magic.chama_breve", "Chama Breve", baseDamage: 8, speed: 10f, range: 6f, damageType: CindarsHope.Combat.DamageType.Fire, resourceCost: 8, cooldownSeconds: 2.5f));
            _registry.Register(new ProjectileSkillEffectExecutor("magic.rajada_gelida", "Rajada Gelida", baseDamage: 6, speed: 9f, range: 6f, damageType: CindarsHope.Combat.DamageType.Ice, resourceCost: 16, projectileCount: 3, spreadDegrees: 30f, cooldownSeconds: 6f, statusEffectId: "status_chill"));

            // ── Crafting offensive gadget ──
            _registry.Register(new ProjectileSkillEffectExecutor("crafting.bomba_improvisada", "Bomba Improvisada", baseDamage: 18, speed: 8f, range: 5f, damageType: CindarsHope.Combat.DamageType.Toxic, resourceCost: 20, maxHitsPerProjectile: 3, cooldownSeconds: 12f));

            // ── Magic area control (fable_70) ── zona de slow reusando status_slow existente.
            _registry.Register(new SlowFieldSkillEffectExecutor("combat.magic.slowing_sigils", "Sigilos Lentificantes", statusEffectId: "status_slow", radius: 2.5f, manaCost: 18, cooldownSeconds: 8f));

            // ── Survival self-restores ──
            _registry.Register(new SelfRestoreSkillEffectExecutor("survival.kit_emergencia", "Kit de Emergencia", restoreHp: 30, restoreStamina: 0, restoreMana: 0, cooldownSeconds: 45f));
            _registry.Register(new SelfRestoreSkillEffectExecutor("survival.instinto_sobrevivencia", "Instinto de Sobrevivencia", restoreHp: 0, restoreStamina: 50, restoreMana: 0, cooldownSeconds: 30f));
            _registry.Register(new SelfRestoreSkillEffectExecutor("survival.campo_seguro", "Campo Seguro", restoreHp: 15, restoreStamina: 25, restoreMana: 15, cooldownSeconds: 60f));
            // fable_70: Ultimo Folego — panic heal de CD alto (escudo temporario deferido; sem sistema de shield ainda).
            _registry.Register(new SelfRestoreSkillEffectExecutor("survival.last_breath", "Ultimo Folego", restoreHp: 40, restoreStamina: 0, restoreMana: 0, cooldownSeconds: 90f));
        }

        private void RegisterFeedbackExecutors()
        {
            // DEFERRED_RUNTIME_EFFECT: effects below need systems that do not exist yet
            // (target marking, wards, aggro reduction, lure, crafting speed buff, field repair).
            // TODO_INTEGRATION_NOT_FINAL: substituir quando o sistema alvo existir.
            // Ledger de debito canonico: docs/validation/WAVE_INTEGRATION_11_SKILL_EFFECT_CATALOG.md
            // (secao "Skill Effect Debt Ledger — spec_codex_05").
            // fable_70: combat.melee.block removido (guarded_block cortado; Block e ability Shift).
            _registry.Register(new FeedbackOnlySkillEffectExecutor("combat.ranged.marked_prey", "Presa Marcada. (Sistema de marcacao pendente.)", SkillEffectCategory.Combat));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("combat.magic.elemental_ward", "Barreira Elemental ativada. (Sistema de ward pendente.)", SkillEffectCategory.Combat));
            // fable_70: combat.magic.slowing_sigils agora tem executor real (SlowFieldSkillEffectExecutor).
            _registry.Register(new FeedbackOnlySkillEffectExecutor("survival.sinal_retirada", "Sinal de Retirada ativado. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
            _registry.Register(new FeedbackOnlySkillEffectExecutor("survival.isca_improvisada", "Isca Improvisada lançada. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
            // spec_codex_05: crafting.irrigador_portatil agora tem executor REAL (farm.crop.water_skill,
            // registrado em Bootstrap()) — removido daqui. crafting.field_patch assume o slot
            // feedback-only com mensagem honesta de reparo (nunca mais reusa o efeito de water).
            _registry.Register(new FeedbackOnlySkillEffectExecutor("crafting.field_patch", "Reparo de Campo aplicado. (Efeito de reparo pendente.)", SkillEffectCategory.Utility));
            // fable_70: crafting.mecanismo_campo removido (cortado).
            _registry.Register(new FeedbackOnlySkillEffectExecutor("crafting.marca_eficiencia", "Marca de Eficiência aplicada. (Efeito de utilidade pendente.)", SkillEffectCategory.Utility));
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
