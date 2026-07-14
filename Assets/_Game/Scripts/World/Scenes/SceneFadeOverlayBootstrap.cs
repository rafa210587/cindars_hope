using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.World.Scenes
{
    public static class SceneFadeOverlayBootstrap
    {
        private static SceneFadeOverlayController _instance;

        public static void Install(Transform owner)
        {
            if (_instance != null) return;

            var go = new GameObject("SceneFadeOverlay");
            go.transform.SetParent(owner);
            if (owner == null) Object.DontDestroyOnLoad(go);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            go.AddComponent<CanvasScaler>();

            var raycaster = go.AddComponent<GraphicRaycaster>();
            raycaster.enabled = false;

            var canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.ignoreParentGroups = true;

            var imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(go.transform, false);
            var image = imageGo.AddComponent<Image>();
            image.color = Color.black;
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin  = Vector2.zero;
            rect.offsetMax  = Vector2.zero;

            _instance = go.AddComponent<SceneFadeOverlayController>();

            Debug.Log("[SceneFadeOverlayBootstrap] SceneFadeOverlay criado (DontDestroyOnLoad, sortingOrder 9999).");
        }
    }
}
