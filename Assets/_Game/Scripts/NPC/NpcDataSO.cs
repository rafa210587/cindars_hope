using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.NPC
{
    public enum NpcMovementMode
    {
        Static,
        RandomWander
    }

    [CreateAssetMenu(fileName = "NpcData", menuName = "CindarsHope/NPC/NPC Data")]
    public class NpcDataSO : ScriptableObject, IIdentifiedData
    {
        public string NpcId;
        public string DisplayName;
        public string OpeningLine;
        public string ClosingLine;
        public DialogueTreeSO DialogueTree;
        public string ShopId;
        public string DefaultSceneId = "TownScene";
        public Vector2 DefaultPosition;
        public NpcMovementMode MovementMode = NpcMovementMode.Static;
        public NpcWanderData WanderData;

        string IIdentifiedData.Id => NpcId;
    }

    [System.Serializable]
    public class NpcWanderData
    {
        public float WanderSpeed = 1.5f;
        public float WanderRadius = 10f;
        public float PauseMinDuration = 2f;
        public float PauseMaxDuration = 5f;
    }
}
