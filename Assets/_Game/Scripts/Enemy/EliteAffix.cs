using CindarsHope.Foundation;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_24 — named-elite affixes. One affix per elite; +25% base stats on top of the affix.
    ///
    /// DECISOES §B / spec CA-3: elites are decided by a DETERMINISTIC StableHash of the cave seed,
    /// never by a GUID/timestamp/unseeded Random, so revisiting the same level in the same
    /// CaveRunSeed reproduces the SAME elites in the SAME slots (ADR-0005 / cave-stable-run).
    /// </summary>
    public enum EliteAffix
    {
        /// <summary>No affix — the spawn is a normal enemy.</summary>
        None = 0,

        /// <summary>+30% attack speed (shorter windup/cooldown cadence).</summary>
        Frenzied,

        /// <summary>+50% defense and posture resilience.</summary>
        Armored,

        /// <summary>Heals 25% of damage dealt to the player.</summary>
        Vampiric,

        /// <summary>Explodes on death after a 0.8s telegraph (damage capped at 25% of player maxHP).</summary>
        Volatile,

        /// <summary>Resists (ignores) the first status effect applied to it.</summary>
        Warded
    }

    /// <summary>
    /// Pure, deterministic elite resolution and stat math. No UnityEngine dependency in the
    /// decision/stat helpers so the whole contract is EditMode-testable (spec CA-3 evidence).
    /// Runtime behaviour is wired by <see cref="EnemyBrain"/>; the spawn decision is wired by
    /// <c>CaveEnemySpawnPlanner</c>.
    /// </summary>
    public static class EliteAffixRules
    {
        /// <summary>Elites only appear from this cave level onward (spec: "a partir do nível 6").</summary>
        public const int MinEliteCaveLevel = 6;

        /// <summary>Fraction of slots that become elite once <see cref="MinEliteCaveLevel"/> is reached (8%).</summary>
        public const float EliteSlotChance = 0.08f;

        /// <summary>Base stat bonus applied to every elite, on top of its affix (+25%).</summary>
        public const float EliteStatMultiplier = 1.25f;

        /// <summary>Frenzied attack-speed bonus (+30% → 0.7692x cadence on windup/cooldown).</summary>
        public const float FrenziedAttackSpeedMultiplier = 1.30f;

        /// <summary>Armored defence/posture bonus (+50%).</summary>
        public const float ArmoredDefenseMultiplier = 1.50f;

        /// <summary>Fraction of damage dealt that a Vampiric elite heals back (25%).</summary>
        public const float VampiricLifestealFraction = 0.25f;

        /// <summary>Volatile death-explosion telegraph length, seconds (player must be able to read it).</summary>
        public const float VolatileExplosionTelegraphSeconds = 0.8f;

        /// <summary>Volatile explosion damage cap as a fraction of the player's maxHP (anti one-shot).</summary>
        public const float VolatileExplosionMaxHpFraction = 0.25f;

        /// <summary>Essence/loot bonus an elite grants — F06 reads the flag to apply +25% essence.</summary>
        public const float EliteEssenceDropMultiplier = 1.25f;

        // The five named affixes in a fixed order. The order is part of the deterministic contract:
        // changing it would re-roll which affix a given seed/slot gets, breaking stable-run replays.
        private static readonly EliteAffix[] s_affixCycle =
        {
            EliteAffix.Frenzied,
            EliteAffix.Armored,
            EliteAffix.Vampiric,
            EliteAffix.Volatile,
            EliteAffix.Warded
        };

        /// <summary>
        /// Deterministic per-slot elite decision. Returns true when the slot should spawn an elite,
        /// with the chosen <paramref name="affix"/>. Below <see cref="MinEliteCaveLevel"/> it is always
        /// false. Uses the same FNV-1a <see cref="StableHash32.Compute"/> as the rest of
        /// the cave so the result is identical on every revisit of the same run (ADR-0005).
        /// </summary>
        /// <param name="caveWorldSeed">Stable world seed for the cave.</param>
        /// <param name="caveRunSeed">Stable run seed (only changes on new game/death/debug regen).</param>
        /// <param name="caveLevel">Cave level (depth).</param>
        /// <param name="slotIndex">Spawn slot index within the level plan.</param>
        /// <param name="enemyId">Enemy type id occupying the slot (keeps decision stable per enemy).</param>
        public static bool TryResolveElite(
            string caveWorldSeed,
            string caveRunSeed,
            int caveLevel,
            int slotIndex,
            string enemyId,
            out EliteAffix affix)
        {
            affix = EliteAffix.None;

            if (caveLevel < MinEliteCaveLevel)
            {
                return false;
            }

            // Salted seed source — the "|elite" salt keeps the elite roll independent of the spawn
            // plan / position rolls that use the same seed components elsewhere.
            var seedSource = $"{caveWorldSeed}|{caveRunSeed}|{caveLevel}|{slotIndex}|{enemyId}|elite";
            int hash = StableHash32.Compute(seedSource);
            uint roll = unchecked((uint)hash);

            // Map the hash into [0,10000) and compare against the 8% threshold.
            const int resolution = 10000;
            int bucket = (int)(roll % resolution);
            int threshold = (int)(EliteSlotChance * resolution); // 800
            if (bucket >= threshold)
            {
                return false;
            }

            // Choose the affix from a second, independent slice of the same hash so neighbouring
            // elites do not all share the first affix in the cycle.
            uint affixRoll = roll / (uint)resolution;
            affix = s_affixCycle[affixRoll % (uint)s_affixCycle.Length];
            return true;
        }

        /// <summary>Apply the flat +25% elite bonus to an integer stat (HP, defence baseline, ...).</summary>
        public static int ApplyEliteStatBonus(int baseStat)
        {
            if (baseStat <= 0)
            {
                return baseStat;
            }

            // Round to nearest so small stats still move (e.g. 10 -> 13, not 12 via truncation).
            return (int)System.Math.Round(baseStat * (double)EliteStatMultiplier, System.MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Final defence for an elite: +25% base bonus, then ×1.5 again if the affix is Armored.
        /// </summary>
        public static int ResolveEliteDefense(int baseDefense, EliteAffix affix)
        {
            int withBase = ApplyEliteStatBonus(baseDefense);
            if (affix != EliteAffix.Armored)
            {
                return withBase;
            }

            return (int)System.Math.Round(withBase * (double)ArmoredDefenseMultiplier, System.MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Cadence multiplier applied to windup/cooldown timers. Frenzied attacks 30% faster, i.e.
        /// timers are divided by 1.30 (returns 1/1.30 ≈ 0.769); every other affix leaves cadence at 1.
        /// </summary>
        public static float ResolveAttackCadenceFactor(EliteAffix affix)
        {
            return affix == EliteAffix.Frenzied ? 1f / FrenziedAttackSpeedMultiplier : 1f;
        }

        /// <summary>Lifesteal fraction for the affix (Vampiric heals 25% of damage dealt; others 0).</summary>
        public static float ResolveLifestealFraction(EliteAffix affix)
        {
            return affix == EliteAffix.Vampiric ? VampiricLifestealFraction : 0f;
        }

        /// <summary>Heal amount from lifesteal for a given damage dealt (rounded, never negative).</summary>
        public static int ResolveLifestealHeal(EliteAffix affix, int damageDealt)
        {
            if (damageDealt <= 0)
            {
                return 0;
            }

            float fraction = ResolveLifestealFraction(affix);
            if (fraction <= 0f)
            {
                return 0;
            }

            return (int)System.Math.Round(damageDealt * (double)fraction, System.MidpointRounding.AwayFromZero);
        }

        /// <summary>True if the affix explodes on death (Volatile).</summary>
        public static bool ExplodesOnDeath(EliteAffix affix) => affix == EliteAffix.Volatile;

        /// <summary>True if the affix resists the first status applied to it (Warded).</summary>
        public static bool ResistsFirstStatus(EliteAffix affix) => affix == EliteAffix.Warded;

        /// <summary>Volatile explosion damage, capped at 25% of the player's maxHP (anti one-shot).</summary>
        public static int ResolveVolatileExplosionDamage(int rawDamage, int playerMaxHp)
        {
            int safeRaw = rawDamage < 0 ? 0 : rawDamage;
            if (playerMaxHp <= 0)
            {
                return safeRaw;
            }

            int cap = (int)System.Math.Round(playerMaxHp * (double)VolatileExplosionMaxHpFraction, System.MidpointRounding.AwayFromZero);
            return safeRaw < cap ? safeRaw : cap;
        }

        /// <summary>Localized-ish display prefix for the floating label (e.g. "Frenzied Wisp").</summary>
        public static string ResolveDisplayPrefix(EliteAffix affix)
        {
            switch (affix)
            {
                case EliteAffix.Frenzied: return "Frenzied";
                case EliteAffix.Armored: return "Armored";
                case EliteAffix.Vampiric: return "Vampiric";
                case EliteAffix.Volatile: return "Volatile";
                case EliteAffix.Warded: return "Warded";
                default: return string.Empty;
            }
        }

        /// <summary>
        /// Build the elite display name by prefixing the base name (e.g. "Veilkin Scout" →
        /// "Frenzied Veilkin Scout"). Returns the base name unchanged for <see cref="EliteAffix.None"/>.
        /// </summary>
        public static string BuildEliteDisplayName(EliteAffix affix, string baseDisplayName)
        {
            string baseName = string.IsNullOrWhiteSpace(baseDisplayName) ? "Enemy" : baseDisplayName.Trim();
            string prefix = ResolveDisplayPrefix(affix);
            return string.IsNullOrEmpty(prefix) ? baseName : $"{prefix} {baseName}";
        }
    }
}
