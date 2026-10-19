using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using CindarsHope.Editor.SceneCreation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace CindarsHope.Editor.Dev
{
    /// <summary>Captura diagnostica da TownScene salva, sem regenerar ou alterar a cena.</summary>
    public static class TownSceneCapture
    {
        private const string ScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const string Evidence = "art/town-keyart-rework/evidence/";
        private const int Width = 1536;
        private const int Height = 1024;

        public static void CaptureBaseline() => CaptureSet("baseline-20260909");
        public static void CaptureRevision() => CaptureSet(Environment.GetEnvironmentVariable("CINDARS_TOWN_REVISION") ?? "revision-02");
        public static void CaptureTownTopDown() => CaptureRevision();
        public static void GenerateAndCaptureRevision()
        {
            CreateMvpTownScene.CreateScene();
            CaptureRevision();
        }

        public static void GenerateValidateAndCaptureRevision()
        {
            string output=Evidence+(Environment.GetEnvironmentVariable("CINDARS_TOWN_REVISION")??"revision-04-final")+"/";
            if(Directory.Exists(output)&&Directory.GetFiles(output).Length>0)
                throw new InvalidOperationException("Town final output already exists; choose a fresh revision.");
            bool reportedError=false;
            void RecordError(string message,string stack,LogType type)
            {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)reportedError=true;}
            Application.logMessageReceived+=RecordError;
            try
            {
                CreateMvpTownScene.CreateScene();
                CindarsHope.Editor.Validation.ValidateTownKeyartScene.Validate();
                CindarsHope.Editor.Validation.ValidateFableCitySchedule.Validate();
                if(reportedError)throw new InvalidOperationException("Town generation/validation reported an error; full capture was not accepted.");
                CaptureRevision();
                if(reportedError)throw new InvalidOperationException("Town capture reported an error; outputs are diagnostic only.");
            }
            finally {Application.logMessageReceived-=RecordError;}
        }

        // Two diagnostic views first. The full fourteen-image acceptance set remains a separate step.
        public static void GenerateValidateAndPreviewRevision()
        {
            string output=Evidence+(Environment.GetEnvironmentVariable("CINDARS_TOWN_REVISION")??"revision-04-preview")+"/";
            if(Directory.Exists(output)&&Directory.GetFiles(output).Length>0)
                throw new InvalidOperationException("Town preview output already exists; choose a fresh revision.");
            bool reportedError=false;
            void RecordError(string message,string stack,LogType type)
            {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)reportedError=true;}
            Application.logMessageReceived+=RecordError;
            try
            {
                CreateMvpTownScene.CreateScene();
                var scene=SceneManager.GetActiveScene();
                Exception validationFailure=null;
                try
                {
                    CindarsHope.Editor.Validation.ValidateTownKeyartScene.Validate();
                    CindarsHope.Editor.Validation.ValidateFableCitySchedule.Validate();
                }
                catch(Exception error){validationFailure=error;}
                Capture(output+"town-comparable45.png",Vector2.zero,45);
                var hidden=new List<SpriteRenderer>();
                try
                {
                    foreach(var root in scene.GetRootGameObjects())
                    foreach(var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        if(!renderer.enabled)continue;
                        bool building=false;
                        for(var p=renderer.transform;p!=null;p=p.parent)
                            if(p.name.StartsWith("House_")||p.name=="TownHallBuilding"){building=true;break;}
                        if(building)continue;hidden.Add(renderer);renderer.enabled=false;
                    }
                    Capture(output+"town-terrain-lots45.png",Vector2.zero,45);
                }
                finally {foreach(var renderer in hidden)if(renderer!=null)renderer.enabled=true;}
                WriteInputs(output,scene);
                if(validationFailure!=null)throw validationFailure;
                if(reportedError)throw new InvalidOperationException("Town generation/validation reported an error; diagnostic views are NOT acceptance.");
                Debug.Log("[TownSceneCapture] Two diagnostic views and both saved-scene validators completed; visual/PlayMode acceptance is separate: "+output);
            }
            finally {Application.logMessageReceived-=RecordError;}
        }

        // Explicit, one-off repair of a verified overscope import. Never called by normal generation.
        public static void RestoreTownHallImportAndGenerateRevision()
        {
            const string path="Assets/_Game/Art/Generated/World/locations/town_hall/town_hall.png";
            const string source="FED4056EC10AA9411590FA46E40F1AD54202A87DC213DD8D886C2CE30206A4CE";
            const string before="7AA21372B186BBAF0A02D3D6DDC4A5E97E13AB04C18B53174C7C1135205777CA";
            string report=Evidence+"townhall-import-restoration-20260909.json";
            if(File.Exists(report))throw new InvalidOperationException("Town hall repair evidence already exists; use normal generation after checking it.");
            if(Hash(path)!=source||Hash(path+".meta")!=before)
                throw new InvalidOperationException("Town hall repair inputs differ from the audited source/meta; no importer was changed.");
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null||importer.spritePixelsPerUnit!=32||!importer.isReadable)
                throw new InvalidOperationException("Expected only audited PPU32/readable=true overscope.");
            importer.spritePixelsPerUnit=128;importer.isReadable=false;
            importer.SaveAndReimport();
            importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null||importer.spritePixelsPerUnit!=128||importer.isReadable||Hash(path)!=source)
                throw new InvalidOperationException("Town hall import restoration did not persist exactly.");
            Directory.CreateDirectory(Evidence);
            File.WriteAllText(report,JsonUtility.ToJson(new TownHallImportRepair { utc=DateTime.UtcNow.ToString("O"),
                asset=path,sourceSha256=source,beforeMetaSha256=before,afterMetaSha256=Hash(path+".meta"),
                outcome="Restored only spritePixelsPerUnit32->128 and isReadabletrue->false through Editor API; PNG unchanged." },true));
            Debug.Log("[TownHallImportRepair] PASS "+Path.GetFullPath(report));
            GenerateAndCaptureRevision();
        }

        [Serializable] private sealed class TownHallImportRepair
        {
            public string utc,asset,sourceSha256,beforeMetaSha256,afterMetaSha256,outcome;
        }

        private static string Hash(string path)
        {
            using(var sha=SHA256.Create())using(var stream=File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","");
        }

        private static void CaptureSet(string revision)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save unsaved scenes before capturing Town; no work was discarded.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid()) throw new InvalidOperationException("TownScene could not be opened.");
                string output = Evidence + revision + "/";
                Vector2 plaza=TownCityLayout.CentralPlazaCenter;
                Vector2 temple=DoorPoint("House_Temple");
                Vector2 mill=DoorPoint("House_Fishery");
                Vector2 bakery=DoorPoint("House_Bakery");
                Vector2 alchemy=DoorPoint("House_AlchemyLab");
                Vector2 market=DoorPoint("House_MarketHall");
                Vector2 hall=TownDistrictLayout.TownHallMural;
                Vector2 south=TownDistrictLayout.FromFarmSpawn;
                Capture(output + "town-overview-ortho58.png", Vector2.zero, 58f);
                Capture(output + "town-detail-plaza.png", plaza, 8f);
                Capture(output + "town-detail-temple-door.png", temple, 8f);
                Capture(output + "town-detail-watermill-door.png", mill, 8f);
                Capture(output + "town-detail-bakery-door.png", bakery, 8f);
                Capture(output + "town-detail-alchemy-door.png", alchemy, 8f);
                Capture(output + "town-detail-market.png", market, 8f);
                Capture(output + "town-detail-town-hall.png", hall, 8f);
                Capture(output + "town-detail-south-gate.png", south, 8f);
                var overlay = CreateColliderOverlay(scene);
                try
                {
                    Capture(output + "town-overlay-plaza.png", plaza, 8f);
                    Capture(output + "town-overlay-temple-door.png", temple, 8f);
                    Capture(output + "town-overlay-watermill-door.png", mill, 8f);
                    Capture(output + "town-overlay-bakery-door.png", bakery, 8f);
                    Capture(output + "town-overlay-alchemy-door.png", alchemy, 8f);
                    Capture(output + "town-overlay-market.png", market, 8f);
                    Capture(output + "town-overlay-town-hall.png", hall, 8f);
                    Capture(output + "town-overlay-south-gate.png", south, 8f);
                }
                finally { UnityEngine.Object.DestroyImmediate(overlay); }
                WriteInputs(output, scene);
                Debug.Log("[TownSceneCapture] PASS saved-scene captures: " + Path.GetFullPath(output) + "; not PlayMode evidence.");
            }
            finally
            {
                if (setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private static Vector2 DoorPoint(string houseName)
        {
            if(!TownCityLayout.TryGetBuilding(houseName,out var lot))
                throw new InvalidOperationException("Missing capture lot: "+houseName);
            return lot.DoorApproach;
        }

        private static GameObject CreateColliderOverlay(Scene scene)
        {
            var parent = new GameObject("__TownColliderEvidence");
            var material = new Material(Shader.Find("Sprites/Default"));
            foreach (var root in scene.GetRootGameObjects())
            foreach (var collider in root.GetComponentsInChildren<Collider2D>(true))
            {
                if (!collider.enabled || !collider.gameObject.activeInHierarchy) continue;
                var points = new List<Vector3>();
                if (collider is BoxCollider2D box)
                {
                    Vector2 half = box.size * .5f;
                    points.Add(box.transform.TransformPoint(box.offset + new Vector2(-half.x,-half.y)));
                    points.Add(box.transform.TransformPoint(box.offset + new Vector2(half.x,-half.y)));
                    points.Add(box.transform.TransformPoint(box.offset + new Vector2(half.x,half.y)));
                    points.Add(box.transform.TransformPoint(box.offset + new Vector2(-half.x,half.y)));
                }
                else if (collider is PolygonCollider2D polygon)
                {
                    foreach (Vector2 point in polygon.points) points.Add(polygon.transform.TransformPoint(point+polygon.offset));
                }
                else if (collider is CircleCollider2D circle)
                {
                    for (int i=0;i<32;i++)
                    {
                        float angle=i*Mathf.PI*2/32;
                        points.Add(circle.transform.TransformPoint(circle.offset+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*circle.radius));
                    }
                }
                else continue;
                var go=new GameObject(collider.name+"_Outline");
                go.transform.SetParent(parent.transform,false);
                var line=go.AddComponent<LineRenderer>();
                line.sharedMaterial=material;
                line.useWorldSpace=true;
                line.loop=true;
                line.positionCount=points.Count;
                line.SetPositions(points.ToArray());
                line.startWidth=line.endWidth=.045f;
                line.startColor=line.endColor=collider.isTrigger?new Color(0,1,1,.7f):new Color(1,.15f,.1f,.85f);
                line.sortingLayerName="Roof";
                line.sortingOrder=32760;
            }
            return parent;
        }

        [Serializable] private sealed class CaptureInputs
        {
            public string utc;
            public string observation = "Saved scene; PlayMode NOT RUN by this capture command";
            public string cameras = "1536x1024; center0,0 ortho60 and ortho45; gameplay details ortho8";
            public string[] sha256;
            public string[] grids;
            public string[] visualMeasurements;
        }

        private static void WriteInputs(string output, Scene scene)
        {
            var paths = new HashSet<string> { ScenePath,
                "Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs",
                "Assets/_Game/Scripts/Editor/SceneCreation/City/TownCityLayout.cs",
                "Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs",
                "Assets/_Game/Scripts/Editor/Dev/TownSceneCapture.cs",
                "Assets/_Game/Scripts/Editor/Validation/ValidateTownKeyartScene.cs" };
            foreach (string path in Directory.GetFiles("Assets/_Game/Scripts/Editor/SceneCreation/City", "TownKeyart*.cs")) paths.Add(path);
            var grids = new List<string>();
            var visualMeasurements = new List<string>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (renderer.sprite == null) continue;
                    paths.Add(AssetDatabase.GetAssetPath(renderer.sprite));
                    string spriteName=renderer.sprite.name;
                    if (!spriteName.StartsWith("town_") && spriteName!="temple" && !renderer.name.StartsWith("NPC_") && renderer.name!="Player") continue;
                    var occupied=TownKeyartSceneArt.OpaqueBounds(renderer.sprite);
                    Vector3 min=renderer.transform.TransformPoint(new Vector3(occupied.xMin,occupied.yMin));
                    Vector3 max=renderer.transform.TransformPoint(new Vector3(occupied.xMax,occupied.yMax));
                    visualMeasurements.Add(renderer.transform.parent?.name+"/"+renderer.name+" | "+spriteName+
                        " | rect="+renderer.sprite.rect+" | PPU="+renderer.sprite.pixelsPerUnit+
                        " | alphaMeasured="+renderer.sprite.texture.isReadable+" | scale="+renderer.transform.lossyScale+
                        " | measuredWorldMin="+min+" | measuredWorldMax="+max);
                }
                foreach (var map in root.GetComponentsInChildren<Tilemap>(true))
                {
                    grids.Add(map.name+" | cell="+map.layoutGrid.cellSize+" | origin="+map.transform.position+" | bounds="+map.cellBounds);
                    var tiles=new TileBase[map.GetUsedTilesCount()];
                    map.GetUsedTilesNonAlloc(tiles);
                    foreach (var baseTile in tiles)
                    {
                        paths.Add(AssetDatabase.GetAssetPath(baseTile));
                        if(baseTile is Tile tile && tile.sprite!=null)
                        {
                            paths.Add(AssetDatabase.GetAssetPath(tile.sprite));
                            paths.Add(AssetDatabase.GetAssetPath(tile.sprite.texture));
                        }
                    }
                }
            }
            foreach (string path in new List<string>(paths)) if(File.Exists(path+".meta")) paths.Add(path+".meta");
            var hashes = new List<string>();
            using (var sha = SHA256.Create())
            foreach (string path in paths)
            {
                if (!File.Exists(path)) continue;
                using (var stream=File.OpenRead(path)) hashes.Add(path.Replace('\\','/')+" "+BitConverter.ToString(sha.ComputeHash(stream)).Replace("-",""));
            }
            hashes.Sort(StringComparer.Ordinal);
            File.WriteAllText(output+"inputs.json",JsonUtility.ToJson(new CaptureInputs { utc=DateTime.UtcNow.ToString("O"),sha256=hashes.ToArray(),
                grids=grids.ToArray(),visualMeasurements=visualMeasurements.ToArray() },true));
        }

        private static void Capture(string output, Vector2 center, float ortho)
        {
            if (File.Exists(output)) throw new IOException("Capture already exists: " + output);
            var cameraObject = new GameObject("__TownSceneCaptureCamera");
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;

            try
            {
                var camera = cameraObject.AddComponent<UnityEngine.Camera>();
                camera.orthographic = true;
                camera.orthographicSize = ortho;
                camera.aspect = Width / (float)Height;
                camera.transform.position = new Vector3(center.x, center.y, -100f);
                camera.transform.rotation = Quaternion.identity;
                camera.transparencySortMode = TransparencySortMode.CustomAxis;
                camera.transparencySortAxis = Vector3.up;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.20f, 0.27f, 0.18f);
                camera.cullingMask = ~0;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 1000f;
                camera.targetTexture = target;

                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                texture.Apply();

                string absolutePath = Path.GetFullPath(output);
                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
                File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
                Debug.Log($"[TownSceneCapture] salvo: {absolutePath} ({Width}x{Height}, center={center}, ortho={ortho}; saved scene, not PlayMode).");
            }
            finally
            {
                RenderTexture.active = previous;
                cameraObject.GetComponent<UnityEngine.Camera>().targetTexture = null;
                target.Release();
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
