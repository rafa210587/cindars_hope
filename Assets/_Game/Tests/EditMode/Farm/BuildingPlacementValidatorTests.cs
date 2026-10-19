using CindarsHope.Farm.Buildings;
using NUnit.Framework;
using System.Collections.Generic;

namespace CindarsHope.Tests.EditMode.Farm
{
    public class BuildingPlacementValidatorTests
    {
        private BuildingDefinition _farmHouse;

        [SetUp]
        public void SetUp()
        {
            _farmHouse = new BuildingDefinition("farm_house", "Farmhouse", 10, 8)
            {
                CanMove = true,
                CanRotate = false,
                BlocksPath = true,
                RequiredFarmLevel = 1,
                IsLoreAnchor = false,
                EntranceTiles = new() { (1, 0) }
            };
        }

        [Test]
        public void ValidPlacementReturnsValidState()
        {
            var result = PlacementValidator.ValidatePlacement(_farmHouse, 5, 5, new(), new(), 1);
            Assert.That(result, Is.EqualTo(PlacementState.ValidPlacement));
        }

        [Test]
        public void PlacementRequiringLowFarmLevelPasses()
        {
            _farmHouse.RequiredFarmLevel = 1;
            var result = PlacementValidator.ValidatePlacement(_farmHouse, 5, 5, new(), new(), 1);
            Assert.That(result, Is.EqualTo(PlacementState.ValidPlacement));
        }

        [Test]
        public void PlacementRequiringHighFarmLevelFails()
        {
            _farmHouse.RequiredFarmLevel = 3;
            var result = PlacementValidator.ValidatePlacement(_farmHouse, 5, 5, new(), new(), 1);
            Assert.That(result, Is.EqualTo(PlacementState.RequiresUpgrade));
        }

        [Test]
        public void CollisionWithExistingBuildingReturnsBlockedByObject()
        {
            var existing = new List<(string id, float x, float y, int w, int h)>
            {
                ("storage", 5, 5, 2, 1)
            };

            var result = PlacementValidator.ValidatePlacement(_farmHouse, 4, 4, existing, new(), 1);
            Assert.That(result, Is.EqualTo(PlacementState.BlockedByObject));
        }

        [Test]
        public void CollisionWithLoreAnchorReturnsBlockedByLoreAnchor()
        {
            var anchors = new List<(float x, float y, int w, int h)>
            {
                (10, 10, 6, 6) // Fonte
            };

            var result = PlacementValidator.ValidatePlacement(_farmHouse, 8, 9, new(), anchors, 1);
            Assert.That(result, Is.EqualTo(PlacementState.BlockedByLoreAnchor));
        }

        [Test]
        public void EntranceClearanceBlockedByBuilding()
        {
            var existing = new List<(string id, float x, float y, int w, int h)>
            {
                ("obstacle", 5, 4, 1, 1) // fora do footprint, dentro da folga da entrada em (6,5)
            };

            var result = PlacementValidator.ValidatePlacement(_farmHouse, 5, 5, existing, new(), 1);
            Assert.That(result, Is.EqualTo(PlacementState.BlockedByPath));
        }

        [Test]
        public void BuildingWithNoEntranceHasNoClearanceRequirement()
        {
            var building = new BuildingDefinition("storage", "Storage Chest", 2, 1)
            {
                EntranceTiles = new() // Empty = no entrance
            };

            var result = PlacementValidator.ValidatePlacement(building, 5, 5, new(), new(), 1);
            Assert.That(result, Is.EqualTo(PlacementState.ValidPlacement));
        }
    }
}
