using System;
using CindarsHope.Combat;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Cave.Traps
{
    /// <summary>
    /// fable_60 — armadilha materializada na caverna. Máquina de estados telegrafada (CA-2):
    /// Armed → (proximidade/pisada) → Telegraphing (telegraphSeconds) → Triggered (efeito). O efeito
    /// usa os caminhos EXISTENTES — dano via <see cref="PlayerDamageReceiver"/>, status via
    /// <see cref="PlayerStatusReceiver"/> (F01) — nunca um pipeline novo. Desarme via
    /// <see cref="IInteractable"/> com chance por tier de ferramenta (<see cref="TrapDisarmResolver"/>).
    /// Detecção do amuleto de Nyx (F23) revela o telegraph à distância.
    ///
    /// Sem busca global de cena (mesmo padrão do <c>CaveHazardTile</c>): o player é resolvido pelo
    /// trigger; o <c>PlayerManager</c> pelo <c>GameBootstrap</c> ou pelo collider. Determinismo de
    /// posição/tipo/quantidade é do <see cref="CaveTrapPlanner"/>; o desfecho do desarme é
    /// determinístico por seed da tentativa. O estado final é persistido pelo callback de snapshot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrapBehaviour : MonoBehaviour, IInteractable
    {
        private const int FallbackStatusDamage = 4; // dano simbólico quando o status F01 não aplica

        private TrapDefinition _definition;
        private string _trapInstanceId = string.Empty;
        private int _caveLevel;
        private int _band = 1;
        private string _caveRunSeed = string.Empty;
        private TrapState _state = TrapState.Armed;
        private bool _detected;
        private float _telegraphEndsAt = -1f;

        private SpriteRenderer _spriteRenderer;
        private PlayerManager _playerManager;
        private Action<string, TrapState> _onStateChanged;

        public string TrapInstanceId => _trapInstanceId;
        public TrapState State => _state;
        public TrapId TrapId => _definition != null ? _definition.Id : TrapId.None;
        public bool IsDetected => _detected;

        public string InteractionPrompt
        {
            get
            {
                if (_state == TrapState.Disarmed) return "Armadilha desarmada";
                if (_definition == null || !_definition.Disarmable) return string.Empty;
                return "Desarmar armadilha";
            }
        }

        public void Configure(
            CaveTrapPlacement placement,
            string caveRunSeed,
            int caveLevel,
            TrapState initialState,
            SpriteRenderer spriteRenderer,
            Action<string, TrapState> onStateChanged)
        {
            _definition = placement != null ? TrapDefinition.Get(placement.TrapId) : null;
            _trapInstanceId = placement != null ? placement.TrapInstanceId : string.Empty;
            _band = placement != null ? Mathf.Clamp(placement.Band, 1, 7) : 1;
            _caveRunSeed = caveRunSeed ?? string.Empty;
            _caveLevel = caveLevel;
            _state = initialState;
            _detected = false;
            _spriteRenderer = spriteRenderer != null ? spriteRenderer : GetComponent<SpriteRenderer>();
            _onStateChanged = onStateChanged;
            ApplyVisual();
        }

        /// <summary>
        /// fable_60 — o efeito de detecção da F23 revelou esta armadilha (CA-6). Idempotente; só faz
        /// sentido enquanto armada. Publica <c>TrapDetectedEvent</c> uma vez e troca o visual para o
        /// aviso. Sem o efeito (raio 0), nunca é chamada — a flag permanece inerte.
        /// </summary>
        public void RevealByDetection()
        {
            if (_state != TrapState.Armed || _detected)
            {
                return;
            }

            _detected = true;
            ApplyVisual();
            GameEventBus.Publish(new TrapDetectedEvent(_trapInstanceId, TrapKey(), transform.position, _caveLevel));
        }

        public bool CanInteract(GameObject interactor)
        {
            return _definition != null && _definition.Disarmable && _state == TrapState.Armed;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            var toolTier = ResolveToolTier();
            var success = TrapDisarmResolver.TryDisarm(toolTier, _caveRunSeed, _caveLevel, _trapInstanceId);
            if (success)
            {
                Disarm();
            }
            else
            {
                // Falha = ativação imediata (risco/recompensa §22).
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Desarme falhou!"));
                BeginTelegraph();
            }
        }

        private void Disarm()
        {
            _state = TrapState.Disarmed;
            _telegraphEndsAt = -1f;
            ApplyVisual();
            GameEventBus.Publish(new TrapDisarmedEvent(_trapInstanceId, TrapKey(), transform.position, _caveLevel));
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Armadilha desarmada."));
            _onStateChanged?.Invoke(_trapInstanceId, TrapState.Disarmed);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryArmTelegraph(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            TryArmTelegraph(collision);
        }

        private void TryArmTelegraph(Collider2D collision)
        {
            if (_state != TrapState.Armed)
            {
                return;
            }

            var playerController = collision.GetComponentInParent<PlayerController>();
            if (playerController == null)
            {
                playerController = collision.GetComponent<PlayerController>();
            }

            if (playerController == null)
            {
                return;
            }

            ResolvePlayerManager(collision);
            BeginTelegraph();
        }

        private void BeginTelegraph()
        {
            if (_state == TrapState.Triggered || _state == TrapState.Disarmed)
            {
                return;
            }

            if (_state == TrapState.Telegraphing)
            {
                return; // já contando o windup
            }

            _state = TrapState.Telegraphing;
            var seconds = _definition != null ? Mathf.Max(0.05f, _definition.TelegraphSeconds) : 0.5f;
            _telegraphEndsAt = Time.time + seconds;
            ApplyVisual();
        }

        private void Update()
        {
            if (_state == TrapState.Telegraphing && _telegraphEndsAt > 0f && Time.time >= _telegraphEndsAt)
            {
                Trigger();
            }
        }

        private void Trigger()
        {
            if (_state == TrapState.Triggered || _state == TrapState.Disarmed)
            {
                return;
            }

            _state = TrapState.Triggered;
            _telegraphEndsAt = -1f;

            if (_definition != null)
            {
                ApplyEffect();
            }

            ApplyVisual();
            GameEventBus.Publish(new TrapTriggeredEvent(_trapInstanceId, TrapKey(), transform.position, _caveLevel));
            _onStateChanged?.Invoke(_trapInstanceId, TrapState.Triggered);
        }

        private void ApplyEffect()
        {
            switch (_definition.Category)
            {
                case TrapEffectCategory.DirectDamage:
                    ApplyDamage();
                    break;
                case TrapEffectCategory.Status:
                    ApplyStatus(primaryOnly: false, allowFallbackDamage: true);
                    break;
                case TrapEffectCategory.DamageAndStatus:
                    ApplyDamage();
                    ApplyStatus(primaryOnly: false, allowFallbackDamage: false);
                    break;
                case TrapEffectCategory.SpawnEnemy:
                    // Baú falso materializa como FalseChestTrap, não como TrapBehaviour — sem efeito aqui.
                    break;
            }
        }

        private void ApplyDamage()
        {
            if (_playerManager == null)
            {
                return;
            }

            var amount = _definition.ResolveDamage(_band);
            if (amount <= 0)
            {
                return;
            }

            var dealt = PlayerDamageReceiver.ApplyDamage(_playerManager, amount, _trapInstanceId, ResolveDamageType());
            GameEventBus.Publish(new PlayerDamagedEvent(dealt, transform.position, _trapInstanceId, _definition.DisplayName));
            GameEventBus.Publish(new PlayerActionFeedbackEvent(_definition.DisplayName + "!"));
        }

        private void ApplyStatus(bool primaryOnly, bool allowFallbackDamage)
        {
            var receiver = PlayerStatusReceiver.Instance;
            var appliedAny = false;

            if (receiver != null)
            {
                if (!string.IsNullOrEmpty(_definition.StatusId))
                {
                    appliedAny |= receiver.TryApplyFromEnemyAction(_definition.StatusId, 1f);
                }

                if (!primaryOnly && !string.IsNullOrEmpty(_definition.SecondaryStatusId))
                {
                    appliedAny |= receiver.TryApplyFromEnemyAction(_definition.SecondaryStatusId, 1f);
                }
            }

            if (appliedAny)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(_definition.DisplayName + "!"));
                return;
            }

            // Fallback sem F01 (receiver/database ausente): dano simbólico para a armadilha não ser inerte.
            if (allowFallbackDamage && _playerManager != null)
            {
                var dealt = PlayerDamageReceiver.ApplyDamage(_playerManager, FallbackStatusDamage, _trapInstanceId, ResolveDamageType());
                GameEventBus.Publish(new PlayerDamagedEvent(dealt, transform.position, _trapInstanceId, _definition.DisplayName));
                GameEventBus.Publish(new PlayerActionFeedbackEvent(_definition.DisplayName + "!"));
            }
        }

        private ToolTier ResolveToolTier()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null && bootstrap.EquipmentManager != null)
            {
                var tier = bootstrap.EquipmentManager.EquippedToolTier;
                return tier > ToolTier.None ? tier : ToolTier.Basic;
            }

            return ToolTier.Basic; // mãos/ferramenta básica sempre disponível
        }

        private DamageType ResolveDamageType()
        {
            if (_definition == null)
            {
                return DamageType.Physical;
            }

            return _definition.Id switch
            {
                TrapId.EmberVent => DamageType.Fire,
                TrapId.FrostBurstRune => DamageType.Ice,
                TrapId.SporePod => DamageType.Toxic,
                _ => DamageType.Physical
            };
        }

        private void ResolvePlayerManager(Collider2D collision)
        {
            if (_playerManager != null)
            {
                return;
            }

            if (GameBootstrap.Instance != null && GameBootstrap.Instance.PlayerManager != null)
            {
                _playerManager = GameBootstrap.Instance.PlayerManager;
                return;
            }

            _playerManager = collision.GetComponentInParent<PlayerManager>();
            if (_playerManager == null)
            {
                _playerManager = collision.GetComponent<PlayerManager>();
            }
        }

        private string TrapKey()
        {
            return _definition != null ? _definition.TrapKey : TrapId.ToString().ToLowerInvariant();
        }

        /// <summary>Cor placeholder por estado/tipo (telegraph de cena — CA-2). Substituível por arte.</summary>
        public static Color ResolveTelegraphColor(TrapState state, bool detected, TrapId trapId)
        {
            switch (state)
            {
                case TrapState.Disarmed:
                    return new Color(0.4f, 0.45f, 0.4f, 0.45f);   // neutralizada
                case TrapState.Triggered:
                    return new Color(0.5f, 0.2f, 0.2f, 0.5f);     // já disparada
                case TrapState.Telegraphing:
                    return new Color(1f, 0.35f, 0.1f, 0.9f);      // aviso ativo (windup)
                default:
                    // Armed: oculta a menos que detectada (F23) — então mostra aviso suave.
                    return detected
                        ? new Color(1f, 0.85f, 0.25f, 0.7f)
                        : new Color(0.5f, 0.45f, 0.4f, 0.15f);
            }
        }

        private void ApplyVisual()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            _spriteRenderer.color = ResolveTelegraphColor(_state, _detected, TrapId);
        }
    }
}
