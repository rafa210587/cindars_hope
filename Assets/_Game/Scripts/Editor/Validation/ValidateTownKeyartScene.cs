using System;
using System.Collections.Generic;
using CindarsHope.Editor.SceneCreation;
using CindarsHope.NPC.Schedule;
using CindarsHope.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Saved-scene physics queries. These checks do not claim observed PlayMode behavior.</summary>
    public static class ValidateTownKeyartScene
    {
        public static void Validate()
        {
            var active=SceneManager.GetActiveScene();
            var scene=SceneManager.GetSceneByPath("Assets/_Game/Scenes/TownScene.unity");
            bool openedHere=!scene.isLoaded;
            if(openedHere)scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/TownScene.unity",OpenSceneMode.Additive);
            int passes=0,failures=0;
            try
            {
                var transforms=new Dictionary<string,Transform>();var colliders=new List<Collider2D>();var anchors=new List<NpcScheduleAnchor>();
                foreach(var root in scene.GetRootGameObjects())
                {
                    foreach(var t in root.GetComponentsInChildren<Transform>(true)) if(!transforms.ContainsKey(t.name)) transforms.Add(t.name,t);
                    colliders.AddRange(root.GetComponentsInChildren<Collider2D>(true));
                    anchors.AddRange(root.GetComponentsInChildren<NpcScheduleAnchor>(true));
                }
                Physics2D.SyncTransforms();
                int houseCount=0, npcCount=0, npcStallCount=0, marketStallCount=0, spawnCount=0;
                foreach (var pair in transforms)
                {
                    if (pair.Key.StartsWith("House_")) houseCount++;
                    if (pair.Key.StartsWith("Stall_npc_")) npcStallCount++;
                    if (pair.Key.StartsWith("MarketSquare_Stall_")) marketStallCount++;
                    if (pair.Key.StartsWith("Spawn_")) spawnCount++;
                    foreach (var behaviour in pair.Value.GetComponents<MonoBehaviour>())
                        if (behaviour != null && (behaviour.GetType().Name == "NpcController" ||
                            behaviour.GetType().Name == "NpcShopController")) npcCount++;
                }
                Check("Twenty-four materialized houses",houseCount==24,ref passes,ref failures);
                Check("Twenty-nine materialized NPCs",npcCount==29,ref passes,ref failures);
                Check("Eighty-four schedule anchors",anchors.Count==84,ref passes,ref failures);
                Check("Twenty-three NPC stalls",npcStallCount==23,ref passes,ref failures);
                Check("Six market stalls",marketStallCount==6,ref passes,ref failures);
                Check("Two spawn points",spawnCount==2,ref passes,ref failures);
                bool classificationValid = TownKeyartDoorClassification.Validate(out var classificationReason);
                Check("24/24 door/interior classifications (14/9/1)", classificationValid, ref passes, ref failures, classificationReason);
                bool hasOutflow=transforms.TryGetValue("WestWaterfallOutflow",out var outflow);
                bool outflowAligned=hasOutflow && outflow.parent!=null && outflow.parent.name=="WestWaterfall" &&
                    outflow.parent.GetComponent<SpriteRenderer>() is SpriteRenderer waterfall && waterfall.enabled &&
                    Mathf.Abs(outflow.position.y-6f)<.01f &&
                    Mathf.Abs(outflow.position.x-TownKeyartGeometry.RiverCenter(outflow.position.y))<.01f &&
                    TownKeyartGeometry.ContainsWater(outflow.position);
                Check("Authored waterfall spillway remains in the current",outflowAligned,ref passes,ref failures);
                int marketPropsChecked=0;
                foreach(var pair in transforms)
                {
                    if(!pair.Key.StartsWith("MarketGoods_")&&!pair.Key.StartsWith("MarketCommonsTable_"))continue;
                    marketPropsChecked++;var prop=pair.Value.GetComponent<SpriteRenderer>();
                    bool supported=prop!=null&&prop.enabled;
                    if(supported)
                        for(int sample=0;sample<=4;sample++)
                        {
                            var p=new Vector2(Mathf.Lerp(prop.bounds.min.x,prop.bounds.max.x,sample/4f),prop.bounds.min.y);
                            supported&=TownKeyartGeometry.ClearPoint(p,TownCityLayout.AllBuildings,.1f);
                        }
                    Check("Authored market prop has dry support "+pair.Key,supported,ref passes,ref failures);
                }
                Check("Six goods and two shared tables retain identity",marketPropsChecked==8,ref passes,ref failures);
                bool hasWater = transforms.TryGetValue("LakeWaterTiles",out var waterTransform);
                Check("Materialized lake tilemap exists",hasWater,ref passes,ref failures);
                if (hasWater)
                {
                    var map = waterTransform.GetComponent<Tilemap>();
                    Check("Water grid uses the same authored raster as paths and shore",map!=null && map.layoutGrid.cellSize==new Vector3(TownKeyartGround.CellSize,TownKeyartGround.CellSize,0),ref passes,ref failures);
                    Check("Lake painted southwest",map!=null && map.HasTile(map.WorldToCell(new Vector3(-60,-45))),ref passes,ref failures);
                    Check("Water is absent beneath central statue",map!=null && !map.HasTile(map.WorldToCell(new Vector3(0,4))),ref passes,ref failures);
                }
                ValidateMaterialComposition(transforms, ref passes, ref failures);
                int treeCount=0;
                int standardGroups=0,workCount=0;
                foreach(var collider in colliders)
                {
                    if(!collider.name.StartsWith("TownTree_"))continue;
                    treeCount++;var renderer=collider.GetComponent<SpriteRenderer>();
                    bool aligned=renderer!=null&&Mathf.Abs(collider.bounds.center.x-renderer.bounds.center.x)<.4f&&
                                 Mathf.Abs(collider.bounds.center.y-(renderer.bounds.min.y+.25f))<.4f&&collider.bounds.size.y<=.6f;
                    Check("Tree trunk support "+collider.name,aligned,ref passes,ref failures);
                }
                Check("Eight physical tree trunks",treeCount==8,ref passes,ref failures);
                foreach(var pair in transforms)
                {
                    if(!pair.Key.StartsWith("CivicStandard_")&&!pair.Key.StartsWith("MarketStandard_"))continue;
                    standardGroups++;
                    foreach(Transform child in pair.Value)
                        Check("Standard child remains local "+pair.Key+"/"+child.name,child.localPosition.sqrMagnitude<16,ref passes,ref failures);
                }
                Check("Six authored standard groups",standardGroups==6,ref passes,ref failures);
                foreach(var lot in TownCityLayout.AllBuildings)
                {
                    Check("Door approach clear "+lot.Name,ClearActor(lot.DoorApproach,colliders,null,out string blocker),ref passes,ref failures,blocker);
                    if(transforms.TryGetValue(lot.Name,out var house))
                    {
                        var reveal=house.GetComponentInChildren<RoofRevealController>(true);
                        bool configured=reveal!=null;
                        if(configured)
                        {
                            var config=new SerializedObject(reveal);
                            var interiors=config.FindProperty("_interiorRenderers");
                            var roofs=config.FindProperty("_roofRenderers");
                            configured=lot.Name=="House_AnimalYard"?roofs!=null&&roofs.arraySize==0:
                                interiors!=null&&interiors.arraySize>=4;
                            if(lot.Name!="House_AnimalYard"&&interiors!=null)
                                for(int i=0;i<interiors.arraySize;i++)
                                    configured&=interiors.GetArrayElementAtIndex(i).objectReferenceValue is SpriteRenderer hidden&&!hidden.enabled;
                        }
                        Check("Interior visibility wiring "+lot.Name,configured,ref passes,ref failures);
                    }
                    if(lot.Name=="House_GateKeeper")
                        Check("Gatekeeper usable interior",ClearActor(lot.Center,colliders,null,out blocker)&&lot.Size.x-2>=2&&lot.Size.y-2>=2,ref passes,ref failures,blocker);
                }
                foreach(var anchor in anchors)
                {
                    if(!anchor.AnchorId.EndsWith("_work"))continue;
                    workCount++;
                    Check("Work anchor clear "+anchor.AnchorId,ClearActor(anchor.transform.position,colliders,anchor.NpcId,out string blocker),ref passes,ref failures,blocker);
                }
                Check("Twenty-eight materialized work anchors",workCount==28,ref passes,ref failures);
                Check("Town hall mural exterior accessible",ClearActor(TownDistrictLayout.TownHallMural,colliders,null,out string muralBlocker),ref passes,ref failures,muralBlocker);
                Check("Dock deck physically dry and clear",ClearActor(TownKeyartGeometry.DockCenter,colliders,null,out string dockBlocker),ref passes,ref failures,dockBlocker);
                ValidateReachability(transforms,colliders,anchors,ref passes,ref failures);
                Debug.Log($"[TownKeyartPhysics] PASS:{passes} FAIL:{failures}; saved-scene closest-point circle intersections radius0.35, navigation grid0.5/edge sampling0.1; PlayMode NOT RUN.");
                int accessPasses=0,accessFailures=0;
                ValidateTownAccess(transforms,colliders,anchors,houseCount,npcCount,npcStallCount,marketStallCount,spawnCount,
                    ref accessPasses,ref accessFailures);
                Debug.Log($"[TownAccess] PASS:{accessPasses} FAIL:{accessFailures}");
                if(failures>0||accessFailures>0)throw new InvalidOperationException(
                    "Town keyart physical scene checks failed: legacy="+failures+", access="+accessFailures);
            }
            finally
            {
                if(openedHere&&scene.isLoaded)EditorSceneManager.CloseScene(scene,true);
                if(active.IsValid()&&active.isLoaded)SceneManager.SetActiveScene(active);
            }
        }

        private static void ValidateMaterialComposition(Dictionary<string,Transform> transforms, ref int passes, ref int failures)
        {
            int painted = 0, invalid = 0;
            foreach (var pair in transforms)
            {
                if (!pair.Key.StartsWith("MaterialPocket_", StringComparison.Ordinal)) continue;
                var map = pair.Value.GetComponent<Tilemap>();
                if (map == null) continue;
                foreach (var cell in map.cellBounds.allPositionsWithin)
                {
                    if (!map.HasTile(cell)) continue;
                    painted++;
                    var point = map.CellToWorld(cell) + new Vector3(TownKeyartGround.CellSize * .5f, TownKeyartGround.CellSize * .5f);
                    bool belongs = false;
                    foreach (var polygon in TownKeyartGeometry.MaterialPocketPolygons)
                        belongs |= TownKeyartGeometry.MaterialPocketPointAllowed(point, polygon, .35f, .45f);
                    if (!belongs) invalid++;
                }
            }
            float actualArea = painted * TownKeyartGround.CellSize * TownKeyartGround.CellSize;
            float expectedArea = TownKeyartGeometry.EffectiveMaterialPocketArea();
            Check($"Material pockets have effective painted area >=180 (actual={actualArea:0.##}, expected={expectedArea:0.##})",
                actualArea >= 180f && Mathf.Abs(actualArea - expectedArea) <= TownKeyartGround.CellSize * TownKeyartGround.CellSize * 8f,
                ref passes, ref failures);
            Check($"Material pocket cells do not invade filtered lots/roads/water (invalid={invalid})", invalid == 0, ref passes, ref failures);

            int decorative = 0, decorativeCollider = 0, physical = 0, physicalMissingCollider = 0;
            foreach (var pair in transforms)
            {
                if (pair.Key.StartsWith("MaterialPocketWall_", StringComparison.Ordinal))
                {
                    decorative++;
                    foreach (var collider in pair.Value.GetComponentsInChildren<Collider2D>(true))
                        if (collider.enabled && !collider.isTrigger) decorativeCollider++;
                }
                if (!pair.Key.StartsWith("SouthStoneWall_", StringComparison.Ordinal)) continue;
                physical++;
                bool hasCollider = false;
                foreach (var collider in pair.Value.GetComponentsInChildren<Collider2D>(true))
                    hasCollider |= collider.enabled && !collider.isTrigger;
                if (!hasCollider) physicalMissingCollider++;
            }
            Check($"Decorative pocket walls remain non-physical (count={decorative}, colliders={decorativeCollider})", decorative >= 2 && decorativeCollider == 0, ref passes, ref failures);
            Check($"South stone walls retain physical supports (count={physical})", physical > 0 && physicalMissingCollider == 0, ref passes, ref failures);

            Check("Market depth props are visible without physical occlusion", VisibleClear(transforms, "MarketDepthCanopy_", false) && VisibleClear(transforms, "MarketDepthCrate_", false), ref passes, ref failures);
            Check("Cascade spill remains visible over the authored current", VisibleClear(transforms, "WestCascadeSpill", false), ref passes, ref failures);
            Check("Gate material pillars do not narrow the south portal", VisibleClear(transforms, "SouthGateMaterialPillar_", false) &&
                TownCityLayout.IsPointOnRoad(new Vector3(0f, -52f), .5f), ref passes, ref failures);
        }

        private static bool VisibleClear(Dictionary<string,Transform> transforms, string prefix, bool drySupport)
        {
            bool found = false;
            foreach (var pair in transforms)
            {
                if (!pair.Key.StartsWith(prefix, StringComparison.Ordinal)) continue;
                found = true;
                var renderer = pair.Value.GetComponent<SpriteRenderer>();
                if (renderer == null || !renderer.enabled) return false;
                if (drySupport && !TownKeyartGeometry.ClearPoint(pair.Value.position, TownCityLayout.AllBuildings, .1f)) return false;
                foreach (var collider in pair.Value.GetComponentsInChildren<Collider2D>(true))
                    if (collider.enabled && !collider.isTrigger) return false;
            }
            return found;
        }

        private static bool ClearActor(Vector2 point,List<Collider2D> colliders,string npcId,out string blocker,
            float radius=.35f,Transform ignoredRoot=null,Collider2D ignoredCollider=null)
        {
            foreach(var collider in colliders)
            {
                if(!collider.enabled||collider.isTrigger||!collider.gameObject.activeInHierarchy)continue;
                if(collider==ignoredCollider)continue;
                if(ignoredRoot!=null&&collider.transform.IsChildOf(ignoredRoot))continue;
                // Actors themselves are not level geometry; retain all static walls, water and counter solids.
                if(collider.attachedRigidbody!=null&&collider.attachedRigidbody.bodyType!=RigidbodyType2D.Static)continue;
                var bounds=collider.bounds;bounds.Expand(radius*2);
                if(point.x<bounds.min.x||point.x>bounds.max.x||point.y<bounds.min.y||point.y>bounds.max.y)continue;
                Vector2 closest=collider.ClosestPoint(point);
                if((closest-point).sqrMagnitude<radius*radius){blocker=collider.name;return false;}
            }
            blocker=null;return true;
        }

        private static void ValidateTownAccess(Dictionary<string,Transform> transforms,List<Collider2D> colliders,
            List<NpcScheduleAnchor> anchors,int houseCount,int npcCount,int npcStallCount,int marketStallCount,int spawnCount,
            ref int passes,ref int failures)
        {
            int treeCount=0;
            foreach(var pair in transforms)if(pair.Key.StartsWith("TownTree_",StringComparison.Ordinal))treeCount++;
            AccessCheck("preservation",houseCount==24&&npcCount==29&&anchors.Count==84&&npcStallCount==23&&
                marketStallCount==6&&spawnCount==2&&treeCount>=TownAccessMetrics.MinimumMaterializedTrees,
                $"houses={houseCount} npcs={npcCount} anchors={anchors.Count} npcStalls={npcStallCount} marketStalls={marketStallCount} spawns={spawnCount} trees={treeCount}",ref passes,ref failures);

            bool gap=true,facade=true,wall=true;
            var lots=TownCityLayout.AllBuildings;
            var visuals=new Dictionary<string,List<Rect>>(StringComparer.Ordinal);
            foreach(var lot in lots)
            {
                if(transforms.TryGetValue(lot.Name,out var visualRoot)&&TryVisualBounds(visualRoot,out var shape))
                    visuals[lot.Name]=shape;
                else
                {
                    facade=false;
                    Debug.LogError($"[TownAccess][visualFacade] lot={lot.Name} missingOpaqueRenderers=true");
                }
            }
            for(int i=0;i<lots.Count;i++)
            {
                var lot=lots[i];
                float lotWallMargin=Mathf.Min(Mathf.Min(lot.MinX+TownDistrictLayout.HalfWidth,
                    TownDistrictLayout.HalfWidth-lot.MaxX),Mathf.Min(lot.MinY+TownDistrictLayout.HalfHeight,
                    TownDistrictLayout.HalfHeight-lot.MaxY));
                if(lotWallMargin<TownAccessMetrics.VisualWallMargin-.001f)
                {
                    wall=false;
                    Debug.LogError($"[TownAccess][wallMargin] lot={lot.Name} kind=physical actual={lotWallMargin:0.###} required={TownAccessMetrics.VisualWallMargin:0.###}");
                }
                bool hasTargetVisual=visuals.TryGetValue(lot.Name,out var visual);
                if(hasTargetVisual)
                {
                    Rect bounds=BoundsOf(visual);
                    float visualWallMargin=Mathf.Min(Mathf.Min(bounds.xMin+TownDistrictLayout.HalfWidth,
                        TownDistrictLayout.HalfWidth-bounds.xMax),Mathf.Min(bounds.yMin+TownDistrictLayout.HalfHeight,
                        TownDistrictLayout.HalfHeight-bounds.yMax));
                    if(visualWallMargin<TownAccessMetrics.VisualWallMargin-.001f)
                    {
                        wall=false;
                        Debug.LogError($"[TownAccess][wallMargin] lot={lot.Name} kind=opaque actual={visualWallMargin:0.###} required={TownAccessMetrics.VisualWallMargin:0.###} bounds={Format(bounds)}");
                    }
                }
                Rect doorBand=DoorBand(lot);
                for(int j=0;j<lots.Count;j++)if(i!=j)
                {
                    if(j>i)gap&=TownAccessMetrics.Gap(lot,lots[j])>=TownAccessMetrics.BuildingGap-.001f;
                    if(hasTargetVisual&&visuals.TryGetValue(lots[j].Name,out var source))
                    {
                        float targetArea=UnionArea(visual);
                        float overlap=OverlapArea(source,visual);
                        float fraction=targetArea<=.0001f?1f:overlap/targetArea;
                        bool doorBlocked=Touches(source,doorBand);
                        if(doorBlocked||fraction>.05f+.0001f)
                        {
                            facade=false;
                            Debug.LogError($"[TownAccess][visualFacade] source={lots[j].Name} target={lot.Name} opaqueOverlap={fraction:P2} max=5.00% doorBandClear={Pass(!doorBlocked)} overlapArea={overlap:0.###} targetOpaqueArea={targetArea:0.###}");
                        }
                    }
                }
                gap&=TownAccessMetrics.RectGap(lot.Center,lot.Size,TownKeyartGeometry.HallFootprintCenter,
                    TownKeyartGeometry.HallFootprintSize)>=TownAccessMetrics.BuildingGap-.001f;
            }
            string envelopeSummary=$"[TownAccess] buildingGap={Pass(gap)} visualFacade={Pass(facade)} wallMargin={Pass(wall)}";
            if(gap&&facade&&wall)Debug.Log(envelopeSummary);else Debug.LogError(envelopeSummary);
            AccessCheck("buildingGap",gap,$"buildingGap={Pass(gap)} min={TownAccessMetrics.BuildingGap}",ref passes,ref failures);
            AccessCheck("visualEnvelope",facade&&wall,$"visualFacade={Pass(facade)} wallMargin={Pass(wall)}",ref passes,ref failures);

            bool roads=true;
            foreach(var road in TownCityLayout.AllRoads)
            {
                var roadClass=TownAccessMetrics.RoadClass(road.Id);
                roads&=road.Width>=TownAccessMetrics.Width(roadClass)-.001f&&
                    TownKeyartGeometry.ClearRoute(road.Start,road.End,road.Width*.5f,lots);
            }
            AccessCheck("roads",roads,"mainMin=4 districtMin=3.2 localMin=2.4",ref passes,ref failures);

            int clearDoors=0,clearInteriors=0;bool yard=false;
            foreach(var lot in lots)
            {
                transforms.TryGetValue(lot.Name,out var owner);
                var apron=TownAccessMetrics.DoorApron(lot);
                bool clear=TownAccessMetrics.ClearsApron(apron,lots)&&ClearRect(apron,colliders,owner)&&!RectTouchesWater(apron);
                var door=owner==null?null:owner.GetComponentInChildren<HouseDoorInteractable>(true);
                var blocker=door==null?null:new SerializedObject(door).FindProperty("_blocker")?.objectReferenceValue as BoxCollider2D;
                float opening=blocker==null?0:lot.DoorSide==TownDoorSide.East||lot.DoorSide==TownDoorSide.West?
                    blocker.bounds.size.y:blocker.bounds.size.x;
                if(clear&&opening>=1.4f-.001f)clearDoors++;
                if(lot.Name=="House_AnimalYard")yard=clear;
                else
                {
                    var lane=TownAccessMetrics.InteriorLane(lot);var pocket=TownAccessMetrics.InteriorPocket(lot);
                    var usable=new Rect(lot.Center-lot.Size*.5f+Vector2.one,lot.Size-Vector2.one*2);
                    bool laneWidth=lot.DoorSide==TownDoorSide.East||lot.DoorSide==TownDoorSide.West?
                        lane.height>=TownAccessMetrics.InteriorLaneWidth-.001f:
                        lane.width>=TownAccessMetrics.InteriorLaneWidth-.001f;
                    bool pocketInside=usable.Contains(pocket.min)&&usable.Contains(pocket.max-new Vector2(.001f,.001f));
                    bool laneClear=ClearRect(lane,colliders,null,out string laneBlocker,blocker);
                    bool pocketClear=ClearRect(pocket,colliders,null,out string pocketBlocker,blocker);
                    if(laneWidth&&pocketInside&&laneClear&&pocketClear)clearInteriors++;
                    else Debug.LogError($"[TownAccess][interior] lot={lot.Name} laneWidth={Pass(laneWidth)} pocketInside={Pass(pocketInside)} laneClear={Pass(laneClear)} laneBlocker={laneBlocker??"none"} pocketClear={Pass(pocketClear)} pocketBlocker={pocketBlocker??"none"} ignoredClosedDoor={blocker?.name??"none"}");
                }
            }
            AccessCheck("doors/interiors",clearDoors==24&&clearInteriors==23&&yard,
                $"doors={clearDoors}/24 interiors={clearInteriors}/23 yard={Pass(yard)}",ref passes,ref failures);

            int reachableDoors=0,reachableWork=0,reachableSpawns=0;var plaza=TownAccessMetrics.PlazaAccessPoint;
            foreach(var lot in lots)if(Finite(TownAccessMetrics.ShortestPathLength(plaza,lot.DoorApproach,TownCityLayout.AllRoads,
                TownAccessMetrics.ComfortProbeRadius)))reachableDoors++;
            foreach(var anchor in anchors)if(anchor.AnchorId.EndsWith("_work")&&
                ClearActor(anchor.transform.position,colliders,anchor.NpcId,out _,TownAccessMetrics.ComfortProbeRadius)&&
                Finite(TownAccessMetrics.ShortestPathLength(plaza,anchor.transform.position,TownCityLayout.AllRoads,
                    TownAccessMetrics.ComfortProbeRadius)))reachableWork++;
            foreach(var spawn in new[]{TownDistrictLayout.DefaultSpawn,TownDistrictLayout.FromFarmSpawn})
                if(ClearActor(spawn,colliders,null,out _,TownAccessMetrics.ComfortProbeRadius)&&
                    Finite(TownAccessMetrics.ShortestPathLength(plaza,spawn,TownCityLayout.AllRoads,TownAccessMetrics.ComfortProbeRadius)))reachableSpawns++;
            AccessCheck("comfortConnectivity",reachableDoors==24&&reachableWork==28&&reachableSpawns==2,
                $"comfortRadius=0.5 work={reachableWork}/28 doors={reachableDoors}/24 spawns={reachableSpawns}/2",ref passes,ref failures);

            float south=TownAccessMetrics.ShortestPathLength(TownDistrictLayout.FromFarmSpawn,plaza,TownCityLayout.AllRoads,
                TownAccessMetrics.ComfortProbeRadius),maxDoor=0;
            foreach(var lot in lots)maxDoor=Mathf.Max(maxDoor,TownAccessMetrics.ShortestPathLength(plaza,lot.DoorApproach,
                TownCityLayout.AllRoads,TownAccessMetrics.ComfortProbeRadius));
            AccessCheck("travelBudget",south<=TownAccessMetrics.SouthSpawnToPlazaMaxPath&&maxDoor<=TownAccessMetrics.PlazaToPublicDoorMaxPath,
                $"southToPlaza={south:0.##}/75 maxPlazaToDoor={maxDoor:0.##}/90",ref passes,ref failures);

            bool ground=transforms.ContainsKey("Ground")&&transforms.ContainsKey("LakeWaterTiles");int portalCount=0;
            foreach(var portal in new Vector2[]{TownDistrictLayout.SouthPortal,TownKeyartGeometry.WestArrival,TownKeyartGeometry.EastArrival})
                if(TownKeyartGeometry.ClearPoint(portal,lots,TownAccessMetrics.ComfortProbeRadius)&&
                    Finite(TownAccessMetrics.ShortestPathLength(plaza,portal,TownCityLayout.AllRoads,TownAccessMetrics.ComfortProbeRadius)))portalCount++;
            AccessCheck("portals/ground",ground&&portalCount==3,$"portals={portalCount}/3 ground={Pass(ground)}",ref passes,ref failures);
        }

        private static bool TryVisualBounds(Transform root,out List<Rect> result)
        {
            result=new List<Rect>();
            foreach(var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if(!renderer.enabled||!renderer.gameObject.activeInHierarchy||renderer.sprite==null||renderer.color.a<=.01f||
                    renderer.name=="Floor"||renderer.name.StartsWith("Furniture_",StringComparison.Ordinal))continue;
                if(renderer.drawMode!=SpriteDrawMode.Simple)
                {
                    var b=renderer.bounds;result.Add(new Rect(b.min,b.size));continue;
                }
                Rect opaque=TownKeyartSceneArt.OpaqueBounds(renderer.sprite);
                Vector3 a=renderer.transform.TransformPoint(new Vector3(opaque.xMin,opaque.yMin));
                Vector3 bWorld=renderer.transform.TransformPoint(new Vector3(opaque.xMax,opaque.yMax));
                result.Add(Rect.MinMaxRect(Mathf.Min(a.x,bWorld.x),Mathf.Min(a.y,bWorld.y),
                    Mathf.Max(a.x,bWorld.x),Mathf.Max(a.y,bWorld.y)));
            }
            return result.Count>0;
        }

        private static Rect BoundsOf(List<Rect> shape)
        {
            Rect result=shape[0];
            for(int i=1;i<shape.Count;i++)result=Rect.MinMaxRect(Mathf.Min(result.xMin,shape[i].xMin),
                Mathf.Min(result.yMin,shape[i].yMin),Mathf.Max(result.xMax,shape[i].xMax),Mathf.Max(result.yMax,shape[i].yMax));
            return result;
        }

        private static float UnionArea(List<Rect> shape)=>CombinedArea(shape,null);
        private static float OverlapArea(List<Rect> a,List<Rect> b)=>CombinedArea(a,b);
        private static float CombinedArea(List<Rect> a,List<Rect> b)
        {
            var xs=new List<float>();var ys=new List<float>();
            void AddEdges(List<Rect> shape){foreach(var r in shape){xs.Add(r.xMin);xs.Add(r.xMax);ys.Add(r.yMin);ys.Add(r.yMax);}}
            AddEdges(a);if(b!=null)AddEdges(b);xs.Sort();ys.Sort();float area=0;
            for(int x=1;x<xs.Count;x++)for(int y=1;y<ys.Count;y++)
            {
                float width=xs[x]-xs[x-1],height=ys[y]-ys[y-1];if(width<=.0001f||height<=.0001f)continue;
                var p=new Vector2((xs[x]+xs[x-1])*.5f,(ys[y]+ys[y-1])*.5f);
                if(Contains(a,p)&&(b==null||Contains(b,p)))area+=width*height;
            }
            return area;
        }

        private static bool Contains(List<Rect> shape,Vector2 point)
        {foreach(var rect in shape)if(rect.Contains(point))return true;return false;}
        private static bool Touches(List<Rect> shape,Rect target)
        {foreach(var rect in shape)if(TownAccessMetrics.OverlapArea(rect,target)>.0001f)return true;return false;}
        private static string Format(Rect rect)=>$"({rect.xMin:0.##},{rect.yMin:0.##})-({rect.xMax:0.##},{rect.yMax:0.##})";

        private static Rect DoorBand(TownBuildingLot lot)
        {
            var outward=TownAccessMetrics.Outward(lot.DoorSide);
            Vector2 size=lot.DoorSide==TownDoorSide.East||lot.DoorSide==TownDoorSide.West?new Vector2(2,1.4f):new Vector2(1.4f,2);
            return new Rect((Vector2)lot.DoorPosition+outward-size*.5f,size);
        }

        private static bool ClearRect(Rect rect,List<Collider2D> colliders,Transform owner)
            =>ClearRect(rect,colliders,owner,out _,null);

        private static bool ClearRect(Rect rect,List<Collider2D> colliders,Transform owner,out string blocker,
            Collider2D ignoredCollider)
        {
            for(float x=rect.xMin+.5f;x<=rect.xMax-.5f+.001f;x+=.5f)
            for(float y=rect.yMin+.5f;y<=rect.yMax-.5f+.001f;y+=.5f)
                if(!ClearActor(new Vector2(x,y),colliders,null,out blocker,TownAccessMetrics.ComfortProbeRadius,owner,ignoredCollider))return false;
            blocker=null;return true;
        }

        private static bool RectTouchesWater(Rect rect)
        {
            for(float x=rect.xMin;x<=rect.xMax+.001f;x+=.5f)
            for(float y=rect.yMin;y<=rect.yMax+.001f;y+=.5f)
                if(TownKeyartGeometry.ContainsWater(new Vector2(x,y)))return true;
            return false;
        }

        private static bool Finite(float value)=>!float.IsInfinity(value)&&!float.IsNaN(value);
        private static string Pass(bool value)=>value?"PASS":"FAIL";
        private static void AccessCheck(string label,bool ok,string detail,ref int passes,ref int failures)
        {
            if(ok){passes++;Debug.Log("[TownAccess] "+label+"=PASS "+detail);}
            else{failures++;Debug.LogError("[TownAccess] "+label+"=FAIL "+detail);}
        }

        private static void ValidateReachability(Dictionary<string,Transform> transforms,List<Collider2D> colliders,List<NpcScheduleAnchor> anchors,ref int passes,ref int failures)
        {
            const float cell=.5f;
            var reached=new HashSet<Vector2Int>();var queue=new Queue<Vector2Int>();
            bool hasDefault=transforms.TryGetValue("Spawn_town_default",out var spawnTransform);
            bool hasFromFarm=transforms.TryGetValue("Spawn_town_from_farm",out var farmSpawnTransform);
            Check("Canonical default spawn exists",hasDefault,ref passes,ref failures);
            Check("Canonical from-farm spawn exists",hasFromFarm,ref passes,ref failures);
            if (!hasDefault || !hasFromFarm) return;
            Vector2 spawn=spawnTransform.position;
            var start=new Vector2Int(Mathf.RoundToInt(spawn.x/cell),Mathf.RoundToInt(spawn.y/cell));
            Check("Spawn circle clear",ClearActor(spawn,colliders,null,out string obstacle),ref passes,ref failures,obstacle);
            reached.Add(start);queue.Enqueue(start);
            var directions=new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down};
            var blocked=new HashSet<Vector2Int>();
            while(queue.Count>0)
            {
                var current=queue.Dequeue();
                foreach(var direction in directions)
                {
                    var next=current+direction;
                    if(reached.Contains(next)||blocked.Contains(next)||Mathf.Abs(next.x*cell)>TownDistrictLayout.HalfWidth-1||
                        Mathf.Abs(next.y*cell)>TownDistrictLayout.HalfHeight-1)continue;
                    if (!ClearActor((Vector2)next*cell,colliders,null,out _))
                    {
                        blocked.Add(next);
                        continue;
                    }
                    bool free=true;
                    for(int sample=1;sample<=5;sample++)
                    {
                        var p=Vector2.Lerp((Vector2)current*cell,(Vector2)next*cell,sample/5f);
                        if(!ClearActor(p,colliders,null,out _)){free=false;break;}
                    }
                    // An obstructed edge does not prove its endpoint blocked from every direction.
                    if(!free)continue;
                    reached.Add(next);queue.Enqueue(next);
                }
            }
            foreach(var lot in TownCityLayout.AllBuildings)
                Check("Spawn route reaches door "+lot.Name,Reached(lot.DoorApproach,reached,colliders),ref passes,ref failures);
            foreach(var anchor in anchors)
                if(anchor.AnchorId.EndsWith("_work"))
                    Check("Spawn route reaches work "+anchor.AnchorId,Reached(anchor.transform.position,reached,colliders),ref passes,ref failures);
            Check("Spawn route reaches mural",Reached(TownDistrictLayout.TownHallMural,reached,colliders),ref passes,ref failures);
            Check("Spawn route reaches dock deck",Reached(TownKeyartGeometry.DockCenter,reached,colliders),ref passes,ref failures);
            Check("From-farm spawn circle clear",ClearActor(farmSpawnTransform.position,colliders,null,out obstacle),ref passes,ref failures,obstacle);
            Check("Both spawns share the materialized reachable component",Reached(farmSpawnTransform.position,reached,colliders),ref passes,ref failures);
            Debug.Log("[TownKeyartPhysics] Reachable half-unit cells="+reached.Count+"; closed house doors retained; not a runtime pathfinder test.");
        }

        private static bool Reached(Vector2 target,HashSet<Vector2Int> reached,List<Collider2D> colliders)
        {
            var cell=new Vector2Int(Mathf.RoundToInt(target.x*2),Mathf.RoundToInt(target.y*2));
            if(!reached.Contains(cell))return false;
            for(int sample=0;sample<=5;sample++)
                if(!ClearActor(Vector2.Lerp((Vector2)cell*.5f,target,sample/5f),colliders,null,out _))return false;
            return true;
        }

        private static void Check(string label,bool ok,ref int passes,ref int failures,string detail=null)
        {
            if(ok){passes++;Debug.Log("[TownKeyartPhysics] PASS "+label);}
            else{failures++;Debug.LogError("[TownKeyartPhysics] FAIL "+label+" blocked by "+detail);}
        }
    }
}
