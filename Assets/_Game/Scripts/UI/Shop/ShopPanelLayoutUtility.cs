using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Shop
{
    public static class ShopPanelLayoutUtility
    {
        private const float MinPanelWidth = 720f;
        private const float MaxPanelWidth = 1100f;
        private const float WidthFraction = 0.75f;
        private const float HeightFraction = 0.80f;

        public static Text EnsureResponsiveLayout(RectTransform panel, Transform itemsContainer, Text detailsText)
        {
            if (panel != null)
            {
                var targetW = Mathf.Clamp(Screen.width * WidthFraction, MinPanelWidth, MaxPanelWidth);
                var targetH = Mathf.Max(Screen.height * HeightFraction, 480f);
                panel.sizeDelta = new Vector2(targetW, targetH);
            }

            if (itemsContainer is RectTransform content && content.GetComponentInParent<ScrollRect>(true) == null)
            {
                WrapItemsInScrollViewport(content, panel);
            }

            return detailsText != null ? detailsText : CreateDetailsText(panel);
        }

        private static void WrapItemsInScrollViewport(RectTransform content, RectTransform panel)
        {
            var listW = panel != null ? panel.sizeDelta.x - 40f : 480f;
            var listH = panel != null ? panel.sizeDelta.y - 180f : 280f;
            var parent = content.parent;
            var viewportObject = new GameObject("ItemsScroll_Runtime", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            viewportObject.transform.SetParent(parent, false);
            var viewport = viewportObject.GetComponent<RectTransform>();
            viewport.anchoredPosition = new Vector2(0f, 62f);
            viewport.sizeDelta = new Vector2(listW, listH);
            viewportObject.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.09f, 0.75f);

            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var fitter = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewportObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;
        }

        private static Text CreateDetailsText(RectTransform panel)
        {
            if (panel == null)
            {
                return null;
            }

            var detailsObject = new GameObject("Details_Runtime", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            detailsObject.transform.SetParent(panel, false);
            var rect = detailsObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, -86f);
            rect.sizeDelta = new Vector2(480f, 100f);
            var details = detailsObject.GetComponent<Text>();
            details.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            details.fontSize = 14;
            details.alignment = TextAnchor.UpperLeft;
            details.color = Color.white;
            details.text = "Selecione um item para ver detalhes.";
            return details;
        }
    }
}
