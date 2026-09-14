using UnityEngine;

namespace CindarsHope.SceneManagement
{
    /// <summary>Explicit content marker authored on the additive interior prefab.</summary>
    [DisallowMultipleComponent]
    public sealed class TownInteriorInstance : MonoBehaviour
    {
        [System.Serializable]
        public sealed class NpcAnchorBinding
        {
            public string NpcId;
            public string AnchorId;
            public Transform Anchor;
        }

        [SerializeField] private NpcAnchorBinding[] _npcAnchors = System.Array.Empty<NpcAnchorBinding>();
        [SerializeField] private string _houseId;
        [SerializeField] private Transform _entryAnchor;
        [SerializeField] private Transform _exitAnchor;
        [SerializeField] private Vector2 _usableCenter;
        [SerializeField] private Vector2 _usableSize;

        public string HouseId => _houseId;
        public Transform EntryAnchor => _entryAnchor;
        public Transform ExitAnchor => _exitAnchor;
        public void ConfigureNpcAnchors(NpcAnchorBinding[] anchors) =>
            _npcAnchors = anchors ?? System.Array.Empty<NpcAnchorBinding>();

        public bool TryGetNpcAnchor(string npcId, string anchorId, out Transform anchor)
        {
            anchor = null;
            if (string.IsNullOrWhiteSpace(npcId) || string.IsNullOrWhiteSpace(anchorId)) return false;
            foreach (var binding in _npcAnchors)
            {
                if (binding == null || binding.NpcId != npcId || binding.AnchorId != anchorId ||
                    binding.Anchor == null || !binding.Anchor.IsChildOf(transform)) continue;
                anchor = binding.Anchor;
                return true;
            }
            return false;
        }
        public Bounds WorldBounds => new Bounds(transform.TransformPoint(_usableCenter),
            new Vector3(_usableSize.x * Mathf.Abs(transform.lossyScale.x),
                _usableSize.y * Mathf.Abs(transform.lossyScale.y), 0f));

        public bool TryValidate(string expectedHouseId, out string reason)
        {
            if (string.IsNullOrWhiteSpace(expectedHouseId) || _houseId != expectedHouseId ||
                _entryAnchor == null || _exitAnchor == null || _usableSize.x <= 0f || _usableSize.y <= 0f ||
                !Finite(_usableCenter.x) || !Finite(_usableCenter.y) ||
                !Finite(_usableSize.x) || !Finite(_usableSize.y))
            {
                reason = "Interior prefab has a mismatched house, missing anchors or invalid floor bounds.";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
