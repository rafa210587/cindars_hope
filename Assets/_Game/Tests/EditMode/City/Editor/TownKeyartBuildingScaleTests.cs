using CindarsHope.Editor.Art;
using CindarsHope.Editor.SceneCreation;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.City
{
    [TestFixture]
    public class TownKeyartBuildingScaleTests
    {
        [Test]
        public void EveryHouseAndHall_EnlargesRevision04WidthByTwentyPercent()
        {
            Assert.AreEqual(24,TownCityLayout.AllBuildings.Count);
            Assert.AreEqual(1.20f,TownKeyartBuildingArt.VisualEnlargement,.00001f);
            foreach(var lot in TownCityLayout.AllBuildings)
            {
                float baseline=TownKeyartBuildingArt.Revision04VisibleWidth(lot.Name,lot.Size);
                Assert.Greater(baseline,0,lot.Name);
                Assert.AreEqual(baseline*1.20f,TownKeyartBuildingArt.VisibleWidth(lot.Name,lot.Size),.0001f,lot.Name);
            }
            Assert.AreEqual(28.8f,TownKeyartBuildingArt.VisibleWidth("TownHallBuilding",TownKeyartGeometry.HallFootprintSize),.0001f);
            Assert.AreEqual(160,TownDistrictLayout.WidthTiles);
            Assert.AreEqual(112,TownDistrictLayout.HeightTiles);
        }

        [TestCase("House_Fishery","town_watermill_keyart",12f,39f,119f)]
        [TestCase("House_Bakery","town_bakery_keyart",10f,67f,147f)]
        [TestCase("House_AlchemyLab","town_alchemy_keyart",10f,76f,195f)]
        [TestCase("House_AnimalYard","town_animal_shed_keyart",7f,55f,118f)]
        [TestCase("House_Residential_1","town_house_red_gable_keyart",8f,282f,598f)]
        public void Fit_UsesEnlargementWithoutOverridingMeasuredDoorAnchor(
            string house,string asset,float baseline,float pixelX,float pixelY)
        {
            var sprite=WorldSpriteLibrary.Building(asset);
            Assert.IsNotNull(sprite,asset);
            Assert.IsTrue(TownCityLayout.TryGetBuilding(house,out var lot));
            var go=new GameObject("Scale06Test");
            try
            {
                var renderer=go.AddComponent<SpriteRenderer>();
                var anchor=house=="House_AnimalYard"?new Vector2(1,1):new Vector2(.25f,-lot.Size.y*.5f);
                TownKeyartBuildingArt.Fit(renderer,house,sprite,anchor,lot.Size);
                var bounds=TownKeyartSceneArt.OpaqueBounds(sprite);
                Assert.AreEqual(baseline*1.20f,bounds.width*go.transform.localScale.x,.0001f,house);
                Assert.AreEqual(go.transform.localScale.x,go.transform.localScale.y,.00001f,"No axis stretching.");
                Vector2 spriteDoor=new Vector2(pixelX-sprite.pivot.x,sprite.rect.height-pixelY-sprite.pivot.y)/sprite.pixelsPerUnit;
                Vector2 fitted=(Vector2)go.transform.localPosition+spriteDoor*go.transform.localScale.x;
                Assert.Less(Vector2.Distance(anchor,fitted),.0001f,"Art anchor drifted away from the physical doorway.");
            }
            finally{Object.DestroyImmediate(go);}
        }
    }
}
