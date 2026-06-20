namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// fable_11 (CA-1) — coarse schedule archetype derived from a town NPC's movement profile
    /// (<c>NpcTownRosterRegistry</c> / generator spec). Drives the per-hour Work/Social/Home/Night
    /// block windows in <see cref="NpcScheduleBlockResolver"/>.
    ///
    /// This is the canonical reconciliation of the design's per-period routine (city_rules.md Rule 6,
    /// CITY_LAYOUT §19-§23) collapsed into the four runtime blocks the schedule service resolves.
    /// </summary>
    public enum NpcScheduleArchetype
    {
        /// <summary>Day shopkeeper/vendor: works the stall by day, taverns in the evening, home at night.</summary>
        Shopkeeper = 0,

        /// <summary>Guard/patrol: present and available almost all day; only sleeps deep in the night.</summary>
        Guard = 1,

        /// <summary>Night-type (Yael, Maelor): inverted — unavailable by day, active in the night market.</summary>
        Night = 2,

        /// <summary>Wanderer/social NPC: roams/socializes by day, home at night, no fixed stall.</summary>
        Wanderer = 3
    }
}
