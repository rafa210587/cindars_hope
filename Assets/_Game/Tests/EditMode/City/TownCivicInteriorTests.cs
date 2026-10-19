using System;
using System.Collections.Generic;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.World;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Tests.EditMode.City
{
    [TestFixture]
    public sealed class TownCivicInteriorTests
    {
        private const string TownScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private Scene _previousActive;
        private Scene _town;
        private bool _openedHere;

        [SetUp]
        public void LoadSavedTownAdditively()
        {
            _previousActive = SceneManager.GetActiveScene();
            _town = SceneManager.GetSceneByPath(TownScenePath);
            _openedHere = !_town.IsValid() || !_town.isLoaded;
            if (_openedHere)
                _town = EditorSceneManager.OpenScene(TownScenePath, OpenSceneMode.Additive);
        }

        [TearDown]
        public void RestorePriorSceneSetup()
        {
            if (_openedHere && _town.IsValid() && _town.isLoaded)
                EditorSceneManager.CloseScene(_town, true);
            // Closing only our additive scene preserves any unsaved scenes owned by the user.
            if (_previousActive.IsValid() && _previousActive.isLoaded)
                SceneManager.SetActiveScene(_previousActive);
        }

        [Test]
        public void SavedTown_CivicLotsKeepHouseDoorAndRevealIdentities()
        {
            foreach (var houseName in CivicHouseNames)
            {
                var house = FindHouse(houseName);
                Assert.NotNull(house, $"Saved Town is missing stable house ID {houseName}.");
                Assert.AreEqual(1, house.GetComponentsInChildren<HouseDoorInteractable>(true).Length,
                    $"{houseName} must retain one physical door.");
                Assert.AreEqual(1, house.GetComponentsInChildren<RoofRevealController>(true).Length,
                    $"{houseName} must retain its walk-in roof reveal controller.");
            }
        }

        [Test]
        public void SavedTown_CivicFurnitureUsesReviewedSourcesAndLeavesNoDomesticArchiveOrRegistryProps()
        {
            AssertFurniture("House_Archive", "Furniture_ArchiveShelfWest", "Furniture_ArchiveShelfNorth",
                "Furniture_ArchiveShelfEast", "Furniture_ArchiveDesk");
            AssertFurniture("House_Registry", "Furniture_RegistryRecordsWest", "Furniture_RegistryServiceDesk",
                "Furniture_RegistryRecordsEast");
            AssertFurniture("House_Prison", "Furniture_PrisonBed", "Furniture_PrisonBench", "Furniture_PrisonStorage");
            AssertFurniture("House_GateKeeper", "Furniture_GateKeeperDesk", "Furniture_GateKeeperStorage",
                "Furniture_GateKeeperRest");

            AssertNoDomesticProps(FindHouse("House_Archive"));
            AssertNoDomesticProps(FindHouse("House_Registry"));
        }

        [Test]
        public void SavedTown_CivicFurnitureCollidersStayInsideVisualSupportAndAnswerPhysicsQueries()
        {
            Physics2D.SyncTransforms();
            foreach (var houseName in CivicHouseNames)
            {
                var house = FindHouse(houseName);
                foreach (var prop in house.GetComponentsInChildren<BoxCollider2D>(true))
                {
                    if (prop.isTrigger || prop.transform.parent != house.transform ||
                        !prop.name.StartsWith("Furniture_", StringComparison.Ordinal))
                        continue;

                    var renderer = prop.GetComponentInChildren<SpriteRenderer>(true);
                    Assert.NotNull(renderer, $"{houseName}/{prop.name} has a solid collider without its visual support.");
                    var visual = renderer.bounds;
                    var solid = prop.bounds;
                    const float tolerance = 0.025f;
                    Assert.GreaterOrEqual(solid.min.x, visual.min.x - tolerance, $"{houseName}/{prop.name} collider escapes visual support west.");
                    Assert.LessOrEqual(solid.max.x, visual.max.x + tolerance, $"{houseName}/{prop.name} collider escapes visual support east.");
                    Assert.GreaterOrEqual(solid.min.y, visual.min.y - tolerance, $"{houseName}/{prop.name} collider escapes visual support south.");
                    Assert.LessOrEqual(solid.max.y, visual.max.y + tolerance, $"{houseName}/{prop.name} collider escapes visual support north.");

                    var hits = Physics2D.OverlapPointAll(solid.center);
                    CollectionAssert.Contains(hits, prop,
                        $"{houseName}/{prop.name} is not present in the real Physics2D overlap at its solid support.");
                }
            }
        }

        [Test]
        public void SavedTown_GateKeeperPilotKeepsPhysicalShellButUsesInteriorEdgeVisuals()
        {
            var house = FindHouse("House_GateKeeper");
            var north = Wall(house, "Wall_Top");
            var left = Wall(house, "Wall_Left");
            var right = Wall(house, "Wall_Right");
            var frontLeft = Wall(house, "Wall_BottomL");
            var frontRight = Wall(house, "Wall_BottomR");

            Assert.GreaterOrEqual(north.size.x, 7.9f, "Pilot must not shrink the north physical wall.");
            Assert.GreaterOrEqual(left.size.y, 5.9f, "Pilot must not shrink the left physical wall.");
            Assert.GreaterOrEqual(right.size.y, 5.9f, "Pilot must not shrink the right physical wall.");
            Assert.Greater(frontLeft.size.x, 0f);
            Assert.Greater(frontRight.size.x, 0f);

            var northFace = Visual(north, "GateKeeperNorthFace");
            Assert.That(northFace.bounds.size.y, Is.InRange(1.60f, 1.65f),
                "North face must remain about 65% of the 2.4961u final Player alpha silhouette.");
            Assert.AreEqual(north.bounds.min.y, northFace.bounds.min.y, 0.025f,
                "North visual must start at the interior edge of its preserved collider.");

            AssertSideTrim(left, left.bounds.max.x - 0.15f);
            AssertSideTrim(right, right.bounds.min.x + 0.15f);
            var jambLeft = Visual(frontLeft, "GateKeeperDoorJamb");
            var jambRight = Visual(frontRight, "GateKeeperDoorJamb");
            Assert.AreEqual(1.8f, jambRight.bounds.min.x - jambLeft.bounds.max.x, 0.01f);
            Assert.AreEqual(1, frontLeft.GetComponentsInChildren<SpriteRenderer>(true).Length, "Only a cutaway jamb, no front wall face.");
            Assert.AreEqual(1, frontRight.GetComponentsInChildren<SpriteRenderer>(true).Length, "Only a cutaway jamb, no front wall face.");

            foreach (var propName in new[] { "Furniture_GateKeeperDesk", "Furniture_GateKeeperStorage", "Furniture_GateKeeperRest" })
            {
                var renderer = FindDirectChild(house.transform, propName).GetComponentInChildren<SpriteRenderer>(true);
                Assert.AreEqual(SpriteSortPoint.Center, renderer.spriteSortPoint,
                    $"{propName} must sort from its visual center, aligned with the Player center pivot.");
            }
        }

        [Test]
        public void SavedTown_GateKeeperFurnitureUsesFinalPlayerRelativeSizesWithUniformScale()
        {
            // walk_down_01: 71 opaque pixels / 128 PPU × final Town VisualScale 4.5.
            const float finalPlayerOpaqueHeight = 2.49609375f;
            var house = FindHouse("House_GateKeeper");
            // Pilot-specific projected bounds, not a universal bed/upright-character ratio.
            // Visual approval remains a separate screenshot review.
            AssertPlayerRelativeSize(house, "Furniture_GateKeeperRest", finalPlayerOpaqueHeight, 0.75f, 0.95f);
            AssertPlayerRelativeSize(house, "Furniture_GateKeeperStorage", finalPlayerOpaqueHeight, 0.30f, 0.45f);
            AssertPlayerRelativeSize(house, "Furniture_GateKeeperDesk", finalPlayerOpaqueHeight, 0.38f, 0.50f);
        }

        [Test]
        public void SavedTown_GateKeeperFloorAndThresholdMatchUsableShell()
        {
            var house = FindHouse("House_GateKeeper");
            var floor = house.transform.Find("Floor").GetComponent<SpriteRenderer>();
            var left = Wall(house, "Wall_Left");
            var right = Wall(house, "Wall_Right");
            var north = Wall(house, "Wall_Top");
            var front = Wall(house, "Wall_BottomL");
            Assert.AreEqual(left.bounds.max.x, floor.bounds.min.x, 0.01f);
            Assert.AreEqual(right.bounds.min.x, floor.bounds.max.x, 0.01f);
            Assert.AreEqual(north.bounds.min.y, floor.bounds.max.y, 0.01f);
            Assert.AreEqual(front.bounds.max.y, floor.bounds.min.y, 0.01f);
            var threshold = floor.transform.Find("GateKeeperDoorThreshold").GetComponent<SpriteRenderer>();
            Assert.AreEqual(1.8f, threshold.bounds.size.x, 0.01f);
            Assert.AreEqual(front.bounds.min.y, threshold.bounds.min.y, 0.01f);
            Assert.AreEqual(floor.bounds.min.y, threshold.bounds.max.y, 0.01f);
            foreach (var wall in new[] { left, right })
            {
                var trim = Visual(wall, "GateKeeperSideTrim");
                Assert.AreEqual(floor.bounds.min.y, trim.bounds.min.y, 0.01f);
                Assert.AreEqual(floor.bounds.max.y, trim.bounds.max.y, 0.01f);
            }
            foreach (var renderer in new[] { floor, Visual(north, "GateKeeperNorthFace") })
            {
                Assert.AreEqual(32f, renderer.sprite.pixelsPerUnit / renderer.transform.lossyScale.x, 0.01f);
                Assert.AreEqual(renderer.transform.lossyScale.x, renderer.transform.lossyScale.y, 0.001f);
            }
            foreach (var propName in new[] { "Furniture_GateKeeperDesk", "Furniture_GateKeeperStorage", "Furniture_GateKeeperRest" })
            {
                var prop = house.transform.Find(propName);
                var visual = prop.GetComponentInChildren<SpriteRenderer>(true).bounds;
                foreach (var solid in prop.GetComponentsInChildren<Collider2D>(true))
                {
                    if (solid.isTrigger || !solid.enabled) continue;
                    Assert.GreaterOrEqual(solid.bounds.min.x, visual.min.x - 0.025f, solid.name);
                    Assert.LessOrEqual(solid.bounds.max.x, visual.max.x + 0.025f, solid.name);
                    Assert.GreaterOrEqual(solid.bounds.min.y, visual.min.y - 0.025f, solid.name);
                    Assert.LessOrEqual(solid.bounds.max.y, visual.max.y + 0.025f, solid.name);
                }
            }
        }

        private static readonly string[] CivicHouseNames =
        {
            "House_Prison", "House_Archive", "House_Registry", "House_GateKeeper"
        };

        private GameObject FindHouse(string houseName)
        {
            foreach (var root in _town.GetRootGameObjects())
            {
                var transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var transform in transforms)
                    if (transform.name == houseName) return transform.gameObject;
            }
            Assert.Fail($"Saved Town has no {houseName}.");
            return null;
        }

        private static void AssertFurniture(string houseName, params string[] propNames)
        {
            var house = FindExistingHouse(houseName);
            foreach (var propName in propNames)
            {
                var prop = FindDirectChild(house.transform, propName);
                Assert.NotNull(prop, $"{houseName} is missing civic prop {propName}.");
                var renderer = prop.GetComponentInChildren<SpriteRenderer>(true);
                Assert.NotNull(renderer, $"{houseName}/{propName} is missing its renderer.");
                string expectedAsset = TownKeyartComponentSupportCatalog.ProposedInteriorAsset(propName);
                Assert.NotNull(expectedAsset, $"{propName} lacks a reviewed source identity.");
                Assert.AreEqual(expectedAsset, AssetDatabase.GetAssetPath(renderer.sprite),
                    $"{houseName}/{propName} must use its reviewed measured source.");
            }
        }

        // AssertFurniture is static to keep fixture state out of the source checks; the saved scene
        // always has a single TownHouses root while this fixture holds the additive Town scene.
        private static GameObject FindExistingHouse(string houseName)
        {
            var town = SceneManager.GetSceneByPath(TownScenePath);
            foreach (var root in town.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    if (transform.name == houseName) return transform.gameObject;
            }
            Assert.Fail($"Saved Town has no {houseName}.");
            return null;
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
                if (parent.GetChild(i).name == name) return parent.GetChild(i);
            return null;
        }

        private static BoxCollider2D Wall(GameObject house, string wallName)
        {
            var wall = FindDirectChild(house.transform, wallName);
            Assert.NotNull(wall, $"Gatekeeper pilot is missing {wallName}.");
            var collider = wall.GetComponent<BoxCollider2D>();
            Assert.NotNull(collider, $"Gatekeeper {wallName} lost its physical wall collider.");
            return collider;
        }

        private static SpriteRenderer Visual(BoxCollider2D wall, string visualName)
        {
            var visual = wall.transform.Find(visualName);
            Assert.NotNull(visual, $"Gatekeeper wall {wall.name} is missing {visualName}.");
            var renderer = visual.GetComponent<SpriteRenderer>();
            Assert.NotNull(renderer, $"Gatekeeper {visualName} is missing its renderer.");
            return renderer;
        }

        private static void AssertSideTrim(BoxCollider2D wall, float expectedInteriorEdge)
        {
            var trim = Visual(wall, "GateKeeperSideTrim");
            Assert.That(trim.bounds.size.x, Is.InRange(0.299f, 0.301f),
                $"{wall.name} trim must be visibly .30u wide, outside the usable room.");
            Assert.AreEqual(expectedInteriorEdge, trim.bounds.center.x, 0.025f,
                $"{wall.name} trim must mark the collider's interior edge, not its invisible center.");
        }

        private static void AssertFrontBase(BoxCollider2D wall, float expectedInteriorEdge)
        {
            var baseRenderer = Visual(wall, "GateKeeperFrontBase");
            Assert.That(baseRenderer.bounds.size.y, Is.InRange(0.099f, 0.101f), $"{wall.name} front visual must be a visible .10u base strip.");
            Assert.AreEqual(expectedInteriorEdge, baseRenderer.bounds.center.y, 0.025f,
                $"{wall.name} base must mark the collider's interior edge while preserving the door gap.");
        }

        private static void AssertPlayerRelativeSize(GameObject house, string propName, float playerHeight, float minimum, float maximum)
        {
            var renderer = FindDirectChild(house.transform, propName).GetComponentInChildren<SpriteRenderer>(true);
            Assert.AreEqual(renderer.transform.lossyScale.x, renderer.transform.lossyScale.y, 0.0001f,
                $"{propName} must preserve sprite aspect with uniform scale.");
            float ratio = renderer.bounds.size.y / playerHeight;
            Assert.That(ratio, Is.InRange(minimum, maximum),
                $"{propName} visual height must remain {minimum:0.##}–{maximum:0.##}x the calibrated final Player silhouette.");
        }

        private static void AssertNoDomesticProps(GameObject house)
        {
            foreach (var transform in house.GetComponentsInChildren<Transform>(true))
            {
                string name = transform.name;
                Assert.IsFalse(name == "Bed" || name == "Table" ||
                    name.StartsWith("Furniture_Kitchen", StringComparison.Ordinal) ||
                    name.StartsWith("Furniture_Stove", StringComparison.Ordinal) ||
                    name.StartsWith("Furniture_Cupboard", StringComparison.Ordinal),
                    $"{house.name} contains invalid domestic prop {name}.");
            }
        }
    }
}
