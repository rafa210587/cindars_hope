using System.IO;
using System.Linq;
using CindarsHope.Farm;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Dev
{
    /// <summary>
    /// Ferramenta de diagnostico (Dev): abre a FarmScene ja salva em Edit Mode e captura um PNG
    /// top-down enquadrando todos os Renderer da cena, para inspecao visual rapida sem depender de
    /// Play Mode. NAO gera/regenera a cena (isso e responsabilidade de CreateMvpFarmScene, chamado
    /// pelo menu "CindarsHope/Inicializar Projeto") — apenas abre a cena existente e fotografa.
    /// </summary>
    public static class FarmSceneCapture
    {
        private const string ScenePath = "Assets/_Game/Scenes/FarmScene.unity";
        private const string OutputPath = "D:/Projetos/Cindars_Hope/cindars_hope/docs/validation/playmode/farm_capture.png";
        private const string CollisionDebugPath = "D:/Projetos/Cindars_Hope/cindars_hope/docs/validation/playmode/farm_capture_colliders.png";
        private const string CloseupPath = "D:/Projetos/Cindars_Hope/cindars_hope/docs/validation/playmode/farm_capture_closeup.png";
        private const string AnimalRowPath = "D:/Projetos/Cindars_Hope/cindars_hope/docs/validation/playmode/farm_capture_animals.png";
        private const float MarginUnits = 2f;
        private const int TextureWidth = 1600;
        private const int TextureHeight = 1200;

        [MenuItem("CindarsHope/Dev/Capturar FarmScene (PNG)")]
        public static void CaptureFarmTopDownFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[FarmSceneCapture] Nao pode capturar durante Play Mode. Saia do Play Mode e rode novamente.");
                return;
            }
            CaptureFarmTopDown();
        }

        [MenuItem("CindarsHope/Dev/Capturar FarmScene Colliders (PNG)")]
        public static void CaptureFarmCollidersFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[FarmSceneCapture] Nao pode capturar durante Play Mode. Saia do Play Mode e rode novamente.");
                return;
            }

            CaptureFarmTopDown(includeColliderDebug: true);
        }

        /// <summary>
        /// Regenera a FarmScene (com o wiring atual de arte) e captura o PNG numa unica chamada —
        /// pratico para rodar via -executeMethod em batchmode (um so start do Unity).
        /// </summary>
        public static void RegenAndCapture()
        {
            CindarsHope.Editor.Farm.FarmAnimalMotionAuthoring.Generate();
            CindarsHope.Editor.SceneCreation.CreateMvpFarmScene.CreateScene();
            CaptureFarmTopDown();
        }

        /// <summary>Captura a câmera de gameplay em Play Mode; batch deve omitir -quit.</summary>
        public static void CaptureFarmGameplayBatch()
        {
            FarmPlayModeCaptureSession.Begin();
        }

        public static void CaptureFarmTopDown()
        {
            CaptureFarmTopDown(includeColliderDebug: false);
        }

        private static void CaptureFarmTopDown(bool includeColliderDebug)
        {
            if (!File.Exists(ScenePath) && !AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath))
            {
                Debug.LogWarning($"[FarmSceneCapture] Cena nao encontrada em '{ScenePath}'. Rode 'CindarsHope/Inicializar Projeto' primeiro.");
                return;
            }

            Scene scene;
            try
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FarmSceneCapture] Falha ao abrir a cena '{ScenePath}': {ex.Message}");
                return;
            }

            if (!scene.IsValid())
            {
                Debug.LogError($"[FarmSceneCapture] EditorSceneManager.OpenScene retornou cena invalida para '{ScenePath}'.");
                return;
            }

            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            if (renderers == null || renderers.Length == 0)
            {
                Debug.LogWarning("[FarmSceneCapture] Nenhum Renderer encontrado na cena — nada para enquadrar/capturar.");
                return;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var captureCameraObject = new GameObject("__FarmSceneCaptureCamera");
            RenderTexture renderTexture = null;
            Texture2D texture = null;
            try
            {
                var camera = captureCameraObject.AddComponent<UnityEngine.Camera>();
                camera.orthographic = true;
                camera.transparencySortMode = TransparencySortMode.CustomAxis;
                camera.transparencySortAxis = Vector3.up;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.35f, 0.42f, 0.30f); // cinza-esverdeado solido
                camera.cullingMask = ~0;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 2000f;

                // Cena e 2D top-down no plano XY: camera olha ao longo de -Z, centralizada nos bounds.
                var center = bounds.center;
                camera.transform.position = new Vector3(center.x, center.y, center.z - 100f);
                camera.transform.rotation = Quaternion.identity;

                var halfWidth = Mathf.Max(bounds.extents.x, 0.01f) + MarginUnits;
                var halfHeight = Mathf.Max(bounds.extents.y, 0.01f) + MarginUnits;
                var aspect = TextureWidth / (float)TextureHeight;
                // orthographicSize = metade da altura visivel; garante que largura tambem caiba dado o aspect.
                camera.orthographicSize = Mathf.Max(halfHeight, halfWidth / aspect);

                // Diagnósticos regionais seguem as âncoras vigentes; continuam distintos da câmera real.
                var fullOrtho = camera.orthographicSize;
                var views = new (string path, float ortho, int w, int h, Vector2 center)[]
                {
                    (includeColliderDebug ? CollisionDebugPath : OutputPath, fullOrtho, TextureWidth, TextureHeight, new Vector2(center.x, center.y)),
                    // Additional real-scene framing: covers canonical 64x44u map; not the gameplay camera.
                    (Path.Combine(Path.GetDirectoryName(OutputPath), "farm_capture_keyart_composition.png"), 28f, 1536, 1024, Vector2.zero),
                    (CloseupPath, 8.5f, 1600, 1200, new Vector2(FarmLevel1LayoutContract.HouseStartX, FarmLevel1LayoutContract.HouseStartY - 3f)),
                    (AnimalRowPath, 12f, 1600, 1200, new Vector2(FarmLevel1LayoutContract.AnimalBuildingsMinX + FarmLevel1LayoutContract.AnimalBuildingsWidth / 2f, FarmLevel1LayoutContract.AnimalBuildingsMinY + 3f)),
                    (RegionalPath("border_north"), 8.5f, 1600, 900, new Vector2(0f, FarmLevel1LayoutContract.MaxY)),
                    (RegionalPath("border_south"), 8.5f, 1600, 900, new Vector2(0f, FarmLevel1LayoutContract.MinY)),
                    (RegionalPath("border_west"), 8.5f, 1600, 900, new Vector2(FarmLevel1LayoutContract.MinX, 0f)),
                    (RegionalPath("border_east"), 8.5f, 1600, 900, new Vector2(FarmLevel1LayoutContract.MaxX, FarmLevel1LayoutContract.CityExitY)),
                    (RegionalPath("cultivation"), 10f, 1600, 1200, new Vector2(FarmLevel1LayoutContract.CropFieldMinX + FarmLevel1LayoutContract.CropFieldWidth / 2f, FarmLevel1LayoutContract.CropFieldMinY + FarmLevel1LayoutContract.CropFieldHeight / 2f)),
                    (RegionalPath("water_bridge"), 14f, 1600, 1200, new Vector2(FarmLevel1LayoutContract.BridgeCenterX - 5f, FarmLevel1LayoutContract.LakeCenterY + 5f)),
                    (RegionalPath("mountain"), 14f, 1600, 1200, new Vector2(0f, FarmLevel1LayoutContract.MountainBaseY - 2f)),
                    (RegionalPath("homestead"), 9f, 1600, 1200, new Vector2(FarmLevel1LayoutContract.HouseStartX + 3f, FarmLevel1LayoutContract.HouseStartY - 2f)),
                    (RegionalPath("forest"), 11f, 1600, 1200, new Vector2(FarmLevel1LayoutContract.FonteAnchorX, FarmLevel1LayoutContract.FonteAnchorY + 3f)),
                    (RegionalPath("animals"), 12f, 1600, 1200, new Vector2(FarmLevel1LayoutContract.AnimalBuildingsMinX + FarmLevel1LayoutContract.AnimalBuildingsWidth / 2f, FarmLevel1LayoutContract.AnimalBuildingsMinY + 3f)),
                };
                foreach (var v in views)
                {
                    camera.transform.position = new Vector3(v.center.x, v.center.y, center.z - 100f);
                    camera.orthographicSize = v.ortho;
                    camera.aspect = v.w / (float)v.h;
                    var rt = new RenderTexture(v.w, v.h, 24, RenderTextureFormat.ARGB32);
                    camera.targetTexture = rt;
                    camera.Render();
                    var prev = RenderTexture.active;
                    RenderTexture.active = rt;
                    var tex = new Texture2D(v.w, v.h, TextureFormat.RGB24, false);
                    tex.ReadPixels(new Rect(0, 0, v.w, v.h), 0, 0);
                    if (includeColliderDebug) DrawColliderBounds(tex, camera, v.center);
                    tex.Apply();
                    RenderTexture.active = prev;
                    var bytes = tex.EncodeToPNG();
                    var outputDirectory = System.Environment.GetEnvironmentVariable("CINDARS_FARM_CAPTURE_OUTPUT");
                    var outputFile = string.IsNullOrEmpty(outputDirectory) ? v.path : Path.Combine(outputDirectory, Path.GetFileName(v.path));
                    var dir = Path.GetDirectoryName(outputFile);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllBytes(outputFile, bytes);
                    Object.DestroyImmediate(tex);
                    camera.targetTexture = null;
                    RenderTexture.active = null;
                    rt.Release();
                    Object.DestroyImmediate(rt);
                    Debug.Log($"[FarmSceneCapture] salvo: {outputFile} ({v.w}x{v.h}, ortho {v.ortho:F1}). center={center}");
                }
            }
            finally
            {
                if (texture != null)
                {
                    Object.DestroyImmediate(texture);
                }
                if (renderTexture != null)
                {
                    RenderTexture.active = null;
                    renderTexture.Release();
                    Object.DestroyImmediate(renderTexture);
                }
                if (captureCameraObject != null)
                {
                    Object.DestroyImmediate(captureCameraObject);
                }
            }
        }

        private static void DrawColliderBounds(Texture2D texture, UnityEngine.Camera camera, Vector2 viewCenter)
        {
            var colliders = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].isTrigger) continue;
                var bounds = colliders[i].bounds;
                DrawBoundsOutline(texture, camera, viewCenter, bounds, Color.yellow);
            }
        }

        private static string RegionalPath(string region) => Path.Combine(Path.GetDirectoryName(OutputPath), "farm_capture_region_" + region + ".png");

        private static void DrawBoundsOutline(Texture2D texture, UnityEngine.Camera camera, Vector2 viewCenter, Bounds bounds, Color color)
        {
            var halfHeight = camera.orthographicSize;
            var halfWidth = halfHeight * camera.aspect;
            var xMin = Mathf.RoundToInt((bounds.min.x - (viewCenter.x - halfWidth)) / (halfWidth * 2f) * (texture.width - 1));
            var xMax = Mathf.RoundToInt((bounds.max.x - (viewCenter.x - halfWidth)) / (halfWidth * 2f) * (texture.width - 1));
            var yMin = Mathf.RoundToInt((bounds.min.y - (viewCenter.y - halfHeight)) / (halfHeight * 2f) * (texture.height - 1));
            var yMax = Mathf.RoundToInt((bounds.max.y - (viewCenter.y - halfHeight)) / (halfHeight * 2f) * (texture.height - 1));
            for (var x = xMin; x <= xMax; x++) { SetPixelIfVisible(texture, x, yMin, color); SetPixelIfVisible(texture, x, yMax, color); }
            for (var y = yMin; y <= yMax; y++) { SetPixelIfVisible(texture, xMin, y, color); SetPixelIfVisible(texture, xMax, y, color); }
        }

        private static void SetPixelIfVisible(Texture2D texture, int x, int y, Color color)
        {
            if (x >= 0 && y >= 0 && x < texture.width && y < texture.height) texture.SetPixel(x, y, color);
        }
    }
}
