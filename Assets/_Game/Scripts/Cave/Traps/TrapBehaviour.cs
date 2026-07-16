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
using CindarsHope.Foundation;

namespace CindarsHope.Cave.Traps
{
    /// <summary>
    /// fable_60 â€” armadilha materializada na caverna. MÃ¡quina de estados telegrafada (CA-2):
    /// Armed â†’ (proximidade/pisada) â†’ Telegraphing (telegraphSeconds) â†’ Triggered (efeito). O efeito
    /// usa os caminhos EXISTENTES â€” dano via <see cref="PlayerDamageReceiver"/>, status via
    /// <see cref="PlayerStatusReceiver"/> (F01) â€” nunca um pipeline novo. Desarme via
    /// <see cref="IInteractable"/> com chance por tier de ferramenta (<see cref="TrapDisarmResolver"/>).
    /// DetecÃ§Ã£o do amuleto de Nyx (F23) revela o telegraph Ã  distÃ¢ncia.
    ///
    /// Sem busca global de cena (mesmo padrÃ£o do <c>CaveHazardTile</c>): o player Ã© resolvido pelo
    /// trigger; o <c>PlayerManager</c> pelo <c>GameBootstrap</c> ou pelo collider. Determinismo de
    /// posiÃ§Ã£o/tipo/quantidade Ã© do <see cref="CaveTrapPlanner"/>; o desfecho do desarme Ã©
    /// determinÃ­stico por seed da tentativa. O estado final Ã© persistido pelo callback de snapshot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrapBehaviour : MonoBehaviour, IInteractable
    {
        private const int FallbackStatusDamage = 4; // dano simbÃ³lico quando o status F01 nÃ£o aplica

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
        // spec_cave_biome_art_profiles_runtime (CV01): sprite opcional do bioma; null = placeholder
        // atual (retÃ¢ngulo colorido). Quando presente, o tint de estado continua aplicado por cima
        // (telegraph precisa continuar legÃ­vel â€” CAVE_BIOME_VISUAL_REFERENCE Â§2).
        private Sprite _biomeSprite;

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
            Action<string, TrapState> onStateChanged,
            Sprite biomeSprite = null)
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
            _biomeSprite = biomeSprite;
            if (_biomeSprite != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = _biomeSprite;
            }
            ApplyVisual();
        }

        /// <summary>
        /// fable_60 â€” o efeito de detecÃ§Ã£o da F23 revelou esta armadilha (CA-6). Idempotente; sÃ³ faz
        /// sentido enquanto armada. Publica <c>TrapDetectedEvent</c> uma vez e troca o visual para o
        /// aviso. Sem o efeito (raio 0), nunca Ã© chamada â€” a flag permanece inerte.
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
                // Falha = ativaÃ§Ã£o imediata (risco/recompensa Â§22).
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
                return; // jÃ¡ contando o windup
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
                    // BaÃº falso materializa como FalseChestTrap, nÃ£o como TrapBehaviour â€” sem efeito aqui.
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

            // Fallback sem F01 (receiver/database ausente): dano simbÃ³lico para a armadilha nÃ£o ser inerte.
            if (allowFallbackDamage && _playerManager != null)
            {
                var dealt = PlayerDamageReceiver.ApplyDamage(_playerManager, FallbackStatusDamage, _trapInstanceId, ResolveDamageType());
                GameEventBus.Publish(new PlayerDamagedEvent(dealt, transform.position, _trapInstanceId, _definition.DisplayName));
                GameEventBus.Publish(new PlayerActionFeedbackEvent(_definition.DisplayName + "!"));
            }
        }

        private ToolTier ResolveToolTier()
        {
            // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) â€” EquipmentManager
            // resolvido via EquipmentManager.Instance (self-registro, molde Craft/Economy/Skills).
            var equipmentManager = CindarsHope.Equipment.EquipmentManager.Instance;
            if (equipmentManager != null)
            {
                var tier = equipmentManager.EquippedToolTier;
                return tier > ToolTier.None ? tier : ToolTier.Basic;
            }

            return ToolTier.Basic; // mÃ£os/ferramenta bÃ¡sica sempre disponÃ­vel
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

            // arch: quebra do par mutuo Core|Player (2026-07-15) â€” bootstrap.PlayerManager agora
            // retorna MonoBehaviour; cast local para o tipo concreto.
            if (GameBootstrap.Instance != null && GameBootstrap.Instance.PlayerManager as PlayerManager != null)
            {
                _playerManager = GameBootstrap.Instance.PlayerManager as PlayerManager;
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

        /// <summary>Cor placeholder por estado/tipo (telegraph de cena â€” CA-2). SubstituÃ­vel por arte.</summary>
        public static Color ResolveTelegraphColor(TrapState state, bool detected, TrapId trapId)
        {
            switch (state)
            {
                case TrapState.Disarmed:
                    return new Color(0.4f, 0.45f, 0.4f, 0.45f);   // neutralizada
                case TrapState.Triggered:
                    return new Color(0.5f, 0.2f, 0.2f, 0.5f);     // jÃ¡ disparada
                case TrapState.Telegraphing:
                    return new Color(1f, 0.35f, 0.1f, 0.9f);      // aviso ativo (windup)
                default:
                    // Armed: oculta a menos que detectada (F23) â€” entÃ£o mostra aviso suave.
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

            // spec_cave_biome_art_profiles_runtime (CV01): com sprite do bioma, o estado padrÃ£o
            // (Armed, nÃ£o detectado) fica OPACO â€” a prÃ³pria arte jÃ¡ "camufla" a armadilha no cenÃ¡rio
            // (ex.: espinhos discretos no chÃ£o), sem precisar da alpha baixa do placeholder. Os
            // demais estados (Telegraphing/Triggered/Disarmed/detectado) continuam aplicando o tint
            // de aviso por cima â€” o telegraph (CA-2) precisa continuar legÃ­vel mesmo com arte real.
            if (_biomeSprite != null && _state == TrapState.Armed && !_detected)
            {
                _spriteRenderer.color = Color.white;
                return;
            }

            _spriteRenderer.color = ResolveTelegraphColor(_state, _detected, TrapId);
        }
    }
}
