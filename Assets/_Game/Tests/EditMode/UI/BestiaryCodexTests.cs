using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Bestiary;
using CindarsHope.Combat;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Enemy;
using CindarsHope.UI.Runtime.Screens;

namespace CindarsHope.Tests.EditMode.UI
{
    /// <summary>
    /// fable_45 — EditMode tests for the PURE bestiary codex projection (no Unity/scene/canvas).
    /// Covers the spec acceptance criteria that are deterministic logic:
    /// CA-1 progressive discovery (silhouette/"???"/tier hidden/grouping),
    /// CA-2 ficha gated per category (IsVisible is the single source; drops appear after 5 kills),
    /// CA-3 narration only with identity + completeness numerator/denominator,
    /// plus filters and empty state. The Canvas/keyboard contract (CA-4) is the human scenario.
    /// </summary>
    [TestFixture]
    public class BestiaryCodexTests
    {
        // ---------------------------------------------------------------- synthetic catalog

        private static BestiaryCreatureDef Def(
            string id, string name, int band, string family, int tier,
            int minLevel = 1, int maxLevel = 10)
        {
            return new BestiaryCreatureDef
            {
                EnemyId = id,
                DisplayName = name,
                Band = band,
                Family = family,
                SpoilerTier = tier,
                MinLevel = minLevel,
                MaxLevel = maxLevel,
                Role = EnemyRole.Chaser,
                PrimaryDamageTypeId = "fire",
                PrimaryDropItemId = "item_material_stone",
                VulnerabilityMatrixProfileId = "profile_beast",
                Notes = "guarda o ninho",
            };
        }

        // A small deterministic catalog: 2 tier-0 commons (same band/family), 1 tier-1 common in a
        // second family, 1 tier-3 gate boss (hidden until flag), 1 tier-4 final (hidden until flag).
        private static IReadOnlyList<BestiaryCreatureDef> SampleCatalog()
        {
            return new List<BestiaryCreatureDef>
            {
                Def("enemy_a", "Pedrinho", 1, "Beast", 0),
                Def("enemy_b", "Pedrão", 1, "Beast", 0),
                Def("enemy_c", "Fungo", 2, "Plant", 1, 11, 25),
                Def("enemy_gate", "Guardião", 5, "Construct", 3, 56, 70),
                Def("enemy_four", "O Vazio", 8, "Aberration", 4, 101, 101),
            };
        }

        // A knowledge service whose tier gate uses the def's own SpoilerTier passed by the projection,
        // and whose flags can be toggled for the tier-3/4 reveal tests.
        private static EnemyKnowledgeService Knowledge(
            HashSet<string> setFlags = null)
        {
            var k = new EnemyKnowledgeService();
            var flags = setFlags ?? new HashSet<string>();
            k.QuestFlagSource = id => flags.Contains(id);
            // tier 3 → gate flag, tier 4 → final flag (matches service default convention).
            k.SpoilerTierFlagSource = tier =>
                tier == 3 ? "flag_gate" : tier == 4 ? "flag_final" : null;
            return k;
        }

        // ---------------------------------------------------------------- CA-1: progressive discovery

        [Test]
        public void Build_NothingDiscovered_TierLockedEntriesAbsent_CommonsAreSilhouettes()
        {
            var model = BestiaryCodexProjection.Build(SampleCatalog(), Knowledge());

            // Tier 3/4 entries are absent entirely (not even silhouettes) — anti-spoiler §20.
            var ids = FlatIds(model);
            CollectionAssert.DoesNotContain(ids, "enemy_gate");
            CollectionAssert.DoesNotContain(ids, "enemy_four");

            // The 3 visible commons appear, all as silhouettes ("???") since none discovered.
            Assert.AreEqual(3, model.VisibleTotal);
            foreach (var item in FlatItems(model))
            {
                Assert.IsTrue(item.IsSilhouette, $"{item.EnemyId} should be silhouette");
                Assert.AreEqual(BestiaryCodexProjection.UnknownLabel, item.DisplayName);
            }

            // Denominator excludes tier-locked entries (3, not 5).
            StringAssert.Contains("Vistos 0/3", model.CompletenessLabel);
            Assert.IsTrue(model.IsEmpty);
        }

