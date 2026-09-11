using System.Collections.Generic;
using CindarsHope.Editor.Art;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Town-only composition. It does not mutate the shared Farm art/ground pipeline.</summary>
    public static class TownKeyartSceneArt
    {
        private const string PropRoot = "Assets/_Game/Art/Generated/World/props/";
        private static readonly Dictionary<Sprite, Rect> Occupancy = new Dictionary<Sprite, Rect>();

        public static void PrepareAssets()
        {
            // Generation is read-only with respect to Art. Import assets through the authored pipeline first;
            // a mismatched candidate is rejected here instead of being silently rewritten during scene creation.
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_Game/Art/Generated/World/props","Assets/_Game/Art/Generated/World/building"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(!TownKeyartSpriteImporter.IsOwnedAsset(path))continue;
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;
                bool isTile=path.EndsWith("town_cobble_keyart.png");
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                int expectedAlignment=(int)(isTile?SpriteAlignment.Center:SpriteAlignment.BottomCenter);
                bool valid=importer.textureType==TextureImporterType.Sprite&&importer.spriteImportMode==SpriteImportMode.Single&&
                    importer.filterMode==FilterMode.Point&&importer.textureCompression==TextureImporterCompression.Uncompressed&&
                    !importer.mipmapEnabled&&importer.isReadable&&Mathf.Approximately(importer.spritePixelsPerUnit,isTile?64:32)&&
                    settings.spriteMeshType==SpriteMeshType.FullRect&&settings.spriteAlignment==expectedAlignment;
                if(!valid)throw new System.InvalidOperationException("Town art import contract mismatch; run the explicit art importer before scene generation: "+path);
            }
            Occupancy.Clear();
        }

        public static Rect OpaqueBounds(Sprite sprite)
        {
            if(Occupancy.TryGetValue(sprite,out var result))return result;
            if(TownKeyartSpriteImporter.IsOwnedAsset(AssetDatabase.GetAssetPath(sprite))&&!sprite.texture.isReadable)
                throw new System.InvalidOperationException("Town alpha cannot be measured: "+AssetDatabase.GetAssetPath(sprite));
            result=new Rect(sprite.bounds.min.x,sprite.bounds.min.y,sprite.bounds.size.x,sprite.bounds.size.y);
            if(sprite.texture.isReadable)
            {
                var pixels=sprite.texture.GetPixels32();int width=sprite.texture.width;
                int minX=width,minY=sprite.texture.height,maxX=-1,maxY=-1;
                for(int y=(int)sprite.rect.y;y<sprite.rect.yMax;y++)
                for(int x=(int)sprite.rect.x;x<sprite.rect.xMax;x++)
                    if(pixels[y*width+x].a>0){minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}
                if(maxX>=minX)result=new Rect((minX-sprite.rect.x-sprite.pivot.x)/sprite.pixelsPerUnit,(minY-sprite.rect.y-sprite.pivot.y)/sprite.pixelsPerUnit,(maxX-minX+1)/sprite.pixelsPerUnit,(maxY-minY+1)/sprite.pixelsPerUnit);
            }
            Occupancy[sprite]=result;return result;
        }
        public static Sprite CobbleSprite => AssetDatabase.LoadAssetAtPath<Sprite>(PropRoot+"town_cobble_keyart.png") ?? WorldSpriteLibrary.Ground("ground_path_aseprite_v1");

        public static GameObject SpriteObject(Transform parent, string name, Sprite sprite, Vector2 basePoint, float width, string layer="World", int order=0)
        {
            if(sprite==null) return null;
            var go=new GameObject(name); go.transform.SetParent(parent,false);
            var renderer=go.AddComponent<SpriteRenderer>(); renderer.sprite=sprite;
            Rect occupied=OpaqueBounds(sprite);
            float scale=width/occupied.width;
            go.transform.localScale=new Vector3(scale,scale,1);
            go.transform.position=new Vector3(basePoint.x-occupied.center.x*scale,basePoint.y-occupied.yMin*scale,0);
            renderer.sortingLayerName=layer; renderer.sortingOrder=order; renderer.spriteSortPoint=SpriteSortPoint.Pivot;
            return go;
        }

        public static void PaintCourts(Transform parent, Sprite sprite)
        {
            const float cell = TownKeyartGround.CellSize;
            var map = TownKeyartGround.Layer(parent, "WarmStoneCourts", 1);
            var tile = TownKeyartGround.TileFor(sprite);
            var courts = new[] { new Vector4(0,4,13.6f,10.1f), new Vector4(-44,4,7,6),
                new Vector4(-32,15,8,3), new Vector4(43.5f,15.5f,10,3), new Vector4(59,-7,7,3) };
            for(float x=-TownDistrictLayout.HalfWidth;x<TownDistrictLayout.HalfWidth;x+=cell)
            for(float y=-TownDistrictLayout.HalfHeight;y<TownDistrictLayout.HalfHeight;y+=cell)
            {
                var p=new Vector2(x+cell*.5f,y+cell*.5f); bool paint=false;
                foreach(var c in courts)
                {
                    float nx=(p.x-c.x)/c.z, ny=(p.y-c.y)/c.w;
                    if(nx*nx+ny*ny<1f){paint=true;break;}
                }
                if(!paint||!TownKeyartGeometry.ClearPoint(p,TownCityLayout.AllBuildings,.1f)) continue;
                TownKeyartGround.Set(map,p,tile);
            }
        }

        public static void CreateWaterDistrict(Transform parent)
        {
            var district=new GameObject("District_LakePark_SW");district.transform.SetParent(parent,false);
            var water=new GameObject("LakeWater");water.transform.SetParent(district.transform,false);
            water.AddComponent<PolygonCollider2D>().points=TownKeyartGeometry.LakeOutline;
            var shore = TownKeyartGround.Layer(district.transform, "Shore", 2);
            var waterTiles = TownKeyartGround.Layer(district.transform, "LakeWaterTiles", 3);
            var shoreTile = TownKeyartGround.TileFor(CobbleSprite);
            var waterTile = TownKeyartGround.TileFor(WorldSpriteLibrary.Ground("ground_water_keyart_v4"));
            TownKeyartGround.PaintShore(shore, shoreTile, TownKeyartGeometry.LakeOutline);
            TownKeyartGround.PaintPolygon(waterTiles, waterTile, TownKeyartGeometry.LakeOutline);
            // Continuous western river; the two authored crossing decks have real openings in water colliders.
            var river = TownKeyartGeometry.RiverPolygon(TownKeyartGeometry.RiverSolidIntervals[0].x,TownKeyartGeometry.RiverSolidIntervals[2].y);
            TownKeyartGround.PaintShore(shore, shoreTile, river);
            TownKeyartGround.PaintPolygon(waterTiles, waterTile, river);
            var intervals=TownKeyartGeometry.RiverSolidIntervals;
            for(int i=0;i<intervals.Length;i++)
            {
                var segment=new GameObject("RiverWaterCollider_"+i);segment.transform.SetParent(district.transform,false);
                segment.AddComponent<PolygonCollider2D>().points=TownKeyartGeometry.RiverPolygon(intervals[i].x,intervals[i].y);
            }
            var deck=WorldSpriteLibrary.Ground("ground_deck");
            var deckMap = TownKeyartGround.Layer(district.transform, "RiverBridgeDecks", 5);
            var deckTile = TownKeyartGround.TileFor(deck);
            foreach (float bridgeY in TownKeyartGeometry.BridgeCentersY)
            {
                float left=TownKeyartGeometry.RiverWestEdge(bridgeY)-.8f,right=TownKeyartGeometry.RiverEastEdge(bridgeY)+.8f;
                var corners = new[] { new Vector2(left,bridgeY-1.7f), new Vector2(right,bridgeY-1.7f),
                    new Vector2(right,bridgeY+1.7f), new Vector2(left,bridgeY+1.7f) };
                TownKeyartGround.PaintPolygon(deckMap, deckTile, corners);
                SpriteObject(district.transform,"RiverBridgeArt_"+bridgeY,WorldSpriteLibrary.Prop("bridge_keyart_v3"),new Vector2((left+right)*.5f,bridgeY-1.7f),right-left);
            }
            TownKeyartGround.PaintPolygon(deckMap,deckTile,new[]{new Vector2(-60.5333f,-32.8356f),new Vector2(-54.6667f,-32.8356f),new Vector2(-54.6667f,-38.3111f),new Vector2(-60.5333f,-38.3111f)});
            SpriteObject(district.transform,"LakeDock_East",WorldSpriteLibrary.Prop("fishing_dock_keyart_v1"),new Vector2(-57.6f,-39.1822f),8.8f,"World",0);
            AddDockGuard(district.transform,"DockRail_W",new Vector2(-60.5333f,-35.8222f),new Vector2(.22f,4.7f));
            AddDockGuard(district.transform,"DockRail_E",new Vector2(-54.6667f,-35.8222f),new Vector2(.22f,4.7f));
            AddDockGuard(district.transform,"DockRail_S",new Vector2(-57.6f,-38.3111f),new Vector2(5.8666f,.22f));
            var reeds=WorldSpriteLibrary.Prop("reeds_keyart_v1");
            for(int i=0;i<TownKeyartGeometry.LakeOutline.Length;i+=10)
            {
                var p=TownKeyartGeometry.LakeOutline[i];
                if(p.x>-61.4f&&p.x<-53.3f&&p.y<-32)continue;
                SpriteObject(district.transform,"LakeReeds_"+i,reeds,p,1.1f,"World",0);
            }
            // Break the otherwise flat water fill with a few non-physical ripple clusters. The
            // ripple transform is animated, never the water collider or the walkable dock.
            var ripples = WorldSpriteLibrary.Prop("water_ripples_keyart_v4");
            var ripplePoints = new[] { new Vector2(-70.4f,-29.4f), new Vector2(-62.2f,-45.2f),
                new Vector2(-49.6f,-42.3f), new Vector2(-75.1f,-39.8f) };
            for (int i = 0; i < ripplePoints.Length; i++)
            {
                var ripple = SpriteObject(district.transform, "LakeRippleLoop_" + i, ripples,
                    ripplePoints[i], 2.7f, "Ground", 6);
                AddScaleLoop(ripple, "TownLakeRipple_" + i, 1f, 1.08f, 1.6f);
            }
            SpriteObject(district.transform,"LakeLilies",WorldSpriteLibrary.Prop("lily_pads_keyart_v1"),new Vector2(-66.6667f,-43.2889f),2.4f,"Ground",5);
            SpriteObject(district.transform,"ParkBench_W",WorldSpriteLibrary.Prop("bench"),TownDistrictLayout.LakeBenchWest,1.6f);
            SpriteObject(district.transform,"ParkBench_E",WorldSpriteLibrary.Prop("bench"),TownDistrictLayout.LakeBenchEast,1.6f);
            int lotHits=0,roadHits=0;
            foreach(var lot in TownCityLayout.AllBuildings)
            {
                bool hit=false;
                for(float x=lot.MinX+.05f;x<lot.MaxX;x+=.25f)
                for(float y=lot.MinY+.05f;y<lot.MaxY;y+=.25f)
                    hit|=TownKeyartGeometry.ContainsWater(new Vector2(x,y));
                if(hit){lotHits++;Debug.LogError("[town-physics] Water overlaps lot "+lot.Name);}
            }
            foreach(var road in TownCityLayout.AllRoads)
                if(!TownKeyartGeometry.RoadClearsWater(road)){roadHits++;Debug.LogError("[town-physics] Water overlaps full swept route "+road.Id);}
            Debug.Log($"[town-physics] LakeWater audit: lotes={lotHits} vias={roadHits}; actual polygon sampled 0.25u, river included.");
        }

        public static void Apply(Scene scene)
        {
            var all=new List<Transform>();
            foreach(var root in scene.GetRootGameObjects()) all.AddRange(root.GetComponentsInChildren<Transform>(true));
            foreach(var t in all)
            {
                var map=t.GetComponent<Tilemap>();
                if(map!=null&&(t.name=="PlazaCobble"||t.name=="CivicRing"||t.name=="MarketSquare_Ground"))map.ClearAllTiles();
            }
            foreach(var transform in all)
            {
                var renderer=transform.GetComponent<SpriteRenderer>(); if(renderer==null||renderer.sprite==null) continue;
                string name=transform.name;
                // Ground-cover must never overdraw towers/trees. Scale is relative to a ~1.1u actor.
                bool flower=name.Contains("Flower")||name.Contains("Meadow")||name.Contains("Garden_");
                if(flower)
                {
                    float height=renderer.bounds.size.y;
                    Vector3 basePoint=new Vector3(renderer.bounds.center.x,renderer.bounds.min.y,0);
                    if(height>0.9f) transform.localScale*=0.9f/height;
                    transform.position+=basePoint-new Vector3(renderer.bounds.center.x,renderer.bounds.min.y,0);
                    renderer.sortingLayerName="Ground";renderer.sortingOrder=8;
                }
                else if(name.Contains("Bench") && renderer.bounds.size.y>1f)
                    transform.localScale*=1f/renderer.bounds.size.y;
                else if(renderer.sortingLayerName=="World" && transform.GetComponentInParent<UnityEngine.Rendering.SortingGroup>()==null)
                    renderer.sortingOrder=0;
            }
            ApplyStatue(all);
            ApplyMarket(all);
            ApplyNpcStalls(all);
            CreateOpenAnimalYard(all);
            CreateMaterialGround(scene);
            CreateMarketDepth(scene);
            CreateCivicGarden(scene);
            CreatePerimeterStone(scene);
            CreateAmbientLoops(scene);
            TownKeyartSetDressing.Apply(scene);
        }

        private static void CreateMaterialGround(Scene scene)
        {
            var root = new GameObject("TownMaterialGround");
            SceneManager.MoveGameObjectToScene(root, scene);
            var sprites = new[] { WorldSpriteLibrary.Ground("ground_grass_a"),
                WorldSpriteLibrary.Ground("ground_grass_b"), WorldSpriteLibrary.Ground("ground_grass_flower") };
            var maps = new Tilemap[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
                if (sprites[i] != null) maps[i] = TownKeyartGround.Layer(root.transform, "MaterialPocket_" + i, -1 + i);
            for (int index = 0; index < TownKeyartGeometry.MaterialPocketPolygons.Length; index++)
            {
                var polygon = TownKeyartGeometry.MaterialPocketPolygons[index];
                var map = maps[index % maps.Length];
                if (map == null) continue;
                float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
                foreach (var p in polygon) { minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x); minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y); }
                var tile = WorldTilemapGround.GetTile(sprites[index % sprites.Length]);
                for (float x = minX; x <= maxX; x += TownKeyartGround.CellSize)
                for (float y = minY; y <= maxY; y += TownKeyartGround.CellSize)
                {
                    var point = new Vector2(x + TownKeyartGround.CellSize * .5f, y + TownKeyartGround.CellSize * .5f);
                    if (!TownKeyartGeometry.MaterialPocketPointAllowed(point, polygon, .35f, .45f)) continue;
                    TownKeyartGround.Set(map, point, tile);
                }
            }
            Debug.Log($"[town-composition] material pockets nominalArea={TownKeyartGeometry.MaterialPocketArea:0.##} effectivePaintedArea={TownKeyartGeometry.EffectiveMaterialPocketArea():0.##}");
            // Low-wall fragments give each pocket a readable edge without introducing a collider.
            var wall = WorldSpriteLibrary.Prop("town_low_wall_curve_keyart");
            for (int i = 0; i < TownKeyartGeometry.MaterialPocketPolygons.Length; i += 2)
            {
                if (!TownKeyartGeometry.TryFindMaterialPocketSupport(i, out var support, .5f, .35f, .45f)) continue;
                SpriteObject(root.transform, "MaterialPocketWall_" + i, wall, support, 5.6f, "World", 0);
            }
        }

        private static void CreateMarketDepth(Scene scene)
        {
            var root = new GameObject("TownMarketDepth");
            SceneManager.MoveGameObjectToScene(root, scene);
            var canopy = WorldSpriteLibrary.Prop("stall_keyart_v4");
            var crate = WorldSpriteLibrary.Prop("crate");
            var positions = new[] { new Vector2(-57f, 10.5f), new Vector2(-51f, 11.5f),
                new Vector2(-36f, 5.5f), new Vector2(-31f, 3.5f) };
            for (int i = 0; i < positions.Length; i++)
            {
                SpriteObject(root.transform, "MarketDepthCanopy_" + i, canopy, positions[i], 4.2f, "World", 2);
                SpriteObject(root.transform, "MarketDepthCrate_" + i, crate, positions[i] + new Vector2(0f, -1.2f), 1.1f, "World", 2);
            }
        }

        private static void CreateAmbientLoops(Scene scene)
        {
            // Reuse Unity's native Animation component. These loops are presentation-only: no
            // collider-bearing root is animated, and the existing time/weather systems remain
            // authoritative for gameplay state.
            var all = new List<Transform>();
            foreach (var root in scene.GetRootGameObjects()) all.AddRange(root.GetComponentsInChildren<Transform>(true));
            foreach (var t in all)
            {
                if (t.name == "SemanticPlaceholder_WaterWheel")
                    AddRotationLoop(t, "TownWaterWheelLoop", 2.8f);
                else if (t.name.StartsWith("KeyartLamp_"))
                    AddColorLoop(t, "TownLampPulse", 2.4f);
            }
        }

        private static void AddRotationLoop(Transform target, string id, float seconds)
        {
            if (target == null || target.GetComponent<Collider2D>() != null) return;
            var animation = target.GetComponent<Animation>();
            if (animation == null) animation = target.gameObject.AddComponent<Animation>();
            var clip = new AnimationClip { name = id, legacy = true, wrapMode = WrapMode.Loop, frameRate = 12f };
            clip.SetCurve(string.Empty, typeof(Transform), "localEulerAngles.z",
                AnimationCurve.Linear(0f, 0f, seconds, 360f));
            animation.AddClip(clip, id); animation.clip = clip; animation.playAutomatically = true;
        }

        private static void AddScaleLoop(GameObject target, string id, float min, float max, float seconds)
        {
            if (target == null || target.GetComponent<Collider2D>() != null) return;
            var animation = target.GetComponent<Animation>();
            if (animation == null) animation = target.AddComponent<Animation>();
            var clip = new AnimationClip { name = id, legacy = true, wrapMode = WrapMode.Loop, frameRate = 12f };
            var curve = new AnimationCurve(new Keyframe(0f, min), new Keyframe(seconds * .5f, max), new Keyframe(seconds, min));
            clip.SetCurve(string.Empty, typeof(Transform), "localScale.x", curve);
            clip.SetCurve(string.Empty, typeof(Transform), "localScale.y", curve);
            animation.AddClip(clip, id); animation.clip = clip; animation.playAutomatically = true;
        }

        private static void AddColorLoop(Transform target, string id, float seconds)
        {
            var renderer = target == null ? null : target.GetComponent<SpriteRenderer>();
            if (renderer == null || target.GetComponent<Collider2D>() != null) return;
            var animation = target.GetComponent<Animation>();
            if (animation == null) animation = target.gameObject.AddComponent<Animation>();
            var clip = new AnimationClip { name = id, legacy = true, wrapMode = WrapMode.Loop, frameRate = 12f };
            var curve = new AnimationCurve(new Keyframe(0f, .86f), new Keyframe(seconds * .5f, 1f), new Keyframe(seconds, .86f));
            clip.SetCurve(string.Empty, typeof(SpriteRenderer), "m_Color.r", curve);
            clip.SetCurve(string.Empty, typeof(SpriteRenderer), "m_Color.g", curve);
            clip.SetCurve(string.Empty, typeof(SpriteRenderer), "m_Color.b", curve);
            animation.AddClip(clip, id); animation.clip = clip; animation.playAutomatically = true;
        }

        private static void AddDockGuard(Transform parent,string name,Vector2 center,Vector2 size)
        {
            var rail=new GameObject(name);rail.transform.SetParent(parent,false);rail.transform.position=center;
            rail.AddComponent<BoxCollider2D>().size=size;
            var sprite=WorldSpriteLibrary.Prop("fence");
            int count=size.x>size.y?3:4;
            for(int i=0;i<count;i++)
            {
                float offset=(i/(float)(count-1)-.5f);
                Vector2 p=center+new Vector2(size.x>size.y?offset*size.x:0,size.y>size.x?offset*size.y:0);
                SpriteObject(rail.transform,name+"_Art_"+i,sprite,p,1.1f);
            }
        }

        private static void ApplyStatue(List<Transform> all)
        {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(PropRoot+"town_statue_fountain_keyart.png");
            if(sprite==null) return;
            Transform plaza=null;
            foreach(var t in all)
            {
                if(t.name=="CentralPlaza") plaza=t;
                if(t.name=="WarriorStatue")
                    foreach(var r in t.GetComponentsInChildren<SpriteRenderer>(true)) r.enabled=false;
                if(t.name=="FountainHeroArt"||t.name=="FountainWater"||t.name=="FountainBasin")
                {var r=t.GetComponent<SpriteRenderer>();if(r!=null)r.enabled=false;}
                if(t.name=="FountainBasin")
                {
                    var basin=t.GetComponent<BoxCollider2D>();
                    if(basin!=null)basin.size=new Vector2(8f/Mathf.Abs(t.lossyScale.x),3.2f/Mathf.Abs(t.lossyScale.y));
                }
            }
            SpriteObject(plaza,"TownFounderStatueArt",sprite,new Vector2(0,1.5f),9);
        }

        private static void ApplyMarket(List<Transform> all)
        {
            var sprites = new[] { WorldSpriteLibrary.Prop("town_market_stall_blue_keyart"),
                WorldSpriteLibrary.Prop("town_market_stall_red_keyart"), WorldSpriteLibrary.Prop("town_market_stall_green_keyart") };
            if(sprites[0]==null) return;
            var positions=new[]{new Vector2(-49.3333f,7.7333f),new Vector2(-42.6667f,8.9778f),new Vector2(-38.6667f,1.5111f),new Vector2(-37.3333f,-8.4444f),new Vector2(-56,5.2444f),new Vector2(-46.6667f,-15.2889f)};
            int index=0;
            foreach(var t in all)
            {
                if(!t.name.StartsWith("MarketSquare_Stall_")) continue;
                t.position=positions[index%positions.Length];
                foreach(var r in t.GetComponentsInChildren<SpriteRenderer>(true))r.enabled=false;
                SpriteObject(t,"StripedFabricStallArt",sprites[index % sprites.Length] ?? sprites[0],(Vector2)t.position-new Vector2(0,.4f),4.5f);
                index++;
            }
        }

        private static void CreateCivicGarden(Scene scene)
        {
            var root=new GameObject("TownKeyartGardens");SceneManager.MoveGameObjectToScene(root,scene);
            var bush=WorldSpriteLibrary.Foliage("bush_leafy");var flower=WorldSpriteLibrary.Foliage("flower_patch");
            var wall = WorldSpriteLibrary.Prop("town_low_wall_curve_keyart");
            for(int i=0;i<28;i++)
            {
                float a=i*Mathf.PI*2/28;float x=Mathf.Cos(a)*11.2f,y=4+Mathf.Sin(a)*9.5f;
                if(Mathf.Abs(x)<2.5f || Mathf.Abs(y-4)<2.1f)continue;
                SpriteObject(root.transform,"CivicPlanterStone_"+i,wall,new Vector2(x,y-.7f),2.7f);
                SpriteObject(root.transform,"CivicPlanterShrub_"+i,bush,new Vector2(x,y),1.4f);
                SpriteObject(root.transform,"CivicPlanterFlowers_"+i,flower,new Vector2(x+.2f,y+.3f),.6f);
            }
            int garden=0;
            for(float y=-TownDistrictLayout.HalfHeight+6;y<TownDistrictLayout.HalfHeight-8;y+=3.1f)
            for(float x=-TownDistrictLayout.HalfWidth+7;x<TownDistrictLayout.HalfWidth-6;x+=3.3f)
            {
                var p=new Vector2(x+Mathf.Sin(y*2)*.65f,y+Mathf.Cos(x)*.45f);
                if(!TownKeyartGeometry.ClearPoint(p,TownCityLayout.AllBuildings,2.3f)||TownCityLayout.IsPointOnRoad(p,1.1f))continue;
                if(Vector2.Distance(p,TownCityLayout.CentralPlazaCenter)<15f)continue;
                var s=garden%5==0?WorldSpriteLibrary.Tree("tree_pine"):bush;
                SpriteObject(root.transform,"TownInternalPlanting_"+garden,s,p,garden%5==0?3.5f:1.8f);
                SpriteObject(root.transform,"TownInternalFlower_"+garden,flower,p+new Vector2(.8f,-.6f),.7f,"Ground",8);
                garden++;
            }
        }

        private static void ApplyNpcStalls(List<Transform> all)
        {
            var sprites = new[] { WorldSpriteLibrary.Prop("town_market_stall_blue_keyart"),
                WorldSpriteLibrary.Prop("town_market_stall_red_keyart"), WorldSpriteLibrary.Prop("town_market_stall_green_keyart") };
            int index=0;
            foreach (var t in all)
            {
                if (!t.name.StartsWith("Stall_npc_")) continue;
                foreach (var renderer in t.GetComponentsInChildren<SpriteRenderer>(true)) renderer.enabled=false;
                SpriteObject(t,"VendorFabricCanopy",sprites[index++%sprites.Length],(Vector2)t.position-new Vector2(0,.5f),2.8f);
            }
        }

        private static void CreateOpenAnimalYard(List<Transform> all)
        {
            Transform yard=null;
            foreach(var t in all) if(t.name=="House_AnimalYard") { yard=t; break; }
            if(yard==null || !TownCityLayout.TryGetBuilding(yard.name,out var lot)) return;
            var fence=WorldSpriteLibrary.Prop("fence");
            // Every fence run corresponds to a retained physical yard wall; the south gate stays openable.
            for(int i=0;i<6;i++)
            {
                float x=lot.MinX+1.3f+i*2.65f;
                SpriteObject(yard,"YardFence_N_"+i,fence,new Vector2(x,lot.MaxY-.5f),2.9f);
                if(Mathf.Abs(x-lot.Center.x)>2)
                    SpriteObject(yard,"YardFence_S_"+i,fence,new Vector2(x,lot.MinY),2.9f);
            }
            for(int i=0;i<5;i++)
            {
                float y=lot.MinY+1+i*2.1f;
                SpriteObject(yard,"YardFence_W_"+i,fence,new Vector2(lot.MinX,y),1.3f);
                SpriteObject(yard,"YardFence_E_"+i,fence,new Vector2(lot.MaxX,y),1.3f);
            }
            SpriteObject(yard,"OpenHayShelter",WorldSpriteLibrary.Building("town_hay_shed_keyart"),lot.Center+new Vector2(-4,-2),4.6f*TownKeyartBuildingArt.VisualEnlargement);
            SpriteObject(yard,"YardCow",WorldSpriteLibrary.Animal("animal_4"),lot.Center+new Vector2(3,-4),2.2f);
        }

        private static void CreatePerimeterStone(Scene scene)
        {
            var root=new GameObject("TownKeyartStoneWall");SceneManager.MoveGameObjectToScene(root,scene);
            var stone = WorldSpriteLibrary.Prop("town_low_wall_curve_keyart");
            var pillar = WorldSpriteLibrary.Prop("town_gate_pillar_keyart");
            for(int i=0;i<19;i++)
            {
                float x=-TownDistrictLayout.HalfWidth+5f+i*8.5f;if(Mathf.Abs(x)<7)continue;
                SpriteObject(root.transform,"SouthStoneWall_"+i,stone,new Vector2(x,-TownDistrictLayout.HalfHeight+1.5f+Mathf.Sin(x*.12f)*.8f),9f);
            }
            for(int i=0;i<14;i++)
            {
                float y=-TownDistrictLayout.HalfHeight+6+i*8.1f;
                SpriteObject(root.transform,"EastStoneWall_"+i,stone,new Vector2(TownDistrictLayout.HalfWidth-1.5f,y),3.5f);
            }
            // Asymmetric north/west visual banks keep the physical perimeter intact while
            // replacing the former uniform rectangular frame with alternating clearings.
            for (int i = 0; i < 13; i++)
            {
                if (i == 4 || i == 9) continue;
                float x = -TownDistrictLayout.HalfWidth + 8f + i * 11.2f;
                SpriteObject(root.transform, "NorthStoneWall_" + i, stone,
                    new Vector2(x, TownDistrictLayout.HalfHeight - 1.8f + Mathf.Sin(i * 1.7f) * .9f), 4.2f);
            }
            for (int i = 0; i < 9; i++)
            {
                if (i == 2 || i == 7) continue;
                float y = -TownDistrictLayout.HalfHeight + 8f + i * 11.3f;
                SpriteObject(root.transform, "WestStoneWall_" + i, stone,
                    new Vector2(-TownDistrictLayout.HalfWidth + 1.8f, y + Mathf.Cos(i * 1.4f) * .7f), 3.8f);
            }
            SpriteObject(root.transform,"SouthGateStonePillar_W",pillar,new Vector2(-4.7f,-TownDistrictLayout.HalfHeight+1),3.4f);
            SpriteObject(root.transform,"SouthGateStonePillar_E",pillar,new Vector2(4.7f,-TownDistrictLayout.HalfHeight+1),3.4f);
            SpriteObject(root.transform,"NorthGateStonePillar_W",pillar,new Vector2(-4.7f,TownDistrictLayout.HalfHeight-9.5f),3.1f);
            SpriteObject(root.transform,"NorthGateStonePillar_E",pillar,new Vector2(4.7f,TownDistrictLayout.HalfHeight-9.5f),3.1f);
        }
    }
}
