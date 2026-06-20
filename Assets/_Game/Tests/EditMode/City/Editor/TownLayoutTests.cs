using NUnit.Framework;
using UnityEngine;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.NPC.Schedule;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// fable_40 — pure validation of the canonical 48×42 town layout (no scene side effects).
    /// Covers CA-1 (footprint + element coverage), CA-2/CA-4 (stable schedule-anchor & spawn IDs),
    /// CA-3 (lake/park SW + town hall NE + mural exist inside their districts). The scene
    /// regeneration itself is DEFERRED (generator ready; Unity not run) — these tests guard the
    /// deterministic layout math the generator consumes.
    /// </summary>
    [TestFixture]
    public class TownLayoutTests
    {
        private const float Eps = 0.0001f;

        // ── CA-1: canonical footprint ────────────────────────────────────────────────
        [Test]
        public void Footprint_Is48x42()
        {
            Assert.AreEqual(48f, TownDistrictLayout.WidthTiles, Eps, "Town width must be 48 tiles (Q12.1).");
            Assert.AreEqual(42f, TownDistrictLayout.HeightTiles, Eps, "Town height must be 42 tiles (Q12.1).");
            Assert.AreEqual(24f, TownDistrictLayout.HalfWidth, Eps, "Bounds must be -24..24 on x.");
            Assert.AreEqual(21f, TownDistrictLayout.HalfHeight, Eps, "Bounds must be -21..21 on y.");
        }

        // ── CA-1 / data contract: seven canonical districts, all inside bounds ────────
        [Test]
        public void DistrictMap_HasSevenDistricts()
        {
            Assert.AreEqual(7, TownDistrictLayout.AllDistricts.Count,
                "HUD_LAYOUT §3 / city_rules Rule 1 require exactly 7 districts.");
        }

        [Test]
        public void DistrictMap_AllRectanglesWithinBounds()
        {
            foreach (var d in TownDistrictLayout.AllDistricts)
            {
                Assert.IsTrue(d.WithinBounds(),
                    $"District '{d.Id}' rectangle escapes the 48x42 bounds " +
                    $"(x[{d.MinX},{d.MaxX}] y[{d.MinY},{d.MaxY}]).");
            }
        }

        [Test]
        public void DistrictMap_ContainsAllSevenCanonicalIds()
        {
            string[] ids =
            {
                TownDistrictLayout.DistrictCentralPlaza,
                TownDistrictLayout.DistrictMarketWest,
                TownDistrictLayout.DistrictResidentialEast,
                TownDistrictLayout.DistrictTempleNorth,
                TownDistrictLayout.DistrictCorralSouth,
                TownDistrictLayout.DistrictTownHallNortheast,
                TownDistrictLayout.DistrictLakeParkSouthwest,
            };
            foreach (var id in ids)
            {
                Assert.IsTrue(TownDistrictLayout.TryGetDistrict(id, out _),
                    $"Canonical district '{id}' is missing from the layout map.");
            }
        }

        // ── No-overlap: district CENTERS are not contained by another district ────────
        // (Districts may share an edge with the ring road, but no core sits inside another core.)
        [Test]
        public void DistrictMap_CentersDoNotOverlapOtherDistricts()
        {
            var all = TownDistrictLayout.AllDistricts;
            for (int i = 0; i < all.Count; i++)
            {
                for (int j = 0; j < all.Count; j++)
                {
                    if (i == j) continue;
                    Assert.IsFalse(all[j].Contains(all[i].Center),
                        $"District '{all[i].Id}' center lies inside district '{all[j].Id}' — cores overlap.");
                }
            }
        }

        // ── CA-3: new districts exist with landmarks inside their rectangle + bounds ──
        [Test]
        public void LakeParkDistrict_LandmarksInsideDistrictAndBounds()
        {
            Assert.IsTrue(TownDistrictLayout.TryGetDistrict(TownDistrictLayout.DistrictLakeParkSouthwest, out var d));
            AssertInside(d, TownDistrictLayout.LakeCenter, "LakeWater");
            AssertInside(d, TownDistrictLayout.LakeBenchWest, "ParkBench_W");
            AssertInside(d, TownDistrictLayout.LakeBenchEast, "ParkBench_E");
            // SW corner sanity: the lake sits in the south-west quadrant.
            Assert.Less(TownDistrictLayout.LakeCenter.x, 0f, "Lake must be west.");
            Assert.Less(TownDistrictLayout.LakeCenter.y, 0f, "Lake must be south.");
        }

        [Test]
        public void TownHallDistrict_BuildingAndMuralInsideDistrictAndBounds()
        {
            Assert.IsTrue(TownDistrictLayout.TryGetDistrict(TownDistrictLayout.DistrictTownHallNortheast, out var d));
            AssertInside(d, TownDistrictLayout.TownHallCenter, "TownHallBuilding");
            AssertInside(d, TownDistrictLayout.TownHallMural, "TownHallMural");
            // NE corner sanity: the town hall sits in the north-east quadrant.
            Assert.Greater(TownDistrictLayout.TownHallCenter.x, 0f, "Town hall must be east.");
            Assert.Greater(TownDistrictLayout.TownHallCenter.y, 0f, "Town hall must be north.");
        }

        // ── CA-1: element coverage — every repositioned legacy point stays inside the playfield ──
        // (No element is pushed out of the footprint by the relayout = nothing lost off-map.)
        [Test]
        public void Reposition_KeepsExtremeLegacyCornersInsideBounds()
        {
            // Furthest legacy authoring points (perimeter trees / corner houses).
            Vector3[] legacyExtremes =
            {
                new Vector3(-16.5f, 11f, 0f), new Vector3(16.5f, 11.5f, 0f),
                new Vector3(-16.5f, -11f, 0f), new Vector3(16.5f, -11f, 0f),
                new Vector3(14.5f, 10.5f, 0f), new Vector3(-14.5f, -3.5f, 0f),
                new Vector3(0f, -13.5f, 0f),
            };
            foreach (var legacy in legacyExtremes)
            {
                var p = TownDistrictLayout.Reposition(legacy);
                Assert.IsTrue(Mathf.Abs(p.x) <= TownDistrictLayout.HalfWidth - 1f + Eps,
                    $"Repositioned x {p.x} for legacy {legacy} escapes the playfield interior.");
                Assert.IsTrue(Mathf.Abs(p.y) <= TownDistrictLayout.HalfHeight - 1f + Eps,
                    $"Repositioned y {p.y} for legacy {legacy} escapes the playfield interior.");
            }
        }

        [Test]
        public void Reposition_IsDeterministicAndPreservesZ()
        {
            var legacy = new Vector3(3.5f, 7f, 0.25f);
            var a = TownDistrictLayout.Reposition(legacy);
            var b = TownDistrictLayout.Reposition(legacy);
            Assert.AreEqual(a, b, "Reposition must be deterministic.");
            Assert.AreEqual(0.25f, a.z, Eps, "Reposition must preserve z.");
        }

        [Test]
        public void Reposition_ScalesAwayFromCenter()
        {
            // A point off-center moves further from the origin (footprint grows ~1.3x).
            var legacy = new Vector3(6f, -4f, 0f);
            var p = TownDistrictLayout.Reposition(legacy);
            Assert.Greater(Mathf.Abs(p.x), Mathf.Abs(legacy.x), "Relayout should spread x outward.");
            Assert.Greater(Mathf.Abs(p.y), Mathf.Abs(legacy.y), "Relayout should spread y outward.");
            // Origin is a fixed point.
            var origin = TownDistrictLayout.Reposition(Vector3.zero);
            Assert.AreEqual(Vector3.zero, origin, "Town center must remain at the origin.");
        }

        // ── CA-2: schedule-anchor suffix IDs are FROZEN (match the runtime resolver constants) ──
        [Test]
        public void ScheduleAnchorSuffixes_MatchRuntimeResolverConstants()
        {
            CollectionAssert.AreEquivalent(
                new[]
                {
                    NpcScheduleBlockResolver.WorkAnchorSuffix,
                    NpcScheduleBlockResolver.SocialAnchorSuffix,
                    NpcScheduleBlockResolver.HomeAnchorSuffix,
                },
                TownDistrictLayout.StableScheduleAnchorSuffixes,
                "Schedule-anchor suffixes must stay identical to the NPC resolver (IDs frozen).");
        }

        [Test]
        public void ScheduleAnchorSuffixes_AreExactlyWorkSocialHome()
        {
            Assert.AreEqual("work", NpcScheduleBlockResolver.WorkAnchorSuffix);
            Assert.AreEqual("social", NpcScheduleBlockResolver.SocialAnchorSuffix);
            Assert.AreEqual("home", NpcScheduleBlockResolver.HomeAnchorSuffix);
        }

        // ── CA-4: spawn IDs are FROZEN (saves transition without error) ───────────────
        [Test]
        public void SpawnIds_AreStableCanonicalSet()
        {
            CollectionAssert.AreEquivalent(
                new[] { "town_default", "town_from_farm" },
                TownDistrictLayout.StableSpawnIds,
                "Spawn IDs must remain town_default / town_from_farm across the relayout (CA-4).");
        }

        private static void AssertInside(TownDistrictLayout.District d, Vector3 point, string label)
        {
            var p2 = new Vector2(point.x, point.y);
            Assert.IsTrue(d.Contains(p2),
                $"{label} at {p2} is outside district '{d.Id}' (x[{d.MinX},{d.MaxX}] y[{d.MinY},{d.MaxY}]).");
            Assert.IsTrue(Mathf.Abs(point.x) <= TownDistrictLayout.HalfWidth &&
                          Mathf.Abs(point.y) <= TownDistrictLayout.HalfHeight,
                $"{label} at {p2} is outside the 48x42 bounds.");
        }
    }
}