        [Test]
        public void Build_DiscoveredCreature_ShowsName_AndGroupsByBandFamily()
        {
            var k = Knowledge();
            // Identity unlocks at 1 sighting for a tier-0 common.
            k.RecordSighting("enemy_a");

            var model = BestiaryCodexProjection.Build(SampleCatalog(), k);

            var a = FindItem(model, "enemy_a");
            Assert.IsNotNull(a);
            Assert.IsFalse(a.IsSilhouette);
            Assert.AreEqual("Pedrinho", a.DisplayName);

            // enemy_a and enemy_b are band 1 / family Beast → same group; enemy_c is its own group.
            var beastGroup = model.Groups.Find(g => g.Band == 1 && g.Family == "Beast");
            Assert.IsNotNull(beastGroup);
            Assert.AreEqual(2, beastGroup.Entries.Count);
            Assert.AreEqual("Beast", beastGroup.Entries[0].Family);
        }

        [Test]
        public void Build_TierThreeBoss_AppearsOnlyAfterDiscoveredAndFlagSet()
        {
            var flags = new HashSet<string>();
            var k = Knowledge(flags);

            // Even after recording a sighting, with no flag the tier-3 boss stays hidden.
            k.RecordSighting("enemy_gate");
            var hidden = BestiaryCodexProjection.Build(SampleCatalog(), k);
            CollectionAssert.DoesNotContain(FlatIds(hidden), "enemy_gate");

            // Once the gate flag is set, the (now identity-visible) boss surfaces and is documented-gated.
            flags.Add("flag_gate");
            var revealed = BestiaryCodexProjection.Build(SampleCatalog(), k);
            CollectionAssert.Contains(FlatIds(revealed), "enemy_gate");
            Assert.AreEqual(4, revealed.VisibleTotal); // 3 commons + the gate boss
        }

        // ---------------------------------------------------------------- CA-2: ficha gated per category

        [Test]
        public void BuildFicha_LockedCategories_RenderUnknownLabel_NeverRealData()
        {
            var k = Knowledge();
            k.RecordSighting("enemy_a"); // only Identity unlocked

            var def = Find(SampleCatalog(), "enemy_a");
            var ficha = BestiaryCodexProjection.BuildFicha(def, k);

            Assert.IsNotNull(ficha);
            Assert.IsTrue(ficha.IsIdentityKnown);
            Assert.AreEqual("Pedrinho", ficha.DisplayName);

            // Identity line is real; everything else gated stays "???".
            StringAssert.Contains("Beast", ficha.IdentityLine);
            Assert.AreEqual(BestiaryCodexProjection.UnknownLabel, ficha.BehaviorLine);
            Assert.AreEqual(BestiaryCodexProjection.UnknownLabel, ficha.VulnerabilityLine);
            Assert.AreEqual(BestiaryCodexProjection.UnknownLabel, ficha.DropsLine);
            // The real drop id must NOT leak through the locked line.
            Assert.AreNotEqual("item_material_stone", ficha.DropsLine);
        }

        [Test]
        public void BuildFicha_DropsAppearAfterFiveKills_OnRebind()
        {
            var k = Knowledge();
            k.RecordSighting("enemy_a");

            var def = Find(SampleCatalog(), "enemy_a");
            Assert.AreEqual(BestiaryCodexProjection.UnknownLabel,
                BestiaryCodexProjection.BuildFicha(def, k).DropsLine);

            // 5 kills → DropsCommon unlocks (F21 threshold).
            for (int i = 0; i < EnemyKnowledgeService.KillsForCommonDrops; i++)
            {
                k.RecordKill("enemy_a");
            }

            var ficha = BestiaryCodexProjection.BuildFicha(def, k);
            Assert.AreEqual("item_material_stone", ficha.DropsLine);
            Assert.AreEqual(5, ficha.DefeatCount);
            StringAssert.Contains("Derrotas: 5", ficha.DefeatCountLine);
        }

