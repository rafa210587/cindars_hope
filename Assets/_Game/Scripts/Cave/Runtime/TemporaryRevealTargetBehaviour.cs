using System;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class TemporaryRevealTargetBehaviour : MonoBehaviour, ITemporaryRevealTarget
    {
        private string _id;
        private TemporaryRevealKind _kind;
        private Func<bool> _isExhausted;
        private Func<bool> _isActive;
        private Func<bool> _isSecret;
        private SpriteRenderer _source;
        private SpriteRenderer _overlay;
        private float _expiresAt;
        private bool _registered;

        public string RevealTargetId => _id ?? string.Empty;
        public TemporaryRevealKind RevealKind => _kind;
        public float WorldX => transform.position.x;
        public float WorldY => transform.position.y;
        public bool IsEnabled => isActiveAndEnabled && gameObject.activeInHierarchy;
        public bool IsSecret => _isSecret?.Invoke() == true;
        public bool IsExhausted => _isExhausted?.Invoke() == true;
        public bool IsActive => _isActive?.Invoke() != false;

        public static TemporaryRevealTargetBehaviour Attach(GameObject owner, string id,
            TemporaryRevealKind kind, Func<bool> isExhausted = null,
            Func<bool> isActive = null, Func<bool> isSecret = null)
        {
            if (owner == null || string.IsNullOrWhiteSpace(id)) return null;
            var target = owner.GetComponent<TemporaryRevealTargetBehaviour>()
                ?? owner.AddComponent<TemporaryRevealTargetBehaviour>();
            target.Configure(id, kind, isExhausted, isActive, isSecret);
            return target;
        }

        public void Configure(string id, TemporaryRevealKind kind,
            Func<bool> isExhausted, Func<bool> isActive, Func<bool> isSecret)
        {
            Unregister();
            _id = id ?? string.Empty;
            _kind = kind;
            _isExhausted = isExhausted;
            _isActive = isActive;
            _isSecret = isSecret;
            _source = GetComponentInChildren<SpriteRenderer>();
            Register();
        }

        public void ApplyTemporaryReveal(string sourceId, float expiresAt)
        {
            if (!TemporaryRevealRegistry.IsEligible(this)) return;
            _expiresAt = Mathf.Max(_expiresAt, expiresAt);
            EnsureOverlay();
            if (_overlay != null) _overlay.enabled = true;
        }

        private void LateUpdate()
        {
            if (_overlay == null) return;
            if (Time.time >= _expiresAt || !TemporaryRevealRegistry.IsEligible(this))
            {
                _overlay.enabled = false;
                return;
            }
            if (_source != null)
            {
                _overlay.sprite = _source.sprite;
                _overlay.flipX = _source.flipX;
                _overlay.flipY = _source.flipY;
                _overlay.sortingLayerID = _source.sortingLayerID;
                _overlay.sortingOrder = _source.sortingOrder + 1;
            }
        }

        private void OnEnable() => Register();
        private void OnDisable()
        {
            Unregister();
            if (_overlay != null) _overlay.enabled = false;
        }

        private void Register()
        {
            if (_registered || string.IsNullOrWhiteSpace(_id)) return;
            _registered = TemporaryRevealRegistryProvider.Registry.Register(this);
        }

        private void Unregister()
        {
            if (!_registered) return;
            TemporaryRevealRegistryProvider.Registry.Unregister(this);
            _registered = false;
        }

        private void EnsureOverlay()
        {
            if (_overlay != null || _source == null) return;
            var go = new GameObject("TemporaryRevealOverlay");
            go.transform.SetParent(_source.transform, false);
            _overlay = go.AddComponent<SpriteRenderer>();
            _overlay.sprite = _source.sprite;
            _overlay.color = new Color(1f, .82f, .28f, .55f);
            _overlay.sortingLayerID = _source.sortingLayerID;
            _overlay.sortingOrder = _source.sortingOrder + 1;
        }
    }
}
