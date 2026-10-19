using System;

namespace CindarsHope.Player.Death
{
    /// <summary>Owns one death choice; penalties remain in the existing corpse resolver.</summary>
    public sealed class DeathRevivalDecision
    {
        public const string GoddessTearItemId = "item_goddess_tear";
        private readonly PlayerManager _player;
        private readonly CorpseRecoveryManager _corpses;
        private readonly Func<int> _inventoryCount;
        private readonly Func<bool> _consumeInventoryTear;
        private string _corpseId;
        private bool _pending;

        public DeathRevivalDecision(PlayerManager player, CorpseRecoveryManager corpses,
            Func<int> inventoryCount, Func<bool> consumeInventoryTear)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _corpses = corpses ?? throw new ArgumentNullException(nameof(corpses));
            _inventoryCount = inventoryCount ?? throw new ArgumentNullException(nameof(inventoryCount));
            _consumeInventoryTear = consumeInventoryTear ?? throw new ArgumentNullException(nameof(consumeInventoryTear));
        }

        public int EligibleTearCount => !_pending ? 0 : string.IsNullOrEmpty(_corpseId)
            ? _inventoryCount() : _corpses.GetRevivalTearCount(_corpseId);

        public void Begin(string corpseId)
        {
            _corpseId = corpseId;
            _pending = true;
        }

        public void Close() => _pending = false;

        public bool TryRevive()
        {
            if (!_pending || _player.CurrentHP > 0 || EligibleTearCount <= 0) return false;
            // Inventory events can synchronously re-enter this choice.
            _pending = false;
            var consumed = string.IsNullOrEmpty(_corpseId)
                ? _consumeInventoryTear() : _corpses.TryConsumeRevivalTear(_corpseId);
            if (!consumed) { _pending = true; return false; }
            _player.SetHP(_player.MaxHP);
            return true;
        }
    }
}
