using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player.Movement;
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

        private static PlayerStatusReceiver _instance;

        private Player.StatusEffectManager _playerStatusManager;
        private Player.PlayerController _playerController;

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
                // F01: clamp canônico 1–30s da duração base.
                var baseSeconds = Mathf.Clamp(effect.DurationTurns, 1, 30);
                // fable_47 (follow-up 3): resistência do eixo correto encurta a duração
                // (duração × (1 − min(0.5, resist × 0.02))), preservando o clamp 1–30s.
                var resistance = ResistanceForStatus(effect.Type);
                durationSeconds = CindarsHope.Player.DerivedFollowupFormulas.ApplyStatusDurationReduction(baseSeconds, resistance, 1f, 30f);
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
            // arch: quebra do par mutuo Combat|Player (2026-07-16) — registra a porta neutra
            // Foundation.ActionBlockProvider assim que o receiver existe (Install() roda no Start()
            // da composition root, antes de qualquer Update()). PlayerSprintController le a porta
            // em vez de nomear CindarsHope.Combat.StatusEffect.PlayerStatusReceiver diretamente.
            ActionBlockProvider.IsActionBlocked = () => Instance != null && Instance.IsActionBlocked;

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

        // fable_47: velocidade via fator nomeado Status no composer (menor fator entre os status
        // ativos). Root/Stun zeram; slow não-letal é pisado em MinSpeedFloor pelo próprio composer.
        // Sem snapshot/restore: limpar o fator não corrompe block/exhausted simultâneos.
        private void ApplyMovementSemantics()
        {
            if (_playerController == null)
            {
                return;
            }

            var factor = ComputeStatusSpeedFactor();
            if (factor >= 1f)
            {
                _playerController.SpeedComposer.ClearFactor(SpeedFactorKind.Status);
            }
            else
            {
                _playerController.SpeedComposer.SetFactor(SpeedFactorKind.Status, factor);
            }
        }

        /// <summary>Menor fator de velocidade entre os status ativos (0 se Root/Stun).</summary>
        private float ComputeStatusSpeedFactor()
        {
            if (HasStatusOfType(StatusEffectType.Root) || HasStatusOfType(StatusEffectType.Stun))
            {
                return 0f;
            }

            var factor = 1f;
            factor = Mathf.Min(factor, FactorOfType(StatusEffectType.Chill));
            factor = Mathf.Min(factor, FactorOfType(StatusEffectType.Slow));
            factor = Mathf.Min(factor, FactorOfType(StatusEffectType.ColdStress));
            return factor;
        }

        private float FactorOfType(StatusEffectType type)
        {
            var effect = FindActiveOfType(type);
            return effect != null ? StatusEffectSemantics.GetMoveSpeedFactor(effect) : 1f;
        }

        /// <summary>
        /// Eixo de resistência (F18) para encurtar a duração do status (fable_47 follow-up 3).
        /// Reutiliza o mapeamento canônico de DoT (StatusEffectSemantics.GetDamageType) e adiciona
        /// Chill→Ice (Chill não é DoT, mas é gelo para fins de resistência, conforme a spec).
        /// </summary>
        public static DamageType ResistanceAxisFor(StatusEffectType type)
        {
            if (type == StatusEffectType.Chill)
            {
                return DamageType.Ice;
            }

            return StatusEffectSemantics.GetDamageType(type);
        }

        /// <summary>Resistência atual do player no eixo do status, via fonte única F18.</summary>
        private static int ResistanceForStatus(StatusEffectType type)
        {
            var source = ResistanceProvider.Source;
            if (source == null)
            {
                return 0;
            }

            return Mathf.Max(0, source(ResistanceAxisFor(type)));
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
        public static void Install(Transform owner)
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
