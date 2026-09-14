using UnityEngine;
using UnityEngine.UI;
using CindarsHope.Localization;

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

            var hintGo = new GameObject("InteriorLoadingHint", typeof(RectTransform));
            hintGo.transform.SetParent(go.transform, false);
            var hint = hintGo.AddComponent<Text>();
            hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hint.fontSize = 18;
            hint.alignment = TextAnchor.MiddleCenter;
            hint.color = Color.white;
            hint.raycastTarget = false;
            hint.text = LocalizationService.Get("ui.town_interior.loading");
            hint.rectTransform.anchorMin = new Vector2(0.1f, 0.05f);
            hint.rectTransform.anchorMax = new Vector2(0.9f, 0.15f);
            hint.rectTransform.offsetMin = Vector2.zero;
            hint.rectTransform.offsetMax = Vector2.zero;
            hint.enabled = false;
            _instance.ConfigureLoadingHint(hint);

            var recoveryGo = new GameObject("InteriorRecoveryMessage", typeof(RectTransform));
            recoveryGo.transform.SetParent(go.transform, false);
            var recoveryMessage = recoveryGo.AddComponent<Text>();
            recoveryMessage.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            recoveryMessage.fontSize = 20;
            recoveryMessage.alignment = TextAnchor.MiddleCenter;
            recoveryMessage.horizontalOverflow = HorizontalWrapMode.Wrap;
            recoveryMessage.verticalOverflow = VerticalWrapMode.Overflow;
            recoveryMessage.color = Color.white;
            recoveryMessage.raycastTarget = false;
            recoveryMessage.text = LocalizationService.Get("ui.town_interior.recovery_blocked");
            recoveryMessage.rectTransform.anchorMin = new Vector2(.15f, .47f);
            recoveryMessage.rectTransform.anchorMax = new Vector2(.85f, .63f);
            recoveryMessage.rectTransform.offsetMin = Vector2.zero;
            recoveryMessage.rectTransform.offsetMax = Vector2.zero;
            recoveryMessage.enabled = false;

            var retryGo = new GameObject("InteriorRecoveryRetry", typeof(RectTransform));
            retryGo.transform.SetParent(go.transform, false);
            var retryRect = retryGo.GetComponent<RectTransform>();
            retryRect.anchorMin = new Vector2(.35f, .36f);
            retryRect.anchorMax = new Vector2(.65f, .44f);
            retryRect.offsetMin = Vector2.zero;
            retryRect.offsetMax = Vector2.zero;
            var retryImage = retryGo.AddComponent<Image>();
            retryImage.color = new Color(.18f, .23f, .30f, 1f);
            var retryButton = retryGo.AddComponent<Button>();
            retryButton.targetGraphic = retryImage;
            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(retryGo.transform, false);
            var label = labelGo.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 20;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = LocalizationService.Get("ui.town_interior.retry");
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            _instance.ConfigureRecoveryUi(recoveryMessage, retryButton);

            Debug.Log("[SceneFadeOverlayBootstrap] SceneFadeOverlay criado (DontDestroyOnLoad, sortingOrder 9999).");
        }
    }
}
