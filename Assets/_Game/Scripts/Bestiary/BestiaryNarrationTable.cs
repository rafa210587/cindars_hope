using System.Collections.Generic;
using CindarsHope.Combat.Bestiary;

namespace CindarsHope.Bestiary
{
    /// <summary>
    /// fable_45 — static enemyId→narration table for the codex ficha (CA-3). The catalog narration
    /// already lives in code as <see cref="BestiaryCreatureDef.Notes"/> (the authored per-creature
    /// text from CAVE_BESTIARY_CATALOG_DIRECTION_v1.0, written by F33). Rather than duplicate those
    /// 77 strings — or extend the Unity editor generator to bake a NarrationText field onto every
    /// EnemyDataSO .asset (which would require running the Unity Editor for asset evidence, blocked
    /// in this environment) — this table is the single, pure-C# narration source built directly from
    /// the catalog. One source of truth: the catalog. Decision documented in the execution report
    /// (Fase 0: gerador de asset inviável sem Unity → tabela estática enemyId→texto, regra de não
    /// duplicação respeitada, EnemyDataSO NÃO alterado).
    ///
    /// An optional <see cref="Overrides"/> map lets future lore passes give a creature richer
    /// narration than its mechanical Notes without touching the catalog; empty by default.
    /// Pure C# (no Unity refs) so the narration-gating rules are EditMode-testable.
    /// </summary>
    public static class BestiaryNarrationTable
    {
        // Future authored-lore overrides (enemyId → narration). Empty in v1: the catalog Notes are
        // the canonical text. Kept as the documented extension point (spec "tabela estática").
        private static readonly Dictionary<string, string> Overrides = new Dictionary<string, string>();

        private static Dictionary<string, string> _byId;

        /// <summary>
        /// Narration for a creature, or empty string when the id is unknown/blank. Never returns null.
        /// Overrides win over the catalog Notes.
        /// </summary>
        public static string GetNarration(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return string.Empty;
            }

            EnsureBuilt();
            return _byId.TryGetValue(enemyId, out var text) ? text : string.Empty;
        }

        /// <summary>True when the table has a non-empty narration for the id (codex completeness aid).</summary>
        public static bool HasNarration(string enemyId)
        {
            return !string.IsNullOrEmpty(GetNarration(enemyId));
        }

        /// <summary>Number of distinct creature ids with non-empty narration (asserted in tests = 64+).</summary>
        public static int NarrationCount
        {
            get
            {
                EnsureBuilt();
                int count = 0;
                foreach (var text in _byId.Values)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private static void EnsureBuilt()
        {
            if (_byId != null)
            {
                return;
            }

            var map = new Dictionary<string, string>();
            foreach (var def in CanonicalBestiaryCatalog.All)
            {
                if (string.IsNullOrWhiteSpace(def.EnemyId))
                {
                    continue;
                }

                string text = Overrides.TryGetValue(def.EnemyId, out var over) && !string.IsNullOrWhiteSpace(over)
                    ? over
                    : def.Notes ?? string.Empty;
                map[def.EnemyId] = text;
            }

            _byId = map;
        }
    }
}
