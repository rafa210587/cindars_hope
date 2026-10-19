---
doc_type: adr
status: accepted
adr_id: ADR-0007
title: Event Bus Gameplay Communication
date: 2026-06-01
source_documents:
  - .claude/rules/unity-architecture.md
  - docs/game_rules/event_rules.md
supersedes: []
superseded_by: []
applies_to:
  - gameplay-systems
  - system-communication
  - event-flow
---

# ADR-0007 — Event Bus Gameplay Communication

## Status

**accepted** (architectural decision for all gameplay systems)

## Context

Gameplay systems are highly interdependent: combat affects inventory, farming affects UI, player death affects multiple subsystems. Direct MonoBehaviour-to-MonoBehaviour calls create tight coupling, break when either system changes, and are invisible to external observers (save/load, analytics, UI). Question: How should gameplay systems communicate without creating brittle dependencies?

## Decision

**All gameplay systems communicate via `GameEventBus.Publish()` and `Subscribe()`. Direct calls are prohibited.**

### Event Bus Pattern

```csharp
// Publisher: combat system updates health
GameEventBus.Publish(new PlayerHealthChangedEvent { newHealth = 50, maxHealth = 100 });

// Subscriber: UI listens for health changes
public class HUDManager : MonoBehaviour
{
    void Start()
    {
        GameEventBus.Subscribe<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
    }
    
    void OnPlayerHealthChanged(PlayerHealthChangedEvent e)
    {
        healthBar.SetHealth(e.newHealth, e.maxHealth);
    }
}
```

### Applies To

All gameplay systems:
- **Combat:** health changes, damage, status effects, ability activations
- **Inventory:** item pickup, equip, drop, use
- **Farming:** crop growth, harvest, resource depletion
- **UI:** stat updates, modal visibility, button state
- **Cave:** level generation, enemy spawn, exit trigger
- **Day/Night Cycle:** day advancement, weather change
- **Economy:** gold changes, shop interactions
- **NPC/Dialogue:** quest updates, NPC state

### Prohibited (Direct Calls)

```csharp
// PROHIBITED: direct call from combat to UI
_hudManager.UpdateHealth(newHealth);

// PROHIBITED: GetComponent for gameplay communication
GetComponent<InventoryManager>().AddItem(item);

// PROHIBITED: FindObjectOfType to find gameplay system
FindObjectOfType<InventoryManager>().RemoveItem(item);
```

### Allowed Exceptions

1. **Editor Tools** under `Assets/_Game/Scripts/Editor/**`
   - May use direct references for data inspection, repair, generation

2. **Bootstrap Wiring** in `GameBootstrap`
   - Holds direct refs for dependency injection
   - Not gameplay communication; setup only

3. **Local Component Access**
   - `GetComponent<T>()` on same GameObject for setup (Awake/Start)
   - Not runtime gameplay events

4. **Unity Lifecycle** (internal setup)
   - Awake(), Start(), OnDestroy() setup operations
   - Not gameplay state changes

### Benefits

- **Decoupled:** Systems don't know about each other
- **Observable:** All gameplay state changes flow through bus
- **Testable:** Mock bus for unit tests
- **Auditable:** Complete event log for debugging, analytics, save/load
- **Extensible:** New systems subscribe to existing events without modifying publishers

## Consequences

- Event DTOs required for each gameplay change
- Subscribers needed for each consumer of events
- Event bus must be reliable and well-tested
- Circular dependencies prevented by event isolation
- Save/load system can listen to events for persistence

## Applies To

- All combat systems
- All inventory systems
- All farm systems
- All UI update logic
- All gameplay state changes

## If Exception Needed

Spec must explicitly state: "Direct call authorized: `X` calls `Y.Method()` because `[reason event bus unsuitable]`."

## Source Documents

- [unity-architecture.md](./../.claude/rules/unity-architecture.md) — enforcement rule
- [event_rules.md](./../game_rules/event_rules.md) — current event patterns

---

*Created: 2026-06-01*  
*Status: accepted*  
*Related: ADR-0005 (cave events), ADR-0006 (save/load listens to events)*
