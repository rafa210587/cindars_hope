using System.IO;
using System.Linq;
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
        private const string CloseupPath = "D:/Projetos/Cindars_Hope/cindars_hope/docs/validation/playmode/farm_capture_closeup.png";
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

        /// <summary>
        /// Regenera a FarmScene (com o wiring atual de arte) e captura o PNG numa unica chamada —
        /// pratico para rodar via -executeMethod em batchmode (um so start do Unity).
        /// </summary>
        public static void RegenAndCapture()
        {
            CindarsHope.Editor.SceneCreation.CreateMvpFarmScene.CreateScene();
            CaptureFarmTopDown();
        }

        public static void CaptureFarmTopDown()
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

                // Duas vistas: (1) fazenda inteira; (2) close no zoom do jogo (ortho 8.5) para inspecionar seam/grade do chao.
                var fullOrtho = camera.orthographicSize;
                var views = new (string path, float ortho, int w, int h)[]
                {
                    (OutputPath, fullOrtho, TextureWidth, TextureHeight),
                    (CloseupPath, 8.5f, 1600, 1200),
                };
                foreach (var v in views)
                {
                    camera.orthographicSize = v.ortho;
                    camera.aspect = v.w / (float)v.h;
                    var rt = new RenderTexture(v.w, v.h, 24, RenderTextureFormat.ARGB32);
                    camera.targetTexture = rt;
                    camera.Render();
                    var prev = RenderTexture.active;
                    RenderTexture.active = rt;
                    var tex = new Texture2D(v.w, v.h, TextureFormat.RGB24, false);
                    tex.ReadPixels(new Rect(0, 0, v.w, v.h), 0, 0);
                    tex.Apply();
                    RenderTexture.active = prev;
                    var bytes = tex.EncodeToPNG();
                    var dir = Path.GetDirectoryName(v.path);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllBytes(v.path, bytes);
                    Object.DestroyImmediate(tex);
                    camera.targetTexture = null;
                    RenderTexture.active = null;
                    rt.Release();
                    Object.DestroyImmediate(rt);
                    Debug.Log($"[FarmSceneCapture] salvo: {v.path} ({v.w}x{v.h}, ortho {v.ortho:F1}). center={center}");
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
    }
}
