using System;
using CindarsHope.Core;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.NPC.Schedule
{
    /// <summary>Opt-in presentation of the existing Town NPC in one additive house; never owns shop or identity.</summary>
    [DisallowMultipleComponent]
    public sealed class NpcInteriorPresentationAdapter : MonoBehaviour
    {
        private string _npcId, _houseId, _workAnchorId, _homeAnchorId;
        private Func<string, string, Transform> _resolvePresentedAnchor;
        private NpcWanderer _wanderer;
        private Rigidbody2D _body;
        private SpriteRenderer[] _renderers;
        private Collider2D[] _colliders;
        private bool[] _rendererEnabled, _colliderEnabled;
        private bool _bound, _presented;
        private string _playerHouseId;
        private bool _wandererWasEnabled;
        private bool _pendingRestore;
        private string _pendingHouseId, _pendingAnchorId;
        private Vector2 _pendingLocalPosition;
        private Vector3 _pendingExteriorPosition;
        private Vector3 _exteriorPosition;
        private NpcSpaceLocation _location;

        public bool IsInterior => _bound && _location.IsInterior;
        public bool IsBound => _bound;
        public bool IsPresented => _presented;
        public string InteriorHouseId => IsInterior ? _location.HouseId : string.Empty;
        public string AnchorId => IsInterior ? _location.AnchorId : string.Empty;
        public Vector2 InteriorLocalPosition => IsInterior ? _location.LocalPosition : Vector2.zero;
        public Vector3 ExteriorSavePosition => _exteriorPosition;
        public bool CanInteractInCurrentSpace => !_bound ||
            (IsInterior ? _presented && _playerHouseId == _houseId :
                string.IsNullOrEmpty(_playerHouseId) && !_presented);

        private void OnEnable() => GameEventBus.Subscribe<TownInteriorContextChangedEvent>(OnContextChanged);
        private void OnDisable() => GameEventBus.Unsubscribe<TownInteriorContextChangedEvent>(OnContextChanged);

        public bool BindPilot(string npcId, string houseId, string workAnchorId, string homeAnchorId,
            Func<string, string, Transform> resolvePresentedAnchor)
        {
            if (string.IsNullOrEmpty(npcId) || string.IsNullOrEmpty(houseId) ||
                string.IsNullOrEmpty(workAnchorId) || string.IsNullOrEmpty(homeAnchorId) ||
                resolvePresentedAnchor == null) return false;
            _npcId = npcId; _houseId = houseId; _workAnchorId = workAnchorId;
            _homeAnchorId = homeAnchorId; _resolvePresentedAnchor = resolvePresentedAnchor;
            _wanderer = GetComponent<NpcWanderer>();
            if (!_bound) _wandererWasEnabled = _wanderer != null && _wanderer.enabled;
            _body = GetComponent<Rigidbody2D>();
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _colliders = GetComponentsInChildren<Collider2D>(true);
            _rendererEnabled = new bool[_renderers.Length];
            _colliderEnabled = new bool[_colliders.Length];
            for (var i = 0; i < _renderers.Length; i++) _rendererEnabled[i] = _renderers[i].enabled;
            for (var i = 0; i < _colliders.Length; i++) _colliderEnabled[i] = _colliders[i].enabled;
            if (!_bound) _exteriorPosition = transform.position;
            _bound = true;
            if (_pendingRestore)
            {
                _pendingRestore = false;
                RestoreInterior(_pendingHouseId, _pendingAnchorId, _pendingLocalPosition, _pendingExteriorPosition);
            }
            RefreshPresentation();
            return true;
        }

        public bool ApplyScheduleBlock(NpcRuntimeBlock block)
        {
            if (!_bound) return false;
            var anchor = block == NpcRuntimeBlock.Work ? _workAnchorId :
                block == NpcRuntimeBlock.Home || block == NpcRuntimeBlock.Night ? _homeAnchorId : null;
            if (anchor == null)
            {
                if (IsInterior) LeaveInterior();
                return false; // Social uses the existing physical Town route.
            }
            if (!IsInterior && !_presented) _exteriorPosition = transform.position;
            _location = new NpcSpaceLocation { HouseId = _houseId, AnchorId = anchor };
            _wanderer?.ClearDestination();
            if (_wanderer != null) _wanderer.enabled = false;
            RefreshPresentation();
            return true; // Never enqueue a Town route toward an off-playfield prefab.
        }

        public void RestoreInterior(string houseId, string anchorId, Vector2 localPosition, Vector3 exteriorPosition)
        {
            if (!_bound)
            {
                _pendingRestore = true; _pendingHouseId = houseId; _pendingAnchorId = anchorId;
                _pendingLocalPosition = localPosition; _pendingExteriorPosition = exteriorPosition;
                return;
            }
            if (houseId != _houseId || (anchorId != _workAnchorId && anchorId != _homeAnchorId)) return;
            _exteriorPosition = exteriorPosition;
            _location = new NpcSpaceLocation { HouseId = houseId, AnchorId = anchorId, LocalPosition = localPosition };
            _wanderer?.ClearDestination();
            if (_wanderer != null) _wanderer.enabled = false;
            RefreshPresentation();
        }

        public void RestoreExterior(Vector3 exteriorPosition)
        {
            _pendingRestore = false;
            _exteriorPosition = exteriorPosition;
            if (!_bound) return;
            _location = default;
            _presented = false;
            SetPosition(exteriorPosition);
            if (_wanderer != null) _wanderer.enabled = _wandererWasEnabled;
            RefreshPresentation();
        }

        private void OnContextChanged(TownInteriorContextChangedEvent evt)
        {
            _playerHouseId = evt.IsInside ? evt.HouseId : string.Empty;
            RefreshPresentation();
        }

        private void RefreshPresentation()
        {
            if (!_bound) return;
            var anchor = IsInterior && _playerHouseId == _houseId ?
                _resolvePresentedAnchor(_houseId, _location.AnchorId) : null;
            var shouldPresent = anchor != null;
            if (shouldPresent)
            {
                var position = anchor.TransformPoint(_location.LocalPosition);
                SetPosition(position);
            }
            else if (_presented) SetPosition(_exteriorPosition);
            _presented = shouldPresent;
            for (var i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null) _renderers[i].enabled = (!IsInterior || shouldPresent) && _rendererEnabled[i];
            for (var i = 0; i < _colliders.Length; i++)
                if (_colliders[i] != null) _colliders[i].enabled = (!IsInterior || shouldPresent) && _colliderEnabled[i];
        }

        private void LeaveInterior()
        {
            SetPosition(_exteriorPosition);
            _location = default;
            if (_wanderer != null) _wanderer.enabled = _wandererWasEnabled;
            RefreshPresentation();
        }

        private void SetPosition(Vector3 position)
        {
            transform.position = position;
            if (_body != null) _body.position = position;
            Physics2D.SyncTransforms();
        }
    }
}
