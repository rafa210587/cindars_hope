using System.Linq;
using CindarsHope.Editor.Validation;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Tooling
{
    // fable_30 — EditMode coverage for the PURE catalog cross-ref engine.
    //
    // These tests drive CatalogConsistencyEngine with synthetic snapshots (fake assets +
    // fake expectations) so every severity (ERROR/WARN/INFO/SKIPPED) and each of the 8
    // checks (a)-(h) is exercised without AssetDatabase or Play Mode.
    //
    // The engine lives in the Editor assembly (Assembly-CSharp-Editor); this test compiles
    // there too (the Editor csproj references nunit.framework + TestRunner).
    [TestFixture]
    public class CatalogValidatorTests
    {
        private static CategorySnapshot GeneratedCategory(CatalogCategory category, params string[] ids)
        {
            return new CategorySnapshot
            {
                Category = category,
                Generated = true,
                PresentIds = new System.Collections.Generic.HashSet<string>(ids),
                ActualCount = ids.Length
            };
        }

        private static CategorySnapshot NotGeneratedCategory(CatalogCategory category)
        {
            return new CategorySnapshot { Category = category, Generated = false };
        }

        // ── (a) canonical id without asset = ERROR ───────────────────────────────
        [Test]
        public void MissingCanonicalId_IsError()
        {
            var snapshot = new CatalogSnapshot
            {
                Skills = GeneratedCategory(CatalogCategory.Skills, "skill_a")
            };
            var expectations = new CatalogExpectationSet();
            expectations.Set(CatalogCategory.Skills, new CatalogExpectation
            {
                ExpectedCount = 2,
                CanonicalIds = new System.Collections.Generic.HashSet<string> { "skill_a", "skill_b" }
            });

            var result = CatalogConsistencyEngine.Validate(snapshot, expectations);

            Assert.IsTrue(result.HasErrors, "Missing canonical id should produce an ERROR.");
            Assert.IsTrue(result.Findings.Any(f =>
                f.Severity == CatalogSeverity.Error &&
                f.CheckId == "(a)-missing-canonical" &&
                f.InvolvedIds.Contains("skill_b")));
        }

        // ── (b) asset without canonical id = WARN ────────────────────────────────
        [Test]
        public void UndocumentedAsset_IsWarn()
        {
            var snapshot = new CatalogSnapshot
            {
                Skills = GeneratedCategory(CatalogCategory.Skills, "skill_a", "skill_extra")
            };
            var expectations = new CatalogExpectationSet();
            expectations.Set(CatalogCategory.Skills, new CatalogExpectation
            {
                ExpectedCount = 1,
                CanonicalIds = new System.Collections.Generic.HashSet<string> { "skill_a" }
            });

            var result = CatalogConsistencyEngine.Validate(snapshot, expectations);

            Assert.IsFalse(result.HasErrors, "Undocumented asset must not be an ERROR.");
            Assert.IsTrue(result.Findings.Any(f =>
                f.Severity == CatalogSeverity.Warn &&
                f.CheckId == "(b)-undocumented-asset" &&
                f.InvolvedIds.Contains("skill_extra")));
        }

        // ── (c) enemy drop -> missing item = ERROR (CA-1) ────────────────────────
        [Test]
        public void EnemyDropToMissingItem_IsError()
        {
            var snapshot = new CatalogSnapshot
            {
                Items = GeneratedCategory(CatalogCategory.Items, "item_wood"),
                Bestiary = GeneratedCategory(CatalogCategory.Bestiary, "enemy_slime")
            };
            snapshot.EnemyDrops.Add(new EnemyDropSnapshot { EnemyId = "enemy_slime", DropItemId = "item_ghost" });

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsTrue(result.HasErrors);
            var finding = result.Findings.Single(f => f.CheckId == "(c)-drop-to-item");
            Assert.AreEqual(CatalogSeverity.Error, finding.Severity);
            Assert.IsTrue(finding.InvolvedIds.Contains("enemy_slime"));
            Assert.IsTrue(finding.InvolvedIds.Contains("item_ghost"));
        }

        [Test]
        public void EnemyDropToExistingItem_IsClean()
        {
            var snapshot = new CatalogSnapshot
            {
                Items = GeneratedCategory(CatalogCategory.Items, "item_wood"),
                Bestiary = GeneratedCategory(CatalogCategory.Bestiary, "enemy_slime")
            };
            snapshot.EnemyDrops.Add(new EnemyDropSnapshot { EnemyId = "enemy_slime", DropItemId = "item_wood" });

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsFalse(result.Findings.Any(f =>
                f.CheckId == "(c)-drop-to-item" && f.Severity == CatalogSeverity.Error));
        }

        // ── (d) quest reward -> missing item / skill = ERROR ─────────────────────
        [Test]
        public void QuestRewardToMissingItem_IsError()
        {
            var snapshot = new CatalogSnapshot
            {
                Items = GeneratedCategory(CatalogCategory.Items, "item_wood"),
                Quests = GeneratedCategory(CatalogCategory.Quests, "quest_a")
            };
            snapshot.QuestRewards.Add(new QuestRewardSnapshot
            {
                QuestId = "quest_a", RewardId = "reward_1", RewardKind = "item", TargetId = "item_ghost"
            });

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsTrue(result.Findings.Any(f =>
                f.CheckId == "(d)-quest-reward" &&
                f.Severity == CatalogSeverity.Error &&
                f.InvolvedIds.Contains("item_ghost")));
        }

        [Test]
        public void QuestRewardToMissingSkill_IsError()
        {
            var snapshot = new CatalogSnapshot
            {
                Skills = GeneratedCategory(CatalogCategory.Skills, "skill_a"),
                Quests = GeneratedCategory(CatalogCategory.Quests, "quest_a")
            };
            snapshot.QuestRewards.Add(new QuestRewardSnapshot
            {
                QuestId = "quest_a", RewardId = "reward_1", RewardKind = "skill", TargetId = "skill_ghost"
            });

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsTrue(result.Findings.Any(f =>
                f.CheckId == "(d)-quest-reward" &&
                f.Severity == CatalogSeverity.Error &&
                f.InvolvedIds.Contains("skill_ghost")));
        }

        // ── (e) recipe -> missing ingredient = ERROR ─────────────────────────────
        [Test]
        public void RecipeToMissingIngredient_IsError()
        {
            var snapshot = new CatalogSnapshot
            {
                Items = GeneratedCategory(CatalogCategory.Items, "item_wood"),
                Recipes = GeneratedCategory(CatalogCategory.Recipes, "recipe_a")
            };
            snapshot.RecipeIngredients.Add(new RecipeIngredientSnapshot
            {
                RecipeId = "recipe_a", IngredientItemId = "item_ghost"
            });

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsTrue(result.Findings.Any(f =>
                f.CheckId == "(e)-recipe-to-ingredient" &&
                f.Severity == CatalogSeverity.Error &&
                f.InvolvedIds.Contains("item_ghost")));
        }

        // ── (f) shop entry -> missing item = ERROR ───────────────────────────────
        [Test]
        public void ShopEntryToMissingItem_IsError()
        {
            var snapshot = new CatalogSnapshot
            {
                Items = GeneratedCategory(CatalogCategory.Items, "item_wood"),
                Shops = GeneratedCategory(CatalogCategory.Shops, "shop_a")
            };
            snapshot.ShopEntries.Add(new ShopEntrySnapshot { ShopId = "shop_a", ItemId = "item_ghost" });

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsTrue(result.Findings.Any(f =>
                f.CheckId == "(f)-shop-to-item" &&
                f.Severity == CatalogSeverity.Error &&
                f.InvolvedIds.Contains("item_ghost")));
        }

        // ── (g) active skill -> invalid executor = ERROR; dormant = INFO ─────────
        [Test]
        public void ActiveSkillToUnknownExecutor_IsError()
        {
            var snapshot = new CatalogSnapshot
            {
                Skills = GeneratedCategory(CatalogCategory.Skills, "skill_a")
            };
            snapshot.ActiveSkills.Add(new ActiveSkillSnapshot { SkillNodeId = "skill_a", ActionOrEffectId = "fx_unknown" });

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsTrue(result.Findings.Any(f =>
                f.CheckId == "(g)-active-skill-executor" &&
                f.Severity == CatalogSeverity.Error &&
                f.InvolvedIds.Contains("fx_unknown")));
        }

        [Test]
        public void ActiveSkillToDormantFlag_IsInfo()
        {
            var snapshot = new CatalogSnapshot
            {
                Skills = GeneratedCategory(CatalogCategory.Skills, "skill_a")
            };
            snapshot.ActiveSkills.Add(new ActiveSkillSnapshot { SkillNodeId = "skill_a", ActionOrEffectId = "fx_dormant" });
            snapshot.DormantSkillActionIds.Add("fx_dormant");

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsFalse(result.HasErrors);
            Assert.IsTrue(result.Findings.Any(f =>
                f.CheckId == "(g)-active-skill-executor" &&
                f.Severity == CatalogSeverity.Info));
        }

        [Test]
        public void ActiveSkillToValidExecutor_IsClean()
        {
            var snapshot = new CatalogSnapshot
            {
                Skills = GeneratedCategory(CatalogCategory.Skills, "skill_a")
            };
            snapshot.ActiveSkills.Add(new ActiveSkillSnapshot { SkillNodeId = "skill_a", ActionOrEffectId = "fx_real" });
            snapshot.ValidSkillActionIds.Add("fx_real");

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsFalse(result.Findings.Any(f => f.CheckId == "(g)-active-skill-executor"));
        }

        // ── (h) duplicate BestiaryEntryId = ERROR ────────────────────────────────
        [Test]
        public void DuplicateBestiaryEntryId_IsError()
        {
            var snapshot = new CatalogSnapshot
            {
                Bestiary = GeneratedCategory(CatalogCategory.Bestiary, "enemy_a", "enemy_b")
            };
            snapshot.BestiaryEntries.Add(new BestiaryEntrySnapshot { EnemyId = "enemy_a", BestiaryEntryId = "bestiary_dup" });
            snapshot.BestiaryEntries.Add(new BestiaryEntrySnapshot { EnemyId = "enemy_b", BestiaryEntryId = "bestiary_dup" });

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsTrue(result.Findings.Any(f =>
                f.CheckId == "(h)-duplicate-bestiary-entry" &&
                f.Severity == CatalogSeverity.Error &&
                f.InvolvedIds.Contains("bestiary_dup")));
        }

        // ── CA-2: incremental — not generated = SKIPPED, not ERROR ───────────────
        [Test]
        public void NotGeneratedCategory_IsSkipped_NotError()
        {
            var snapshot = new CatalogSnapshot
            {
                Items = NotGeneratedCategory(CatalogCategory.Items),
                Bestiary = NotGeneratedCategory(CatalogCategory.Bestiary),
                Quests = NotGeneratedCategory(CatalogCategory.Quests),
                Skills = NotGeneratedCategory(CatalogCategory.Skills),
                Recipes = NotGeneratedCategory(CatalogCategory.Recipes),
                Shops = NotGeneratedCategory(CatalogCategory.Shops)
            };
            var expectations = new CatalogExpectationSet();
            expectations.Set(CatalogCategory.Items, new CatalogExpectation { ExpectedCount = 118 });

            var result = CatalogConsistencyEngine.Validate(snapshot, expectations);

            Assert.IsFalse(result.HasErrors, "Ungenerated categories must never produce ERRORs.");
            Assert.IsTrue(result.SkippedCount > 0, "Ungenerated categories must produce SKIPPED findings.");
            Assert.IsTrue(result.Findings.Any(f =>
                f.Category == CatalogCategory.Items &&
                f.Severity == CatalogSeverity.Skipped &&
                f.CheckId == "incremental"));
        }

        [Test]
        public void CrossRefSkippedWhenTargetCategoryNotGenerated()
        {
            // Enemy drops exist but the item catalog has not been generated -> SKIPPED, not ERROR.
            var snapshot = new CatalogSnapshot
            {
                Items = NotGeneratedCategory(CatalogCategory.Items),
                Bestiary = GeneratedCategory(CatalogCategory.Bestiary, "enemy_slime")
            };
            snapshot.EnemyDrops.Add(new EnemyDropSnapshot { EnemyId = "enemy_slime", DropItemId = "item_wood" });

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsFalse(result.HasErrors);
            Assert.IsTrue(result.Findings.Any(f =>
                f.CheckId == "(c)-drop-to-item" && f.Severity == CatalogSeverity.Skipped));
        }

        // ── CA-2: partial generation = WARN with X/Y count ───────────────────────
        [Test]
        public void PartialGeneration_IsCountWarn()
        {
            var snapshot = new CatalogSnapshot
            {
                Items = GeneratedCategory(CatalogCategory.Items, "item_a", "item_b")
            };
            var expectations = new CatalogExpectationSet();
            expectations.Set(CatalogCategory.Items, new CatalogExpectation { ExpectedCount = 10 });

            var result = CatalogConsistencyEngine.Validate(snapshot, expectations);

            Assert.IsFalse(result.HasErrors, "Partial count must be WARN, never ERROR.");
            var countFinding = result.Findings.Single(f =>
                f.Category == CatalogCategory.Items && f.CheckId == "count");
            Assert.AreEqual(CatalogSeverity.Warn, countFinding.Severity);
            StringAssert.Contains("2/10", countFinding.Message);
        }

        // ── Canonical expectation table sanity (no drift in the versioned counts) ─
        [Test]
        public void CanonicalExpectations_HaveDocumentedCounts()
        {
            var set = CatalogExpectationSet.BuildCanonical();

            Assert.AreEqual(118, set.For(CatalogCategory.Items).ExpectedCount);
            Assert.AreEqual(64, set.For(CatalogCategory.Bestiary).ExpectedCount);
            Assert.AreEqual(86, set.For(CatalogCategory.Quests).ExpectedCount);

            var skills = set.For(CatalogCategory.Skills);
            Assert.AreEqual(66, skills.ExpectedCount); // fable_70 saneamento: 69 → 66
            Assert.AreEqual(66, skills.CanonicalIds.Count,
                "Skill canonical id list must match the documented node count (DefaultSkillCatalog).");
            Assert.IsTrue(skills.CanonicalIds.Contains("melee_capstone_battle_rhythm"));
        }

        // ── Clean snapshot with all categories generated and consistent = no errors ─
        [Test]
        public void FullyConsistentSnapshot_HasNoErrors()
        {
            var snapshot = new CatalogSnapshot
            {
                Items = GeneratedCategory(CatalogCategory.Items, "item_wood", "item_stone"),
                Bestiary = GeneratedCategory(CatalogCategory.Bestiary, "enemy_slime"),
                Quests = GeneratedCategory(CatalogCategory.Quests, "quest_a"),
                Skills = GeneratedCategory(CatalogCategory.Skills, "skill_a"),
                Recipes = GeneratedCategory(CatalogCategory.Recipes, "recipe_a"),
                Shops = GeneratedCategory(CatalogCategory.Shops, "shop_a")
            };
            snapshot.EnemyDrops.Add(new EnemyDropSnapshot { EnemyId = "enemy_slime", DropItemId = "item_wood" });
            snapshot.RecipeIngredients.Add(new RecipeIngredientSnapshot { RecipeId = "recipe_a", IngredientItemId = "item_stone" });
            snapshot.ShopEntries.Add(new ShopEntrySnapshot { ShopId = "shop_a", ItemId = "item_wood" });
            snapshot.QuestRewards.Add(new QuestRewardSnapshot { QuestId = "quest_a", RewardId = "r1", RewardKind = "item", TargetId = "item_stone" });

            var result = CatalogConsistencyEngine.Validate(snapshot, new CatalogExpectationSet());

            Assert.IsFalse(result.HasErrors, "Consistent snapshot must have zero ERRORs.");
        }
    }
}
