using CindarsHope.Editor.Art;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Editor
{
    public class FarmDecorationPlannerTests
    {
        [Test]
        public void DistributedTrees_PreserveIdsAndKeepNewGroupsClear()
        {
            var forest = FarmDecorationPlanner.PlanForestTreePositions(68);
            var first = FarmDecorationPlanner.PlanFarmTreePositions();
            var repeat = FarmDecorationPlanner.PlanFarmTreePositions();
            Assert.That(first.Count, Is.EqualTo(forest.Count));
            for (var i = 0; i < first.Count; i++)
            {
                Assert.That(first[i], Is.EqualTo(repeat[i]), "Stable ID " + i);
                if (i < 62) Assert.That(first[i], Is.EqualTo(forest[i]));
                else
                {
                    Assert.That(FarmDecorationPlanner.IsForbiddenCell(first[i]), Is.False, "Distributed ID " + i);
                    for (var other = 0; other < i; other++)
                        Assert.That(UnityEngine.Vector2.Distance(first[i], first[other]), Is.GreaterThan(1.3f), "Trunk clearance applies to every stable tree, including the former distributed tail.");
                }
            }
        }

        [Test]
        public void ForestPositions_PreserveStableOrderingAndClearGameplayAreas()
        {
            var first = FarmDecorationPlanner.PlanForestTreePositions(68);
            var repeated = FarmDecorationPlanner.PlanForestTreePositions(68);
            Assert.That(first.Count, Is.EqualTo(68));
            for (var i = 0; i < first.Count; i++)
            {
                Assert.That(repeated[i], Is.EqualTo(first[i]), "Stable tree ID " + i);
                Assert.That(FarmDecorationPlanner.IsForbiddenCell(first[i]), Is.False, "Tree blocks a reserved approach");
                Assert.That(FarmDecorationPlanner.IsTreeCanopyClearOfCave(first[i]), Is.True, "Mature canopy hides cave arrival");
                var canopy = new UnityEngine.Rect(first[i].x - 2.3f, first[i].y, 4.6f, 6f);
                Assert.That(canopy.Overlaps(new UnityEngine.Rect(-17f, 13f, 27f, 8f)), Is.False,
                    "Keep the well-to-house northern clearing open: " + first[i]);
                for (var other = 0; other < i; other++)
                    Assert.That(UnityEngine.Vector2.Distance(first[i], first[other]), Is.GreaterThan(1.3f), "Trunks must leave clearance");
            }
        }

        [Test]
        public void SameSeed_ProducesIdenticalPlan()
        {
            var first = FarmDecorationPlanner.Plan();
            var second = FarmDecorationPlanner.Plan();

            Assert.That(second.Count, Is.EqualTo(first.Count));
            for (var i = 0; i < first.Count; i++)
            {
                Assert.That(second[i].SpriteId, Is.EqualTo(first[i].SpriteId));
                Assert.That(second[i].Position, Is.EqualTo(first[i].Position));
                Assert.That(second[i].Biome, Is.EqualTo(first[i].Biome));
            }
        }

        [Test]
        public void Plan_HasNoForbiddenCells()
        {
            var plan = FarmDecorationPlanner.Plan();
            for (var i = 0; i < plan.Count; i++)
            {
                Assert.That(FarmDecorationPlanner.IsForbiddenCell(plan[i].Position), Is.False, plan[i].SpriteId);
            }
        }

        [Test]
        public void Plan_AllBiomesAreWithinDeclaredRanges()
        {
            var plan = FarmDecorationPlanner.Plan();
            var counts = new int[6];
            for (var i = 0; i < plan.Count; i++) counts[(int)plan[i].Biome]++;

            foreach (FarmDecorationBiome biome in System.Enum.GetValues(typeof(FarmDecorationBiome)))
            {
                Assert.That(FarmDecorationPlanner.IsCountInDeclaredRange(biome, counts[(int)biome]), Is.True, $"{biome}: count={counts[(int)biome]}");
            }
        }
    }
}