        [Test]
        public void BuildFicha_TierLockedCreature_ReturnsNull()
        {
            var k = Knowledge(); // no flags
            var def = Find(SampleCatalog(), "enemy_four"); // tier 4
            Assert.IsNull(BestiaryCodexProjection.BuildFicha(def, k));
        }

        // ---------------------------------------------------------------- CA-3: narration + completeness

        [Test]
        public void BuildFicha_Narration_OnlyWithIdentityDiscovered()
        {
            // Use a real catalog creature so the narration table actually has text for it.
            var def = CanonicalBestiaryCatalog.All[0];
            var k = Knowledge();

            // Undiscovered → no narration.
            var blind = BestiaryCodexProjection.BuildFicha(def, k);
            Assert.AreEqual(string.Empty, blind.NarrationText);

            // Discover identity → narration (the catalog Notes) appears.
            k.RecordSighting(def.EnemyId);
            var seen = BestiaryCodexProjection.BuildFicha(def, k);
            Assert.AreEqual(BestiaryNarrationTable.GetNarration(def.EnemyId), seen.NarrationText);
        }

        [Test]
        public void NarrationTable_HasTextForEveryCatalogCreature()
        {
            // CA-3 evidence: 64+ authored narration strings present (sourced from the catalog Notes).
            Assert.GreaterOrEqual(BestiaryNarrationTable.NarrationCount, 64);

            foreach (var def in CanonicalBestiaryCatalog.All)
            {
                Assert.IsTrue(BestiaryNarrationTable.HasNarration(def.EnemyId),
                    $"missing narration for {def.EnemyId}");
            }
        }

        [Test]
        public void Build_Completeness_NumeratorAndDenominatorTrackKnowledge()
        {
            var k = Knowledge();
            // Discover two of the three visible commons; fully document one of them.
            k.RecordSighting("enemy_a");
            k.RecordSighting("enemy_b");

            // Document enemy_a: behavior + vulnerability + drops (identity already set).
            k.RecordActionSeen("enemy_a", "bite", suffered: true);            // behavior
            for (int i = 0; i < EnemyKnowledgeService.EffectiveHitsForVulnerability; i++)
                k.RecordEffectiveHit("enemy_a", "fire");                       // vulnerability
            for (int i = 0; i < EnemyKnowledgeService.KillsForCommonDrops; i++)
                k.RecordKill("enemy_a");                                       // drops

            var model = BestiaryCodexProjection.Build(SampleCatalog(), k);

            Assert.AreEqual(2, model.SeenCount);
            Assert.AreEqual(1, model.DocumentedCount);
            Assert.AreEqual(3, model.VisibleTotal);
            StringAssert.Contains("Vistos 2/3", model.CompletenessLabel);
            StringAssert.Contains("Documentados 1/3", model.CompletenessLabel);
        }

        [Test]
        public void BuildFicha_DocumentedBonusLine_OnlyWhenFullyDocumented()
        {
            var k = Knowledge();
            k.RecordSighting("enemy_a");
            var def = Find(SampleCatalog(), "enemy_a");
            Assert.AreEqual(string.Empty, BestiaryCodexProjection.BuildFicha(def, k).DocumentedBonusLine);

            k.RecordActionSeen("enemy_a", "bite", suffered: true);
            for (int i = 0; i < EnemyKnowledgeService.EffectiveHitsForVulnerability; i++)
                k.RecordEffectiveHit("enemy_a", "fire");
            for (int i = 0; i < EnemyKnowledgeService.KillsForCommonDrops; i++)
                k.RecordKill("enemy_a");

            var ficha = BestiaryCodexProjection.BuildFicha(def, k);
            Assert.IsTrue(ficha.IsDocumented);
            StringAssert.Contains("Documentado", ficha.DocumentedBonusLine);
        }

