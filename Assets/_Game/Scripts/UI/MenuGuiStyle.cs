namespace CindarsHope.UI
{
    /// <summary>
    /// Compatibilidade para callers legados. A implementação canônica vive em Core para permitir
    /// que menus de gameplay/farm usem o estilo sem depender do módulo UI.
    /// </summary>
    public static class MenuGuiStyle
    {
        public const int BodyFontSize = CindarsHope.Core.MenuGuiStyle.BodyFontSize;
        public const int TitleFontSize = CindarsHope.Core.MenuGuiStyle.TitleFontSize;

        public static void Apply()
        {
            CindarsHope.Core.MenuGuiStyle.Apply();
        }
    }
}
