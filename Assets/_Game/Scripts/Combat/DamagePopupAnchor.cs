using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>
    /// SPEC 14A-FIX10: per-target anchor that says where the floating damage popup should appear.
    /// Avoids guessing positions via Physics2D.OverlapPoint or Vector3.zero. Both the player and
    /// runtime-spawned enemies get one of these and FloatingDamageNumberDisplayer looks it up.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DamagePopupAnchor : MonoBehaviour
    {
        [SerializeField] private float _additionalOffsetY = 0.05f;
        [SerializeField] private Collider2D _collider;

        public Vector3 GetPopupWorldPosition()
        {
            if (_collider == null) _collider = GetComponent<Collider2D>();
            if (_collider != null)
            {
                var b = _collider.bounds;
                return new Vector3(b.center.x, b.max.y + _additionalOffsetY, transform.position.z);
            }
            return transform.position + Vector3.up * (0.5f + _additionalOffsetY);
        }
    }
}