        // ---------------------------------------------------------------- filters + empty state

        [Test]
        public void Build_SeenFilter_KeepsOnlyDiscovered()
        {
            var k = Knowledge();
            k.RecordSighting("enemy_a");

            var model = BestiaryCodexProjection.Build(
                SampleCatalog(), k, BestiaryCodexProjection.CodexFilter.Seen);

            var ids = FlatIds(model);
            CollectionAssert.Contains(ids, "enemy_a");
            CollectionAssert.DoesNotContain(ids, "enemy_b");
        }

        [Test]
        public void Build_FamilyFilter_KeepsOnlyMatchingFamily()
        {
            var k = Knowledge();
            k.RecordSighting("enemy_a");
            k.RecordSighting("enemy_c");

            var model = BestiaryCodexProjection.Build(
                SampleCatalog(), k, BestiaryCodexProjection.CodexFilter.Family, "Plant");

            var ids = FlatIds(model);
            CollectionAssert.Contains(ids, "enemy_c");
            CollectionAssert.DoesNotContain(ids, "enemy_a");
        }

        [Test]
        public void Build_NullEntries_ReturnsEmptyModel_NoThrow()
        {
            var model = BestiaryCodexProjection.Build(null, Knowledge());
            Assert.AreEqual(0, model.VisibleTotal);
            Assert.IsTrue(model.IsEmpty);
            Assert.AreEqual(0, model.Groups.Count);
        }

        [Test]
        public void EmptyStateMessage_IsCanonical()
        {
            var model = BestiaryCodexProjection.Build(SampleCatalog(), Knowledge());
            Assert.AreEqual("Nenhuma criatura observada ainda.", model.EmptyMessage);
        }

        // ---------------------------------------------------------------- view focus order (pure)

        [Test]
        public void BuildFocusOrder_EmptyModel_SkipsListFocus()
        {
            var empty = BestiaryCodexProjection.Build(SampleCatalog(), Knowledge());
            var order = BestiaryScreenView.BuildFocusOrder(empty);
            CollectionAssert.DoesNotContain(order, BestiaryScreenView.FocusList);
            CollectionAssert.Contains(order, BestiaryScreenView.FocusClose);
        }

        [Test]
        public void BuildFocusOrder_NonEmptyModel_IncludesListFirst()
        {
            var k = Knowledge();
            k.RecordSighting("enemy_a");
            var model = BestiaryCodexProjection.Build(SampleCatalog(), k);
            var order = BestiaryScreenView.BuildFocusOrder(model);
            Assert.AreEqual(BestiaryScreenView.FocusList, order[0]);
        }

        // ---------------------------------------------------------------- helpers

        private static BestiaryCreatureDef Find(IReadOnlyList<BestiaryCreatureDef> cat, string id)
        {
            foreach (var d in cat)
            {
                if (d.EnemyId == id) return d;
            }

            throw new AssertionException($"def {id} not in sample catalog");
        }

        private static List<BestiaryCodexListItem> FlatItems(BestiaryCodexModel model)
        {
            var flat = new List<BestiaryCodexListItem>();
            foreach (var g in model.Groups) flat.AddRange(g.Entries);
            return flat;
        }

        private static List<string> FlatIds(BestiaryCodexModel model)
        {
            var ids = new List<string>();
            foreach (var item in FlatItems(model)) ids.Add(item.EnemyId);
            return ids;
        }

        private static BestiaryCodexListItem FindItem(BestiaryCodexModel model, string id)
        {
            foreach (var item in FlatItems(model))
            {
                if (item.EnemyId == id) return item;
            }

            return null;
        }
    }
}
