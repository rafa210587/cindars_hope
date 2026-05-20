using System;
using System.Collections.Generic;

namespace CindarsHope.Save
{
    [Serializable]
    public sealed class CaveSaveData
    {
        public int CurrentCaveLevel = 1;
        public int DeepestLayerReached = 1;
        public string CaveWorldSeed = string.Empty;
        public string CaveRunSeed = string.Empty;
        public List<int> UnlockedCheckpoints = new List<int>();
        public List<string> DepletedNodeIds = new List<string>();
    }
}
