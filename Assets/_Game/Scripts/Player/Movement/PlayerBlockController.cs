using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player.Movement
{
    /// <summary>F27 — regras puras de timing do perfect block (testáveis).</summary>
    public static class BlockTimingRules
    {
        public const float PerfectWindowSeconds = 0.15f;
        public const float RearmCooldownSeconds = 0.4f;
        public const float NormalBlockMitigation = 0.5f;
        public const float PostureReflectFraction = 0.4f;

        /// <summary>Hit dentro da janela inicial do block (medida do INPUT).</summary>
        public static bool IsPerfect(float blockStartTime, float hitTime, bool windowArmed)
        {
            return windowArmed && hitTime - blockStartTime >= 0f && hitTime - blockStartTime <= PerfectWindowSeconds;
        }

        /// <summary>Anti-spam: re-block dentro de 0.4s do fim do anterior não rearma a janela.</summary>
        public static bool CanArmPerfectWindow(float lastBlockEndTime, float newStartTime)
        {
            return newStartTime - lastBlockEndTime >= RearmCooldownSeconds;
        }

        public static int MitigateNormalBlock(int rawDamage)
        {
            return rawDamage <= 0 ? 0 : UnityEngine.Mathf.Max(1, UnityEngine.Mathf.CeilToInt(rawDamage * NormalBlockMitigation));
        }
    }

    // Input: hold Left Shift. Does not occupy active skill slots.
    // Combat Core: block segurado 35%-55% da velocidade base, drain 18 Stamina/s.
    // F27: janela inicial de perfect block (0.15s) nega todo o dano e reflete postura.
    [DisallowMultipleComponent]
    public sealed class PlayerBlockController : MonoBehaviour
    {
        // BALANCE_FINAL_PENDING — mid-range per COMBAT_CORE_DIRECTION (35%-55% = 0.45 multiplier)
        [SerializeField] private float _blockSlowMultiplier = 0.45f;
        [SerializeField] private float _staminaDrainPerSecond = 18f;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private StaminaManager _staminaManager;

        private static PlayerBlockController _activeInstance;

        private bool _isBlocking;
        private float _staminaDrainAccumulator;
        // fable_66: trava de log one-shot do erro de wiring de stamina (evita spam por frame no drain).
        private bool _staminaWiringErrorLogged;
        // F27: estado da janela perfeita.
        private float _blockStartTime = -10f;
        private float _lastBlockEndTime = -10f;
        private bool _perfectWindowArmed;

        public static PlayerBlockController ActiveInstance => _activeInstance;

        public bool IsBlocking => _isBlocking;
        public float BlockSlowMultiplier => _blockSlowMultiplier;
        public float BlockStartTime => _blockStartTime;
        public bool PerfectWindowArmed => _perfectWindowArmed;

        /// <summary>F27: hit recebido enquanto bloqueando é perfeito? (consome a janela).</summary>
        public bool ResolveIncomingHitIsPerfect(float hitTime)
        {
            if (!_isBlocking)
            {
                return false;
            }

            var perfect = BlockTimingRules.IsPerfect(_blockStartTime, hitTime, _perfectWindowArmed);
            if (perfect)
            {
                _perfectWindowArmed = false; // 1 perfect por block
            }

            return perfect;
        }

        private void Start()
        {
            if (_playerController == null) _playerController = GetComponent<PlayerController>();
            _activeInstance = this;

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null && _staminaManager == null)
                _staminaManager = bootstrap.StaminaManager;

            // fable_66 (STAMINA_BLOCK_DEBT): wiring garantido aqui (serialized ref OU bootstrap).
            // Ausente = erro de wiring logado ALTO (rule unity-architecture #1: cena/objeto/campo),
            // nunca um guard silencioso que dreni nada.
            if (_staminaManager == null)
            {
                LogStaminaWiringError();
            }
        }

        private void LogStaminaWiringError()
        {
            if (_staminaWiringErrorLogged)
            {
                return;
            }

            _staminaWiringErrorLogged = true;
            Debug.LogError(
                $"[PlayerBlockController] WIRING ERROR: StaminaManager ausente. " +
                $"Scene='{gameObject.scene.name}', GameObject='{gameObject.name}', " +
                $"campo='_staminaManager'. Block não consegue drenar stamina. " +
                $"Atribua a ref no Inspector ou garanta GameBootstrap.StaminaManager no bootstrap.",
                this);
        }

        private void OnDestroy()
        {
            if (_activeInstance == this)
            {
                _activeInstance = null;
            }
        }

        private void Update()
        {
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
            {
                if (_isBlocking) StopBlock();
                return;
            }

            if (PlayerMovementActionInput.IsBlockHeld())
            {
                if (!_isBlocking) TryStartBlock();
                else DrainStamina();
            }
            else if (_isBlocking)
            {
                StopBlock();
            }
        }

        private void OnDisable()
        {
            if (_isBlocking) StopBlock();
        }

        private void TryStartBlock()
        {
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(1))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente para block."));
                return;
            }

            // fable_47: fator nomeado Block (×_blockSlowMultiplier). Compõe com Exhausted/Status
            // sem corromper os outros fatores ao soltar o block.
            if (_playerController != null)
                _playerController.SpeedComposer.SetFactor(SpeedFactorKind.Block, Mathf.Max(0.01f, _blockSlowMultiplier));

            _isBlocking = true;
            _staminaDrainAccumulator = 0f;
            // F27: janela perfeita armada apenas fora do cooldown anti-spam.
            _blockStartTime = Time.time;
            _perfectWindowArmed = BlockTimingRules.CanArmPerfectWindow(_lastBlockEndTime, _blockStartTime);
            Debug.Log($"[PlayerBlockController] Block started speedMultiplier={(_playerController != null ? _playerController.SpeedMultiplier : 0f):F2}, PerfectArmed={_perfectWindowArmed}");
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Block."));
        }

        private void DrainStamina()
        {
            if (_staminaManager == null)
            {
                // fable_66: sem stamina wired, o block não pode drenar. Em vez de no-op silencioso,
                // expõe o erro de wiring (one-shot) e mantém o block ativo sem custo (não trava o player).
                LogStaminaWiringError();
                return;
            }

            _staminaDrainAccumulator += _staminaDrainPerSecond * Time.deltaTime;
            var spend = Mathf.FloorToInt(_staminaDrainAccumulator);
            if (spend <= 0) return;

            _staminaDrainAccumulator -= spend;
            if (!_staminaManager.TrySpendStamina(spend))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente para block."));
                StopBlock();
            }
        }

        private void StopBlock()
        {
            if (_playerController != null)
                _playerController.SpeedComposer.ClearFactor(SpeedFactorKind.Block);

            _isBlocking = false;
            _staminaDrainAccumulator = 0f;
            _lastBlockEndTime = Time.time; // F27: âncora do cooldown anti-spam
            Debug.Log("[PlayerBlockController] Block stopped — fator Block removido do composer.");
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Block released."));
        }
    }
}
