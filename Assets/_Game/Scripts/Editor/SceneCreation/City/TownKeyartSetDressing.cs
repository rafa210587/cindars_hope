using System.Collections.Generic;
using CindarsHope.Editor.Art;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Final Town-only set dressing follows physical supports and deliberately grouped gardens.</summary>
    public static class TownKeyartSetDressing
    {
        public static void Apply(Scene scene)
        {
            Physics2D.SyncTransforms();
            var all = new List<Transform>();
            foreach (var root in scene.GetRootGameObjects()) all.AddRange(root.GetComponentsInChildren<Transform>(true));
            foreach (var t in all)
            {
                var r = t.GetComponent<SpriteRenderer>();
                if (r == null || r.sprite == null) continue;
                string n = t.name;
                bool replaced = n.StartsWith("CivicPlanter") || n.StartsWith("CivicLowWall_") ||
                    n.StartsWith("CivicGarden_") || n.StartsWith("PlazaFlowerPatch_") ||
                    n.StartsWith("SemanticPlaceholder_") || n.StartsWith("SouthBanner") ||
                    n.StartsWith("AnimalPenFence_") || n.StartsWith("CorralFence") ||
                    n.StartsWith("Merlon_") || n == "TownHallMural";
                if (replaced)
                {
                    if(HasSolidInOwnership(t))
                        Debug.LogWarning("[TownSetDressing] Retained visible legacy physical support until an explicit replacement is wired: "+n);
                    else { r.enabled=false; continue; }
                }
                if (n.Contains("Fence") && r.bounds.size.y > 1.1f)
                    FitHeight(r, 1.1f);
                if (n.StartsWith("CorralCow") || n == "YardCow") FitHeight(r, 1.5f);
                if (n.StartsWith("CorralSheep")) FitHeight(r, .9f);
                // The waterfall has an authored water-outflow anchor, not terrestrial garden support.
                if (n == "WestWaterfall") continue;
                if (!r.enabled || !IsLooseTownDecoration(t)) continue;
                if(n.Contains("Bench"))FitHeight(r,1.3f);
                if(n.StartsWith("KeyartLamp_") || n.StartsWith("Streetlamp_"))FitHeight(r,3.8f);
                if(HasSolidInOwnership(t))continue;
                var support=new Vector2(r.bounds.center.x,r.bounds.min.y);
                bool invalid=TownKeyartGeometry.ContainsWater(support,.3f) ||
                    !TownKeyartGeometry.ClearPoint(support,TownCityLayout.AllBuildings,.1f) ||
                    TownCityLayout.IsPointOnRoad(support,.35f) || ObscuresDoor(support,r.bounds.size);
                if(!invalid)continue;
                // Relocate replaceable loose dressing only to authored garden groups, never the nearest
                // arbitrary free cell. The group choice follows its original neighbourhood.
                Vector2[] gardens={new Vector2(-21,20),new Vector2(52,30),new Vector2(-25,-12),
                    new Vector2(26,-23),new Vector2(70,-42),new Vector2(-24,-29),new Vector2(-21,-44)};
                float best=float.MaxValue;Vector2 target=support;bool found=false;
                foreach(var center in gardens)
                for(int i=0;i<6;i++)
                {
                    var p=center+new Vector2((i%3-1)*1.6f,(i/3-.5f)*1.5f);
                    if(!TownKeyartGeometry.ClearPoint(p,TownCityLayout.AllBuildings,.65f) ||
                        TownCityLayout.IsPointOnRoad(p,.8f) || ObscuresDoor(p,r.bounds.size))continue;
                    float distance=(p-support).sqrMagnitude;
                    if(distance>=best)continue;best=distance;target=p;found=true;
                }
                if(found)t.position+=(Vector3)(target-support);
                else Debug.LogWarning("[TownSetDressing] No approved garden support for "+n+"; retained for review.");
            }
            BuildCivicRing(scene);
            DressAnimalYard(scene);
            AlignWallSupports(scene);
            BuildRockBanks(scene);
            ClearTempleApproach(scene);
        }

        private static bool IsLooseTownDecoration(Transform t)
        {
            if (t.GetComponentInParent<Collider2D>() != null) return false;
            for (var parent=t.parent; parent!=null; parent=parent.parent)
                if (parent.name=="TownProps" || parent.name=="TownKeyartGardens") return true;
            return false;
        }

        private static bool ObscuresDoor(Vector2 support, Vector3 size)
        {
            foreach (var lot in TownCityLayout.AllBuildings)
            {
                var visible = new Rect(support-new Vector2(size.x*.5f,0),new Vector2(size.x,size.y));
                if (TownAccessMetrics.OverlapArea(visible,TownAccessMetrics.DoorApron(lot))>.0001f) return true;
            }
            Vector3 mural=TownDistrictLayout.TownHallMural;
            return Mathf.Abs(support.x-mural.x)<size.x*.5f+2f &&
                support.y<mural.y+1f && support.y+size.y>mural.y-1.4f;
        }

        private static void FitHeight(SpriteRenderer r,float height)
        {
            var support=new Vector3(r.bounds.center.x,r.bounds.min.y,0);
            if(r.bounds.size.y>.001f)r.transform.localScale*=height/r.bounds.size.y;
            r.transform.position+=support-new Vector3(r.bounds.center.x,r.bounds.min.y,0);
        }

        private static bool HasSolidInOwnership(Transform t)
        {
            foreach(var c in t.GetComponentsInChildren<Collider2D>(true))
                if(c.enabled&&!c.isTrigger)return true;
            for(var p=t.parent;p!=null;p=p.parent)
                foreach(var c in p.GetComponents<Collider2D>())if(c.enabled&&!c.isTrigger)return true;
            return false;
        }

        private static void BuildCivicRing(Scene scene)
        {
            var root=new GameObject("TownCivicRingV4");SceneManager.MoveGameObjectToScene(root,scene);
            var lawn=TownKeyartGround.Layer(root.transform,"CivicRaisedGardens",6);
            var grass=TownKeyartGround.TileFor(WorldSpriteLibrary.Ground("ground_grass"));
            string[] directions={"ne","nw","sw","se"};
            Vector2[] origins={new Vector2(7.65625f,1.59375f),new Vector2(-7.625f,1.59375f),
                new Vector2(-7.625f,-9.21875f),new Vector2(7.65625f,-9.21875f)};
            for(int q=0;q<4;q++)
            {
                var polygon=TownKeyartGeometry.PlazaGardenPolygons[q];
                TownKeyartGround.PaintPolygon(lawn,grass,polygon);
                var solid=new GameObject("RaisedGarden_"+q);solid.transform.SetParent(root.transform,false);
                solid.AddComponent<PolygonCollider2D>().points=polygon;
                var sprite=WorldSpriteLibrary.Prop("town_plaza_arc_"+directions[q]+"_keyart");
                if(sprite==null)throw new System.InvalidOperationException("Missing approved Town plaza arc "+directions[q]);
                // Aseprite master ring anchor, not alpha-AABB support: 32PPU, unit scale.
                var art=new GameObject("CivicContinuousArc_"+directions[q]);art.transform.SetParent(root.transform,false);
                art.transform.localPosition=(Vector3)(TownCityLayout.CentralPlazaCenter+origins[q]);
                var renderer=art.AddComponent<SpriteRenderer>();renderer.sprite=sprite;
                renderer.sortingLayerName="World";renderer.sortingOrder=0;renderer.spriteSortPoint=SpriteSortPoint.Pivot;
            }
        }

        private static void DressAnimalYard(Scene scene)
        {
            Transform yard=null;
            foreach(var root in scene.GetRootGameObjects())
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
                if(t.name=="House_AnimalYard")yard=t;
            if(yard==null)return;
            foreach(Transform child in yard)
            {
                if(child.name=="Bed" || child.name=="Table" || child.name.StartsWith("Furniture_"))
                {
                    foreach(var r in child.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        r.sprite=WorldSpriteLibrary.Prop(child.name=="Bed"?"hay_bale":"crate");
                        r.color=Color.white;r.transform.localScale=Vector3.one;
                        FitHeight(r,.65f);
                    }
                }
                if(!child.name.StartsWith("Wall_"))continue;
                foreach(var r in child.GetComponentsInChildren<SpriteRenderer>(true))r.enabled=false;
                var collider=child.GetComponent<BoxCollider2D>();
                if(collider==null)continue;
                // The retained divider and perimeter solids have small fence posts at their real supports.
                var b=collider.bounds;int count=Mathf.Max(2,Mathf.CeilToInt(Mathf.Max(b.size.x,b.size.y)/1.2f));
                for(int i=0;i<count;i++)
                {
                    float f=i/(float)(count-1);
                    var p=b.size.x>b.size.y?new Vector2(Mathf.Lerp(b.min.x,b.max.x,f),b.center.y):
                        new Vector2(b.center.x,Mathf.Lerp(b.min.y,b.max.y,f));
                    var art=TownKeyartSceneArt.SpriteObject(yard,"YardPhysicalFence_"+child.name+"_"+i,WorldSpriteLibrary.Prop("fence"),p,1.2f);
                    if(art!=null)FitHeight(art.GetComponent<SpriteRenderer>(),.85f);
                }
            }
        }

        private static void AlignWallSupports(Scene scene)
        {
            var all=new List<Transform>();
            foreach(var root in scene.GetRootGameObjects())all.AddRange(root.GetComponentsInChildren<Transform>(true));
            Transform wallRoot=null;
            foreach(var t in all)if(t.name=="TownKeyartStoneWall")wallRoot=t;
            if(wallRoot!=null)
            {
                foreach(float x in new[]{-7.2f,7.2f})
                {
                    var connector=TownKeyartSceneArt.SpriteObject(wallRoot,"SouthStoneWall_GateConnector_"+x,
                        WorldSpriteLibrary.Prop("town_low_wall_curve_keyart"),new Vector2(x,-TownDistrictLayout.HalfHeight+1.5f),4.8f);
                    if(connector!=null)all.Add(connector.transform);
                }
            }
            foreach(var t in all)
            {
                if(t.name=="GatePost_W" || t.name=="GatePost_E")
                {
                    // The two existing solid posts are moved to the feet of the visible pillars.
                    t.position=new Vector3(t.name.EndsWith("_W")?-4.7f:4.7f,-TownDistrictLayout.HalfHeight+1.4f,0);
                    var c=t.GetComponent<BoxCollider2D>();if(c!=null)c.size=new Vector2(2.7f,1f);
                    foreach(var r in t.GetComponentsInChildren<SpriteRenderer>(true))r.enabled=false;
                }
                if(t.name.StartsWith("SouthStoneWall_"))
                {
                    // Bottom bound remains the outer safety boundary; this wall has a local ground support.
                    var r=t.GetComponent<SpriteRenderer>();if(r==null)continue;
                    var c=t.gameObject.AddComponent<BoxCollider2D>();
                    c.size=new Vector2(r.bounds.size.x/Mathf.Abs(t.lossyScale.x),.55f/Mathf.Abs(t.lossyScale.y));
                    c.offset=new Vector2((r.bounds.center.x-t.position.x)/t.lossyScale.x,(r.bounds.min.y+.3f-t.position.y)/t.lossyScale.y);
                }
            }
        }

        private static void BuildRockBanks(Scene scene)
        {
            var root=new GameObject("TownNaturalBanksV3");SceneManager.MoveGameObjectToScene(root,scene);
            var rock=WorldSpriteLibrary.Prop("cliff_keyart_v1");
            for(int i=0;i<9;i++)
            {
                float y=-26+i*8.2f;
                var p=new Vector2(TownKeyartGeometry.RiverWestEdge(y)-.4f,y);
                TownKeyartSceneArt.SpriteObject(root.transform,"WestRockBank_"+i,rock,p,4.3f+(i%3)*.7f);
            }
            foreach(float x in new[]{-70f,-52f,-20f,20f,55f,73f})
                TownKeyartSceneArt.SpriteObject(root.transform,"NorthRockRise_"+x,rock,new Vector2(x,TownDistrictLayout.HalfHeight-6f+Mathf.Sin(x)*1.1f),7f);
            var bank = WorldSpriteLibrary.Prop("shore_bank_keyart_v1");
            var outline = TownKeyartGeometry.LakeOutline;
            for (int i = 0; i < outline.Length; i += 13)
            {
                var p = outline[i];
                if (p.x > -40f || (p.x > -62f && p.y < -33f && p.y > -40f)) continue;
                TownKeyartSceneArt.SpriteObject(root.transform, "LakeBankMass_" + i, bank, p, 3.5f + (i % 3) * .45f);
            }
            var cascade = WorldSpriteLibrary.Prop("river_cascade_keyart_v4");
            TownKeyartSceneArt.SpriteObject(root.transform, "WestCascadeSpill", cascade,
                new Vector2(TownKeyartGeometry.RiverCenter(6f), 5.2f), 5.8f, "World", 2);
            var pillar = WorldSpriteLibrary.Prop("town_gate_pillar_keyart");
            TownKeyartSceneArt.SpriteObject(root.transform, "SouthGateMaterialPillar_W", pillar,
                new Vector2(-6.4f, -TownDistrictLayout.HalfHeight + 1.2f), 5.1f, "World", 2);
            TownKeyartSceneArt.SpriteObject(root.transform, "SouthGateMaterialPillar_E", pillar,
                new Vector2(6.4f, -TownDistrictLayout.HalfHeight + 1.2f), 5.1f, "World", 2);
        }

        private static void ClearTempleApproach(Scene scene)
        {
            if (!TownCityLayout.TryGetBuilding("House_Temple", out var temple)) return;
            var apron = TownAccessMetrics.DoorApron(temple);
            apron.xMin -= 1.4f; apron.xMax += 1.4f; apron.yMin -= .6f; apron.yMax += .8f;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (!IsLooseTownDecoration(t)) continue;
                var renderer = t.GetComponent<SpriteRenderer>();
                if (renderer == null || !renderer.enabled) continue;
                var support = new Vector2(renderer.bounds.center.x, renderer.bounds.min.y);
                if (!apron.Contains(support)) continue;
                // Keep the physical temple/door untouched; only presentation props are removed
                // from the stair/door sightline when a later dressing pass lands there.
                renderer.enabled = false;
            }
        }
    }
}
