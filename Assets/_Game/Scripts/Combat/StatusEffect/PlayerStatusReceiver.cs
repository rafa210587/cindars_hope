using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Combat.StatusEffect
{
    /// <summary>
    /// F01 — ponte entre ações inimigas/ambiente e o Player.StatusEffectManager, e aplicador
    /// da semântica canônica no player: Chill/Slow/Root (velocidade), Stun (bloqueia ação),
    /// DurabilityStress (multiplicador consumido por F03). Duração: DurationTurns = segundos.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerStatusReceiver : MonoBehaviour
    {
        private const string PlayerTargetId = "player";
        private const float MinSpeedFloor = 0.5f; // composto com fadiga (F16) — floor documentado

        private static PlayerStatusReceiver _instance;

        private Player.StatusEffectManager _playerStatusManager;
        private Player.PlayerController _playerController;
        private bool _speedPenaltyApplied;
        private float _appliedSpeedFactor = 1f;

        public static PlayerStatusReceiver Instance => _instance;

        /// <summary>Stun ativo bloqueia ataques/ações (consumido por F02/PlayerAttackController).</summary>
        public bool IsActionBlocked => HasStatusOfType(StatusEffectType.Stun);

        /// <summary>Multiplicador de desgaste de durabilidade (consumido por F03).</summary>
        public float DurabilityWearMultiplier
        {
            get
            {
                var effect = FindActiveOfType(StatusEffectType.DurabilityStress);
                return effect != null ? Mathf.Max(1f, effect.DurabilityWearMultiplier) : 1f;
            }
        }

        public void Configure(Player.StatusEffectManager statusManager, Player.PlayerController playerController)
        {
            _playerStatusManager = statusManager;
            _playerController = playerController;
        }

        /// <summary>Aplica status no player a partir de uma ação inimiga (respeita chance).</summary>
        public bool TryApplyFromEnemyAction(string statusEffectId, float applyChance)
        {
            if (string.IsNullOrWhiteSpace(statusEffectId) || _playerStatusManager == null)
            {
                return false;
            }

            if (applyChance < 1f && Random.value > Mathf.Clamp01(applyChance))
            {
                return false;
            }

            var database = GameBootstrap.Instance?.StatusEffectDatabase;
            float durationSeconds = 3f;
            if (database != null && database.TryGetById(statusEffectId, out var effect) && effect != null)
            {
                durationSeconds = Mathf.Clamp(effect.DurationTurns, 1, 30);
            }

            if (!_playerStatusManager.TryAddEffect(statusEffectId, durationSeconds))
            {
                return false;
            }

            GameEventBus.Publish(new StatusEffectAppliedEvent(PlayerTargetId, statusEffectId));
            return true;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            ApplyMovementSemantics();
        }

        // Velocidade: menor fator entre os status ativos; composição multiplicativa reversível.
        private void ApplyMovementSemantics()
        {
            if (_playerController == null)
            {
                return;
            }

            var factor = 1f;
            factor = Mathf.Min(factor, FactorOfType(StatusEffectType.Chill));
            factor = Mathf.Min(factor, FactorOfType(StatusEffectType.Slow));
            factor = Mathf.Min(factor, FactorOfType(StatusEffectType.ColdStress));
            if (HasStatusOfType(StatusEffectType.Root) || HasStatusOfType(StatusEffectType.Stun))
            {
                factor = 0f;
            }

            if (Mathf.Approximately(factor, _appliedSpeedFactor))
            {
                return;
            }

            // Reverte o fator anterior e aplica o novo (auto-expira quando status some).
            if (_speedPenaltyApplied && _appliedSpeedFactor > 0f)
            {
                _playerController.SpeedMultiplier = Mathf.Max(MinSpeedFloor, _playerController.SpeedMultiplier / _appliedSpeedFactor);
            }
            else if (_speedPenaltyApplied)
            {
                _playerController.SpeedMultiplier = 1f;
            }

            if (factor < 1f)
            {
                _playerController.SpeedMultiplier = factor <= 0f
                    ? 0f
                    : Mathf.Max(MinSpeedFloor * factor, _playerController.SpeedMultiplier * factor);
                _speedPenaltyApplied = true;
            }
            else
            {
                _speedPenaltyApplied = false;
            }

            _appliedSpeedFactor = factor;
        }

        private float FactorOfType(StatusEffectType type)
        {
            var effect = FindActiveOfType(type);
            return effect != null ? StatusEffectSemantics.GetMoveSpeedFactor(effect) : 1f;
        }

        private bool HasStatusOfType(StatusEffectType type)
        {
            return FindActiveOfType(type) != null;
        }

        private StatusEffectSO FindActiveOfType(StatusEffectType type)
        {
            if (_playerStatusManager == null)
            {
                return null;
            }

            var database = GameBootstrap.Instance?.StatusEffectDatabase;
            if (database == null)
            {
                return null;
            }

            foreach (var effectId in _playerStatusManager.ActiveEffects.Keys)
            {
                if (database.TryGetById(effectId, out var effect) && effect != null && effect.Type == type)
                {
                    return effect;
                }
            }

            return null;
        }
    }

    /// <summary>Garante o receiver junto ao player (padrão bootstrap do projeto).</summary>
    public static class PlayerStatusReceiverBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Object.FindAnyObjectByType<PlayerStatusReceiver>() != null)
            {
                return;
            }

            var bootstrap = GameBootstrap.Instance;
            var statusManager = bootstrap != null ? bootstrap.GetComponent<Player.StatusEffectManager>() : Object.FindAnyObjectByType<Player.StatusEffectManager>();
            var playerController = Object.FindAnyObjectByType<Player.PlayerController>();
            if (statusManager == null)
            {
                return; // sem manager na cena — nada a receber
            }

            var receiver = statusManager.gameObject.AddComponent<PlayerStatusReceiver>();
            receiver.Configure(statusManager, playerController);
            Debug.Log("[PlayerStatusReceiverBootstrap] PlayerStatusReceiver instanciado via bootstrap.");
        }
    }
}
