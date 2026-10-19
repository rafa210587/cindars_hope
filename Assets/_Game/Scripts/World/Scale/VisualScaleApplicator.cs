using UnityEngine;

namespace CindarsHope.World.Scale
{
    /// <summary>
    /// Reads a VisualScaleProfileSO and applies visual scale + collider adjustments at runtime.
    /// Attach to prefab root. Collider and visual scale are applied independently.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VisualScaleApplicator : MonoBehaviour
    {
        [SerializeField] private VisualScaleProfileSO _profile;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private bool _applyOnAwake = true;

        private void Awake()
        {
            if (_applyOnAwake)
            {
                Apply();
            }
        }

        /// <summary>
        /// Assigns the profile (and optional visual root) so editor scene-creation scripts can wire the
        /// applicator without touching serialized YAML by hand. Scale is applied at runtime on Awake.
        /// </summary>
        public void Configure(VisualScaleProfileSO profile, Transform visualRoot = null)
        {
            _profile = profile;
            if (visualRoot != null)
            {
                _visualRoot = visualRoot;
            }
        }

        public void Apply()
        {
            if (_profile == null)
            {
                return;
            }

            ApplyVisualScale();
            ApplyColliderScale();
        }

        private void ApplyVisualScale()
        {
            var root = _visualRoot != null ? _visualRoot : transform;
            root.localScale = Vector3.one * _profile.VisualScale;
        }

        private void ApplyColliderScale()
        {
            if (_profile.ColliderScale <= 0f)
            {
                return;
            }

            var box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = box.size * _profile.ColliderScale;
                return;
            }

            var circle = GetComponent<CircleCollider2D>();
            if (circle != null)
            {
                circle.radius *= _profile.ColliderScale;
            }
        }

        public VisualScaleProfileSO Profile => _profile;

        public Vector2 GetNameplateOffset() => _profile != null ? _profile.NameplateOffset : new Vector2(0f, 1f);
        public Vector2 GetHintOffset() => _profile != null ? _profile.HintOffset : new Vector2(0f, 1.2f);
        public Vector2 GetDamageNumberOffset() => _profile != null ? _profile.DamageNumberOffset : new Vector2(0f, 1f);
        public float GetInteractionRadius() => _profile != null && _profile.InteractionRadius > 0f ? _profile.InteractionRadius : 0f;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_profile == null)
            {
                return;
            }

            if (_profile.InteractionRadius > 0f)
            {
                UnityEditor.Handles.color = new Color(0f, 1f, 0.5f, 0.3f);
                UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.forward, _profile.InteractionRadius);
            }

            if (_profile.SelectionRadius > 0f)
            {
                UnityEditor.Handles.color = new Color(1f, 1f, 0f, 0.2f);
                UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.forward, _profile.SelectionRadius);
            }
        }
#endif
    }
}
