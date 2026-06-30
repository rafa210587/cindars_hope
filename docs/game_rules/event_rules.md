---
doc_type: game_rule
status: accepted
domain: architecture-communication
source_adrs:
  - ADR-0007
source_documents:
  - docs/decisions/ADR-0007-event-bus-gameplay-communication.md
  - .claude/rules/unity-architecture.md
last_reviewed: 2026-06-01
---

# Event Bus Rules

## Purpose

Defines gameplay communication via GameEventBus and prohibits direct MonoBehaviour calls.

---

## Canonical Rules

### Rule: GameEventBus for All Gameplay Communication

- **Rule:** All gameplay systems communicate via `GameEventBus.Publish()` and `Subscribe()`. Direct MonoBehaviour-to-MonoBehaviour calls are prohibited.

- **Pattern:**
  ```csharp
  // Publisher (combat system):
  GameEventBus.Publish(new PlayerHealthChangedEvent { newHealth = 50, maxHealth = 100 });
  
  // Subscriber (UI):
  GameEventBus.Subscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
  void OnPlayerHealthChanged(PlayerHealthChangedEvent e) => healthBar.SetHealth(e.newHealth, e.maxHealth);
  ```

- **Applies to:**
  - Combat: health changes, damage, status effects, ability activations, enemy spawns
  - Inventory: item pickup, equip, drop, use, capacity changes
  - Farming: crop growth, harvest, resource depletion, weather
  - UI: stat updates, modal visibility, button state changes
  - Cave: level generation complete, enemy spawn, exit trigger, resource node depletion
  - Day/Night: day advancement, time changes, season shifts
  - Economy: gold changes, shop interactions, price updates
  - NPC/Dialogue: quest updates, NPC state changes, dialogue triggers

- **Why:** Decouples systems; makes event flow auditable; enables save/load listening; prevents tight coupling; makes event broadcast possible

### Rule: No Direct Calls (Prohibited)

- **Prohibited:**
  ```csharp
  // ✗ Direct call to another system
  _hudManager.UpdateHealth(newHealth);
  
  // ✗ GetComponent for gameplay communication
  GetComponent<InventoryManager>().AddItem(item);
  
  // ✗ FindObjectOfType to locate gameplay system
  FindObjectOfType<InventoryManager>().RemoveItem(item);
  
  // ✗ Accessing public fields for gameplay state
  otherSystem.currentHealth = newHealth;
  ```

- **Applies to:** All gameplay code; no direct MonoBehaviour wiring for events

### Rule: Allowed Exceptions (Non-Gameplay)

- **Editor Tools** (`Assets/_Game/Scripts/Editor/**`)
  - May use direct references for data inspection, asset generation, debugging
  - Not gameplay communication

- **Bootstrap Wiring** (`GameBootstrap`)
  - Holds direct refs for dependency injection
  - Setup-only; not runtime gameplay events
  - Example: `public SpellDatabaseSO SpellDatabase => _spellDatabase;`

- **Local Component Access**
  - `GetComponent<T>()` on same GameObject for setup
  - Used in Awake/Start for internal initialization, not gameplay events
  - Example: `var _rigidbody = GetComponent<Rigidbody>();`

- **Unity Lifecycle Methods**
  - Awake(), Start(), OnDestroy() setup operations
  - Not gameplay state communication
  - Example: `OnDestroy() { GameEventBus.Unsubscribe<...>(); }`

### Rule: Event DTO Requirement

- **Rule:** Every gameplay event must have a corresponding DTO:
  ```csharp
  public class PlayerHealthChangedEvent
  {
      public int newHealth;
      public int maxHealth;
      public float timestamp;
  }
  ```

- **DTO rules:**
  - Public fields or properties (JSON-serializable)
  - Simple types only (string, int, float, bool, enum)
  - Include relevant context (source, old value if needed)
  - No GameObjects or MonoBehaviour refs

- **Naming:** `[DomainEvent]` suffix (e.g., `PlayerHealthChangedEvent`, `ItemPickedUpEvent`)

### Rule: Unsubscribe on Destroy

- **Rule:** Always unsubscribe from events when MonoBehaviour destroys:
  ```csharp
  void OnDestroy()
  {
      GameEventBus.Unsubscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
  }
  ```

- **Why:** Prevents dead subscribers; prevents memory leaks
- **Applies to:** All MonoBehaviour subscribers

---

## Event Categories

| Category | Example Events | Domain |
|---|---|---|
| Combat | PlayerHealthChanged, DamageDealt, StatusEffectApplied, EnemyDefeated | Combat |
| Inventory | ItemPickedUp, ItemEquipped, ItemDropped, ItemUsed, CapacityChanged | Inventory |
| Farming | CropGrown, CropHarvested, ResourceDepleted, WeatherChanged | Farming |
| UI | ModalOpened, ModalClosed, HUDUpdated, ButtonPressed | UI |
| Cave | LevelGenerated, EnemySpawned, ResourceNodeDepleted, ExitTriggered | Cave |
| Time | DayAdvanced, TimeChanged, SeasonChanged | World |
| Economy | GoldChanged, ShopInteraction, ItemSold, ItemBought | Economy |
| NPC | QuestUpdated, DialogueTriggered, NPCStateChanged | NPC |

---

## Related ADRs

- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md)
- [ADR-0006: Save Data Contracts](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) (save system listens to events)
- [ADR-0005: Cave Stable Run](../decisions/ADR-0005-cave-stable-run-and-replay.md) (cave events if needed)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Source: docs/decisions/ADR-0007-event-bus-gameplay-communication.md*
