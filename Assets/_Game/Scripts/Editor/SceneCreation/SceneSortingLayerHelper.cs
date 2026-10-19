using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    // Helper compartilhado de sorting layer para os scene creators (Town/Farm/Cave). Contrato de
    // sorting (ver docs/decisions e a FASE 1 de correção de sorting da cidade): Ground/World/Roof
    // com m_TransparencySortMode Custom Axis (Y). Antes desta extração, cada *CreateMvp*Scene.cs
    // tinha sua própria cópia idêntica deste método (drift risk); agora só existe aqui.
    internal static class SceneSortingLayerHelper
    {
        // Aplica a sorting layer pedida ao renderer. Se a layer não existir no TagManager, loga um
        // wiring-error claro (em vez de falhar silenciosamente em Default) e aplica o fallbackOrder
        // como sortingOrder puro na layer atual do renderer.
        public static void TrySetSortingLayer(SpriteRenderer renderer, string layerName, int fallbackOrder)
        {
            foreach (var layer in SortingLayer.layers)
            {
                if (layer.name == layerName)
                {
                    renderer.sortingLayerName = layerName;
                    return;
                }
            }

            renderer.sortingOrder = fallbackOrder;
            Debug.LogError(
                $"[SceneCreation] Sorting layer '{layerName}' não existe no TagManager — " +
                $"renderer '{renderer.gameObject.name}' caiu em Default.");
        }
    }

    // Sprites de MUNDO com volume (Assets/_Game/Art/Generated/World: building/, building/houses_modular/,
    // trees/, props/, foliage/, interior/, animals/) são importados com pivot BottomCenter (exigência do
    // Y-sort global — ver WorldSpritePivotImportStep). Os 3 scene creators (Town/Farm/Cave) posicionam
    // esses objetos como se o pivot fosse Center: o transform.position era o CENTRO VISUAL pretendido.
    // Com pivot bottom, um GameObject cujo position = centro pretendido renderiza meia-altura ACIMA do
    // lugar certo (sprite ocupa [y, y+h] a partir do position, em vez de [y-h/2, y+h/2]).
    //
    // Esta classe converte "centro visual pretendido" -> "position que produz esse mesmo centro visual
    // com pivot bottom", preservando EXATAMENTE o visual anterior (pivot Center) nos call sites que já
    // existiam antes da mudança de import. Não se aplica a objetos cujo sprite vem de GetBuiltinSprite()
    // (UI/Skin/UISprite.psd) — esse não é afetado pela mudança de pivot.
    internal static class WorldSpriteBasePlacement
    {
        // visualCenter: onde o CENTRO do sprite deveria ficar (o valor que o código já calculava antes
        // da mudança de pivot). visualHeight: altura visual do sprite já escalado (ex.: sprite.bounds.size.y
        // * scale para drawMode Simple/Tiled com escala uniforme, ou o "size" configurado para Tiled).
        // Retorna o Y que deve ser atribuído a transform.position/localPosition para manter o mesmo
        // centro visual de antes, agora que o pivot é a base.
        public static float BaseYForVisualCenter(float visualCenterY, float visualHeight) =>
            visualCenterY - visualHeight * 0.5f;

        public static Vector3 BaseFromVisualCenter(Vector3 visualCenter, float visualHeight) =>
            new Vector3(visualCenter.x, BaseYForVisualCenter(visualCenter.y, visualHeight), visualCenter.z);

        public static Vector2 BaseFromVisualCenter(Vector2 visualCenter, float visualHeight) =>
            new Vector2(visualCenter.x, BaseYForVisualCenter(visualCenter.y, visualHeight));
    }
}
