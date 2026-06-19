using System;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// fable_33 — single source of the intra-band level scaling formula (CA-2). Assets store the
    /// BASE stats at the band's minimum level (CAVE_BESTIARY_CATALOG); this helper applies the
    /// per-level growth at spawn time so the EnemyDataSO assets stay band-base and the formula lives
    /// in exactly one place (no duplicated scaling). DEF is FIXED per creature (never scaled).
    ///
    /// Formula (BALANCE_CURVES §5, catalog §1):
    ///   HP(lvl)  = round( baseHp  * 1.12^(lvl - bandMin) )
    ///   DMG(lvl) = round( baseDmg * 1.08^(lvl - bandMin) )
    /// Type multipliers (tank/swarm/caster/elite/miniboss/boss) are already baked into the base
    /// numbers in the catalog, so this helper only applies the depth-within-band growth.
    ///
    /// Pure, deterministic, no Unity references — fully EditMode-testable.
    /// </summary>
    public static class CaveBandScaling
    {
        public const double HpPerLevel = 1.12d;   // +12% HP per level above the band minimum
        public const double DamagePerLevel = 1.08d; // +8% DMG per level above the band minimum

        // Theme band thresholds (inclusive min level of each band) — match CanonicalBestiaryCatalog.Bands.
        // Kept here as plain ints so the planner has no Editor/data dependency.
        public static int BandMinLevel(int band)
        {
            switch (band)
            {
                case 1: return 1;
                case 2: return 11;
                case 3: return 26;
                case 4: return 41;
                case 5: return 56;
                case 6: return 71;
                case 7: return 86;
                default: return 1;
            }
        }

        /// <summary>Band index (1..7) that owns a given cave level. Level 101 finale = band 7 (void).</summary>
        public static int BandForLevel(int caveLevel)
        {
            if (caveLevel <= 10) return 1;
            if (caveLevel <= 25) return 2;
            if (caveLevel <= 40) return 3;
            if (caveLevel <= 55) return 4;
            if (caveLevel <= 70) return 5;
            if (caveLevel <= 85) return 6;
            return 7;
        }

        /// <summary>Number of levels above the creature's band minimum (clamped at 0).</summary>
        public static int LevelsAboveBandMin(int spawnLevel, int bandMinLevel)
        {
            return Math.Max(0, spawnLevel - bandMinLevel);
        }

        public static int ScaleHp(int baseHp, int spawnLevel, int bandMinLevel)
        {
            var steps = LevelsAboveBandMin(spawnLevel, bandMinLevel);
            if (steps == 0)
            {
                return baseHp;
            }

            var scaled = baseHp * Math.Pow(HpPerLevel, steps);
            return Math.Max(1, (int)Math.Round(scaled, MidpointRounding.AwayFromZero));
        }

        public static int ScaleDamage(int baseDamage, int spawnLevel, int bandMinLevel)
        {
            var steps = LevelsAboveBandMin(spawnLevel, bandMinLevel);
            if (steps == 0)
            {
                return baseDamage;
            }

            var scaled = baseDamage * Math.Pow(DamagePerLevel, steps);
            return Math.Max(0, (int)Math.Round(scaled, MidpointRounding.AwayFromZero));
        }

        /// <summary>Defense is fixed per creature — this is the explicit identity used by the planner
        /// so the "DEF fixed" rule (catalog §1) is grep-able and test-asserted, not implicit.</summary>
        public static int ScaleDefense(int baseDefense, int spawnLevel, int bandMinLevel)
        {
            return baseDefense;
        }
    }
}
