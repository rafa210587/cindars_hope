using UnityEngine;

namespace CindarsHope.Farm.Scene
{
    public enum FarmSceneZoneType
    {
        Unknown = 0,
        PlayerSpawn = 1,
        CropField = 2,
        ResourceTrees = 3,
        ResourceRocks = 4,
        Forage = 5,
        LakeFishing = 6,
        ShippingSellpoint = 7,
        Construction = 8,
        HouseEntrance = 9,
        TownExit = 10,
        CaveEntrance = 11
    }

    /// <summary>
    /// Marca uma zona logica da FarmScene (spawn, colheita, recurso, etc.).
    /// v5 (spec_farm_scene_relayout_v4 §15.3 item 1): a zona NAO renderiza em jogo.
    /// A visualizacao e feita exclusivamente via OnDrawGizmos (visivel apenas no Editor).
    /// O BoxCollider2D trigger no mesmo GameObject provee a logica de deteccao.
    /// </summary>
    public sealed class FarmSceneZoneMarker : MonoBehaviour
    {
        [SerializeField] private FarmSceneZoneType zoneType = FarmSceneZoneType.Unknown;
        [SerializeField] private string stableId = "";

        public FarmSceneZoneType ZoneType => zoneType;
        public string StableId => stableId;

#if UNITY_EDITOR
        // Mapa de tipo de zona para cor de gizmo (legivel no editor sem saturar a viewport).
        private static readonly System.Collections.Generic.Dictionary<FarmSceneZoneType, Color> GizmoColors =
            new System.Collections.Generic.Dictionary<FarmSceneZoneType, Color>
            {
                { FarmSceneZoneType.PlayerSpawn,       new Color(0.2f,  0.45f, 0.95f, 0.40f) },
                { FarmSceneZoneType.CropField,         new Color(0.35f, 0.22f, 0.12f, 0.20f) },
                { FarmSceneZoneType.ResourceTrees,     new Color(0.14f, 0.48f, 0.18f, 0.25f) },
                { FarmSceneZoneType.ResourceRocks,     new Color(0.42f, 0.42f, 0.42f, 0.40f) },
                { FarmSceneZoneType.Forage,            new Color(0.45f, 0.64f, 0.25f, 0.30f) },
                { FarmSceneZoneType.LakeFishing,       new Color(0.18f, 0.44f, 0.82f, 0.35f) },
                { FarmSceneZoneType.ShippingSellpoint, new Color(0.85f, 0.62f, 0.18f, 0.40f) },
                { FarmSceneZoneType.Construction,      new Color(0.58f, 0.45f, 0.32f, 0.30f) },
                { FarmSceneZoneType.HouseEntrance,     new Color(0.62f, 0.36f, 0.25f, 0.40f) },
                { FarmSceneZoneType.TownExit,          new Color(0.82f, 0.82f, 0.25f, 0.40f) },
                { FarmSceneZoneType.CaveEntrance,      new Color(0.35f, 0.28f, 0.50f, 0.45f) },
            };

        // v7 (spec_farm_scene_relayout_v4 §15.5 / §15.6): trocado OnDrawGizmos → OnDrawGizmosSelected.
        // Overlay de zonas SÓ aparece quando o objeto está selecionado no Editor;
        // o Game View fica limpo mesmo com Gizmos ligado.
        private void OnDrawGizmosSelected()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null) return;

            if (!GizmoColors.TryGetValue(zoneType, out var gizmoColor))
                gizmoColor = new Color(0.8f, 0.8f, 0.8f, 0.25f);

            // Retangulo preenchido semi-transparente.
            Gizmos.color = gizmoColor;
            var center = (Vector3)col.offset + transform.position;
            var size3D = new Vector3(col.size.x, col.size.y, 0.01f);
            Gizmos.DrawCube(center, size3D);

            // Borda opaca para legibilidade.
            var borderColor = gizmoColor;
            borderColor.a = Mathf.Min(1f, gizmoColor.a * 2.5f);
            Gizmos.color = borderColor;
            Gizmos.DrawWireCube(center, size3D);
        }
#endif
    }
}
