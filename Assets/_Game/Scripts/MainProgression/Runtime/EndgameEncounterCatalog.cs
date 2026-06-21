using System.Collections.Generic;

namespace CindarsHope.MainProgression.Runtime
{
    /// <summary>
    /// fable_43 — canonical catalog of the four level-101 final encounters (CAVE_BESTIARY_CATALOG
    /// PARTE I). Pure data: stable boss ids (matching the dormant F33 assets), canonical order, and
    /// the special mechanic each encounter carries. No Unity reference, no HP/DMG numbers duplicated
    /// here (those live in the F33 boss assets — this is the orchestration order/identity only).
    ///
    /// The ids are CANONICAL and must match the F33 assets and the quest catalog. Do not rename.
    /// </summary>
    public enum EndgameSpecialMechanic
    {
        None = 0,
        MirrorBuild = 1,     // Cindrathel: mirrors the player's real build
        HudSuppression = 2   // Archivist of Silence: hides HUD widgets by phase
    }

    public sealed class EndgameEncounterDefinition
    {
        public int Index { get; }
        public string BossId { get; }
        public EndgameSpecialMechanic Mechanic { get; }

        public EndgameEncounterDefinition(int index, string bossId, EndgameSpecialMechanic mechanic)
        {
            Index = index;
            BossId = bossId;
            Mechanic = mechanic;
        }
    }

    public static class EndgameEncounterCatalog
    {
        // Canonical boss ids (F33 dormant assets). Stable — never rename.
        public const string VelKaraumBossId = "boss_vel_karaum";
        public const string CindrathelBossId = "boss_cindrathel";
        public const string ArchivistBossId = "boss_archivist_of_silence";
        public const string IthryndorBossId = "boss_ithryndor";

        // Stable HUD widget ids suppressed by the Archivist (existing widgets — never created here).
        public const string WidgetBossHpBar = "hud_boss_hp_bar";
        public const string WidgetMinimap = "hud_minimap";
        public const string WidgetDamageNumbers = "hud_damage_numbers";

        /// <summary>Canonical encounter order (index 0..3). Recovery window sits between each.</summary>
        public static IReadOnlyList<EndgameEncounterDefinition> Order { get; } =
            new List<EndgameEncounterDefinition>
            {
                new EndgameEncounterDefinition(0, VelKaraumBossId, EndgameSpecialMechanic.None),
                new EndgameEncounterDefinition(1, CindrathelBossId, EndgameSpecialMechanic.MirrorBuild),
                new EndgameEncounterDefinition(2, ArchivistBossId, EndgameSpecialMechanic.HudSuppression),
                new EndgameEncounterDefinition(3, IthryndorBossId, EndgameSpecialMechanic.None)
            };

        public static int Count => Order.Count;

        /// <summary>The encounter at the given index, or null if out of range.</summary>
        public static EndgameEncounterDefinition At(int index)
        {
            if (index < 0 || index >= Order.Count) return null;
            return Order[index];
        }
    }
}
