using System.Collections.Generic;

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveRuntimeState
    {
        public int CurrentCaveLevel = 1;
        public int DeepestLayerReached = 1;
        public string CaveWorldSeed;
        public string CaveRunSeed;
        public HashSet<int> UnlockedCheckpoints = new HashSet<int>();
        public HashSet<string> DepletedNodeIds = new HashSet<string>();
        public Dictionary<int, VisitedLevelSnapshot> VisitedLevelSnapshots = new Dictionary<int, VisitedLevelSnapshot>();
    }
}
