namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// fable_11 (CA-1) — the four runtime blocks the schedule service collapses the day into.
    /// Each block maps to an anchor suffix (work/social/home) the generator emits per NPC.
    /// </summary>
    public enum NpcRuntimeBlock
    {
        /// <summary>At the work anchor (stall / post / desk). Available for shop/dialogue.</summary>
        Work = 0,

        /// <summary>At the social anchor (tavern / plaza / garden). Available for dialogue.</summary>
        Social = 1,

        /// <summary>At the home anchor (house door). Not available for shop/dialogue (day NPCs).</summary>
        Home = 2,

        /// <summary>Sleeping / off (deep night). Not available.</summary>
        Night = 3
    }

    /// <summary>
    /// fable_11 (CA-1, CA-4) — pure, deterministic resolution of the current Work/Social/Home/Night
    /// block (and availability) from an integer hour [0..23] and an <see cref="NpcScheduleArchetype"/>.
    ///
    /// This is the testable heart of the spec: it closes the WAVE25 TIME_BLOCK_DEBT (schedule per day
    /// only) by giving intra-day, per-hour resolution. The hour windows derive from the canonical
    /// periods in city_rules.md Rule 6 / CITY_LAYOUT §19, collapsed into four blocks per archetype.
    ///
    /// No Unity types here on purpose — fully unit-testable with synthetic hours (CA-1 / CA-4 evidence).
    /// </summary>
    public static class NpcScheduleBlockResolver
    {
        /// <summary>Anchor ID suffix for the work anchor of an NPC: <c>npc_&lt;id&gt;_work</c>.</summary>
        public const string WorkAnchorSuffix = "work";

        /// <summary>Anchor ID suffix for the social anchor of an NPC: <c>npc_&lt;id&gt;_social</c>.</summary>
        public const string SocialAnchorSuffix = "social";

        /// <summary>Anchor ID suffix for the home anchor of an NPC: <c>npc_&lt;id&gt;_home</c>.</summary>
        public const string HomeAnchorSuffix = "home";

        /// <summary>Normalize any integer hour into the canonical [0..23] range (wraps days).</summary>
        public static int NormalizeHour(int hour)
        {
            var h = hour % 24;
            return h < 0 ? h + 24 : h;
        }

        /// <summary>
        /// Resolve the current block for the given archetype at the given hour.
        /// Windows (canonical, derived from city_rules.md Rule 6):
        /// <list type="bullet">
        /// <item>Shopkeeper: Home/Night 22-09 (sleep 00-06), Work 09-18, Social 18-22.</item>
        /// <item>Guard: Work (on duty) 06-22, Night 22-06 — present/available almost all day.</item>
        /// <item>Night (Yael/Maelor): Work (night market) 20-02, Home 02-20 — inverted.</item>
        /// <item>Wanderer: Social 08-20, Home/Night 20-08.</item>
        /// </list>
        /// </summary>
        public static NpcRuntimeBlock ResolveBlock(NpcScheduleArchetype archetype, int hour)
        {
            var h = NormalizeHour(hour);
            switch (archetype)
            {
                case NpcScheduleArchetype.Shopkeeper:
                    if (h >= 9 && h < 18) return NpcRuntimeBlock.Work;
                    if (h >= 18 && h < 22) return NpcRuntimeBlock.Social;
                    // 22:00-00:00 winding down at home; 00:00-06:00 deep sleep; 06:00-09:00 at home.
                    if (h >= 0 && h < 6) return NpcRuntimeBlock.Night;
                    return NpcRuntimeBlock.Home;

                case NpcScheduleArchetype.Guard:
                    // On duty / patrolling most of the day; only deep night is off.
                    return (h >= 6 && h < 22) ? NpcRuntimeBlock.Work : NpcRuntimeBlock.Night;

                case NpcScheduleArchetype.Night:
                    // Night market open 20:00-02:00 (wraps midnight); otherwise resting at home.
                    return (h >= 20 || h < 2) ? NpcRuntimeBlock.Work : NpcRuntimeBlock.Home;

                case NpcScheduleArchetype.Wanderer:
                default:
                    if (h >= 8 && h < 20) return NpcRuntimeBlock.Social;
                    return (h >= 0 && h < 6) ? NpcRuntimeBlock.Night : NpcRuntimeBlock.Home;
            }
        }

        /// <summary>
        /// True when the NPC is available for shop/dialogue at this hour — i.e. resolved to the Work
        /// or Social block. Home and Night mean unavailable (CA-4). Night-type NPCs invert naturally:
        /// they resolve to Work at night and Home by day, so this returns true only at night for them.
        /// </summary>
        public static bool IsAvailable(NpcScheduleArchetype archetype, int hour)
        {
            var block = ResolveBlock(archetype, hour);
            return block == NpcRuntimeBlock.Work || block == NpcRuntimeBlock.Social;
        }

        /// <summary>Anchor ID suffix the NPC should be at for the resolved block (work/social/home).</summary>
        public static string AnchorSuffixForBlock(NpcRuntimeBlock block)
        {
            switch (block)
            {
                case NpcRuntimeBlock.Work: return WorkAnchorSuffix;
                case NpcRuntimeBlock.Social: return SocialAnchorSuffix;
                default: return HomeAnchorSuffix; // Home and Night both resolve to the home anchor.
            }
        }

        /// <summary>
        /// Map a town movement-profile string (from the generator spec / roster) to a coarse archetype.
        /// "NightOnly*" → Night; "Patrol*" / guard routes → Guard; fixed workers and shopkeepers →
        /// Shopkeeper work/social/home windows; everything else (wander) → Wanderer.
        /// </summary>
        public static NpcScheduleArchetype ArchetypeFromMovementProfile(string movementProfile, bool hasShop)
        {
            var profile = movementProfile ?? string.Empty;

            if (profile.StartsWith("NightOnly", System.StringComparison.OrdinalIgnoreCase))
            {
                return NpcScheduleArchetype.Night;
            }

            if (profile.StartsWith("Patrol", System.StringComparison.OrdinalIgnoreCase))
            {
                return NpcScheduleArchetype.Guard;
            }

            if (hasShop || profile.StartsWith("Stationary", System.StringComparison.OrdinalIgnoreCase))
            {
                return NpcScheduleArchetype.Shopkeeper;
            }

            return NpcScheduleArchetype.Wanderer;
        }
    }
}
