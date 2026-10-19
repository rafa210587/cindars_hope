using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Player.Movement
{
    /// <summary>
    /// fable_69 — regras PURAS (sem Unity) do sprint em combate. Testáveis em EditMode:
    /// fator de mobilidade resolvido, decisão de sprintar, drenagem por acumulador (mesmo
    /// padrão do block). Mantém a canonização (COMBAT_CORE: 3.4-3.8 em combate, 3.8-4.2 fora;
    /// sprint restaura a faixa de fora ao custo de 8 stamina/s).
    /// </summary>
    public static class SprintRules
    {
        /// <summary>Drenagem canônica F69: 8 stamina/s enquanto o sprint estiver ativo.</summary>
        public const float StaminaDrainPerSecond = 8f;

        /// <summary>
        /// Penalidade de combate (fator de mobilidade quando em combate e NÃO sprintando).
        /// 3.6/4.0 = 0.9 (mid das faixas canônicas 3.4-3.8 vs 3.8-4.2). Sprint neutraliza isto.
        /// </summary>
        public const float CombatReadyMultiplier = 0.9f;

        /// <summary>Fator neutro (sprint ativo restaura a velocidade plena de fora-de-combate).</summary>
        public const float FullMobilityMultiplier = 1f;

        /// <summary>
        /// O sprint deve estar ATIVO neste frame? Só em combate, com a tecla segurada, sem
        /// stun e sem modal aberto, e com stamina disponível. Fora de combate é no-op.
        /// </summary>
        public static bool ShouldSprint(bool inCombat, bool sprintHeld, bool stunned, bool modalOpen, bool hasStamina)
        {
            return inCombat && sprintHeld && hasStamina && !stunned && !modalOpen;
        }

        /// <summary>
        /// Fator de mobilidade a aplicar no composer dado o estado.
        /// Fora de combate: <see cref="FullMobilityMultiplier"/> (= 1, controller pode limpar o
        /// fator — no-op). Em combate sprintando: 1.0 (plena). Em combate sem sprint: penalidade.
        /// </summary>
        public static float ResolveMobilityFactor(bool inCombat, bool sprinting)
        {
            if (!inCombat)
            {
                return FullMobilityMultiplier;
            }

            return sprinting ? FullMobilityMultiplier : CombatReadyMultiplier;
        }

        /// <summary>
        /// Avança o acumulador de drenagem e devolve quantos pontos inteiros gastar neste frame
        /// (mesmo padrão de PlayerBlockController.DrainStamina). O resto fica no acumulador.
        /// </summary>
        public static int AdvanceDrain(ref float accumulator, float deltaTime,
            float costMultiplier = 1f)
        {
            accumulator += StaminaDrainPerSecond * Mathf.Max(0f, deltaTime)
                * Mathf.Max(0f, costMultiplier);
            var spend = Mathf.FloorToInt(accumulator);
            if (spend <= 0)
            {
                return 0;
            }

            accumulator -= spend;
            return spend;
        }
    }

    /// <summary>
    /// fable_69 — sprint segurável (Left Ctrl) que MANTÉM a velocidade de fora-de-combate
    /// DENTRO de combate, drenando 8 stamina/s (acumulador, mesmo padrão do block). Escreve
    /// SOMENTE o fator nomeado <see cref="SpeedFactorKind.CombatMobility"/> no
    /// <see cref="PlayerSpeedComposer"/> (F47) — nunca toca SpeedMultiplier direto.
    ///
    /// Estados: fora de combate → fator removido (no-op silencioso, velocidade já plena).
    /// Em combate sem sprint → fator de penalidade (~0.9, faixa 3.4-3.8). Em combate com sprint
    /// e stamina → fator 1.0 (faixa 3.8-4.2). Stun (F01) e modal cancelam o sprint; sem stamina
    /// o sprint cai sozinho mas a penalidade de combate permanece enquanto em combate.
    ///
    /// Mutuamente exclusivo com block na prática (block compõe seu próprio fator; soltar block
    /// não corrompe este fator — garantia do composer). Auto-registro estático sem global search.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerSprintController : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private StaminaManager _staminaManager;

        private static PlayerSprintController _activeInstance;

        private bool _isSprinting;
        private float _staminaDrainAccumulator;
        private float _directionalSprintCostMultiplier = 1f;

        public static PlayerSprintController ActiveInstance => _activeInstance;

        /// <summary>True enquanto o sprint está ativo (em combate, tecla segurada, com stamina).</summary>
        public bool IsSprinting => _isSprinting;

        private void Start()
        {
            if (_playerController == null)
            {
                _playerController = GetComponent<PlayerController>();
            }

            _activeInstance = this;

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null && _staminaManager == null)
            {
                _staminaManager = bootstrap.StaminaManager as StaminaManager;
            }
        }

        private void OnDestroy()
        {
            ClearMobilityFactor();
            if (_activeInstance == this)
            {
                _activeInstance = null;
            }
        }

        private void OnDisable()
        {
            StopSprint();
            ClearMobilityFactor();
        }

        private void Update()
        {
            ResolveDirectionalModifier();

            // arch: quebra do par mutuo Combat|Player (2026-07-16) — le via porta neutra em vez de
            // nomear CindarsHope.Combat.CombatStateTracker.
            var inCombat = CombatStateProvider.IsInCombat != null && CombatStateProvider.IsInCombat();

            // Fora de combate: sprint é no-op silencioso e o fator de mobilidade não contribui.
            if (!inCombat)
            {
                if (_isSprinting)
                {
                    StopSprint();
                }

                ClearMobilityFactor();
                return;
            }

            var modalOpen = GameBootstrap.Instance?.ModalManager?.HasActiveModal == true;
            // arch: quebra do par mutuo Combat|Player (2026-07-16) — le via porta neutra em vez de
            // nomear CindarsHope.Combat.StatusEffect.PlayerStatusReceiver.
            var stunned = ActionBlockProvider.IsActionBlocked != null && ActionBlockProvider.IsActionBlocked();
            var sprintHeld = PlayerMovementActionInput.IsSprintHeld();
            var hasStamina = _staminaManager == null || _staminaManager.CurrentStamina > 0;

            var shouldSprint = SprintRules.ShouldSprint(inCombat, sprintHeld, stunned, modalOpen, hasStamina);

            if (shouldSprint)
            {
                if (!_isSprinting)
                {
                    StartSprint();
                }
                else
                {
                    DrainStamina();
                }
            }
            else if (_isSprinting)
            {
                StopSprint();
            }

            // Em combate, o fator de mobilidade está sempre presente: 1.0 sprintando, penalidade caso contrário.
            ApplyMobilityFactor(SprintRules.ResolveMobilityFactor(inCombat, _isSprinting));
        }

        private void StartSprint()
        {
            _isSprinting = true;
            _staminaDrainAccumulator = 0f;
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Sprint."));
        }

        private void StopSprint()
        {
            if (!_isSprinting)
            {
                return;
            }

            _isSprinting = false;
            _staminaDrainAccumulator = 0f;
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Sprint encerrado."));
        }

        private void DrainStamina()
        {
            if (_staminaManager == null)
            {
                return; // STAMINA_SPRINT_DEBT (mesmo tratamento do block sem manager)
            }

            var spend = SprintRules.AdvanceDrain(ref _staminaDrainAccumulator, Time.deltaTime,
                _directionalSprintCostMultiplier);
            if (spend <= 0)
            {
                return;
            }

            if (!_staminaManager.TrySpendStamina(spend))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente para sprint."));
                StopSprint();
            }
        }

        private void ApplyMobilityFactor(float factor)
        {
            if (_playerController == null)
            {
                return;
            }

            if (factor >= 1f)
            {
                _playerController.SpeedComposer.ClearFactor(SpeedFactorKind.CombatMobility);
            }
            else
            {
                _playerController.SpeedComposer.SetFactor(SpeedFactorKind.CombatMobility, Mathf.Max(0.01f, factor));
            }
        }

        private void ClearMobilityFactor()
        {
            if (_playerController != null)
            {
                _playerController.SpeedComposer.ClearFactor(SpeedFactorKind.CombatMobility);
                _playerController.SpeedComposer.ClearFactor(SpeedFactorKind.RetreatSignal);
            }
        }

        private void ResolveDirectionalModifier()
        {
            if (_playerController == null)
            {
                _directionalSprintCostMultiplier = 1f;
                return;
            }

            var direction = _playerController.MoveInput;
            var modifier = DirectionalMobilityModifierProvider.Resolve(
                MobilityActionKind.SprintTick, direction.x, direction.y);
            _directionalSprintCostMultiplier = modifier.CostMultiplier;
            if (direction.sqrMagnitude > .0001f && modifier.SpeedMultiplier > 1f)
                _playerController.SpeedComposer.SetFactor(
                    SpeedFactorKind.RetreatSignal, modifier.SpeedMultiplier);
            else
                _playerController.SpeedComposer.ClearFactor(SpeedFactorKind.RetreatSignal);
        }
    }

    /// <summary>Garante o sprint controller junto ao player (padrão bootstrap, sem global search recorrente).</summary>
    public static class PlayerSprintControllerBootstrap
    {
        public static void Install(Transform owner)
        {
            if (PlayerSprintController.ActiveInstance != null)
            {
                return;
            }

            var player = PlayerController.ActiveInstance;
            if (player == null)
            {
                return; // sem player na cena — nada a fazer
            }

            if (player.GetComponent<PlayerSprintController>() == null)
            {
                player.gameObject.AddComponent<PlayerSprintController>();
                Debug.Log("[PlayerSprintControllerBootstrap] PlayerSprintController anexado ao player (fable_69).");
            }
        }
    }
}
