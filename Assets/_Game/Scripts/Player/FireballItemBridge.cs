using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Player
{
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
