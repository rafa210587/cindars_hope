using UnityEngine;

namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// fable_78 — DTO imutável de UMA colocação de elemento ambiental no grid do nível.
    /// Tipos simples + IDs estáveis (sem refs Unity), apto a persistir no snapshot stable-run.
    /// <see cref="ElementId"/> é determinístico (FNV-1a via CaveLayoutStableHash) — nunca GUID/timestamp.
    /// </summary>
    public readonly struct CaveEnvironmentElementPlacement
    {
        public readonly string ElementId;
        public readonly CaveEnvironmentElementKind Kind;
        public readonly Vector2Int GridPosition;
        public readonly bool IsMineable;

        /// <summary>Id do ResourceNodeDataSO quando <see cref="IsMineable"/>; vazio caso contrário.</summary>
        public readonly string MineNodeDataId;

        public CaveEnvironmentElementPlacement(
            string elementId,
            CaveEnvironmentElementKind kind,
            Vector2Int gridPosition,
            bool isMineable,
            string mineNodeDataId)
        {
            ElementId = elementId ?? string.Empty;
            Kind = kind;
            GridPosition = gridPosition;
            IsMineable = isMineable;
            MineNodeDataId = mineNodeDataId ?? string.Empty;
        }
    }
}
