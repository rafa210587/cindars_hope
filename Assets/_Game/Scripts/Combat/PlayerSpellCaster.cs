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
        [SerializeField] private float _spellCastCooldown = 1f;

        private float _lastSpellCastTime = float.MinValue;

        public bool CanCastSpell => Time.time - _lastSpellCastTime >= _spellCastCooldown;

        public bool TrycastSpell(SpellDataSO spell, Vector2 direction)
        {
            if (spell == null)
                return false;

            if (!CanCastSpell)
                return false;

            if (_manaManager != null && !_manaManager.TrySpendMana(spell.ManaCost))
                return false;

            ExecuteSpell(spell, direction);
            _lastSpellCastTime = Time.time;
            return true;
        }

        private void ExecuteSpell(SpellDataSO spell, Vector2 direction)
        {
            Debug.Log($"Casting spell.");
        }
    }
}
