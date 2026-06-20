using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Animals
{
    /// <summary>
    /// fable_12 — animal de fazenda em cena. Anda em bounds estritos do abrigo (clamp por frame —
    /// mitiga o risco de "escapar do cercado") e é <see cref="IInteractable"/>: alimenta (consome
    /// ração do inventário) quando não alimentado, ou coleta o produto (AddItem) quando pronto.
    ///
    /// Sem GameObject.Find/FindObjectOfType: o registry e o InventoryManager chegam por referência
    /// serializada (wired no gerador de cena) com fallback ao singleton do registry. Comunicação de
    /// gameplay via GameEventBus (feedback). Lógica de produto/saúde mora no
    /// <see cref="FarmAnimalRegistry"/> — este componente só projeta visual e roteia interação.
    /// </summary>
    [DisallowMultipleComponent]
    public class FarmAnimalRuntime : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _animalInstanceId;
        [SerializeField] private string _animalDataId;
        [SerializeField] private string _feedItemId = FarmAnimalCatalog.ItemFeed;
        [SerializeField] private string _displayName = "Animal";

        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private FarmAnimalRegistry _registry;
        [SerializeField] private InventoryManager _inventoryManager;

        [SerializeField] private Vector2 _wanderBoundsMin = new Vector2(-1.2f, -1.2f);
        [SerializeField] private Vector2 _wanderBoundsMax = new Vector2(1.2f, 1.2f);
        [SerializeField] private float _wanderSpeed = 0.7f;
        [SerializeField] private float _wanderRadius = 1.0f;
        [SerializeField] private float _pauseMin = 0.8f;
        [SerializeField] private float _pauseMax = 2.5f;

        private Vector2 _anchor;
        private Vector3 _target;
        private float _pauseTimer;
        private bool _isPaused = true;

        public string AnimalInstanceId => _animalInstanceId;

        private FarmAnimalRegistry Registry => _registry != null ? _registry : FarmAnimalRegistry.Instance;

        private void Start()
        {
            _anchor = transform.position;
            _target = transform.position;
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            var registry = Registry;
            if (registry != null && !string.IsNullOrWhiteSpace(_animalInstanceId))
            {
                registry.RegisterRuntime(_animalInstanceId, this);
                var state = registry.GetAnimal(_animalInstanceId);
                if (state != null)
                {
                    OnHealthStateChanged(state.HealthState);
                }
            }

            StartPause();
        }

        private void OnDestroy()
        {
            var registry = Registry;
            if (registry != null && !string.IsNullOrWhiteSpace(_animalInstanceId))
            {
                registry.UnregisterRuntime(_animalInstanceId);
            }
        }

        public void Configure(string animalInstanceId, string animalDataId, string feedItemId, string displayName, FarmAnimalRegistry registry)
        {
            _animalInstanceId = animalInstanceId;
            _animalDataId = animalDataId;
            _feedItemId = feedItemId;
            _displayName = displayName;
            _registry = registry;
        }

        public void SetWanderBounds(Vector2 min, Vector2 max)
        {
            _wanderBoundsMin = min;
            _wanderBoundsMax = max;
        }

        private void FixedUpdate()
        {
            var state = Registry?.GetAnimal(_animalInstanceId);
            if (state != null && (state.HealthState == AnimalHealthState.Dead
                || state.HealthState == AnimalHealthState.Unavailable))
            {
                StopMotion();
                return;
            }

            if (_isPaused)
            {
                _pauseTimer -= Time.fixedDeltaTime;
                if (_pauseTimer <= 0f)
                {
                    ChooseNewTarget();
                }
                return;
            }

            MoveTowardTarget();
        }

        private void MoveTowardTarget()
        {
            float distance = Vector2.Distance(transform.position, _target);
            if (distance < 0.05f)
            {
                StartPause();
                return;
            }

            Vector2 direction = ((Vector2)(_target - transform.position)).normalized;
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = direction * _wanderSpeed;
            }
            else
            {
                transform.position += (Vector3)(direction * _wanderSpeed * Time.fixedDeltaTime);
            }

            ClampInsideBounds();
        }

        private void ClampInsideBounds()
        {
            // Clamp por frame — garantia dura de que o animal nunca sai do cercado.
            var pos = transform.position;
            float minX = _anchor.x + _wanderBoundsMin.x;
            float maxX = _anchor.x + _wanderBoundsMax.x;
            float minY = _anchor.y + _wanderBoundsMin.y;
            float maxY = _anchor.y + _wanderBoundsMax.y;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }

        private void ChooseNewTarget()
        {
            _isPaused = false;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(0f, _wanderRadius);
            Vector2 offset = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
            Vector2 intended = _anchor + offset;
            _target = new Vector3(
                Mathf.Clamp(intended.x, _anchor.x + _wanderBoundsMin.x, _anchor.x + _wanderBoundsMax.x),
                Mathf.Clamp(intended.y, _anchor.y + _wanderBoundsMin.y, _anchor.y + _wanderBoundsMax.y),
                transform.position.z);
        }

        private void StartPause()
        {
            _isPaused = true;
            _pauseTimer = Random.Range(_pauseMin, _pauseMax);
            StopMotion();
        }

        private void StopMotion()
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        public void OnHealthStateChanged(AnimalHealthState health)
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            switch (health)
            {
                case AnimalHealthState.Dead:
                    _spriteRenderer.color = new Color(0.35f, 0.35f, 0.35f, 0.6f);
                    break;
                case AnimalHealthState.Unavailable:
                    _spriteRenderer.color = new Color(0.7f, 0.45f, 0.45f, 1f);
                    break;
                case AnimalHealthState.Hungry:
                    _spriteRenderer.color = new Color(0.85f, 0.78f, 0.55f, 1f);
                    break;
                default:
                    _spriteRenderer.color = Color.white;
                    break;
            }
        }

        // ───────────────────────────── IInteractable ─────────────────────────────

        public string InteractionPrompt
        {
            get
            {
                var state = Registry?.GetAnimal(_animalInstanceId);
                if (state == null)
                {
                    return _displayName;
                }
                if (state.HealthState == AnimalHealthState.Dead)
                {
                    return $"{_displayName} (morto)";
                }
                if (state.ProductReady)
                {
                    return $"Coletar produto ({_displayName})";
                }
                if (!state.FedToday)
                {
                    return $"Alimentar {_displayName}";
                }
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
            if (registry == null)
            {
                return;
            }

            var state = registry.GetAnimal(_animalInstanceId);
            if (state == null || state.HealthState == AnimalHealthState.Dead)
            {
                return;
            }

            // Prioridade 1: coletar produto pronto.
            if (state.ProductReady)
            {
                var collect = registry.Collect(_animalInstanceId, out var productItemId, out var quantity);
                if (collect == FarmAnimalRegistry.CollectResult.Success)
                {
                    var inventory = ResolveInventory(interactor);
                    if (inventory != null && !string.IsNullOrWhiteSpace(productItemId))
                    {
                        inventory.AddItem(productItemId, quantity);
                    }
                    GameEventBus.Publish(new PlayerActionFeedbackEvent(
                        $"Coletou {productItemId} x{quantity}.", 2f));
                }
                return;
            }

            // Prioridade 2: alimentar (consome ração do inventário).
            if (!state.FedToday)
            {
                var inventory = ResolveInventory(interactor);
                if (inventory != null && !inventory.HasItem(_feedItemId, 1))
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent(
                        "Sem racao para alimentar.", 2f));
                    return;
                }

                var feed = registry.Feed(_animalInstanceId);
                if (feed == FarmAnimalRegistry.FeedResult.Success)
                {
                    if (inventory != null)
                    {
                        inventory.RemoveItem(_feedItemId, 1);
                    }
                    OnHealthStateChanged(state.HealthState);
                    GameEventBus.Publish(new PlayerActionFeedbackEvent(
                        $"Alimentou {_displayName}.", 2f));
                }
            }
        }

        private InventoryManager ResolveInventory(GameObject interactor)
        {
            if (_inventoryManager != null)
            {
                return _inventoryManager;
            }
            if (interactor != null)
            {
                return interactor.GetComponentInParent<InventoryManager>();
            }
            return null;
        }

        public void SetInventoryManager(InventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
        }
    }
}
