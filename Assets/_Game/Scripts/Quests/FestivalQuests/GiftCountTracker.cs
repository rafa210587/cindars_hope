using System.Collections.Generic;

namespace CindarsHope.Quests.FestivalQuests
{
    /// <summary>
    /// fable_53 (CA-3) — pure, deterministic "deliver gifts to N DISTINCT NPCs before midnight" state
    /// machine for fq_anonovo. No Unity dependency — fully EditMode-testable.
    /// <see cref="FestivalQuestService"/> drives it from the existing F26/F72 gift-accepted event
    /// (<c>NpcGiftReactionEvent</c>, counted by distinct NpcId) and from the day/midnight boundary.
    ///
    /// Contract: only DISTINCT NPCs count — a second gift to the same NPC does not advance. Once the
    /// midnight deadline passes the window closes (further gifts do not count) until reset for a new
    /// festival night. Completion = distinct NPC count reaches the required amount (5).
    /// </summary>
    public sealed class GiftCountTracker
    {
        private readonly int _requiredNpcs;
        private readonly HashSet<string> _giftedNpcs = new HashSet<string>();
        private bool _windowClosed;

        public GiftCountTracker(int requiredNpcs = 5)
        {
            _requiredNpcs = requiredNpcs < 1 ? 1 : requiredNpcs;
        }

        /// <summary>Distinct NPCs gifted before the midnight deadline this night.</summary>
        public int DistinctNpcsGifted => _giftedNpcs.Count;

        /// <summary>True once the required number of distinct NPCs have been gifted in time.</summary>
        public bool IsComplete => _giftedNpcs.Count >= _requiredNpcs;

        /// <summary>True after the midnight deadline closed the window (further gifts do not count).</summary>
        public bool WindowClosed => _windowClosed;

        /// <summary>
        /// Register a gift accepted by <paramref name="npcId"/>. Returns true when this added a NEW
        /// distinct NPC (i.e. progressed). A repeated NPC, an empty id, or a closed window is ignored.
        /// </summary>
        public bool RegisterGift(string npcId)
        {
            if (_windowClosed || string.IsNullOrEmpty(npcId)) return false;
            return _giftedNpcs.Add(npcId);
        }

        /// <summary>True when a gift was already counted for <paramref name="npcId"/> this night.</summary>
        public bool HasNpc(string npcId) => !string.IsNullOrEmpty(npcId) && _giftedNpcs.Contains(npcId);

        /// <summary>Midnight passed — close the counting window (the deadline is real).</summary>
        public void CloseAtMidnight()
        {
            _windowClosed = true;
        }

        /// <summary>Start a fresh festival night (reopen the window, clear the gifted set).</summary>
        public void ResetNight()
        {
            _giftedNpcs.Clear();
            _windowClosed = false;
        }
    }
}
