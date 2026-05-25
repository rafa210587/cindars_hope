using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
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
        [SerializeField] private SpellDatabaseSO _spellDatabase;
        [SerializeField] private float _knockbackForce = 1.5f;

        private float _lastSpellCastTime = float.MinValue;

        private void Start()
        {
            if (_spellDatabase == null && GameBootstrap.Instance != null)
            {
                _spellDatabase = GameBootstrap.Instance.SpellDatabase;
            }
        }

        public bool TrycastSpell(string spellId, Vector2 direction)
        {
            if (string.IsNullOrEmpty(spellId))
                return false;

            if (_spellDatabase == null || !_spellDatabase.TryGetById(spellId, out var spell))
            {
                GameEventBus.Publish(new SpellCastFailedEvent(spellId, "Spell not found"));
                return false;
            }

            return TrycastSpellWithData(spell, direction);
        }

        public bool TrycastSpell(SpellDataSO spell, Vector2 direction)
        {
            if (spell == null)
                return false;

            return TrycastSpellWithData(spell, direction);
        }

        private bool TrycastSpellWithData(SpellDataSO spell, Vector2 direction)
        {
            if (Time.time - _lastSpellCastTime < spell.CooldownSeconds)
            {
                GameEventBus.Publish(new SpellCastFailedEvent(spell.Id, "Cooldown active"));
                return false;
            }

            if (_manaManager != null && !_manaManager.TrySpendMana(spell.ManaCost))
            {
                GameEventBus.Publish(new SpellCastFailedEvent(spell.Id, "Insufficient mana"));
                return false;
            }

            GameEventBus.Publish(new SpellCastStartedEvent(spell.Id));
            ExecuteSpell(spell, direction);
            GameEventBus.Publish(new SpellCastSucceededEvent(spell.Id));
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
                    KnockbackForce = _knockbackForce
                };

                var result = DamageCalculator.Calculate(damageRequest);
                enemyHealth.TakeDamage(damageRequest);
            }
        }
    }
}
