using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.DebugTools;
using UnityEngine;

namespace CindarsHope.Combat.Magic
{
    /// <summary>
    /// fable_08 — MonoBehaviour FINO no player que gerencia a JANELA de cast time de uma magia e a
    /// cancela se o caster tomar dano (CA-2). Criado pelo <see cref="PlayerAttackController"/>; não
    /// contém lógica de shape (isso é do <see cref="CindarsHope.Combat.SpellCastService"/>).
    ///
    /// Fluxo:
    ///   - <see cref="BeginCast"/> recebe um plano já validado/reservado (mana gasta) + o serviço;
    ///   - se CastTimeSeconds &lt;= 0 resolve imediatamente (caminho legado);
    ///   - senão telegrafa (flash) e espera; <see cref="PlayerDamagedEvent"/> cancela com reembolso
    ///     (<see cref="CindarsHope.Combat.SpellCastService.RefundCast"/>) e
    ///     <see cref="SpellCastInterruptedEvent"/>; timeout máximo evita soft-lock; modal cancela.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpellCastRoutine : MonoBehaviour
    {
        /// <summary>Teto absoluto de cast time (anti soft-lock; risco documentado na spec).</summary>
        public const float MaxCastSeconds = 5f;

        private CindarsHope.Combat.SpellCastService _service;
        private CindarsHope.Combat.SpellCastService.SpellCastPlan _activePlan;
        private Coroutine _activeCast;
        private bool _subscribed;
        private SpriteRenderer _telegraphRenderer;
        private Color _telegraphBaseColor = Color.white;

        /// <summary>True enquanto uma magia com cast time está em andamento (bloqueia novo cast).</summary>
        public bool IsCasting => _activeCast != null;

        public void Configure(SpriteRenderer telegraphRenderer)
        {
            _telegraphRenderer = telegraphRenderer;
            if (_telegraphRenderer != null)
            {
                _telegraphBaseColor = _telegraphRenderer.color;
            }
        }

        private void OnEnable()
        {
            if (!_subscribed)
            {
                GameEventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
                _subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                GameEventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
                _subscribed = false;
            }

            CancelActiveCast("Disabled");
        }

        /// <summary>
        /// Inicia (ou resolve imediatamente) um cast. Retorna false se já há cast em andamento
        /// (um cast por vez). castTime &lt;= 0 resolve já; senão agenda a resolução pós-janela.
        /// </summary>
        public bool BeginCast(CindarsHope.Combat.SpellCastService service, CindarsHope.Combat.SpellCastService.SpellCastPlan plan)
        {
            if (service == null || plan == null || plan.Spell == null)
            {
                return false;
            }

            if (IsCasting)
            {
                CombatLog.Log($"CombatLog: SpellCastBlocked. Reason=AlreadyCasting, Spell={plan.Spell.Id}", this);
                return false;
            }

            float castTime = Mathf.Clamp(plan.Spell.CastTimeSeconds, 0f, MaxCastSeconds);
            if (castTime <= 0f)
            {
                service.ResolveCast(plan);
                return true;
            }

            _service = service;
            _activePlan = plan;
            GameEventBus.Publish(new SpellCastStartedEvent(plan.Spell.Id));
            CombatLog.Log($"CombatLog: SpellCastStarted. Spell={plan.Spell.Id}, CastTime={castTime:F2}", this);
            _activeCast = StartCoroutine(CastWindow(castTime));
            return true;
        }

        private IEnumerator CastWindow(float castTime)
        {
            float elapsed = 0f;
            while (elapsed < castTime)
            {
                // Modal aberto cancela o cast (anti soft-lock — mira do risco da spec).
                if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
                {
                    CancelActiveCast("ModalOpen");
                    yield break;
                }

                UpdateTelegraph(elapsed / castTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Janela completa sem interrupção: resolve a magia.
            var plan = _activePlan;
            var service = _service;
            ClearActiveCast();
            service?.ResolveCast(plan);
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            if (IsCasting)
            {
                CancelActiveCast("PlayerDamaged");
            }
        }

        private void CancelActiveCast(string reason)
        {
            if (_activeCast != null)
            {
                StopCoroutine(_activeCast);
            }

            var plan = _activePlan;
            var service = _service;
            ClearActiveCast();

            if (plan != null && service != null)
            {
                CombatLog.Log($"CombatLog: SpellCastCanceled. Spell={plan.Spell?.Id}, Reason={reason}", this);
                service.RefundCast(plan);
            }
        }

        private void ClearActiveCast()
        {
            _activeCast = null;
            _activePlan = null;
            _service = null;
            ResetTelegraph();
        }

        private void UpdateTelegraph(float progress01)
        {
            if (_telegraphRenderer == null)
            {
                return;
            }

            // Pulso simples enquanto conjura (flash branco/azulado proporcional ao progresso).
            var castTint = new Color(0.5f, 0.7f, 1f);
            float pulse = 0.4f + 0.4f * Mathf.PingPong(progress01 * 4f, 1f);
            _telegraphRenderer.color = Color.Lerp(_telegraphBaseColor, castTint, pulse);
        }

        private void ResetTelegraph()
        {
            if (_telegraphRenderer != null)
            {
                _telegraphRenderer.color = _telegraphBaseColor;
            }
        }
    }
}
