using CindarsHope.Combat;
using CindarsHope.Combat.Magic;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class PlayerSpellCaster : MonoBehaviour
    {
        [SerializeField] private ManaManager _manaManager;
        [SerializeField] private SpellDatabaseSO _spellDatabase;
        [SerializeField] private Transform _spellOrigin;

        private float _lastSpellCastTime;
        private float _spellCooldownEnd;

        public void CastSpell(string spellId)
        {
            if (_spellDatabase == null || !_spellDatabase.TryGetById(spellId, out var spellData))
            {
                Debug.LogWarning($"PlayerSpellCaster: Spell not found: {spellId}", this);
                return;
            }

            if (Time.time < _spellCooldownEnd)
            {
                GameEventBus.Publish(new SpellCastFailedEvent(spellId, "Spell on cooldown"));
                return;
            }

            if (_manaManager != null && !_manaManager.TrySpendMana(spellData.ManaCost))
            {
                GameEventBus.Publish(new SpellCastFailedEvent(spellId, "Insufficient mana"));
                return;
            }

            ExecuteSpell(spellData);
            _spellCooldownEnd = Time.time + spellData.CooldownSeconds;
            GameEventBus.Publish(new SpellCastSucceededEvent(spellId));
        }

        private void ExecuteSpell(SpellDataSO spell)
        {
            var damageRequest = new DamageRequest(
                "player",
                spell.BaseDamage
            )
            {
                DamageType = spell.DamageType,
                SourcePosition = _spellOrigin != null ? _spellOrigin.position : transform.position
            };

            var damageResult = DamageCalculator.CalculateDirectDamage(
                spell.BaseDamage,
                0,
                1f
            );

            Debug.Log($"PlayerSpellCaster: Cast {spell.SpellName}, damage={spell.BaseDamage}, mana cost={spell.ManaCost}", this);
            GameEventBus.Publish(new DamageAppliedEvent(damageResult, transform.position));
        }
    }
}
