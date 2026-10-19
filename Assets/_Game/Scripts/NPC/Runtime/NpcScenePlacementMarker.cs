using UnityEngine;

namespace CindarsHope.NPC.Runtime
{
    [DisallowMultipleComponent]
    public sealed class NpcScenePlacementMarker : MonoBehaviour
    {
        [SerializeField] private string _npcId;
        [SerializeField] private string _sceneId = "TownScene";
        [SerializeField] private string _placementId;
        [SerializeField] private string _movementProfile;
        [SerializeField] private bool _reachable = true;

        public string NpcId => _npcId;
        public string SceneId => _sceneId;
        public string PlacementId => _placementId;
        public string MovementProfile => _movementProfile;
        public bool Reachable => _reachable;

        public void Configure(string npcId, string sceneId, string placementId, string movementProfile, bool reachable)
        {
            _npcId = npcId;
            _sceneId = sceneId;
            _placementId = placementId;
            _movementProfile = movementProfile;
            _reachable = reachable;
        }
    }
}
