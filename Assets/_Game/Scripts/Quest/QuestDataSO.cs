using System;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Quest
{
    public enum QuestType
    {
        Fetch,
        Kill,
        Discover,
        Custom
    }

    [CreateAssetMenu(fileName = "QuestData", menuName = "CindarsHope/Quest/Quest Data")]
    public class QuestDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string Title;
        [TextArea] public string Description;
        public string GiverNpcId;
        public QuestType Type;
        public int RewardXp = 100;
        public int RewardGold = 50;
        public string[] RewardItemIds;
        public bool IsActive = true;

        string IIdentifiedData.Id => Id;
    }

    [System.Serializable]
    public class QuestSaveData
    {
        public string QuestId;
        public bool IsCompleted;
        public bool IsAbandoned;
        public int Progress;
    }
}
