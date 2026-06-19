using System.Collections.Generic;

namespace CindarsHope.Items
{
    /// <summary>
    /// fable_31 — Runtime source of truth for the cave magic items, mirroring the canonical
    /// ids materialized by fable_32 (editor catalog <c>CanonicalItemCatalog.AddCaveMagicItems</c>).
    ///
    /// PURE C# (no Unity refs) so the identification swap, passive flags, deterministic reveal,
    /// loot weighting and Veska rotation can be unit-tested in EditMode without Play Mode.
    ///
    /// Pattern PAR DE ITENS: <see cref="UnidentifiedTrinketId"/> ↔ one real magic item. Identifying
    /// is a 1:1 inventory swap — no per-instance stack metadata, no new save schema. Which real item a
    /// given unidentified instance reveals is DETERMINISTIC per cave run+level (see
    /// <see cref="ResolveRevealedItemId"/>), preserving the cave stable-run contract (ADR-0005).
    /// </summary>
    public static class MagicItemCatalog
    {
        // Canonical ids (must match fable_32 CanonicalItemCatalog exactly).
        public const string UnidentifiedTrinketId = "item_unidentified_trinket";
        public const string ScrollIdentifyId = "item_consumable_scroll_identify";

        public const string PendantOfEchoes = "item_magic_pendant_of_echoes";
        public const string LanternOfTrueSight = "item_magic_lantern_of_true_sight";
        public const string PouchOfHolding = "item_magic_pouch_of_holding";
        public const string CandleOfTheDepths = "item_magic_candle_of_the_depths";
        public const string WhetstoneEternal = "item_magic_whetstone_eternal";
        public const string BellOfWarding = "item_magic_bell_of_warding";
        public const string MirrorOfReturn = "item_magic_mirror_of_return";
        public const string HourglassOfDawn = "item_magic_hourglass_of_dawn";

        // Named passive flags (continuous effects via presence in inventory — ItemPassiveTracker).
        // These are stable string keys that future consumers (HUD, fable_24 reveal, light system)
        // observe. They are NOT save state (derived from current inventory each evaluation).
        public const string FlagShowEnemyHealth = "magic_flag_show_enemy_health";        // pendant
        public const string FlagRevealAmbush = "magic_flag_reveal_ambush";               // lantern -> fable_24
        public const string FlagInventorySlotBonus = "magic_flag_inventory_slot_bonus";  // pouch (+6)
        public const string FlagPersistentLight = "magic_flag_persistent_light";         // candle

        /// <summary>Extra inventory slots granted by Pouch of Holding while it is present.</summary>
        public const int PouchSlotBonus = 6;

        /// <summary>Light radius (grid units) requested by Candle of the Depths while present.</summary>
        public const int CandleLightRadius = 8;

        /// <summary>The 8 canonical magic item ids, in catalog order.</summary>
        public static IReadOnlyList<string> MagicItemIds => _magicItemIds;

        private static readonly string[] _magicItemIds =
        {
            PendantOfEchoes,
            LanternOfTrueSight,
            PouchOfHolding,
            CandleOfTheDepths,
            WhetstoneEternal,
            BellOfWarding,
            MirrorOfReturn,
            HourglassOfDawn
        };

        // The four passive (presence-based) magic items and the flag each one raises.
        private static readonly Dictionary<string, string> _passiveFlagByItem = new Dictionary<string, string>
        {
            { PendantOfEchoes, FlagShowEnemyHealth },
            { LanternOfTrueSight, FlagRevealAmbush },
            { PouchOfHolding, FlagInventorySlotBonus },
            { CandleOfTheDepths, FlagPersistentLight }
        };

        // The four "use" magic items routed through the F08 consumable pipeline (ItemUseManager).
        private static readonly HashSet<string> _useItems = new HashSet<string>
        {
            BellOfWarding,
            MirrorOfReturn,
            HourglassOfDawn,
            WhetstoneEternal
        };

        public static bool IsMagicItem(string itemId) =>
            !string.IsNullOrWhiteSpace(itemId) && System.Array.IndexOf(_magicItemIds, itemId) >= 0;

        public static bool IsUnidentifiedTrinket(string itemId) =>
            string.Equals(itemId, UnidentifiedTrinketId, System.StringComparison.Ordinal);

        public static bool IsPassiveItem(string itemId) =>
            !string.IsNullOrWhiteSpace(itemId) && _passiveFlagByItem.ContainsKey(itemId);

        public static bool IsUseItem(string itemId) =>
            !string.IsNullOrWhiteSpace(itemId) && _useItems.Contains(itemId);

        /// <summary>Returns the passive flag for a magic item, or empty when it has no passive effect.</summary>
        public static string GetPassiveFlag(string itemId) =>
            !string.IsNullOrWhiteSpace(itemId) && _passiveFlagByItem.TryGetValue(itemId, out var flag)
                ? flag
                : string.Empty;

        /// <summary>
        /// Deterministically resolves which real magic item an unidentified trinket reveals, from the
        /// cave run seed + cave level + a stable per-source salt. Same inputs ⇒ same reveal (stable-run).
        /// This binds the identity of a found trinket to WHERE/WHEN it dropped, not to when it is identified.
        /// </summary>
        /// <param name="caveRunSeed">The active CaveRunManager.CaveRunSeed (stable within a run).</param>
        /// <param name="caveLevel">The cave level the trinket dropped on.</param>
        /// <param name="sourceSalt">A stable per-source id (e.g. chest instance id) so two chests differ.</param>
        public static string ResolveRevealedItemId(string caveRunSeed, int caveLevel, string sourceSalt)
        {
            var key = $"{caveRunSeed}|{caveLevel}|{sourceSalt}|magic_reveal";
            var hash = StableHash(key);
            // Map the unsigned hash into [0, count) with no negative-modulo bias.
            var index = (int)((uint)hash % (uint)_magicItemIds.Length);
            return _magicItemIds[index];
        }

        /// <summary>
        /// FNV-1a 32-bit stable hash (same family used by the cave stable-run code, e.g.
        /// EnemyLootResolver/CaveEnemySpawnPlanner). Platform-independent and deterministic —
        /// never use string.GetHashCode() for stable-run content.
        /// </summary>
        public static int StableHash(string value)
        {
            unchecked
            {
                const int fnvOffset = (int)2166136261;
                const int fnvPrime = 16777619;
                var hash = fnvOffset;
                var s = value ?? string.Empty;
                for (int i = 0; i < s.Length; i++)
                {
                    hash ^= s[i];
                    hash *= fnvPrime;
                }
                return hash == int.MinValue ? 0 : hash;
            }
        }
    }
}
