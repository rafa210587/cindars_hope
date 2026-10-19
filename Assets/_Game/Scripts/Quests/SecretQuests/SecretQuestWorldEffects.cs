using System.Collections.Generic;
using CindarsHope.Quests.Flags;

namespace CindarsHope.Quests.SecretQuests
{
    /// <summary>
    /// fable_52 — reads/queries the world-effect flags granted by the secret quests and applies them
    /// at the existing integration points (CA-2 / CA-3). It owns NO new state repository: every flag
    /// lives in <see cref="QuestFlagService"/> (persisted), and these are pure queries / pure
    /// transforms over it.
    ///
    /// World effects:
    /// - merchant_discount_10  → 10% off the wandering merchant's prices (pricing point reads this).
    /// - warden_peaceful_forever → the Silence Warden's AI materializes as peaceful (behavior, never
    ///   the spawn plan/seed — ADR-0005).
    /// - goblin_band_neutral_run → the goblin band is neutral for the CURRENT run only; cleared on a
    ///   new run (same lifecycle as CaveRunSeed).
    /// - scrounger_vendor_unlocked_&lt;level&gt; → the Old Scrounger King is a fixed vendor on that level.
    /// </summary>
    public sealed class SecretQuestWorldEffects
    {
        public const string SetterSystem = "fable_52_secret_quests";

        /// <summary>Catalog permanent discount: 10% off merchant prices once the discount flag is set.</summary>
        public const int MerchantDiscountPercent = 10;

        private readonly QuestFlagService _flags;
        private readonly QuestFlagRegistry _registry;

        public SecretQuestWorldEffects(QuestFlagService flags, QuestFlagRegistry registry = null)
        {
            _flags = flags;
            _registry = registry;
        }

        // ─── Queries (consumed by pricing / AI / vendor integration points) ───────────────────────

        public bool IsMerchantDiscountActive() => _flags != null && _flags.IsSet(SecretQuestCatalog.MerchantDiscountFlag);

        public bool IsWardenPeacefulForever() => _flags != null && _flags.IsSet(SecretQuestCatalog.WardenPeacefulFlag);

        public bool IsGoblinBandNeutralThisRun() => _flags != null && _flags.IsSet(SecretQuestCatalog.GoblinBandNeutralFlag);

        public bool IsScroungerVendorUnlocked(int caveLevel) =>
            _flags != null && _flags.IsSet(SecretQuestCatalog.ScroungerVendorFlag(caveLevel));

        // ─── Pricing (CA-2) ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies the merchant discount to a base price when the flag is active. Pure, non-stacking:
        /// the discount is read from the single flag (never accumulated), so completing more than one
        /// merchant list never deepens it. Rounds down; never below 0.
        /// </summary>
        public int ApplyMerchantDiscount(int basePrice)
        {
            if (basePrice <= 0 || !IsMerchantDiscountActive()) return basePrice < 0 ? 0 : basePrice;
            long discounted = (long)basePrice * (100 - MerchantDiscountPercent) / 100;
            return discounted < 0 ? 0 : (int)discounted;
        }

        // ─── Run lifecycle (CA-3) ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Clears the per-run goblin neutral band when a NEW run begins (death / new game / explicit
        /// regen — same triggers that reroll CaveRunSeed). The permanent flags (warden, discount,
        /// scrounger vendor) are NOT touched. Returns true when the flag was actually cleared.
        /// </summary>
        public bool OnNewRunStarted()
        {
            if (_flags == null || !_flags.IsSet(SecretQuestCatalog.GoblinBandNeutralFlag)) return false;
            var result = _flags.ClearFlag(SecretQuestCatalog.GoblinBandNeutralFlag, SetterSystem);
            return result.Success;
        }

        /// <summary>
        /// CA-3 — re-applies the permanent world-effect flags after a save load. The persisted source
        /// of truth is each completed secret quest's GrantedFlagIds (carried in QuestStateSection, which
        /// DOES survive save/load); QuestFlagService's live dict is rebuilt from it here so queries work
        /// after a reload. The per-run goblin neutral flag is intentionally NOT rehydrated — a new run
        /// must start neutral-free. Dynamic scrounger vendor flags are (re)registered on the fly.
        /// </summary>
        public void RehydrateFromGrantedFlags(IEnumerable<string> grantedFlagIds)
        {
            if (_flags == null || grantedFlagIds == null) return;
            foreach (var flagId in grantedFlagIds)
            {
                if (string.IsNullOrEmpty(flagId)) continue;
                if (flagId == SecretQuestCatalog.GoblinBandNeutralFlag) continue; // per-run, never rehydrated

                if (_registry != null && !_registry.IsRegistered(flagId) &&
                    flagId.StartsWith("scrounger_vendor_unlocked_"))
                {
                    // re-register the concrete vendor flag so GrantFlag resolves.
                    _registry.Register(new QuestFlagDefinition
                    {
                        FlagId = flagId,
                        FlagType = QuestFlagType.Boolean,
                        Scope = QuestFlagScope.Cave,
                        Visibility = QuestFlagVisibility.PlayerKnownAfterDiscovery,
                        OwnerSystem = SetterSystem,
                        Persists = true,
                        DefaultValue = "false"
                    });
                }

                _flags.SetFlag(flagId, "true", SetterSystem);
            }
        }
    }
}
