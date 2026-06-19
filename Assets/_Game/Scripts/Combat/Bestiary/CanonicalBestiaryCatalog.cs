using System.Collections.Generic;

namespace CindarsHope.Combat.Bestiary
{
    /// <summary>
    /// fable_33 — canonical, code-side source of truth for the 64-headline (77 distinct fichas)
    /// creature catalog derived from CAVE_BESTIARY_CATALOG_DIRECTION_v1.0. One partial file per
    /// theme band (CanonicalBestiaryCatalog.BandStone.cs ... BandVoid.cs + BandFinalFour.cs), each
    /// returning its band's fichas in catalog order so the table is reviewable side-by-side with the
    /// design doc. No Unity references — consumed by the editor generator and by EditMode tests.
    ///
    /// Headline count note (catalog PARTE J): "60 band creatures + 4 finals = 64 fichas" is the
    /// catalog's framing. Materializing every distinct id in the band tables yields All.Count == 77,
    /// split as 50 commons + 14 minibosses + 9 gate bosses + 4 finals (commons + minibosses == the 64
    /// "band roster" headline). See CanonicalBestiaryCatalogCounts for the exact reconciliation tested.
    /// </summary>
    public static partial class CanonicalBestiaryCatalog
    {
        private static List<BestiaryCreatureDef> _all;

        /// <summary>All 77 canonical fichas (band creatures + minibosses + gate bosses + the Four).</summary>
        public static IReadOnlyList<BestiaryCreatureDef> All
        {
            get
            {
                if (_all != null)
                {
                    return _all;
                }

                var list = new List<BestiaryCreatureDef>(96);
                list.AddRange(BandStone());      // band 1
                list.AddRange(BandFungal());     // band 2
                list.AddRange(BandIce());        // band 3
                list.AddRange(BandFire());       // band 4
                list.AddRange(BandRuins());      // band 5
                list.AddRange(BandDeep());       // band 6
                list.AddRange(BandVoid());       // band 7
                list.AddRange(BandFinalFour());  // level 101 finale
                _all = list;
                return _all;
            }
        }

        // ── Band metadata (theme + inclusive level window + gate boss level) ─────────────────────
        // Used by the spawn-table builder; level windows match the catalog band headers.
        public struct BandInfo
        {
            public int Band;
            public string Theme;
            public int MinLevel;
            public int MaxLevel;
        }

        public static readonly IReadOnlyList<BandInfo> Bands = new[]
        {
            new BandInfo { Band = 1, Theme = "stone",  MinLevel = 1,  MaxLevel = 10 },
            new BandInfo { Band = 2, Theme = "fungal", MinLevel = 11, MaxLevel = 25 },
            new BandInfo { Band = 3, Theme = "ice",    MinLevel = 26, MaxLevel = 40 },
            new BandInfo { Band = 4, Theme = "fire",   MinLevel = 41, MaxLevel = 55 },
            new BandInfo { Band = 5, Theme = "ruins",  MinLevel = 56, MaxLevel = 70 },
            new BandInfo { Band = 6, Theme = "deep",   MinLevel = 71, MaxLevel = 85 },
            new BandInfo { Band = 7, Theme = "void",   MinLevel = 86, MaxLevel = 101 },
        };
    }

    /// <summary>
    /// Explicit, test-asserted count reconciliation for the catalog (keeps "64 headline vs 77 distinct"
    /// honest). The catalog's PARTE J headline "60 band creatures + 4 finals = 64" counts band ROWS
    /// excluding the gate bosses; materializing every distinct ficha in the band tables yields 77 ids.
    /// </summary>
    public static class CanonicalBestiaryCatalogCounts
    {
        public const int Commons = 50;          // band creatures that are neither miniboss nor boss
        public const int Minibosses = 14;       // 2 wandering minibosses per band (7 bands)
        public const int GateBosses = 9;        // one per 10-level gate; RUINS and VOID each have two
        public const int FinalFour = 4;         // the Four of level 101 (SpoilerTier 4)
        public const int BandRosterHeadline = Commons + Minibosses; // 64 = catalog "band creatures"
        public const int TotalDistinct = Commons + Minibosses + GateBosses + FinalFour; // 77
    }
}
