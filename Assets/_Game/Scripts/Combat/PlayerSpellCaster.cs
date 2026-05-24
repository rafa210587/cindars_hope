using CindarsHope.Core;
using CindarsHope.Combat.Magic;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Combat
{
    [DisallowMultipleComponent]
    public class PlayerSpellCaster : MonoBehaviour
    {
        [SerializeField] private ManaManager _manaManager;
        [SerializeField] private PlayerController _playerController;

        private float _lastSpellCastTime = float.MinValue;

        public bool CanCastSpell(SpellDataSO spell)
        {
            if (spell == null)
                return false;

            return Time.time - _lastSpellCastTime >= spell.CooldownSeconds;
        }

        public bool TrycastSpell(SpellDataSO spell, Vector2 direction)
        {
            if (spell == null)
                return false;

            if (!CanCastSpell(spell))
                return false;

            if (_manaManager != null && !_manaManager.TrySpendMana(spell.ManaCost))
                return false;

            ExecuteSpell(spell, direction);
            _lastSpellCastTime = Time.time;
            return true;
        }

        private void ExecuteSpell(SpellDataSO spell, Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
                direction = _playerController?.LastFacingDirection ?? Vector2.right;

            Vector2 spawnPos = (Vector2)transform.position + direction.normalized * 0.5f;

            var hitColliders = Physics2D.OverlapCircleAll(spawnPos, spell.Range);
            foreach (var collider in hitColliders)
            {
                if (collider.gameObject == gameObject)
                    continue;

                var enemyHealth = collider.GetComponentInParent<EnemyHealth>() ?? collider.GetComponent<EnemyHealth>();
                if (enemyHealth == null)
                    continue;

                var damageRequest = new DamageRequest(enemyHealth.EnemyId, spell.BaseDamage)
                {
                    DamageType = spell.DamageType,
                    SourcePosition = transform.position,
                    KnockbackForce = 1.5f
                };

                var result = DamageCalculator.Calculate(damageRequest);
                enemyHealth.TakeDamage(damageRequest);
            }
        }
    }
}
