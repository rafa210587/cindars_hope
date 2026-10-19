using UnityEngine;

namespace CindarsHope.Cave.Generation
{
    public readonly struct CaveGenerationPoint
    {
        public readonly CaveGenerationPointType PointType;
        public readonly Vector2Int Position;

        public CaveGenerationPoint(CaveGenerationPointType pointType, Vector2Int position)
        {
            PointType = pointType;
            Position = position;
        }
    }
}
