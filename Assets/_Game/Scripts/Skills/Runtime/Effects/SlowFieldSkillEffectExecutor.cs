using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.DebugTools;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>
    /// fable_70 — área de controle: aplica um StatusEffect (slow) a todos os inimigos dentro de
    /// um raio centrado no caster. Reutiliza o sistema de status existente (EnemyHealth.ApplyStatusEffect
    /// + StatusEffectDatabase), sem criar asset novo. Gasta mana (efeito mágico).
    /// </summary>
    public sealed class SlowFieldSkillEffectExecutor : ISkillEffectExecutor
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly string _statusEffectId;
        private readonly float _radius;
        private readonly int _manaCost;
        private readonly float _cooldownSeconds;
        private readonly Physics2DOverlapBuffer _overlapBuffer = new Physics2DOverlapBuffer();

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public SlowFieldSkillEffectExecutor(
            string effectId,
            string displayName,
            string statusEffectId,
            float radius,
            int manaCost,
            float cooldownSeconds)
        {
            _effectId = effectId;
            _displayName = displayName;
            _statusEffectId = statusEffectId;
            _radius = Mathf.Max(0.5f, radius);
            _manaCost = manaCost;
            _cooldownSeconds = cooldownSeconds;
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            if (context?.Caster == null)
                return SkillEffectResult.Failed("NoCaster", "Jogador nao encontrado.");

            var manaManager = GameBootstrap.Instance?.ManaManager;
            if (manaManager != null && !manaManager.TrySpendMana(_manaCost))
                return SkillEffectResult.Failed("InsufficientMana", $"Mana insuficiente ({_manaCost}).");

            var database = GameBootstrap.Instance?.StatusEffectDatabase;
            CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect = null;
            if (database != null)
                database.TryGetById(_statusEffectId, out statusEffect);

            var playerController = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            Vector2 center = playerController != null
                ? (Vector2)playerController.transform.position
                : context.WorldPosition;

            int colliderCount = _overlapBuffer.QueryCircle(
                center,
                _radius,
                ContactFilter2D.noFilter);
            int affected = 0;
            for (int i = 0; i < colliderCount; i++)
            {
                Collider2D collider = _overlapBuffer[i];
                var enemyHealth = collider.GetComponentInParent<EnemyHealth>() ?? collider.GetComponent<EnemyHealth>();
                if (enemyHealth == null || enemyHealth.IsDead)
                    continue;

                if (statusEffect != null)
                    enemyHealth.ApplyStatusEffect(statusEffect);
                affected++;
            }

            CombatLog.Log($"CombatLog: SkillSlowField. EffectId={_effectId}, Affected={affected}, Radius={_radius:F1}, Status={_statusEffectId}");
            string feedback = affected > 0
                ? $"{_displayName}: {affected} inimigo(s) lentificado(s)!"
                : $"{_displayName}: nenhum inimigo na area.";
            return SkillEffectResult.Succeeded(feedback, costSpent: manaManager != null, cooldownStarted: true, cooldownSeconds: _cooldownSeconds);
        }
    }
}
