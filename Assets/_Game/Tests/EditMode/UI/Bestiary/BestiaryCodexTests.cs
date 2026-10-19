using System.Collections.Generic;
using CindarsHope.Bestiary;
using CindarsHope.Combat;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Enemy;
using CindarsHope.UI.Runtime.Screens;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.UI.Bestiary
{
    /// <summary>
    /// fable_45 — EditMode coverage for the codex projection (the single data path to the view).
    /// Validates: list grouping/ordering/tier-hiding (CA-1), ficha gated by F21 IsVisible per
    /// category (CA-2), completeness numerator/denominator + narration-only-with-identity (CA-3),
    /// filters, empty state, and the live catalog/narration consistency (no spoiler leak).
    /// </summary>
    [TestFixture]
    public class BestiaryCodexTests
    {
        // ── Synthetic catalog (deterministic, independent of the live 77-ficha table) ───────────

        private static BestiaryCreatureDef Def(
            string id, int band, string family, int tier, int minLevel = 1, bool boss = false)
        {
            return new BestiaryCreatureDef
            {
                EnemyId = id,
                DisplayName = id.Replace("enemy_", string.Empty),
                Band = band,
                MinLevel = minLevel,
                MaxLevel = minLevel + 4,
                Family = family,
                Role = EnemyRole.Chaser,
                Hp = 10,
                Damage = 1,
                SpoilerTier = tier,
                IsBoss = boss,
                PrimaryDropItemId = "item_material_chitin",
                PrimaryDamageTypeId = "fire",
                VulnerabilityMatrixProfileId = "vulnmatrix_test",
                Notes = "nota de " + id,
            };
        }

        private static List<BestiaryCreatureDef> SampleCatalog()
        {
            return new List<BestiaryCreatureDef>
            {
                Def("enemy_b_insect", 1, "Insect", 0, 2),
                Def("enemy_a_insect", 1, "Insect", 0, 1),
                Def("enemy_plant", 1, "Plant", 0, 1),
                Def("enemy_gateboss", 1, "Beast", 3, 10, boss: true),
                Def("enemy_void_thing", 7, "Aberration", 4, 101),
            };
        }

        /// <summary>A service that treats every supplied category id as unlocked + tier-resolved
        /// from the catalog, with an injectable flag set so tier 3+ gating is testable.</summary>
        private static EnemyKnowledgeService Knowledge(HashSet<string> flagsSet = null)
        {
            var service = new EnemyKnowledgeService
            {
                QuestFlagSource = flag => flagsSet != null && flagsSet.Contains(flag),
            };
            return service;
        }

        // ── CA-1 list: ordering, grouping, silhouette, tier hiding ───────────────────────────────

        [Test]
        public void Build_UndiscoveredCreature_IsSilhouetteWithUnknownLabel()
        {
            var k = Knowledge();
            var model = BestiaryCodexProjection.Build(SampleCatalog(), k);

            var item = FindItem(model, "enemy_a_insect");
            Assert.IsNotNull(item, "tier-0 entries are always present (silhouette when undiscovered)");
            Assert.IsFalse(item.IsDiscovered);
            Assert.IsTrue(item.IsSilhouette);
            Assert.AreEqual(BestiaryCodexProjection.UnknownLabel, item.DisplayName);
        }

        [Test]
        public void Build_DiscoveredCreature_ShowsName()
        {
            var k = Knowledge();
            k.RecordSighting("enemy_a_insect"); // unlocks Identity (tier 0 → visible)
            var model = BestiaryCodexProjection.Build(SampleCatalog(), k);

            var item = FindItem(model, "enemy_a_insect");
            Assert.IsTrue(item.IsDiscovered);
            Assert.IsFalse(item.IsSilhouette);
            Assert.AreEqual("a_insect", item.DisplayName);
        }

        [Test]
        public void Build_TierLockedCreature_AbsentFromList()
        {
            var k = Knowledge(); // no flags set
            k.RecordSighting("enemy_gateboss");
            k.RecordSighting("enemy_void_thing");
            var model = BestiaryCodexProjection.Build(SampleCatalog(), k);

            Assert.IsNull(FindItem(model, "enemy_gateboss"), "tier 3 with no flag must be absent");
            Assert.IsNull(FindItem(model, "enemy_void_thing"), "tier 4 with no flag must be absent");
        }

        [Test]
        public void Build_TierLockedCreature_AppearsOnceFlagSet()
        {
            var flags = new HashSet<string> { "flag_bestiary_gate_boss_revealed" };
            var k = Knowledge(flags);
            k.RecordSighting("enemy_gateboss"); // unlock identity
            var model = BestiaryCodexProjection.Build(SampleCatalog(), k);

            var item = FindItem(model, "enemy_gateboss");
            Assert.IsNotNull(item, "tier 3 with its flag set + discovered must appear");
            Assert.IsTrue(item.IsDiscovered);
        }

        [Test]
        public void Build_GroupsAreStableByBandThenFamily()
        {
            var k = Knowledge();
            var model = BestiaryCodexProjection.Build(SampleCatalog(), k);

            // Only tier 0 entries are visible (3 of them): band 1 Insect (2 entries) then band 1 Plant.
            Assert.AreEqual(2, model.Groups.Count);
            Assert.AreEqual("Insect", model.Groups[0].Family);
            Assert.AreEqual("Plant", model.Groups[1].Family);

            // Within the Insect group, ordering is by min-level (a=1 before b=2).
            Assert.AreEqual("enemy_a_insect", model.Groups[0].Entries[0].EnemyId);
            Assert.AreEqual("enemy_b_insect", model.Groups[0].Entries[1].EnemyId);
        }

        // ── CA-2 ficha gated by category ─────────────────────────────────────────────────────────

        [Test]
        public void BuildFicha_OnlyVisibleCategoriesRenderContent()
        {
            var def = Def("enemy_a_insect", 1, "Insect", 0, 1);
            var k = Knowledge();
            k.RecordSighting("enemy_a_insect"); // Identity only

            var ficha = BestiaryCodexProjection.BuildFicha(def, k);
            Assert.IsTrue(ficha.IsIdentityKnown);
            Assert.AreNotEqual(BestiaryCodexProjection.UnknownLabel, ficha.IdentityLine);
            // Drops not unlocked yet (needs 5 kills) → "???".
            Assert.AreEqual(BestiaryCodexProjection.UnknownLabel, ficha.DropsLine);
        }

        [Test]
        public void BuildFicha_DropsAppearAfterKillThreshold()
        {
            var def = Def("enemy_a_insect", 1, "Insect", 0, 1);
            var k = Knowledge();
            for (int i = 0; i < EnemyKnowledgeService.KillsForCommonDrops; i++)
            {
                k.RecordKill("enemy_a_insect");
            }

            var ficha = BestiaryCodexProjection.BuildFicha(def, k);
            Assert.AreEqual("item_material_chitin", ficha.DropsLine, "5 kills unlock DropsCommon");
            Assert.AreEqual(EnemyKnowledgeService.KillsForCommonDrops, ficha.DefeatCount);
        }

        [Test]
        public void BuildFicha_TierLockedReturnsNull()
        {
            var def = Def("enemy_void_thing", 7, "Aberration", 4, 101);
            var k = Knowledge(); // no flag
            k.RecordSighting("enemy_void_thing");

            Assert.IsNull(BestiaryCodexProjection.BuildFicha(def, k));
        }

        // ── CA-3 narration + completeness ────────────────────────────────────────────────────────

        [Test]
        public void BuildFicha_NarrationOnlyWithIdentity()
        {
            var def = Def("enemy_a_insect", 1, "Insect", 0, 1);

            var undiscovered = BestiaryCodexProjection.BuildFicha(def, Knowledge());
            Assert.AreEqual(string.Empty, undiscovered.NarrationText, "no narration before identity");

            var k = Knowledge();
            k.RecordSighting("enemy_a_insect");
            var discovered = BestiaryCodexProjection.BuildFicha(def, k);
            Assert.IsFalse(string.IsNullOrEmpty(discovered.NarrationText), "narration appears with identity");
        }

        [Test]
        public void Completeness_NumeratorAndDenominatorExcludeTierLocked()
        {
            var k = Knowledge();
            k.RecordSighting("enemy_a_insect"); // seen
            var model = BestiaryCodexProjection.Build(SampleCatalog(), k);

            // Visible total = 3 tier-0 entries (the gate boss + void are tier-locked, excluded).
            Assert.AreEqual(3, model.VisibleTotal);
            Assert.AreEqual(1, model.SeenCount);
            Assert.AreEqual(0, model.DocumentedCount);
            StringAssert.Contains("Vistos 1/3", model.CompletenessLabel);
        }

        [Test]
        public void Completeness_DocumentedRequiresAllCoreCategories()
        {
            var k = Knowledge();
            // Grant all four core categories via external grant (idempotent).
            foreach (var cat in BestiaryCodexProjection.CoreCategories)
            {
                k.GrantKnowledge("enemy_a_insect", cat, "test");
            }

            var model = BestiaryCodexProjection.Build(SampleCatalog(), k);
            Assert.AreEqual(1, model.DocumentedCount);

            var ficha = BestiaryCodexProjection.BuildFicha(Def("enemy_a_insect", 1, "Insect", 0, 1), k);
            Assert.IsTrue(ficha.IsDocumented);
            Assert.IsFalse(string.IsNullOrEmpty(ficha.DocumentedBonusLine), "documented shows the +dmg bonus line");
        }

        // ── Filters + empty state ────────────────────────────────────────────────────────────────

        [Test]
        public void Filter_Seen_OnlyDiscoveredEntries()
        {
            var k = Knowledge();
            k.RecordSighting("enemy_a_insect");
            var model = BestiaryCodexProjection.Build(SampleCatalog(), k, BestiaryCodexProjection.CodexFilter.Seen);

            var flat = FlattenIds(model);
            Assert.AreEqual(1, flat.Count);
            Assert.Contains("enemy_a_insect", flat);
        }

        [Test]
        public void Filter_Family_OnlyMatchingFamily()
        {
            var k = Knowledge();
            var model = BestiaryCodexProjection.Build(
                SampleCatalog(), k, BestiaryCodexProjection.CodexFilter.Family, "Plant");

            var flat = FlattenIds(model);
            Assert.AreEqual(1, flat.Count);
            Assert.Contains("enemy_plant", flat);
        }

        [Test]
        public void EmptyState_NothingDiscovered_IsEmptyTrue()
        {
            var model = BestiaryCodexProjection.Build(SampleCatalog(), Knowledge());
            Assert.IsTrue(model.IsEmpty, "no creature discovered → empty codex");
            Assert.AreEqual(BestiaryCodexProjection.EmptyStateMessage, model.EmptyMessage);
        }

        // ── Live catalog + narration consistency (real F33 data, anti-spoiler) ───────────────────

        [Test]
        public void LiveCatalog_NarrationTableCoversHeadlineCount()
        {
            // Every band creature has authored narration (the "64 textos presentes" evidence).
            Assert.GreaterOrEqual(BestiaryNarrationTable.NarrationCount,
                CanonicalBestiaryCatalogCounts.BandRosterHeadline);
        }

        [Test]
        public void LiveCatalog_NoTier4LeaksWithoutFlag()
        {
            var k = new EnemyKnowledgeService { QuestFlagSource = _ => false };
            // Discover everything we can; tier 4 must still be absent.
            foreach (var def in CanonicalBestiaryCatalog.All)
            {
                k.RecordSighting(def.EnemyId);
            }

            var model = BestiaryCodexProjection.Build(CanonicalBestiaryCatalog.All, k);
            foreach (var group in model.Groups)
            {
                foreach (var item in group.Entries)
                {
                    Assert.AreNotEqual("boss_ithryndor", item.EnemyId, "the Four must never appear without the tier-4 flag");
                    Assert.AreNotEqual("boss_archivist_of_silence", item.EnemyId);
                }
            }
        }

        // ── helpers ──────────────────────────────────────────────────────────────────────────────

        private static BestiaryCodexListItem FindItem(BestiaryCodexModel model, string id)
        {
            foreach (var group in model.Groups)
            {
                foreach (var item in group.Entries)
                {
                    if (item.EnemyId == id)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        private static List<string> FlattenIds(BestiaryCodexModel model)
        {
            var ids = new List<string>();
            foreach (var group in model.Groups)
            {
                foreach (var item in group.Entries)
                {
                    ids.Add(item.EnemyId);
                }
            }

            return ids;
        }
    }
}
