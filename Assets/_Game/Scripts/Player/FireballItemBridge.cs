using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Player
{
    /// <summary>
    /// LEGACY: This class is part of the deprecated fireball flow.
    ///
    /// Status: QUARANTINED - Registers a FireballUseHandler with ItemUseManager, which was the old
    /// spell casting mechanism. The new flow uses PlayerAttackController.TryExecuteSpellAttack(),
    /// which instanc iates projectile prefabs directly.
    ///
    /// Reference: SPEC_04 (Wave 1 - Legacy Combat Quarantine)
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FireballItemBridge : MonoBehaviour
    {
        private const string FireballItemId = "item_spell_fireball_test";
        private const string FireballSpellId = "spell_fireball";

        [SerializeField] private CindarsHope.Combat.PlayerSpellCaster _spellCaster;

        private void Start()
        {
            if (_spellCaster == null)
                _spellCaster = GetComponent<CindarsHope.Combat.PlayerSpellCaster>();

            var handler = ScriptableObject.CreateInstance<FireballUseHandler>();
            handler.Configure(this);
            ItemUseManager.Instance?.RegisterHandler(FireballItemId, handler);
        }

        internal void CastFireball()
        {
            if (_spellCaster != null)
                _spellCaster.TrycastSpell(FireballSpellId, Vector2.zero);
            else
                Debug.LogWarning("FireballItemBridge: PlayerSpellCaster not found.", this);
        }
    }
}
