using CindarsHope.Cave.Generation;
using CindarsHope.Combat;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player;
using CindarsHope.Player.Movement;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// fable_09 â€” hazard de tile materializado na caverna. Telegrafado por cor prÃ³pria por tipo
    /// (CA-2). Detecta o player por trigger (mesmo padrÃ£o do EnemyContactDamage â€” sem busca global):
    ///
    /// - ToxicPool: aplica Poison (F01) ao pisar via PlayerStatusReceiver; fallback = dano direto
    ///   pequeno se nÃ£o houver receiver/status na cena;
    /// - IceSlick: reduz o controle do player (fator de velocidade) por ~1.5s ao pisar;
    /// - FallingRock: dano Ãºnico TELEGRAFADO ao entrar no tile (uma vez por entrada).
    ///
    /// Determinismo Ã© responsabilidade do CaveHazardPlanner (posiÃ§Ã£o/tipo). O componente sÃ³ executa.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CaveHazardTile : MonoBehaviour
    {
        private const string ToxicStatusId = "status_poison";
        private const float ToxicApplyChance = 1f;
        private const int ToxicFallbackDamage = 4;
        private const int FallingRockDamage = 8;
        private const float IceSlickFactor = 0.45f;
        private const float IceSlickDurationSeconds = 1.5f;
        private const float ReentryCooldownSeconds = 1.0f;

        private CaveHazardKind _kind;
        private string _hazardId = string.Empty;
        private SpriteRenderer _spriteRenderer;

        private PlayerManager _playerManager;
        private PlayerController _activePlayerController;
        private float _iceSlickExpireTime = -1f;
        private float _lastTriggerTime = -999f;

        public string HazardId => _hazardId;
        public CaveHazardKind Kind => _kind;

        // spec_cave_biome_art_profiles_runtime (CV01): quando o profile de arte jÃ¡ forneceu um
        // sprite dedicado, mantÃ©m a cor branca (nÃ£o aplica o tint placeholder por cima da arte real).
        private bool _hasCustomSprite;

        public void Configure(string hazardId, CaveHazardKind kind, SpriteRenderer spriteRenderer, bool hasCustomSprite = false)
        {
            _hazardId = hazardId;
            _kind = kind;
            _spriteRenderer = spriteRenderer != null ? spriteRenderer : GetComponent<SpriteRenderer>();
            _hasCustomSprite = hasCustomSprite;
            ApplyTelegraphColor();
        }

        /// <summary>Cor placeholder por tipo (telegraph de tile â€” CA-2). SubstituÃ­vel por arte futura.</summary>
        public static Color ResolveTelegraphColor(CaveHazardKind kind)
        {
            switch (kind)
            {
                case CaveHazardKind.ToxicPool:
                    return new Color(0.35f, 0.85f, 0.25f, 0.65f); // verde tÃ³xico
                case CaveHazardKind.IceSlick:
                    return new Color(0.55f, 0.85f, 1f, 0.6f);     // azul gelo
                case CaveHazardKind.FallingRock:
                    return new Color(0.75f, 0.55f, 0.3f, 0.7f);   // Ã¢mbar/rocha
                default:
                    return new Color(1f, 0f, 1f, 0.6f);
            }
        }

        private void ApplyTelegraphColor()
        {
            if (_spriteRenderer == null || _hasCustomSprite)
            {
                return;
            }

            _spriteRenderer.color = ResolveTelegraphColor(_kind);
        }

        private void Update()
        {
            // IceSlick: limpa o fator quando expira (mesmo padrÃ£o SetFactor/ClearFactor do projeto).
            if (_iceSlickExpireTime > 0f && Time.time >= _iceSlickExpireTime)
            {
                _activePlayerController?.SpeedComposer.ClearFactor(SpeedFactorKind.Status);
                _iceSlickExpireTime = -1f;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryAffectPlayer(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            // ToxicPool/IceSlick reaplicam ao permanecer (com cooldown de reentrada); FallingRock nÃ£o.
            if (_kind == CaveHazardKind.FallingRock)
            {
                return;
            }

            TryAffectPlayer(collision);
        }

        private void TryAffectPlayer(Collider2D collision)
        {
            var playerController = collision.GetComponentInParent<PlayerController>();
            if (playerController == null)
            {
                playerController = collision.GetComponent<PlayerController>();
            }

            if (playerController == null)
            {
                return;
            }

            if (Time.time < _lastTriggerTime + ReentryCooldownSeconds)
            {
                return;
            }

            _lastTriggerTime = Time.time;
            _activePlayerController = playerController;
            ResolvePlayerManager(collision);

            switch (_kind)
            {
                case CaveHazardKind.ToxicPool:
                    ApplyToxic();
                    break;
                case CaveHazardKind.IceSlick:
                    ApplyIceSlick(playerController);
                    break;
                case CaveHazardKind.FallingRock:
                    ApplyFallingRock();
                    break;
            }
        }

        private void ApplyToxic()
        {
            var receiver = PlayerStatusReceiver.Instance;
            var applied = receiver != null && receiver.TryApplyFromEnemyAction(ToxicStatusId, ToxicApplyChance);
            if (applied)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Veneno!"));
                return;
            }

            // Fallback sem status (F01 ausente): dano direto pequeno pelo redutor central.
            if (_playerManager != null)
            {
                PlayerDamageReceiver.ApplyDamage(_playerManager, ToxicFallbackDamage, _hazardId, DamageType.Toxic);
                GameEventBus.Publish(new PlayerActionFeedbackEvent("PoÃ§a tÃ³xica!"));
            }
        }

        private void ApplyIceSlick(PlayerController playerController)
        {
            playerController.SpeedComposer.SetFactor(SpeedFactorKind.Status, Mathf.Max(0.01f, IceSlickFactor));
            _iceSlickExpireTime = Time.time + IceSlickDurationSeconds;
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Gelo escorregadio!"));
        }

        private void ApplyFallingRock()
        {
            if (_playerManager == null)
            {
                return;
            }

            var dealt = PlayerDamageReceiver.ApplyDamage(_playerManager, FallingRockDamage, _hazardId, DamageType.Physical);
            GameEventBus.Publish(new PlayerDamagedEvent(dealt, transform.position, _hazardId, "Estalactite"));
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Queda de rocha!"));
        }

        private void ResolvePlayerManager(Collider2D collision)
        {
            if (_playerManager != null)
            {
                return;
            }

            if (GameBootstrap.Instance != null && GameBootstrap.Instance.PlayerManager != null)
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

        private void OnDestroy()
        {
            // Garante que o fator de gelo nÃ£o fique preso se o tile for destruÃ­do com o player dentro.
            if (_iceSlickExpireTime > 0f)
            {
                _activePlayerController?.SpeedComposer.ClearFactor(SpeedFactorKind.Status);
            }
        }
    }
}
