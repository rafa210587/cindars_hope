namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_31 — Request to teleport the player to the entrance of the CURRENT cave level (Mirror of Return).
    /// Named hook: the cave runtime consumes this by restoring the current level from its existing snapshot at
    /// the Entrance anchor (CaveLevelRuntimeController.RestoreFromSnapshot + CaveSpawnAnchor.Entrance) — it must
    /// NOT regenerate (stable-run, ADR-0005). Carries the run seed + level the request was raised against so the
    /// consumer can assert the run seed is unchanged before applying.
    /// </summary>
    public readonly struct MirrorReturnRequestedEvent
    {
        public string CaveRunSeed { get; }
        public int CaveLevel { get; }

        public MirrorReturnRequestedEvent(string caveRunSeed, int caveLevel)
        {
            CaveRunSeed = caveRunSeed;
            CaveLevel = caveLevel;
        }
    }

    /// <summary>
    /// fable_31 — Request to ward off nearby enemies (Bell of Warding): enemies within a radius flee briefly.
    /// Named hook for the cave/enemy AI runtime; payload uses simple types only.
    /// </summary>
    public readonly struct EnemyWardRequestedEvent
    {
        public float Radius { get; }
        public float FleeSeconds { get; }

        public EnemyWardRequestedEvent(float radius, float fleeSeconds)
        {
            Radius = radius;
            FleeSeconds = fleeSeconds;
        }
    }

    /// <summary>
    /// fable_31 — Request to advance the in-game clock to dawn / 06:00 (Hourglass of Dawn). Named hook for the
    /// time system (TimeManager / GameTimeManager own the phase clock); payload carries the current day so the
    /// consumer can decide whether dawn means today or the next day.
    /// </summary>
    public readonly struct AdvanceToDawnRequestedEvent
    {
        public int CurrentDay { get; }

        public AdvanceToDawnRequestedEvent(int currentDay)
        {
            CurrentDay = currentDay;
        }
    }

    /// <summary>
    /// fable_31 — Request a single free equipment repair (Eternal Whetstone, gated once per in-game day).
    /// Named hook for the equipment/repair runtime (EquipmentManager.RepairItem). Daily gating is enforced
    /// upstream by MagicItemUseService before this is published.
    /// </summary>
    public readonly struct FreeRepairRequestedEvent
    {
        public int Day { get; }

        public FreeRepairRequestedEvent(int day)
        {
            Day = day;
        }
    }
}
