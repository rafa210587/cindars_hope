using System;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Cave.Traps
{
    /// <summary>
    /// fable_60 — variante "baú falso" (trap_false_chest, CA-4). Aparenta um baú comum (mesmo visual
    /// dourado fechado do <c>TreasureChestInteractable</c>) mas, ao "abrir", spawna o Hoardmaw
    /// (<c>enemy_hoardmaw</c>) pelo caminho de spawn EXISTENTE e remove o falso baú — a razão de bater
    /// no baú antes de abrir (CAVE_BESTIARY). Nunca desarmável, mas detectável (F23).
    ///
    /// Spawn idempotente: abre 1×; o estado Triggered é persistido no snapshot (revisita não duplica).
    /// Sem busca global: o spawn é injetado por callback do materializer (mesmo padrão de persistência
    /// de baús abertos do fable_09).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FalseChestTrap : MonoBehaviour, IInteractable
    {
        private string _trapInstanceId = string.Empty;
        private int _caveLevel;
        private TrapState _state = TrapState.Armed;
        private bool _detected;

        private SpriteRenderer _spriteRenderer;
        private Func<string, bool> _spawnEnemyById;
        private Action<string, TrapState> _onStateChanged;
        // spec_cave_biome_art_profiles_runtime (CV01): sprites opcionais do bioma; null = placeholder
        // de cor atual (fallback-first). Closed reusa o mesmo sprite do baú real (CAVE_BIOME_VISUAL_REFERENCE §2.1).
        private Sprite _closedSprite;
        private Sprite _revealedSprite;

        public string TrapInstanceId => _trapInstanceId;
        public TrapState State => _state;
        public bool IsDetected => _detected;
        public string InteractionPrompt => _state == TrapState.Triggered ? "Baú vazio" : "Abrir baú";

        public void Configure(
            CaveTrapPlacement placement,
            int caveLevel,
            TrapState initialState,
            SpriteRenderer spriteRenderer,
            Func<string, bool> spawnEnemyById,
            Action<string, TrapState> onStateChanged,
            Sprite closedSprite = null,
            Sprite revealedSprite = null)
        {
            _trapInstanceId = placement != null ? placement.TrapInstanceId : string.Empty;
            _caveLevel = caveLevel;
            _state = initialState;
            _detected = false;
            _spriteRenderer = spriteRenderer != null ? spriteRenderer : GetComponent<SpriteRenderer>();
            _spawnEnemyById = spawnEnemyById;
            _onStateChanged = onStateChanged;
            _closedSprite = closedSprite;
            _revealedSprite = revealedSprite;
            ApplyVisual();
        }

        /// <summary>fable_60 — detecção F23 (CA-6): revela que o baú é uma armadilha. Idempotente.</summary>
        public void RevealByDetection()
        {
            if (_state != TrapState.Armed || _detected)
            {
                return;
            }

            _detected = true;
            ApplyVisual();
            GameEventBus.Publish(new TrapDetectedEvent(
                _trapInstanceId, "trap_false_chest", transform.position, _caveLevel));
        }

        public bool CanInteract(GameObject interactor)
        {
            return _state == TrapState.Armed;
        }

        public void Interact(GameObject interactor)
        {
            if (_state != TrapState.Armed)
            {
                return;
            }

            _state = TrapState.Triggered;

            var spawned = _spawnEnemyById != null && _spawnEnemyById.Invoke(TrapDefinition.HoardmawEnemyId);

            GameEventBus.Publish(new TrapTriggeredEvent(
                _trapInstanceId, "trap_false_chest", transform.position, _caveLevel));
            GameEventBus.Publish(new PlayerActionFeedbackEvent(spawned ? "Era um Hoardmaw!" : "Baú falso!"));
            _onStateChanged?.Invoke(_trapInstanceId, TrapState.Triggered);

            ApplyVisual();

            // Remove o falso baú: o Hoardmaw assume o lugar (estado Triggered fica no snapshot).
            if (spawned)
            {
                Destroy(gameObject);
            }
        }

        private void ApplyVisual()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            // spec_cave_biome_art_profiles_runtime (CV01): sprite do bioma vence quando presente para
            // "fechado"/"detectado"; Triggered (vazio) e ausência de sprite mantêm o placeholder atual.
            if (_state != TrapState.Triggered)
            {
                var customSprite = _detected ? _revealedSprite : _closedSprite;
                if (customSprite != null)
                {
                    _spriteRenderer.sprite = customSprite;
                    _spriteRenderer.color = Color.white;
                    return;
                }
            }

            if (_state == TrapState.Triggered)
            {
                _spriteRenderer.color = new Color(0.45f, 0.4f, 0.25f, 0.7f); // baú apagado/vazio
            }
            else if (_detected)
            {
                _spriteRenderer.color = new Color(1f, 0.45f, 0.2f, 0.95f);   // aviso de detecção (perigo)
            }
            else
            {
                _spriteRenderer.color = new Color(1f, 0.85f, 0.3f, 1f);      // baú dourado (engana o jogador)
            }
        }
    }
}
