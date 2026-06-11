using UnityEngine;

namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// Marks a named anchor position for NPC schedule resolution in a scene.
    /// Place in TownScene for each NPC home/work anchor position.
    /// </summary>
    [DisallowMultipleComponent]
    public class NpcScheduleAnchor : MonoBehaviour
    {
        [SerializeField] private string _anchorId;
        [SerializeField] private string _npcId;

        public string AnchorId => _anchorId;
        public string NpcId => _npcId;

        public Vector3 GetPosition() => transform.position;
    }
}
