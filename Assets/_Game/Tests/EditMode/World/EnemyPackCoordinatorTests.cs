using NUnit.Framework;
using UnityEngine;
using CindarsHope.Cave.Runtime;
using CindarsHope.Enemy;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// fable_04 CA-3 — the pack leash anchor is a pure function of the (deterministic) spawn plan:
    /// it is the centroid of the world positions of every entry sharing a PackId. Revisiting a level
    /// reproduces the same plan, hence the same anchors — no new RNG (ADR-0005 / cave-stable-run).
    /// </summary>
    [TestFixture]
    public class EnemyPackCoordinatorTests
    {
        private static CaveEnemySpawnPlanEntry Entry(string packId, float x, float y)
        {
            return new CaveEnemySpawnPlanEntry
            {
                PackId = packId,
                WorldPosition = new Vector3(x, y, 0f)
            };
        }

        private static CaveEnemySpawnPlan PlanWith(params CaveEnemySpawnPlanEntry[] entries)
        {
            var plan = new CaveEnemySpawnPlan { CaveLevel = 3 };
            plan.Entries.Clear();
            plan.Entries.AddRange(entries);
            return plan;
        }

        [Test]
        public void ComputeAnchor_IsCentroidOfPackMembers()
        {
            var plan = PlanWith(
                Entry("pack_a", 0f, 0f),
                Entry("pack_a", 4f, 0f),
                Entry("pack_a", 2f, 6f),
                Entry("pack_b", 100f, 100f)); // different pack must not pollute pack_a's anchor

            bool found = EnemyPackCoordinator.ComputeAnchorFromPlan(plan, "pack_a", out var anchor);

            Assert.IsTrue(found);
            Assert.AreEqual(2f, anchor.x, 1e-4, "Centroid X = (0+4+2)/3.");
            Assert.AreEqual(2f, anchor.y, 1e-4, "Centroid Y = (0+0+6)/3.");
        }

        [Test]
        public void ComputeAnchor_IsDeterministic_ForSamePlan()
        {
            var plan = PlanWith(
                Entry("pack_x", -3f, 7f),
                Entry("pack_x", 5f, -1f),
                Entry("pack_x", 1f, 1f));

            EnemyPackCoordinator.ComputeAnchorFromPlan(plan, "pack_x", out var first);
            EnemyPackCoordinator.ComputeAnchorFromPlan(plan, "pack_x", out var second);

            Assert.AreEqual(first, second, "Same plan must always yield the same anchor (stable run).");
        }

        [Test]
        public void ComputeAnchor_OnlyAggregatesMatchingPackId()
        {
            var plan = PlanWith(
                Entry("pack_a", 0f, 0f),
                Entry("pack_b", 10f, 10f),
                Entry("pack_b", 20f, 20f));

            EnemyPackCoordinator.ComputeAnchorFromPlan(plan, "pack_b", out var anchorB);

            Assert.AreEqual(15f, anchorB.x, 1e-4);
            Assert.AreEqual(15f, anchorB.y, 1e-4);
        }

        [Test]
        public void ComputeAnchor_ReturnsFalse_ForUnknownPack()
        {
            var plan = PlanWith(Entry("pack_a", 1f, 1f));

            Assert.IsFalse(EnemyPackCoordinator.ComputeAnchorFromPlan(plan, "pack_missing", out var anchor));
            Assert.AreEqual(Vector2.zero, anchor);
        }

        [Test]
        public void ComputeAnchor_ReturnsFalse_ForNullPlanOrEmptyPackId()
        {
            Assert.IsFalse(EnemyPackCoordinator.ComputeAnchorFromPlan(null, "pack_a", out _));

            var plan = PlanWith(Entry("pack_a", 1f, 1f));
            Assert.IsFalse(EnemyPackCoordinator.ComputeAnchorFromPlan(plan, "", out _));
            Assert.IsFalse(EnemyPackCoordinator.ComputeAnchorFromPlan(plan, null, out _));
        }

        [Test]
        public void ComputeAnchor_IgnoresNullEntries()
        {
            var plan = PlanWith(
                Entry("pack_a", 0f, 0f),
                null,
                Entry("pack_a", 4f, 4f));

            bool found = EnemyPackCoordinator.ComputeAnchorFromPlan(plan, "pack_a", out var anchor);

            Assert.IsTrue(found);
            Assert.AreEqual(2f, anchor.x, 1e-4);
            Assert.AreEqual(2f, anchor.y, 1e-4);
        }

        [Test]
        public void ComputeAnchor_SingleMember_EqualsItsPosition()
        {
            var plan = PlanWith(Entry("solo_pack", 7.5f, -2.5f));

            EnemyPackCoordinator.ComputeAnchorFromPlan(plan, "solo_pack", out var anchor);

            Assert.AreEqual(new Vector2(7.5f, -2.5f), anchor);
        }
    }
}
