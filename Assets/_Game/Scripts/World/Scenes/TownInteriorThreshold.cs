using System;
using System.Collections.Generic;
using CindarsHope.Foundation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.World.Scenes
{
    /// <summary>Body-only directed crossing sensor. Interaction range alone never starts a visit.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public sealed class TownInteriorThreshold : MonoBehaviour
    {
        [SerializeField] private string _houseId, _entryAnchorId, _returnAnchorId;
        [SerializeField] private HouseDoorInteractable _door;
        [SerializeField] private bool _exiting;
        [SerializeField] private Vector2 _crossingNormal = Vector2.up;

        private readonly HashSet<Collider2D> _bodyColliders = new HashSet<Collider2D>();
        private Rigidbody2D _trackedBody;
        private float _previousSide;
        private bool _fired;

        public void Configure(string houseId, string entryAnchorId, string returnAnchorId,
            HouseDoorInteractable door, bool exiting, Vector2 crossingNormal)
        {
            _houseId = houseId; _entryAnchorId = entryAnchorId; _returnAnchorId = returnAnchorId;
            _door = door; _exiting = exiting; _crossingNormal = crossingNormal.normalized;
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnDisable() { _bodyColliders.Clear(); _trackedBody = null; _fired = false; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!Eligible(other) || (_trackedBody != null && other.attachedRigidbody != _trackedBody)) return;
            if (_trackedBody == null)
            {
                _trackedBody = other.attachedRigidbody;
                _previousSide = Side(_trackedBody);
                _fired = false;
            }
            _bodyColliders.Add(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_bodyColliders.Contains(other) || _fired) return;
            float side = Side(_trackedBody);
            if (_previousSide < 0f && side >= 0f && CanCross())
            {
                var request = new TownInteriorVisitRequest(Guid.NewGuid().ToString("N"), _houseId,
                    _entryAnchorId, _returnAnchorId, _exiting);
                // Router performs the one admission/Single guard check before publishing.
                var result = SceneTransitionRouter.Execute(new SceneTransitionRequest(
                    SceneManager.GetActiveScene().name, request, _houseId));
                _fired = result.Succeeded;
            }
            _previousSide = side;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!_bodyColliders.Remove(other)) return;
            if (_bodyColliders.Count != 0) return;
            _trackedBody = null;
            _fired = false; // Rearm only after the body has left the volume.
        }

        private bool CanCross()
        {
            if (_exiting) return true; // A closed shop cannot prevent exit.
            return _door != null && _door.IsOpen && !_door.IsAnimating &&
                (_door.PhysicalBlocker == null || !_door.PhysicalBlocker.enabled);
        }

        private bool Eligible(Collider2D other) => other != null && !other.isTrigger &&
            other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player") &&
            !string.IsNullOrWhiteSpace(_houseId) && !string.IsNullOrWhiteSpace(_entryAnchorId) &&
            !string.IsNullOrWhiteSpace(_returnAnchorId) && _crossingNormal.sqrMagnitude > 0.99f;

        private float Side(Rigidbody2D body) =>
            Vector2.Dot(body.position - (Vector2)transform.position, _crossingNormal);
    }
}
