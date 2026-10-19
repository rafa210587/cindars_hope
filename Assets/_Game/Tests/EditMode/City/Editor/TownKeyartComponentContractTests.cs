using System;
using System.Collections.Generic;
using CindarsHope.Editor.SceneCreation;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>
    /// Staged deterministic regression contracts. No GameObjects, lifecycle or Unity physics execution.
    /// Proposed promoted path: Assets/_Game/Tests/EditMode/City/Editor/TownKeyartComponentContractTests.cs.
    /// </summary>
    [TestFixture]
    public sealed class TownKeyartComponentContractTests
    {
        [Test]
        public void SceneIdentity_UnsavedCompleteTownDoesNotRequireNameOrFirstSave()
        {
            var houses = CanonicalHouses();
            Assert.IsNull(TownKeyartComponentPreflight.SceneIdentityIssue("",Roots(),houses,houses));
        }

        [TestCase("Assets/_Game/Scenes/FarmScene.unity")]
        [TestCase("Assets/_Game/Scenes/CaveScene.unity")]
        public void SceneIdentity_OtherSavedSceneCannotPassWithTownLikeNames(string path)
        {
            var houses = CanonicalHouses();
            Assert.IsNotNull(TownKeyartComponentPreflight.SceneIdentityIssue(path,Roots(),houses,houses));
        }

        [Test]
        public void SceneIdentity_TwentyFourNamesDoNotHideDuplicatedOrWrongHouseId()
        {
            var expected = CanonicalHouses(); var materialized = new List<string>(expected);
            materialized[3] = materialized[4];
            Assert.IsNotNull(TownKeyartComponentPreflight.SceneIdentityIssue("",Roots(),materialized,expected));
            materialized[3] = "House_Farm";
            Assert.IsNotNull(TownKeyartComponentPreflight.SceneIdentityIssue("",Roots(),materialized,expected));
        }

        [Test]
        public void SceneIdentity_MissingOrDuplicateTownRootRejectsUnsavedScene()
        {
            var houses = CanonicalHouses(); var roots = Roots(); roots.Remove("TownTrees");
            Assert.IsNotNull(TownKeyartComponentPreflight.SceneIdentityIssue("",roots,houses,houses));
            roots = Roots(); roots.Add("TownProps");
            Assert.IsNotNull(TownKeyartComponentPreflight.SceneIdentityIssue("",roots,houses,houses));
        }

        [Test]
        public void BlockingFacts_PlainStaticSupportCanSeparateCanonicalDynamicPlayer()
        {
            Assert.IsNull(TownKeyartComponentPreflight.BlockingIssue(WorkingPair()));
        }

        [TestCase("attached Rigidbody")]
        [TestCase("composite support")]
        [TestCase("effector support")]
        [TestCase("kinematic or unsimulated Player")]
        [TestCase("trigger Player")]
        [TestCase("disabled support")]
        [TestCase("auto tiled support")]
        public void BlockingFacts_GeometryAloneDoesNotAcceptUnsupportedBodyResponse(string mutation)
        {
            var facts = WorkingPair();
            switch (mutation)
            {
                case "attached Rigidbody": facts.solidHasRigidbody = true; break;
                case "composite support": facts.solidComposite = true; break;
                case "effector support": facts.solidEffector = true; break;
                case "kinematic or unsimulated Player": facts.playerDynamicSimulated = false; break;
                case "trigger Player": facts.playerTrigger = true; break;
                case "disabled support": facts.solidEnabled = false; break;
                case "auto tiled support": facts.solidAutoTiling = true; break;
                default: Assert.Fail("Unknown scenario"); break;
            }
            Assert.IsNotNull(TownKeyartComponentPreflight.BlockingIssue(facts),mutation);
        }

        [TestCase("layer matrix ignores pair")]
        [TestCase("explicit collider pair ignored")]
        [TestCase("collider or body layer override")]
        [TestCase("solid layer not simulated")]
        [TestCase("player layer not simulated")]
        [TestCase("solid cannot send force")]
        [TestCase("player cannot receive force")]
        public void BlockingFacts_AlignedShapeStillFailsWhenContactOrSeparationIsFiltered(string mutation)
        {
            var facts = WorkingPair();
            switch (mutation)
            {
                case "layer matrix ignores pair": facts.layersIgnored = true; break;
                case "explicit collider pair ignored": facts.pairIgnored = true; break;
                case "collider or body layer override": facts.layerOverrides = true; break;
                case "solid layer not simulated": facts.solidLayerSimulated = false; break;
                case "player layer not simulated": facts.playerLayerSimulated = false; break;
                case "solid cannot send force": facts.solidSendsToPlayer = false; break;
                case "player cannot receive force": facts.playerReceivesFromSolid = false; break;
                default: Assert.Fail("Unknown scenario"); break;
            }
            Assert.IsNotNull(TownKeyartComponentPreflight.BlockingIssue(facts),mutation);
        }

        [Test]
        public void SupportOwnership_AncestorOrSiblingSupportCannotBeDuplicatedByGeometryAuthoring()
        {
            Assert.IsTrue(TownKeyartComponentPreflight.RequiresExplicitSupportBinding(false,true,false,false),"An ancestor remains ownership even without a bounds hit.");
            Assert.IsTrue(TownKeyartComponentPreflight.RequiresExplicitSupportBinding(false,false,false,true),"Overlapping unbound sibling support requires a binding.");
            Assert.IsFalse(TownKeyartComponentPreflight.RequiresExplicitSupportBinding(true,false,false,true),"Reapply may update its own dedicated shape.");
            Assert.IsFalse(TownKeyartComponentPreflight.RequiresExplicitSupportBinding(false,false,true,true),"Another verified measured prop is independent ownership.");
        }

        [TestCase(-1f,1f,1f)]
        [TestCase(1f,-1f,1f)]
        [TestCase(1f,1f,-1f)]
        [TestCase(0f,1f,1f)]
        public void Presentation_NegativeOrDegenerateLocalParentIsRejectedBeforeUniformSizing(float x,float y,float z)
        {
            Assert.IsFalse(TownKeyartComponentPreflight.SupportedLocalTransform(new Vector3(x,y,z),Quaternion.identity));
        }

        [Test]
        public void Presentation_RotationAndTwoCancellingNegativeParentsRemainUnsupported()
        {
            Assert.IsFalse(TownKeyartComponentPreflight.SupportedLocalTransform(Vector3.one,Quaternion.Euler(0,0,5)));
            var reflectedParent = new Vector3(-1,1,1);
            Assert.IsFalse(TownKeyartComponentPreflight.SupportedLocalTransform(reflectedParent,Quaternion.identity),
                "Each local parent is checked even when two reflections produce positive final lossyScale.");
            Assert.IsFalse(TownKeyartComponentPreflight.SupportedLocalTransform(new Vector3(float.NaN,1,1),Quaternion.identity));
            Assert.IsTrue(TownKeyartComponentPreflight.SupportedLocalTransform(new Vector3(2,3,1),Quaternion.identity),
                "Positive nonuniform prop ancestors can be compensated to a uniform world sprite.");
        }

        [Test]
        public void Presentation_BaseOverlapFarFromDoorRoutesStillProducesConflict()
        {
            var first = Furniture("A",new Rect(12,15,2,1));
            var second = Furniture("B",new Rect(13,15.5f,2,1));
            var conflicts = TownKeyartInteriorPresentation.PairwiseSupportConflicts(new[] { first,second });
            Assert.AreEqual(1,conflicts.Count);
            StringAssert.Contains("A and B",conflicts[0]);
        }

        [Test]
        public void Presentation_FlatRugDoesNotBlockFurnitureAndTouchingEdgesAreNotOverlap()
        {
            var first = Furniture("A",new Rect(0,0,2,1));
            var adjacent = Furniture("B",new Rect(2,0,2,1));
            var rug = Furniture("Furniture_Rug");
            Assert.IsEmpty(TownKeyartInteriorPresentation.PairwiseSupportConflicts(new[] { first,adjacent,rug }));
        }

        [Test]
        public void Presentation_SecondMachineSupportAlsoParticipatesInPairwiseCheck()
        {
            var forge = Furniture("Forge",new Rect(0,0,1,1),new Rect(3,0,1,1));
            var table = Furniture("Table",new Rect(3.5f,.5f,1,1));
            Assert.AreEqual(1,TownKeyartInteriorPresentation.PairwiseSupportConflicts(new[] { forge,table }).Count);
        }

        [Test]
        public void Report_ReverseCensusSolidsCountAsPendingAndRecountDoesNotAccumulate()
        {
            var report = new TownKeyartComponentPhysics.Report();
            report.entries.Add(new TownKeyartComponentPhysics.Entry { category = "TreeTrunk",state = "PASS_MEASURED_SUPPORT" });
            report.entries.Add(new TownKeyartComponentPhysics.Entry { category = "ExistingSolidUnmapped",state = "PENDING_GROUPED_SUPPORT" });
            report.entries.Add(new TownKeyartComponentPhysics.Entry { category = "Station",state = "PENDING_PRESENTATION_IDENTITY" });
            report.entries.Add(new TownKeyartComponentPhysics.Entry { category = "FloorRug",state = "EXEMPT" });
            TownKeyartComponentPhysics.Recount(report);
            TownKeyartComponentPhysics.Recount(report);
            Assert.AreEqual(1,report.measuredPass); Assert.AreEqual(2,report.pending);
            Assert.AreEqual(1,report.unmappedSolids); Assert.AreEqual(1,report.exemptions);
            Assert.AreEqual(4,report.categories.Count); Assert.AreEqual("PENDING",report.c5Global);
            report.entries.Add(new TownKeyartComponentPhysics.Entry { category = "Bench",state = "FAIL_MEASURED_SUPPORT" });
            TownKeyartComponentPhysics.Recount(report);
            Assert.AreEqual("FAIL",report.c5Global);
        }

        [Test]
        public void CensusComparison_RequiresExactEqualityAndReportsDiagnostics()
        {
            var result = TownKeyartComponentPhysics.CompareCensusIds(
                new[] { "tree_a", "door_a", "door_a" }, new[] { "tree_a", "crate_a" });
            Assert.IsFalse(result.IsEqual);
            CollectionAssert.AreEqual(new[] { "door_a" }, result.MissingIds);
            CollectionAssert.AreEqual(new[] { "crate_a" }, result.UnexpectedIds);
            CollectionAssert.AreEqual(new[] { "door_a" }, result.DuplicateEligibleIds);
            StringAssert.Contains("eligibleIds != censusIds", result.Diagnostics);
        }

        [Test]
        public void CensusComparison_ExactSetsPassWithoutDuplicates()
        {
            var result = TownKeyartComponentPhysics.CompareCensusIds(new[] { "a", "b" }, new[] { "b", "a" });
            Assert.IsTrue(result.IsEqual);
            Assert.AreEqual("eligibleIds == censusIds", result.Diagnostics);
        }

        [Test]
        public void FloorExemption_OnlyKnownTownFloorsAreWalkableByAssetContract()
        {
            const string root = TownKeyartComponentSupportCatalog.WorldRoot;
            Assert.IsTrue(TownKeyartComponentPreflight.IsKnownWalkableFloorAsset(root + "tiles/ground_deck.png"));
            Assert.IsTrue(TownKeyartComponentPreflight.IsKnownWalkableFloorAsset(root + "tiles/ground_grass.png"));
            Assert.IsFalse(TownKeyartComponentPreflight.IsKnownWalkableFloorAsset(root + "tiles/unknown_solid_wall.png"));
            Assert.IsFalse(TownKeyartComponentPreflight.IsKnownWalkableFloorAsset(root + "tiles/ground_water_keyart_v4.png"));
        }

        private static List<string> Roots() => new List<string> { "TownHouses","TownTrees","TownProps","CentralPlaza" };
        private static List<string> CanonicalHouses()
        {
            var result = new List<string>(); foreach (var lot in TownCityLayout.AllBuildings) result.Add(lot.Name); return result;
        }
        private static TownKeyartInteriorPresentation.Placement Furniture(string name, params Rect[] supports) =>
            new TownKeyartInteriorPresentation.Placement { house = "House_Test",prop = name,supportWorldRects = supports };
        private static TownKeyartComponentPreflight.BlockingFacts WorkingPair() => new TownKeyartComponentPreflight.BlockingFacts
        {
            solidActive = true, solidEnabled = true, playerActive = true, playerEnabled = true, playerDynamicSimulated = true,
            solidLayerSimulated = true, playerLayerSimulated = true, solidSendsToPlayer = true, playerReceivesFromSolid = true
        };
    }
}
