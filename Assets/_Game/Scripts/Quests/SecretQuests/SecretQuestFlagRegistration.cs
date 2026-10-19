using CindarsHope.Quests.Flags;

namespace CindarsHope.Quests.SecretQuests
{
    /// <summary>
    /// fable_52 — registers the secret-quest world-effect flags into the EXISTING
    /// <see cref="QuestFlagRegistry"/> so they can be granted by the reward applicator and queried by
    /// <see cref="QuestFlagService.IsSet"/>. No new flag store — this only populates the canonical
    /// registry with the 8 world-effect flags the secrets grant.
    ///
    /// The per-level scrounger vendor flag (scrounger_vendor_unlocked_&lt;level&gt;) is registered on
    /// demand by <see cref="RegisterScroungerVendorFlag"/> when a Scrounger bargain is offered on a
    /// concrete level.
    /// </summary>
    public static class SecretQuestFlagRegistration
    {
        public const string OwnerSystem = "fable_52_secret_quests";

        /// <summary>Registers the 8 fixed world-effect flags. Idempotent (re-register overwrites).</summary>
        public static void RegisterAll(QuestFlagRegistry registry)
        {
            if (registry == null) return;
            Register(registry, SecretQuestCatalog.MerchantDiscountFlag);
            Register(registry, SecretQuestCatalog.WardenPeacefulFlag);
            Register(registry, SecretQuestCatalog.GoblinBandNeutralFlag);
            Register(registry, SecretQuestCatalog.WarchiefCrestFlag);
            Register(registry, SecretQuestCatalog.NymirianEngravingFlag);
            Register(registry, SecretQuestCatalog.ThrallNameKnownFlag);
            Register(registry, SecretQuestCatalog.DragonEggIncubatingFlag);
            Register(registry, SecretQuestCatalog.DragonEggHatchedFlag);
        }

        /// <summary>Registers a concrete scrounger_vendor_unlocked_&lt;level&gt; flag (called per level).</summary>
        public static void RegisterScroungerVendorFlag(QuestFlagRegistry registry, int caveLevel)
        {
            if (registry == null) return;
            Register(registry, SecretQuestCatalog.ScroungerVendorFlag(caveLevel));
        }

        private static void Register(QuestFlagRegistry registry, string flagId)
        {
            registry.Register(new QuestFlagDefinition
            {
                FlagId = flagId,
                FlagType = QuestFlagType.Boolean,
                Scope = QuestFlagScope.Cave,
                Visibility = QuestFlagVisibility.PlayerKnownAfterDiscovery,
                OwnerSystem = OwnerSystem,
                Persists = true,
                DefaultValue = "false",
                CanBeGrantedByReward = true,
                CanBeUsedByConditions = true
            });
        }
    }
}
