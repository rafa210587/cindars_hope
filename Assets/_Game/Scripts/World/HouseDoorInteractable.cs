using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.World
{
    /// <summary>Physical walk-in door. Optional source frames animate the existing leaf and blocker.</summary>
    [DisallowMultipleComponent]
    public sealed class HouseDoorInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _leaf;
        [SerializeField] private SpriteRenderer _leafRenderer;
        [SerializeField] private Collider2D _blocker;
        [SerializeField] private Vector3 _closedLocalPos;
        [SerializeField] private Vector3 _openLocalPos;
        [SerializeField] private bool _isOpen;
        [SerializeField] private Sprite[] _closedToOpenFrames;
        [SerializeField] private float _secondsPerFrame = 0.08f;

        private readonly Collider2D[] _occupancyResults = new Collider2D[32];
        private int _npcNearby;
        private float _autoCloseTimer;
        private bool _autoOpened;
        private const float AutoCloseDelaySeconds = 1.2f;
        private int _frameIndex;
        private float _frameElapsed;
        private bool _targetOpen;
        private bool _animating;

        private bool HasPresentation => _leafRenderer != null &&
            _closedToOpenFrames != null && _closedToOpenFrames.Length >= 2;
        public bool IsOpen => _isOpen;
        public bool IsAnimating => _animating;
        public Collider2D PhysicalBlocker => _blocker;
        public string InteractionPrompt => (_animating ? _targetOpen : _isOpen) ? "Fechar porta" : "Abrir porta";
        public bool CanInteract(GameObject interactor) => interactor != null;

        private void OnEnable() => ApplyImmediate(_isOpen);
        private void OnDisable()
        {
            _npcNearby = 0;
            _autoCloseTimer = 0f;
            _autoOpened = false;
            ApplyImmediate(_isOpen);
        }

        public void Interact(GameObject interactor)
        {
            if (interactor == null) return;
            var open = !(_animating ? _targetOpen : _isOpen);
            if (!SetOpen(open)) return;
            // A manual choice supersedes the NPC's pending automatic close.
            _autoOpened = false;
            _autoCloseTimer = 0f;
            GameEventBus.Publish(new PlayerActionFeedbackEvent(open ? "Porta aberta" : "Porta fechada"));
        }

        private bool SetOpen(bool open)
        {
            if (!HasPresentation)
            {
                ApplyImmediate(open);
                PublishTransitionCompleted(open);
                return true;
            }
            if (!open && IsDoorwayOccupied()) return false;
            _targetOpen = open;
            _frameElapsed = 0f;
            _animating = _frameIndex != (open ? _closedToOpenFrames.Length - 1 : 0);
            if (!_animating)
            {
                ApplyImmediate(open);
                PublishTransitionCompleted(open);
            }
            return true;
        }

        private void ApplyImmediate(bool open)
        {
            _isOpen = open;
            _targetOpen = open;
            _animating = false;
            _frameElapsed = 0f;
            if (_blocker != null) _blocker.enabled = !open;
            if (_leaf != null) _leaf.localPosition = HasPresentation ? _closedLocalPos
                : open ? _openLocalPos : _closedLocalPos;
            if (_leafRenderer == null) return;
            if (HasPresentation)
            {
                _frameIndex = open ? _closedToOpenFrames.Length - 1 : 0;
                _leafRenderer.sprite = _closedToOpenFrames[_frameIndex];
                _leafRenderer.enabled = true;
            }
            else _leafRenderer.enabled = !open;
        }

        // Query authored box geometry, not disabled Collider.bounds (which may be empty).
        // Trigger sensors, the door itself and adjacent static walls never count as occupants.
        private bool IsDoorwayOccupied()
        {
            if (!(_blocker is BoxCollider2D box)) return false;
            var scale = box.transform.lossyScale;
            var size = new Vector2(box.size.x * Mathf.Abs(scale.x), box.size.y * Mathf.Abs(scale.y));
            var center = box.transform.TransformPoint(box.offset);
            var filter = new ContactFilter2D { useTriggers = false };
            var count = Physics2D.OverlapBox(center, size + Vector2.one * 0.04f,
                box.transform.eulerAngles.z, filter, _occupancyResults);
            for (var i = 0; i < count; i++)
            {
                var other = _occupancyResults[i];
                if (other == null || other.isTrigger || other.transform.IsChildOf(transform)) continue;
                var body = other.attachedRigidbody;
                if (other.CompareTag("Player") || (body != null && body.CompareTag("Player")) ||
                    other.GetComponentInParent<INpcDoorTraveler>() != null) return true;
            }
            // A saturated query cannot prove the doorway clear. Keep it open conservatively.
            return count == _occupancyResults.Length;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || other.GetComponentInParent<INpcDoorTraveler>() == null) return;
            _npcNearby++;
            _autoCloseTimer = 0f;
            if (!_isOpen || (_animating && !_targetOpen))
            {
                SetOpen(true);
                _autoOpened = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other == null || other.GetComponentInParent<INpcDoorTraveler>() == null) return;
            _npcNearby = Mathf.Max(0, _npcNearby - 1);
            if (_npcNearby == 0 && _autoOpened) _autoCloseTimer = AutoCloseDelaySeconds;
        }

        private void Update() => Advance(Time.deltaTime);

        private void Advance(float deltaTime)
        {
            if (_autoCloseTimer > 0f)
            {
                _autoCloseTimer -= deltaTime;
                if (_autoCloseTimer <= 0f && _npcNearby == 0)
                {
                    if (!SetOpen(false)) _autoCloseTimer = AutoCloseDelaySeconds;
                    else _autoOpened = false;
                }
            }
            if (!_animating) return;
            // Actors may enter after closing began: reverse before committing a solid leaf.
            if (!_targetOpen && IsDoorwayOccupied()) SetOpen(true);
            if (!_animating) return;
            _frameElapsed += Mathf.Max(0f, deltaTime);
            while (_frameElapsed >= _secondsPerFrame && _animating)
            {
                _frameElapsed -= _secondsPerFrame;
                _frameIndex += _targetOpen ? 1 : -1;
                _leafRenderer.sprite = _closedToOpenFrames[_frameIndex];
                if (_frameIndex == (_targetOpen ? _closedToOpenFrames.Length - 1 : 0))
                {
                    ApplyImmediate(_targetOpen);
                    PublishTransitionCompleted(_targetOpen);
                }
            }
        }

        private static void PublishTransitionCompleted(bool isOpen) =>
            GameEventBus.Publish(new HouseDoorTransitionCompletedEvent(isOpen));

        /// <summary>Legacy configuration: immediate movement/hiding, unchanged without presentation.</summary>
        public void Configure(Transform leaf, Collider2D blocker, Vector3 closedLocalPos, Vector3 openLocalPos)
        {
            _leaf = leaf;
            _leafRenderer = leaf != null ? leaf.GetComponent<SpriteRenderer>() : null;
            // Re-enter the legacy path explicitly; a prior presentation must not leak across configuration.
            _closedToOpenFrames = null;
            _secondsPerFrame = 0.08f;
            _blocker = blocker;
            _closedLocalPos = closedLocalPos;
            _openLocalPos = openLocalPos;
            ApplyImmediate(false);
        }

        /// <summary>Optional fixed-canvas poses, ordered closed to clear/open. Final pose stays visible.</summary>
        public void ConfigurePresentation(SpriteRenderer renderer, Sprite[] closedToOpenFrames, float secondsPerFrame = 0.08f)
        {
            if (renderer == null || closedToOpenFrames == null || closedToOpenFrames.Length < 2 ||
                secondsPerFrame <= 0f || float.IsNaN(secondsPerFrame) || float.IsInfinity(secondsPerFrame) ||
                !(_blocker is BoxCollider2D))
                throw new System.ArgumentException("Door presentation requires a renderer, box blocker, frames and positive timing.");
            foreach (var frame in closedToOpenFrames)
                if (frame == null) throw new System.ArgumentException("Door presentation frames cannot be null.");
            _leafRenderer = renderer;
            _closedToOpenFrames = closedToOpenFrames;
            _secondsPerFrame = secondsPerFrame;
            ApplyImmediate(_isOpen);
        }
    }
}
