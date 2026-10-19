using UnityEngine;

namespace CindarsHope.Core
{
    /// <summary>
    /// Estilo compartilhado dos menus IMGUI de runtime.
    /// Vive em Core para consumidores de cena/farm não dependerem do módulo UI.
    /// </summary>
    public static class MenuGuiStyle
    {
        public const int BodyFontSize = 14;
        public const int TitleFontSize = 16;

        private static Texture2D _panelBackground;

        public static void Apply()
        {
            var skin = GUI.skin;
            if (skin == null)
            {
                return;
            }

            if (_panelBackground == null)
            {
                _panelBackground = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
                _panelBackground.SetPixel(0, 0, new Color(0.075f, 0.085f, 0.11f, 1f));
                _panelBackground.Apply();
            }
            skin.window.normal.background = _panelBackground;
            skin.window.onNormal.background = _panelBackground;
            skin.label.wordWrap = true;
            SetSize(skin.label, BodyFontSize);
            SetSize(skin.button, BodyFontSize);
            SetSize(skin.box, BodyFontSize);
            SetSize(skin.textField, BodyFontSize);
            SetSize(skin.textArea, BodyFontSize);
            SetSize(skin.toggle, BodyFontSize);
            SetSize(skin.window, TitleFontSize);
        }

        private static void SetSize(GUIStyle style, int size)
        {
            if (style != null && style.fontSize != size)
            {
                style.fontSize = size;
            }
        }
    }
}
