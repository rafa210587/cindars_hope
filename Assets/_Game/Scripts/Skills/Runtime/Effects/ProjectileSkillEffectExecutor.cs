using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    /// <summary>
    /// Real ranged/magic skill executor: fires one or more runtime-built projectiles in the
    /// facing direction (fan spread for multishot, pierce for line skills). Spends mana for
    /// magic damage types and stamina for physical ones.
    /// </summary>
    public sealed class ProjectileSkillEffectExecutor : ISkillEffectExecutor
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly int _baseDamage;
        private readonly float _speed;
        private readonly float _range;
        private readonly DamageType _damageType;
        private readonly int _projectileCount;
        private readonly float _spreadDegrees;
        private readonly int _maxHitsPerProjectile;
        private readonly int _resourceCost;
        private readonly float _cooldownSeconds;
        private readonly string _statusEffectId;
        private readonly float _statusApplyChance;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public ProjectileSkillEffectExecutor(
            string effectId,
            string displayName,
            int baseDamage,
            float speed,
            float range,
            DamageType damageType,
            int resourceCost,
            int projectileCount = 1,
            float spreadDegrees = 0f,
            int maxHitsPerProjectile = 1,
            float cooldownSeconds = 4f,
            string statusEffectId = null,
            float statusApplyChance = 0f)
        {
            _effectId = effectId;
            _displayName = displayName;
            _baseDamage = baseDamage;
            _speed = speed;
            _range = range;
            _damageType = damageType;
            _resourceCost = resourceCost;
            _projectileCount = Mathf.Max(1, projectileCount);
            _spreadDegrees = spreadDegrees;
            _maxHitsPerProjectile = Mathf.Max(1, maxHitsPerProjectile);
            _cooldownSeconds = cooldownSeconds;
            _statusEffectId = statusEffectId;
            _statusApplyChance = statusApplyChance;
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            if (context?.Caster == null)
                return SkillEffectResult.Failed("NoCaster", "Jogador nao encontrado.");

            // Physical shots cost stamina; elemental/arcane shots cost mana.
            if (_damageType == DamageType.Physical)
            {
                var staminaManager = GameBootstrap.Instance?.StaminaManager;
                if (staminaManager != null && !staminaManager.TrySpendStamina(_resourceCost))
                    return SkillEffectResult.Failed("InsufficientStamina", $"Stamina insuficiente ({_resourceCost}).");
            }
            else
            {
                var manaManager = GameBootstrap.Instance?.ManaManager;
                if (manaManager != null && !manaManager.TrySpendMana(_resourceCost))
                    return SkillEffectResult.Failed("InsufficientMana", $"Mana insuficiente ({_resourceCost}).");
            }

            var playerController = context.Caster.GetComponentInChildren<PlayerController>()
                ?? context.Caster.GetComponent<PlayerController>();
            Vector2 facing = playerController != null ? playerController.LastFacingDirection : Vector2.right;
            if (facing.sqrMagnitude < 0.01f)
                facing = Vector2.right;

            Vector2 origin = playerController != null
                ? (Vector2)playerController.transform.position
                : context.WorldPosition;

            var statusEffect = ResolveStatusEffect();
            int spawned = 0;
            float baseAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            float step = _projectileCount > 1 ? _spreadDegrees / (_projectileCount - 1) : 0f;
            float startAngle = baseAngle - _spreadDegrees * 0.5f;

            for (int i = 0; i < _projectileCount; i++)
            {
                float angle = _projectileCount > 1 ? startAngle + step * i : baseAngle;
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                var request = new ProjectileSpawnRequest(
                    prefab: null,
                    sourcePosition: origin,
                    direction: direction,
                    speed: _speed,
                    range: _range,
                    baseDamage: _baseDamage,
                    damageType: _damageType,
                    knockbackForce: 2.5f,
                    spawnOffset: 0.5f,
                    statusEffect: statusEffect,
                    statusApplyChance: _statusApplyChance);
                request.VisualStyle = ProjectileVisualStyle.SkillBolt;
                request.MaxHits = _maxHitsPerProjectile;

                var result = ProjectileSpawnService.SpawnProjectile(request);
                if (result.Success)
                    spawned++;
            }

            if (spawned == 0)
                return SkillEffectResult.Failed("ProjectileSpawnFailed", $"{_displayName}: falha ao disparar.");

            Debug.Log($"CombatLog: SkillProjectileFired. EffectId={_effectId}, Count={spawned}, Type={_damageType}, Pierce={_maxHitsPerProjectile}");
            return SkillEffectResult.Succeeded($"{_displayName}!", costSpent: true, cooldownStarted: true, cooldownSeconds: _cooldownSeconds);
        }

        private CindarsHope.Combat.StatusEffect.StatusEffectSO ResolveStatusEffect()
        {
            if (string.IsNullOrEmpty(_statusEffectId))
                return null;

            var database = GameBootstrap.Instance?.StatusEffectDatabase;
            if (database != null && database.TryGetById(_statusEffectId, out var effect))
                return effect;

            return null;
        }
    }
}
