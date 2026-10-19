using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.City.Interiors;
using CindarsHope.City.Services;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.Editor.Validation;
using CindarsHope.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.City
{
    [TestFixture]
    public sealed class TownInteriorCatalogTests
    {
        private TownInteriorDefinitionSO[] definitions;

        [SetUp] public void SetUp() => definitions=GenerateTownInteriorCatalog.BuildDefinitions();
        [TearDown] public void TearDown()
        {
            foreach(var definition in definitions)
                if(definition!=null)UnityEngine.Object.DestroyImmediate(definition);
        }

        [Test] public void CanonicalCensusAndOwnership_AreExact()
        {
            GenerateTownInteriorCatalog.ValidateManifest(definitions);
            Assert.AreEqual(24,definitions.Select(d=>d.HouseId).Distinct().Count());
            Assert.AreEqual(21,definitions.Count(d=>d.ResidentNpcIds.Length>0));
            Assert.AreEqual(23,definitions.Sum(d=>d.ResidentNpcIds.Length));
            foreach(var home in GenerateTownInteriorCatalog.CanonicalHomes)
                CollectionAssert.Contains(definitions.Single(d=>d.HouseId==home.Value).ResidentNpcIds,home.Key);
            foreach(string house in new[]{"House_Chamber","House_Prison","House_MarketHall"})
                Assert.IsEmpty(definitions.Single(d=>d.HouseId==house).ResidentNpcIds);
        }

        [Test] public void SpatialProgram_HasDistinctRoomsAndIndividualProps()
        {
            GenerateTownInteriorCatalog.ValidateManifest(definitions);
            var inn=definitions.Single(d=>d.HouseId=="House_Inn");
            foreach(string id in new[]{"guest_01","guest_02","orlan_bedroom","gruta_bedroom"})
            {
                var bounds=inn.Rooms.Single(r=>r.RoomId==id).LocalFloorBounds;
                Assert.AreEqual(6f,bounds.width);Assert.AreEqual(6f,bounds.height);
            }
            Assert.AreEqual(4,inn.Rooms.Count(r=>r.RoomId.Contains("bedroom")||r.RoomId.StartsWith("guest_",StringComparison.Ordinal)));
            var carvalho=definitions.Single(d=>d.HouseId=="House_CarvalhoTorto");
            foreach(string id in new[]{"hund_bedroom","gurd_bedroom"})
            {
                var bounds=carvalho.Rooms.Single(r=>r.RoomId==id).LocalFloorBounds;
                Assert.AreEqual(9f,bounds.width);Assert.AreEqual(5f,bounds.height);
            }
            var temple=definitions.Single(d=>d.HouseId=="House_Temple");
            Assert.AreEqual(4,temple.Props.Count(p=>p.PropId.Contains("/bench_")));
            Assert.IsTrue(temple.Rooms.Any(r=>r.RoomId=="sacristy"));
            Assert.IsTrue(temple.Rooms.Any(r=>r.RoomId=="consultation"));
        }

        [Test] public void ShopAndServiceBindings_MatchExistingData()
        {
            GenerateTownInteriorCatalog.ValidateSourceContracts(definitions);
            GenerateTownInteriorCatalog.ValidateProviderAssets(definitions);
            Assert.AreEqual(13,definitions.Sum(d=>d.PrimaryIndoorProviderNpcIds.Length));
            Assert.AreEqual(10,definitions.Sum(d=>d.PrimaryOutdoorProviderNpcIds.Length)+
                GenerateTownInteriorCatalog.UnhousedOutdoorProviderNpcIds.Length);
            foreach(string npc in GenerateTownInteriorCatalog.CanonicalProviderVenues.Keys)
            {
                string path="Assets/_Game/Data/Economy/Shop_"+char.ToUpperInvariant(npc[4])+npc.Substring(5)+".asset";
                var shop=AssetDatabase.LoadAssetAtPath<ShopDataSO>(path);
                Assert.NotNull(shop,npc+" source ShopDataPath");
                Assert.AreEqual("shop_"+npc.Substring(4),shop.Id,npc+" source ShopId");
                Assert.AreEqual(npc,shop.NpcId);
            }
            var registry=definitions.Single(d=>d.HouseId=="House_Registry");
            var tovin=definitions.Single(d=>d.HouseId=="House_Tovin");
            CollectionAssert.Contains(registry.ExistingServiceIds,CityServiceCatalog.FarmRegistryService.ServiceId);
            CollectionAssert.Contains(tovin.ExistingServiceIds,CityServiceCatalog.MarketStallService.ServiceId);
            Assert.IsTrue(registry.Props.Any(p=>p.ExistingInteractionId==CityServiceCatalog.FarmRegistryService.ServiceId));
            Assert.IsTrue(tovin.Props.Any(p=>p.ExistingInteractionId==CityServiceCatalog.MarketStallService.ServiceId));
        }

        [Test] public void PublicEntryPolicy_DiffersFromShopServiceSchedule()
        {
            var temple=definitions.Single(d=>d.HouseId=="House_Temple");
            Assert.AreEqual(6,temple.Access.CivicStartHour);
            Assert.AreEqual(22,temple.Access.CivicEndHour);
            Assert.IsFalse(temple.Access.CommercialEntryFollowsProviderSchedule);
            var inn=definitions.Single(d=>d.HouseId=="House_Inn");
            Assert.IsTrue(inn.Access.PublicLobbyAlwaysOpen);
            Assert.IsFalse(inn.Access.CommercialEntryFollowsProviderSchedule);
            Assert.IsTrue(definitions.Single(d=>d.HouseId=="House_Bakery").Access.CommercialEntryFollowsProviderSchedule);
        }

        [Test] public void ValidatorRejectsOwnershipRoomPropAndBindingRegressions()
        {
            ExpectRejected("House_Inn",d=>d.Rooms=d.Rooms.Where(r=>r.RoomId!="guest_02").ToArray());
            ExpectRejected("House_Temple",d=>d.Props=d.Props.Where(p=>!p.PropId.EndsWith("/bench_04",StringComparison.Ordinal)).ToArray());
            ExpectRejected("House_Temple",d=>d.Access.CommercialEntryFollowsProviderSchedule=true);
            ExpectRejected("House_Registry",d=>d.ExistingServiceIds=d.ExistingServiceIds.Where(id=>id!=CityServiceCatalog.FarmRegistryService.ServiceId).ToArray());
            ExpectRejected("House_Blacksmith",d=>d.Props.Single(p=>p.PropId.EndsWith("/Station_Forge",StringComparison.Ordinal)).ExistingInteractionId="Station_Forge");
            ExpectRejected("House_CarvalhoTorto",d=>d.Rooms.Single(r=>r.RoomId=="hund_bedroom").LocalFloorBounds=new Rect(-9,2,8,5));
            ExpectRejected("House_MarketHall",d=>d.Props[0].LocalPosition=new Vector2(100,100));
        }

        [Test] public void GeneratorIsGuidPreservingAndIdempotent()
        {
            GenerateTownInteriorCatalog.Generate();
            ValidateTownInteriorCatalog.Validate();
            var before=new Dictionary<string,string>(StringComparer.Ordinal);
            foreach(var definition in definitions)
            {
                string path=GenerateTownInteriorCatalog.PathFor(definition.HouseId);
                before.Add(path,AssetDatabase.AssetPathToGUID(path)+"|"+
                    JsonUtility.ToJson(AssetDatabase.LoadAssetAtPath<TownInteriorDefinitionSO>(path)));
            }
            before.Add(GenerateTownInteriorCatalog.CatalogPath,
                AssetDatabase.AssetPathToGUID(GenerateTownInteriorCatalog.CatalogPath)+"|"+
                JsonUtility.ToJson(AssetDatabase.LoadAssetAtPath<TownInteriorCatalogSO>(GenerateTownInteriorCatalog.CatalogPath)));
            GenerateTownInteriorCatalog.Generate();
            ValidateTownInteriorCatalog.Validate();
            foreach(var entry in before)
            {
                var asset=AssetDatabase.LoadMainAssetAtPath(entry.Key);
                Assert.AreEqual(entry.Value,AssetDatabase.AssetPathToGUID(entry.Key)+"|"+JsonUtility.ToJson(asset),entry.Key);
            }
        }

        private void ExpectRejected(string houseId,Action<TownInteriorDefinitionSO> mutate)
        {
            var fresh=GenerateTownInteriorCatalog.BuildDefinitions();
            try
            {
                mutate(fresh.Single(d=>d.HouseId==houseId));
                var error=Assert.Throws<InvalidOperationException>(()=>GenerateTownInteriorCatalog.ValidateManifest(fresh));
                StringAssert.Contains(houseId,error.Message);
            }
            finally{foreach(var definition in fresh)UnityEngine.Object.DestroyImmediate(definition);}
        }
    }
}
