using UnityEngine;

namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// Marks a named anchor position for NPC schedule resolution in a scene.
    /// Place in TownScene for each NPC home/work anchor position.
    ///
    /// Um anchor pode carregar um ponto de APROXIMAÇÃO opcional (o vão da porta, logo fora da casa):
    /// ao rotear para este anchor, o NPC passa primeiro pelo waypoint e só então entra — assim ele
    /// usa a porta em vez de cruzar a parede.
    /// </summary>
    [DisallowMultipleComponent]
    public class NpcScheduleAnchor : MonoBehaviour
    {
        [SerializeField] private string _anchorId;
        [SerializeField] private string _npcId;
        [SerializeField] private bool _hasApproach;
        [SerializeField] private Vector3 _approachPoint;

        public string AnchorId => _anchorId;
        public string NpcId => _npcId;
        public bool HasApproach => _hasApproach;
        public Vector3 ApproachPoint => _approachPoint;

        public Vector3 GetPosition() => transform.position;
    }
}
