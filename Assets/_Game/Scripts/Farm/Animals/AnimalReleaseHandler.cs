using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Animals
{
    /// <summary>
    /// fable_12 — porta de abrigo interativa. Usar um item filhote (chick/kid/calf) perto do abrigo
    /// solta o animal correspondente: consome o item do inventário e registra o animal no
    /// <see cref="FarmAnimalRegistry"/> (que valida capacidade via AnimalHousingCapacityState).
    ///
    /// Sem prefab e sem GameObject.Find: o runtime do animal é instanciado programaticamente e a
    /// instância recebe registry/inventário por chamada explícita. Comunicação via GameEventBus.
    /// </summary>
    [DisallowMultipleComponent]
    public class AnimalReleaseHandler : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _housingId;
        [SerializeField] private AnimalHousingBuildingType _housingType = AnimalHousingBuildingType.Coop;
        [SerializeField] private int _capacity = 4;
        [SerializeField] private FarmAnimalRegistry _registry;
        [SerializeField] private InventoryManager _inventoryManager;

        [Tooltip("Bounds (relativos ao abrigo) onde os animais soltos vagam.")]
        [SerializeField] private Vector2 _penBoundsMin = new Vector2(-1.6f, -1.2f);
        [SerializeField] private Vector2 _penBoundsMax = new Vector2(1.6f, 1.2f);

        private FarmAnimalRegistry Registry => _registry != null ? _registry : FarmAnimalRegistry.Instance;

        private int _spawnFanCount;

        private void Start()
        {
            var registry = Registry;
            if (registry != null && !string.IsNullOrWhiteSpace(_housingId))
            {
                registry.RegisterHousing(_housingId, _housingType, _capacity);
            }
        }

        public void Configure(string housingId, AnimalHousingBuildingType housingType, int capacity, FarmAnimalRegistry registry, InventoryManager inventoryManager)
        {
            _housingId = housingId;
            _housingType = housingType;
            _capacity = capacity;
            _registry = registry;
            _inventoryManager = inventoryManager;
        }

        public string InteractionPrompt => "Soltar animal no abrigo";

        public bool CanInteract(GameObject interactor)
        {
            return Registry != null && !string.IsNullOrWhiteSpace(_housingId);
        }

        public void Interact(GameObject interactor)
        {
            var registry = Registry;
            if (registry == null)
            {
                return;
            }

            var inventory = ResolveInventory(interactor);
            if (inventory == null)
            {
                return;
            }

            // Encontrar o primeiro filhote compatível com este abrigo presente no inventário.
            foreach (var entry in FarmAnimalCatalog.GetAll())
            {
                if (entry.HousingType != _housingType)
                {
                    continue;
                }
                if (!inventory.HasItem(entry.PurchaseItemId, 1))
                {
                    continue;
                }

                var housing = registry.GetHousing(_housingId);
                if (housing != null && !housing.CanAddAnimal())
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent(
                        "Abrigo cheio.", 2f));
                    return;
                }

                var result = registry.TryRelease(entry.AnimalId, _housingId, out var instanceId);
                if (result == FarmAnimalRegistry.ReleaseResult.Success)
                {
                    inventory.RemoveItem(entry.PurchaseItemId, 1);
                    SpawnAnimalRuntime(entry, instanceId, inventory, registry);
                    GameEventBus.Publish(new PlayerActionFeedbackEvent(
                        $"{entry.DisplayName} solta no abrigo!", 2f));
                }
                else if (result == FarmAnimalRegistry.ReleaseResult.HousingFull)
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Abrigo cheio.", 2f));
                }
                return;
            }

            GameEventBus.Publish(new PlayerActionFeedbackEvent(
                "Nenhum filhote compativel no inventario.", 2f));
        }

        private void SpawnAnimalRuntime(
            FarmAnimalCatalog.AnimalCatalogEntry entry,
            string instanceId,
            InventoryManager inventory,
            FarmAnimalRegistry registry)
        {
            var go = new GameObject($"FarmAnimal_{instanceId}");
            // Espalhar levemente os spawns dentro do cercado.
            float jitterX = ((_spawnFanCount % 4) - 1.5f) * 0.4f;
            float jitterY = (((_spawnFanCount / 4) % 4) - 1.5f) * 0.35f;
            _spawnFanCount++;
            go.transform.position = transform.position + new Vector3(jitterX, jitterY, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = ColorForSpecies(entry.Species);
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one * 0.6f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var animal = go.AddComponent<FarmAnimalRuntime>();
            animal.Configure(instanceId, entry.AnimalId, entry.FeedItemId, entry.DisplayName, registry);
            animal.SetInventoryManager(inventory);
            animal.SetWanderBounds(_penBoundsMin, _penBoundsMax);
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

        /// <summary>
        /// Re-hidrata runtimes de animais já existentes no save após o load (chamado pelo bootstrap de
        /// cena). Cria um FarmAnimalRuntime para cada animal vivo deste abrigo.
        /// </summary>
        public void RespawnExistingAnimals(IReadOnlyList<AnimalInstanceState> animals, InventoryManager inventory, FarmAnimalRegistry registry)
        {
            if (animals == null)
            {
                return;
            }

            foreach (var state in animals)
            {
                if (state == null || state.HealthState == AnimalHealthState.Dead)
                {
                    continue;
                }

                FarmAnimalCatalog.AnimalCatalogEntry match = default;
                bool found = false;
                foreach (var entry in FarmAnimalCatalog.GetAll())
                {
                    if (entry.AnimalId == state.AnimalDataId)
                    {
                        match = entry;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    continue;
                }

                SpawnAnimalRuntime(match, state.AnimalInstanceId, inventory, registry);
            }
        }
    }
}
