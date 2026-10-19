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

        /// <summary>
        /// spec_cave_decor_composition_runtime (CV03) — contexto de célula (teto/wall-hug/chão) usado
        /// SÓ para decor visual (<see cref="CaveEnvironmentElementKind.DecorNonBlocking"/> /
        /// <see cref="CaveEnvironmentElementKind.DecorBlocking"/>). Aditivo: derivável de
        /// <see cref="GridPosition"/> via <see cref="CaveDecorContextClassifier"/>, por isso NÃO precisa
        /// ser persistido no save (recomputado ao restaurar/revisitar). Para WaterTile/MineableNode este
        /// campo é irrelevante (sempre <see cref="CaveDecorPlacementContext.FloorCluster"/> por default,
        /// nunca consultado pelo materializer para esses Kinds).
        /// </summary>
        public readonly CaveDecorPlacementContext Context;

        public CaveEnvironmentElementPlacement(
            string elementId,
            CaveEnvironmentElementKind kind,
            Vector2Int gridPosition,
            bool isMineable,
            string mineNodeDataId)
            : this(elementId, kind, gridPosition, isMineable, mineNodeDataId, CaveDecorPlacementContext.FloorCluster)
        {
        }

        public CaveEnvironmentElementPlacement(
            string elementId,
            CaveEnvironmentElementKind kind,
            Vector2Int gridPosition,
            bool isMineable,
            string mineNodeDataId,
            CaveDecorPlacementContext context)
        {
            ElementId = elementId ?? string.Empty;
            Kind = kind;
            GridPosition = gridPosition;
            IsMineable = isMineable;
            MineNodeDataId = mineNodeDataId ?? string.Empty;
            Context = context;
        }
    }
}
