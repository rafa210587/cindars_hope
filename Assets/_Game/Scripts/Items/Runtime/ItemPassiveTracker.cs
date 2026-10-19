using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Items.Runtime
{
    /// <summary>
    /// fable_31 — Observes the inventory and keeps the continuous magic-item flags up to date (pendant → show
    /// enemy HP, lantern → reveal ambush, pouch → +6 slot bonus, candle → persistent light). The actual flag
    /// computation lives in the pure <see cref="MagicItemPassiveState"/> ("ponto único por efeito"); this thin
    /// MonoBehaviour just wires it to <see cref="InventoryChangedEvent"/> and exposes the result for consumers.
    ///
    /// No global scene search: the InventoryManager reference is injected via <see cref="Bind"/> by the
    /// runtime bootstrap. Consumers READ flags through the static accessor; they never recompute them.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ItemPassiveTracker : MonoBehaviour
    {
        private static ItemPassiveTracker _instance;
        public static ItemPassiveTracker Instance => _instance;

        private readonly MagicItemPassiveState _state = new MagicItemPassiveState();
        private InventoryManager _inventory;
        private bool _subscribed;

        /// <summary>Extra inventory slots currently granted by Pouch of Holding (0 when absent).</summary>
        public int SlotBonus => _state.SlotBonus;

        public bool IsFlagActive(string flag) => _state.IsFlagActive(flag);

        public IReadOnlyCollection<string> ActiveFlags => _state.ActiveFlags;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            Unsubscribe();
        }

        /// <summary>Injects the inventory and performs the first recompute. Idempotent.</summary>
        public void Bind(InventoryManager inventory)
        {
            _inventory = inventory;
            Subscribe();
            Refresh();
        }

        private void Subscribe()
        {
            if (_subscribed)
            {
                return;
            }

            GameEventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            GameEventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            _subscribed = false;
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            // Only recompute when a magic passive item moved (cheap fast-path).
            if (!MagicItemCatalog.IsPassiveItem(evt.ItemId))
            {
                return;
            }

            Refresh();
        }

        private void Refresh()
        {
            if (_inventory == null)
            {
                return;
            }

            var present = new List<string>();
            foreach (var kvp in _inventory.Items)
            {
                if (kvp.Value > 0)
                {
                    present.Add(kvp.Key);
                }
            }

            if (_state.Recompute(present))
            {
                CindarsHope.DebugTools.CombatLog.Log(
                    $"CombatLog: MagicPassiveFlagsChanged. SlotBonus={_state.SlotBonus}, Flags=[{string.Join(",", _state.ActiveFlags)}].",
                    this);
            }
        }
    }
}
