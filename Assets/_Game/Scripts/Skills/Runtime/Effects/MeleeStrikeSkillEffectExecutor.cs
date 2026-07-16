using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Player;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>
    /// Real melee skill executor: hits enemies in an arc (or full circle) around the caster,
    /// optionally lunging forward first (leap/charge skills). Spends stamina via StaminaManager.
    /// One configured instance per EffectId (strategy pattern over the skill effect registry).
    /// </summary>
    public sealed class MeleeStrikeSkillEffectExecutor : ISkillEffectExecutor
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly int _baseDamage;
        private readonly float _range;
        private readonly float _arcDegrees;
        private readonly int _staminaCost;
        private readonly float _lungeDistance;
        private readonly float _knockbackForce;
        private readonly float _cooldownSeconds;
        // F02: multiplicador de dano de posture (quebra-guarda usa 3x).
        private readonly float _postureDamageMultiplier;
        private readonly Physics2DOverlapBuffer _overlapBuffer = new Physics2DOverlapBuffer();

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public MeleeStrikeSkillEffectExecutor(
            string effectId,
            string displayName,
            int baseDamage,
            float range,
            float arcDegrees,
            int staminaCost,
            float lungeDistance = 0f,
            float knockbackForce = 3f,
            float cooldownSeconds = 3f,
            float postureDamageMultiplier = 1f)
        {
            _postureDamageMultiplier = Mathf.Max(0f, postureDamageMultiplier);
            _effectId = effectId;
            _displayName = displayName;
            _baseDamage = baseDamage;
            _range = range;
            _arcDegrees = arcDegrees;
            _staminaCost = staminaCost;
            _lungeDistance = lungeDistance;
            _knockbackForce = knockbackForce;
            _cooldownSeconds = cooldownSeconds;
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            if (context?.Caster == null)
                return SkillEffectResult.Failed("NoCaster", "Jogador nao encontrado.");

            var staminaManager = GameBootstrap.Instance?.StaminaManager as CindarsHope.Player.StaminaManager;
            if (staminaManager != null && !staminaManager.TrySpendStamina(_staminaCost))
                return SkillEffectResult.Failed("InsufficientStamina", $"Stamina insuficiente ({_staminaCost}).");

            var playerController = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            Vector2 facing = playerController != null ? playerController.LastFacingDirection : Vector2.right;
            if (facing.sqrMagnitude < 0.01f)
                facing = Vector2.right;

            Transform bodyTransform = playerController != null ? playerController.transform : context.Caster.transform;

            if (_lungeDistance > 0f)
            {
                var body = bodyTransform.GetComponent<Rigidbody2D>();
                Vector2 destination = (Vector2)bodyTransform.position + facing.normalized * _lungeDistance;
                if (body != null)
                    body.MovePosition(destination);
                else
                    bodyTransform.position = destination;
            }

            Vector2 attackCenter = (Vector2)bodyTransform.position + facing.normalized * (_range * 0.4f);
            int colliderCount = _overlapBuffer.QueryCircle(
                attackCenter,
                _range,
                ContactFilter2D.noFilter);
            int hits = 0;
            float halfArc = _arcDegrees * 0.5f;

            for (int i = 0; i < colliderCount; i++)
            {
                Collider2D collider = _overlapBuffer[i];
                var enemyHealth = collider.GetComponentInParent<EnemyHealth>() ?? collider.GetComponent<EnemyHealth>();
                if (enemyHealth == null || enemyHealth.IsDead)
                    continue;

                if (_arcDegrees < 360f)
                {
                    Vector2 toEnemy = ((Vector2)collider.transform.position - (Vector2)bodyTransform.position).normalized;
                    if (Vector2.Angle(facing, toEnemy) > halfArc)
                        continue;
                }

                var request = new DamageRequest(enemyHealth.EnemyId, _baseDamage)
                {
                    DamageType = DamageType.Physical,
                    SourcePosition = bodyTransform.position,
                    KnockbackForce = _knockbackForce
                };
                enemyHealth.TakeDamage(request);
                hits++;

                // F02: dano de posture (skills aplicam 1x; quebra-guarda 3x).
                if (_postureDamageMultiplier > 0f)
                {
                    var posture = enemyHealth.GetComponent<EnemyPostureState>();
                    if (posture != null)
                    {
                        posture.ApplyPostureDamage(_baseDamage * _postureDamageMultiplier);
                    }
                }
            }

            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: SkillMeleeStrike. EffectId={_effectId}, Hits={hits}, Range={_range:F1}, Arc={_arcDegrees:F0}");
            string feedback = hits > 0
                ? $"{_displayName}: acertou {hits} inimigo(s)!"
                : $"{_displayName}: nenhum inimigo no alcance.";
            return SkillEffectResult.Succeeded(feedback, costSpent: true, cooldownStarted: true, cooldownSeconds: _cooldownSeconds);
        }
    }
}
