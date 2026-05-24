using System;

namespace CindarsHope.World
{
    [Serializable]
    public class TreeSaveData
    {
        public int TreeIndex;
        public string TreeId;
        public int HitsTaken;
        public int CurrentHp;
        public bool IsChopped;
        public bool IsStump;
        public int RegrowthRemainingDays;
    }
}
