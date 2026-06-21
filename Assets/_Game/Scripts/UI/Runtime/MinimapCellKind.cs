namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_38 — classificação por célula do minimapa, mapeada para uma cor placeholder no
    /// <see cref="MinimapRenderer"/> (v1 usa shapes/cores, não sprites reais — fora de escopo).
    ///
    /// Hidden = célula ainda não revelada pelo fog-of-war (caverna). Town/Farm nunca usa Hidden
    /// (mapa estático completo, sem fog).
    /// </summary>
    public enum MinimapCellKind
    {
        Hidden = 0,
        Floor = 1,
        Wall = 2,
        Water = 3,
        Hazard = 4
    }
}
