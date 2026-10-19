using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Animals
{
    /// <summary>Canonical release/restore adapter for one housing and its shared world-space pen.</summary>
    [DisallowMultipleComponent]
    public class AnimalReleaseHandler : MonoBehaviour, IInteractable
    {
        public enum ReleaseAttemptResult
        {
            Success = 0,
            RegistryUnavailable = 1,
            InventoryUnavailable = 2,
            NoCompatibleAnimal = 3,
            HousingFull = 4,
            ReleaseRejected = 5,
            NoSafeSpawn = 6
        }

        [SerializeField] private string _housingId;
        [SerializeField] private AnimalHousingBuildingType _housingType = AnimalHousingBuildingType.Coop;
        [SerializeField] private int _capacity = 4;
        [SerializeField] private FarmAnimalRegistry _registry;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private Vector2 _penBoundsMin = new Vector2(-1.6f, -1.2f);
        [SerializeField] private Vector2 _penBoundsMax = new Vector2(1.6f, 1.2f);
        [SerializeField] private AnimalMotionProfileSO[] _motionProfiles;

        private readonly Dictionary<string, FarmAnimalRuntime> _runtimes = new Dictionary<string, FarmAnimalRuntime>();
        private readonly Dictionary<string, PendingRestore> _pendingRestores = new Dictionary<string, PendingRestore>();
        private readonly List<string> _pendingRemovalBuffer = new List<string>(4);
        private readonly Collider2D[] _spawnHits = new Collider2D[16];
        private int _spawnFanCount;
        private float _restoreRetryTimer;

        private sealed class PendingRestore
        {
            public InventoryManager Inventory;
            public FarmAnimalRegistry Registry;
        }

        public string HousingId => _housingId;
        public Rect SharedWorldBounds => Rect.MinMaxRect(
            transform.position.x + Mathf.Min(_penBoundsMin.x, _penBoundsMax.x),
            transform.position.y + Mathf.Min(_penBoundsMin.y, _penBoundsMax.y),
            transform.position.x + Mathf.Max(_penBoundsMin.x, _penBoundsMax.x),
            transform.position.y + Mathf.Max(_penBoundsMin.y, _penBoundsMax.y));
        public int RuntimeCount
        {
            get
            {
                RemoveDestroyedRuntimeEntries();
                return _runtimes.Count;
            }
        }
        public int PendingRestoreCount => _pendingRestores.Count;

        private FarmAnimalRegistry Registry => _registry != null ? _registry : FarmAnimalRegistry.Instance;

        private void Start() => RegisterHousingIfReady();

        private void Update() => TickPendingRestores(Time.deltaTime);

        public void Configure(string housingId, AnimalHousingBuildingType housingType, int capacity,
            FarmAnimalRegistry registry, InventoryManager inventoryManager)
        {
            _housingId = housingId;
            _housingType = housingType;
            _capacity = capacity;
            _registry = registry;
            _inventoryManager = inventoryManager;
            RegisterHousingIfReady();
        }

        public void SetPenBounds(Vector2 relativeMin, Vector2 relativeMax)
        {
            _penBoundsMin = Vector2.Min(relativeMin, relativeMax);
            _penBoundsMax = Vector2.Max(relativeMin, relativeMax);
        }

        public void SetMotionProfiles(params AnimalMotionProfileSO[] profiles)
        {
            _motionProfiles = profiles;
        }

        public bool TryGetRuntime(string instanceId, out FarmAnimalRuntime runtime)
        {
            if (!string.IsNullOrWhiteSpace(instanceId) && _runtimes.TryGetValue(instanceId, out runtime))
            {
                if (runtime != null) return true;
                _runtimes.Remove(instanceId);
            }
            runtime = null;
            return false;
        }

        public string InteractionPrompt => "Soltar animal no abrigo";
        public bool CanInteract(GameObject interactor) => Registry != null && !string.IsNullOrWhiteSpace(_housingId);

        public void Interact(GameObject interactor)
        {
            TryReleaseFromInventory(interactor, out _);
        }

        public ReleaseAttemptResult TryReleaseFromInventory(GameObject interactor, out string instanceId)
        {
            instanceId = null;
            var registry = Registry;
            if (registry == null) return ReleaseAttemptResult.RegistryUnavailable;
            RegisterHousingIfReady();

            var inventory = ResolveInventory(interactor);
            if (inventory == null) return ReleaseAttemptResult.InventoryUnavailable;

            foreach (var entry in FarmAnimalCatalog.GetAll())
            {
                if (entry.HousingType != _housingType || !inventory.HasItem(entry.PurchaseItemId, 1)) continue;
                var housing = registry.GetHousing(_housingId);
                if (housing != null && !housing.CanAddAnimal())
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Abrigo cheio.", 2f));
                    return ReleaseAttemptResult.HousingFull;
                }

                AnimalMotionProfileSO profile = ResolveProfile(entry.AnimalId);
                if (!TryFindSafeSpawn(profile, out Vector2 spawnPoint))
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Sem espaco seguro no abrigo.", 2f));
                    return ReleaseAttemptResult.NoSafeSpawn;
                }

                var result = registry.TryRelease(entry.AnimalId, _housingId, out instanceId);
                if (result == FarmAnimalRegistry.ReleaseResult.Success)
                {
                    inventory.RemoveItem(entry.PurchaseItemId, 1);
                    SpawnAnimalRuntime(entry, instanceId, inventory, registry, profile, spawnPoint);
                    GameEventBus.Publish(new PlayerActionFeedbackEvent($"{entry.DisplayName} solta no abrigo!", 2f));
                    return ReleaseAttemptResult.Success;
                }
                if (result == FarmAnimalRegistry.ReleaseResult.HousingFull)
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Abrigo cheio.", 2f));
                    return ReleaseAttemptResult.HousingFull;
                }
                return ReleaseAttemptResult.ReleaseRejected;
            }

            GameEventBus.Publish(new PlayerActionFeedbackEvent("Nenhum filhote compativel no inventario.", 2f));
            return ReleaseAttemptResult.NoCompatibleAnimal;
        }

        /// <summary>Rehydrates this housing only and creates at most one runtime per stable instance ID.</summary>
        public void RespawnExistingAnimals(IReadOnlyList<AnimalInstanceState> animals, InventoryManager inventory,
            FarmAnimalRegistry registry)
        {
            if (animals == null) return;
            foreach (var state in animals)
            {
                if (state == null) continue;
                if (state.HealthState == AnimalHealthState.Dead || state.HomeBuildingId != _housingId)
                {
                    _pendingRestores.Remove(state.AnimalInstanceId);
                    continue;
                }
                if (TryGetRuntime(state.AnimalInstanceId, out _))
                {
                    _pendingRestores.Remove(state.AnimalInstanceId);
                    continue;
                }

                if (!TryFindCatalogEntry(state.AnimalDataId, out var entry)) continue;
                AnimalMotionProfileSO profile = ResolveProfile(state.AnimalDataId);
                if (!TryFindSafeSpawn(profile, out Vector2 spawnPoint))
                {
                    _pendingRestores[state.AnimalInstanceId] = new PendingRestore
                    {
                        Inventory = inventory,
                        Registry = registry != null ? registry : Registry
                    };
                    continue;
                }
                SpawnAnimalRuntime(entry, state.AnimalInstanceId, inventory, registry, profile, spawnPoint);
                _pendingRestores.Remove(state.AnimalInstanceId);
            }
        }

        /// <summary>Deterministic lifecycle seam used by Update and the isolated motion probe.</summary>
        public void TickPendingRestores(float deltaTime)
        {
            if (_pendingRestores.Count == 0) return;
            _restoreRetryTimer -= Mathf.Max(0f, deltaTime);
            if (_restoreRetryTimer > 0f) return;
            _restoreRetryTimer = 0.75f;

            _pendingRemovalBuffer.Clear();
            foreach (var pair in _pendingRestores)
            {
                string instanceId = pair.Key;
                var pending = pair.Value;
                var registry = pending.Registry != null ? pending.Registry : Registry;
                var state = registry != null ? registry.GetAnimal(instanceId) : null;
                if (state == null || state.HealthState == AnimalHealthState.Dead || state.HomeBuildingId != _housingId)
                {
                    _pendingRemovalBuffer.Add(instanceId);
                    continue;
                }
                if (TryGetRuntime(instanceId, out _))
                {
                    _pendingRemovalBuffer.Add(instanceId);
                    continue;
                }
                if (!TryFindCatalogEntry(state.AnimalDataId, out var entry))
                {
                    _pendingRemovalBuffer.Add(instanceId);
                    continue;
                }

                AnimalMotionProfileSO profile = ResolveProfile(state.AnimalDataId);
                if (!TryFindSafeSpawn(profile, out Vector2 spawnPoint)) continue;
                SpawnAnimalRuntime(entry, instanceId, pending.Inventory, registry, profile, spawnPoint);
                _pendingRemovalBuffer.Add(instanceId);
            }

            for (int i = 0; i < _pendingRemovalBuffer.Count; i++)
                _pendingRestores.Remove(_pendingRemovalBuffer[i]);
        }

        private FarmAnimalRuntime SpawnAnimalRuntime(FarmAnimalCatalog.AnimalCatalogEntry entry, string instanceId,
            InventoryManager inventory, FarmAnimalRegistry registry, AnimalMotionProfileSO profile, Vector2 spawnPoint)
        {
            if (TryGetRuntime(instanceId, out var existing)) return existing;

            var go = new GameObject($"FarmAnimal_{instanceId}");
            go.transform.position = new Vector3(spawnPoint.x, spawnPoint.y, transform.position.z);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = profile != null ? Color.white : ColorForSpecies(entry.Species);
            sr.sortingOrder = 0;
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            sr.sortingLayerName = CindarsHope.World.WorldSortingLayers.World;

            var interaction = go.AddComponent<BoxCollider2D>();
            interaction.isTrigger = true;
            interaction.size = profile != null ? profile.BodySize : new Vector2(0.55f, 0.42f);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;

            var presenter = go.AddComponent<AnimalSpriteAnimator>();
            presenter.Configure(sr, profile);
            var animal = go.AddComponent<FarmAnimalRuntime>();
            animal.Configure(instanceId, entry.AnimalId, entry.FeedItemId, entry.DisplayName, registry);
            animal.SetInventoryManager(inventory);
            animal.ConfigureMotion(profile, SharedWorldBounds);
            _runtimes[instanceId] = animal;
            return animal;
        }

        private bool TryFindSafeSpawn(AnimalMotionProfileSO profile, out Vector2 spawnPoint)
        {
            Rect outer = SharedWorldBounds;
            Vector2 body = profile != null ? profile.BodySize : new Vector2(0.55f, 0.42f);
            float skin = profile != null ? profile.BoundsSkin : 0.03f;
            spawnPoint = outer.center;
            if (!AnimalMotionBounds.CanContain(outer, body, skin)) return false;

            Rect centers = AnimalMotionBounds.Contract(outer, body, skin);
            for (int attempt = 0; attempt < 12; attempt++)
            {
                int slot = _spawnFanCount + attempt;
                float x01 = ((slot * 5) % 11) / 10f;
                float y01 = ((slot * 7) % 9) / 8f;
                Vector2 candidate = new Vector2(Mathf.Lerp(centers.xMin, centers.xMax, x01),
                    Mathf.Lerp(centers.yMin, centers.yMax, y01));
                if (!IsBodyBlockedAt(candidate, body, profile != null ? profile.ObstacleMask.value : ~0))
                {
                    _spawnFanCount = slot + 1;
                    spawnPoint = candidate;
                    return true;
                }
            }
            return false;
        }

        private bool IsBodyBlockedAt(Vector2 center, Vector2 bodySize, int layerMask)
        {
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = layerMask,
                useTriggers = true
            };
            int count = Physics2D.OverlapBox(center, bodySize, 0f, filter, _spawnHits);
            if (count >= _spawnHits.Length) return true;
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _spawnHits[i];
                if (hit == null || hit.gameObject == gameObject) continue;
                if (hit.CompareTag("Player") || hit.GetComponentInParent<CindarsHope.Player.PlayerController>() != null) continue;
                if (hit.GetComponentInParent<FarmAnimalRuntime>() != null || !hit.isTrigger) return true;
            }
            return false;
        }

        private AnimalMotionProfileSO ResolveProfile(string animalDataId)
        {
            if (_motionProfiles == null) return null;
            for (int i = 0; i < _motionProfiles.Length; i++)
            {
                var profile = _motionProfiles[i];
                if (profile != null && profile.AnimalDataId == animalDataId) return profile;
            }
            return null;
        }

        private static bool TryFindCatalogEntry(string animalDataId, out FarmAnimalCatalog.AnimalCatalogEntry match)
        {
            foreach (var entry in FarmAnimalCatalog.GetAll())
            {
                if (entry.AnimalId == animalDataId)
                {
                    match = entry;
                    return true;
                }
            }
            match = default;
            return false;
        }

        private static Color ColorForSpecies(FarmAnimalSpecies species)
        {
            switch (species)
            {
                case FarmAnimalSpecies.Chicken: return new Color(0.96f, 0.92f, 0.7f);
                case FarmAnimalSpecies.Sheep: return new Color(0.85f, 0.82f, 0.78f);
                case FarmAnimalSpecies.Cow: return new Color(0.7f, 0.55f, 0.4f);
                default: return Color.white;
            }
        }

        private InventoryManager ResolveInventory(GameObject interactor)
        {
            if (_inventoryManager != null) return _inventoryManager;
            return interactor != null ? interactor.GetComponentInParent<InventoryManager>() : null;
        }

        private void RegisterHousingIfReady()
        {
            var registry = Registry;
            if (registry != null && !string.IsNullOrWhiteSpace(_housingId))
                registry.RegisterHousing(_housingId, _housingType, _capacity);
        }

        private void RemoveDestroyedRuntimeEntries()
        {
            if (_runtimes.Count == 0) return;
            var stale = new List<string>();
            foreach (var pair in _runtimes)
                if (pair.Value == null) stale.Add(pair.Key);
            for (int i = 0; i < stale.Count; i++) _runtimes.Remove(stale[i]);
        }
    }
}
