using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.NPC
{
    [CreateAssetMenu(fileName = "NpcDialogueData", menuName = "CindarsHope/NPC/NPC Dialogue Data")]
    public class NpcDialogueDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string OpeningLine;
        public string ClosingLine;

        string IIdentifiedData.Id => Id;
    }
}
