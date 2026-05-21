using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public class CaveRuntimeMaterializationResult
    {
        public int CreatedFloorTiles { get; set; }
        public int CreatedWallTiles { get; set; }
        public int ResourceCandidateCount { get; set; }
        public int CreatedResourceNodes { get; set; }
        public int CreatedEnemies { get; set; }
        public Vector3 BackExitPosition { get; set; }
        public Vector3 ForwardExitPosition { get; set; }

        public CaveRuntimeMaterializationResult()
        {
            CreatedFloorTiles = 0;
            CreatedWallTiles = 0;
            ResourceCandidateCount = 0;
            CreatedResourceNodes = 0;
            CreatedEnemies = 0;
            BackExitPosition = Vector3.zero;
            ForwardExitPosition = Vector3.zero;
        }
    }
}
