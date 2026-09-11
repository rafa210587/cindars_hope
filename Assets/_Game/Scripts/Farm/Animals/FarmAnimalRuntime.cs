using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Animals
{
    /// <summary>Unity adapter for safe cosmetic motion; care and save remain in the registry.</summary>
    [DisallowMultipleComponent]
    public class FarmAnimalRuntime : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _animalInstanceId;
        [SerializeField] private string _animalDataId;
        [SerializeField] private string _feedItemId = FarmAnimalCatalog.ItemFeed;
        [SerializeField] private string _displayName = "Animal";
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _interactionCollider;
        [SerializeField] private FarmAnimalRegistry _registry;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private AnimalMotionProfileSO _motionProfile;
        [SerializeField] private AnimalSpriteAnimator _spriteAnimator;

        // Preserved serialized fallback for animal IDs without a bespoke presentation profile.
        [SerializeField] private Vector2 _wanderBoundsMin = new Vector2(-1.2f, -1.2f);
        [SerializeField] private Vector2 _wanderBoundsMax = new Vector2(1.2f, 1.2f);
        [SerializeField] private float _wanderSpeed = 0.7f;
        [SerializeField] private float _wanderRadius = 1f;
        [SerializeField] private float _pauseMin = 0.8f;
        [SerializeField] private float _pauseMax = 2.5f;

        private readonly RaycastHit2D[] _sweepHits = new RaycastHit2D[16];
        private AnimalMotionState _motionState;
        private Rect _motionBoundsWorld;
        private Vector2 _bodySize = new Vector2(0.55f, 0.42f);
        private float _boundsSkin = 0.03f;
        private int _obstacleMask = ~0;
        private Vector2 _lastObservedPosition;
        private AnimalHealthState _currentHealthState = AnimalHealthState.Healthy;

        public string AnimalInstanceId => _animalInstanceId;
        public string AnimalDataId => _animalDataId;
        public AnimalMotionMode MotionMode => _motionState != null ? _motionState.Mode : AnimalMotionMode.Idle;
        public AnimalFacingDirection FacingDirection => _motionState != null ? _motionState.FacingDirection : AnimalFacingDirection.Down;
        public bool IsMotionPaused => _motionState == null || _motionState.IsPaused;
        public bool IsInteractionFrozen => _motionState != null && _motionState.IsFrozen;
        public Rect MotionBoundsWorld => _motionBoundsWorld;
        public Rect AllowedCenterBounds => AnimalMotionBounds.Contract(_motionBoundsWorld, _bodySize, _boundsSkin);
        public Vector2 BodySize => _bodySize;
        public AnimalHealthState CurrentHealthState => _currentHealthState;
        public AnimalMotionProfileSO MotionProfile => _motionProfile;
        public AnimalSpriteAnimator SpriteAnimator => _spriteAnimator;

        private FarmAnimalRegistry Registry => _registry != null ? _registry : FarmAnimalRegistry.Instance;

        private void Start()
        {
            if (_spriteAnimator == null) _spriteAnimator = GetComponent<AnimalSpriteAnimator>();
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_interactionCollider == null) _interactionCollider = GetComponent<Collider2D>();
            if (_motionBoundsWorld.width <= 0f || _motionBoundsWorld.height <= 0f)
            {
                Vector2 anchor = transform.position;
                _motionBoundsWorld = Rect.MinMaxRect(anchor.x + _wanderBoundsMin.x, anchor.y + _wanderBoundsMin.y,
                    anchor.x + _wanderBoundsMax.x, anchor.y + _wanderBoundsMax.y);
            }

            BuildMotionState();
            _lastObservedPosition = transform.position;
            var registry = Registry;
            if (registry != null && !string.IsNullOrWhiteSpace(_animalInstanceId))
            {
                registry.RegisterRuntime(_animalInstanceId, this);
                var state = registry.GetAnimal(_animalInstanceId);
                if (state != null) OnHealthStateChanged(state.HealthState);
            }
        }

        private void OnDestroy()
        {
            var registry = Registry;
            if (registry != null && !string.IsNullOrWhiteSpace(_animalInstanceId))
                registry.UnregisterRuntime(_animalInstanceId);
        }

        public void Configure(string animalInstanceId, string animalDataId, string feedItemId, string displayName, FarmAnimalRegistry registry)
        {
            _animalInstanceId = animalInstanceId;
            _animalDataId = animalDataId;
            _feedItemId = feedItemId;
            _displayName = displayName;
            _registry = registry;
        }

        public void ConfigureMotion(AnimalMotionProfileSO profile, Rect sharedWorldBounds)
        {
            if (_spriteAnimator == null) _spriteAnimator = GetComponent<AnimalSpriteAnimator>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            _motionProfile = profile;
            _motionBoundsWorld = NormalizeBounds(sharedWorldBounds);
            if (profile != null)
            {
                _bodySize = profile.BodySize;
                _boundsSkin = profile.BoundsSkin;
                _obstacleMask = profile.ObstacleMask.value;
            }
            BuildMotionState();
            if (_spriteAnimator != null) _spriteAnimator.Configure(_spriteRenderer, profile);
        }

        /// <summary>Compatibility hook: older callers supply offsets around this runtime.</summary>
        public void SetWanderBounds(Vector2 min, Vector2 max)
        {
            _wanderBoundsMin = Vector2.Min(min, max);
            _wanderBoundsMax = Vector2.Max(min, max);
            Vector2 anchor = transform.position;
            _motionBoundsWorld = Rect.MinMaxRect(anchor.x + _wanderBoundsMin.x, anchor.y + _wanderBoundsMin.y,
                anchor.x + _wanderBoundsMax.x, anchor.y + _wanderBoundsMax.y);
        }

        private void FixedUpdate()
        {
            Vector2 current = _rigidbody != null ? _rigidbody.position : (Vector2)transform.position;
            Vector2 observedDelta = current - _lastObservedPosition;
            _lastObservedPosition = current;

            var state = Registry?.GetAnimal(_animalInstanceId);
            if (state != null && state.HealthState != _currentHealthState) OnHealthStateChanged(state.HealthState);
            bool unavailable = state == null ||
                state.HealthState == AnimalHealthState.Dead || state.HealthState == AnimalHealthState.Unavailable ||
                !AnimalMotionBounds.CanContain(_motionBoundsWorld, _bodySize, _boundsSkin);

            EnsureMotionState();
            _motionState.SetHealthStopped(unavailable);
            Vector2 displacement = _motionState.Advance(Time.fixedDeltaTime, current, AllowedCenterBounds, CanSweepBody);
            if (displacement.sqrMagnitude > 0f)
            {
                Vector2 next = current + displacement;
                if (_rigidbody != null) _rigidbody.MovePosition(next);
                else transform.position = new Vector3(next.x, next.y, transform.position.z);
            }
            else StopMotion();

            if (_spriteAnimator != null)
            {
                var visualMode = _motionState.IsFrozen ? AnimalMotionMode.Idle : _motionState.Mode;
                _spriteAnimator.UpdatePresentation(Time.fixedDeltaTime, visualMode, _motionState.FacingDirection,
                    observedDelta.magnitude, displacement.x);
            }
        }

        private bool CanSweepBody(Vector2 origin, Vector2 displacement)
        {
            float distance = displacement.magnitude;
            if (distance <= 0.00001f) return true;
            var filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(_obstacleMask);
            int count = Physics2D.BoxCast(origin, _bodySize, 0f, displacement / distance,
                filter, _sweepHits, distance + _boundsSkin);
            if (count == _sweepHits.Length) return false; // Incomplete query must never open a path.
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _sweepHits[i].collider;
                if (hit == null || hit == _interactionCollider || hit.transform.IsChildOf(transform)) continue;
                if (hit.GetComponentInParent<CindarsHope.Player.PlayerController>() != null) continue;

                var otherAnimal = hit.GetComponentInParent<FarmAnimalRuntime>();
                if (otherAnimal != null)
                {
                    if (otherAnimal != this) return false;
                    continue;
                }
                if (!hit.isTrigger) return false;
            }
            return true;
        }

        private void BuildMotionState()
        {
            AnimalMotionSettings settings = _motionProfile != null
                ? _motionProfile.CreateSettings()
                : new AnimalMotionSettings(_wanderSpeed, _wanderRadius, _pauseMin, _pauseMax,
                    1.1f, 1.8f, 0.58f, 0.27f, 6, 0.04f);
            _motionState = new AnimalMotionState(settings, _animalInstanceId, _animalDataId);
            _motionState.SetHealthStopped(_currentHealthState == AnimalHealthState.Dead ||
                _currentHealthState == AnimalHealthState.Unavailable);
        }

        private void EnsureMotionState()
        {
            if (_motionState == null) BuildMotionState();
        }

        private static Rect NormalizeBounds(Rect bounds)
        {
            return Rect.MinMaxRect(Mathf.Min(bounds.xMin, bounds.xMax), Mathf.Min(bounds.yMin, bounds.yMax),
                Mathf.Max(bounds.xMin, bounds.xMax), Mathf.Max(bounds.yMin, bounds.yMax));
        }

        private void StopMotion()
        {
            if (_rigidbody != null) _rigidbody.linearVelocity = Vector2.zero;
        }

        public void OnHealthStateChanged(AnimalHealthState health)
        {
            _currentHealthState = health;
            if (_motionState != null)
                _motionState.SetHealthStopped(health == AnimalHealthState.Dead || health == AnimalHealthState.Unavailable);
            if (_spriteRenderer == null) return;

            switch (health)
            {
                case AnimalHealthState.Dead:
                    _spriteRenderer.color = new Color(0.35f, 0.35f, 0.35f, 0.6f); break;
                case AnimalHealthState.Unavailable:
                    _spriteRenderer.color = new Color(0.7f, 0.45f, 0.45f, 1f); break;
                case AnimalHealthState.Hungry:
                    _spriteRenderer.color = new Color(0.85f, 0.78f, 0.55f, 1f); break;
                default:
                    _spriteRenderer.color = Color.white; break;
            }
        }

        public string InteractionPrompt
        {
            get
            {
                var state = Registry?.GetAnimal(_animalInstanceId);
                if (state == null) return _displayName;
                if (state.HealthState == AnimalHealthState.Dead) return $"{_displayName} (morto)";
                if (state.ProductReady) return $"Coletar produto ({_displayName})";
                if (!state.FedToday) return $"Alimentar {_displayName}";
                return $"{_displayName} (alimentado)";
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            var state = Registry?.GetAnimal(_animalInstanceId);
            return state != null && state.HealthState != AnimalHealthState.Dead;
        }

        public void Interact(GameObject interactor)
        {
            var registry = Registry;
            if (registry == null) return;
            var state = registry.GetAnimal(_animalInstanceId);
            if (state == null || state.HealthState == AnimalHealthState.Dead) return;

            EnsureMotionState();
            _motionState.Freeze(_motionProfile != null ? _motionProfile.InteractionFreezeSeconds : 0.45f);
            StopMotion();

            if (state.ProductReady)
            {
                var collect = registry.Collect(_animalInstanceId, out var productItemId, out var quantity);
                if (collect == FarmAnimalRegistry.CollectResult.Success)
                {
                    var inventory = ResolveInventory(interactor);
                    if (inventory != null && !string.IsNullOrWhiteSpace(productItemId)) inventory.AddItem(productItemId, quantity);
                    GameEventBus.Publish(new PlayerActionFeedbackEvent($"Coletou {productItemId} x{quantity}.", 2f));
                }
                return;
            }

            if (!state.FedToday)
            {
                var inventory = ResolveInventory(interactor);
                if (inventory != null && !inventory.HasItem(_feedItemId, 1))
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Sem racao para alimentar.", 2f));
                    return;
                }
                var feed = registry.Feed(_animalInstanceId);
                if (feed == FarmAnimalRegistry.FeedResult.Success)
                {
                    if (inventory != null) inventory.RemoveItem(_feedItemId, 1);
                    OnHealthStateChanged(state.HealthState);
                    GameEventBus.Publish(new PlayerActionFeedbackEvent($"Alimentou {_displayName}.", 2f));
                }
            }
        }

        private InventoryManager ResolveInventory(GameObject interactor)
        {
            if (_inventoryManager != null) return _inventoryManager;
            return interactor != null ? interactor.GetComponentInParent<InventoryManager>() : null;
        }

        public void SetInventoryManager(InventoryManager inventoryManager) => _inventoryManager = inventoryManager;
    }
}
