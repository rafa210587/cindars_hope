---
name: event-bus-pattern
description: Implement gameplay communication via GameEventBus instead of direct calls
version: 1.0
---

# Event Bus Pattern

Use when the task involves communication between gameplay systems, events, or decoupling systems.

## Core Rule

**NEVER** direct MonoBehaviour-to-MonoBehaviour calls for gameplay logic.  
**ALWAYS** use `GameEventBus.Publish()` and `Subscribe()`.

## Why?

- Decouples systems (one change doesn't break others)
- Avoids circular dependencies
- Enables testing without full game setup
- Supports multiple subscribers to same event
- Clear event-driven architecture

## Event Definition Pattern

### Event Class Structure

```csharp
namespace CindarsHope.Runtime.Events
{
    // ✅ Correct pattern
    public class DamageAppliedEvent
    {
        public int targetId;           // ID, not reference
        public float damageAmount;
        public Vector2 targetPosition;  // Primitive, not Transform
        public int damagerId;          // ID of attacker
        public bool isCritical;
    }
    
    public class ItemUsedEvent
    {
        public int itemId;
        public int userId;  // Player or NPC
    }
    
    public class LevelCompleteEvent
    {
        public int levelIndex;
        public float completionTime;
    }
}
```

### Naming Convention

```
[Noun][Verb]Event

ItemCraftedEvent
DayStartedEvent
ToolEquippedEvent
DamageAppliedEvent
EnemyDefeatedEvent
LootPickedEvent
```

### ❌ DO NOT

```csharp
// Wrong: Carrying references
public class DamageAppliedEvent
{
    public Transform targetTransform;  // ❌ NO Transform!
    public MonoBehaviour damager;      // ❌ NO MonoBehaviour!
    public GameObject targetGameObject; // ❌ NO GameObject!
}

// Wrong: Carrying large payloads
public class DamageAppliedEvent
{
    public List<EnemyHealth> allEnemiesInRange; // ❌ Too much data!
}
```

## Publishing Events

### Pattern

```csharp
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private GameEventBus eventBus;
    
    public void Attack(int targetId)
    {
        var damage = CalculateDamage();
        
        // Publish event
        eventBus.Publish(new DamageAppliedEvent
        {
            targetId = targetId,
            damageAmount = damage,
            damagerId = playerId,
            isCritical = IsCriticalHit()
        });
    }
}
```

### ❌ DO NOT

```csharp
// Wrong: Direct call
public void Attack(int targetId)
{
    var enemy = FindObjectOfType<EnemyHealth>();
    enemy.TakeDamage(damage); // ❌ Direct call!
}

// Wrong: Passing object reference
eventBus.Publish(new DamageAppliedEvent
{
    targetTransform = target.transform, // ❌ NO Transform!
    damager = this // ❌ NO MonoBehaviour!
});
```

## Subscribing to Events

### Pattern

```csharp
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private GameEventBus eventBus;
    private int enemyId;
    
    private void Start()
    {
        eventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
    }
    
    private void OnDisable() // or OnDestroy
    {
        // CRITICAL: Always unsubscribe
        eventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
    }
    
    private void OnDamageApplied(DamageAppliedEvent evt)
    {
        if (evt.targetId != enemyId)
            return; // Not for us
        
        ApplyDamage(evt.damageAmount);
    }
    
    private void ApplyDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }
}
```

### Rules

- [ ] Subscribe in `Start()` or `Awake()`
- [ ] **ALWAYS** unsubscribe in `OnDisable()` or `OnDestroy()`
- [ ] Check event.targetId or similar to filter (not all events for you)
- [ ] Keep listener logic lightweight (don't loop or do heavy work)
- [ ] Use correct event type in Subscribe/Unsubscribe

### ❌ DO NOT

```csharp
// Wrong: No unsubscribe
private void Start()
{
    eventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
    // Missing OnDisable unsubscribe!
}

// Wrong: Forgetting to filter
private void OnDamageApplied(DamageAppliedEvent evt)
{
    ApplyDamage(evt.damageAmount); // Could apply damage to everyone!
}

// Wrong: Heavy work in listener
private void OnItemUsed(ItemUsedEvent evt)
{
    // Regenerating entire world in event listener? NO!
    RegenerateAllCaves();
}
```

## Complex Event Chains

### Pattern: Event Triggers Another Event

```csharp
public class StatusEffectManager : MonoBehaviour
{
    [SerializeField] private GameEventBus eventBus;
    
    private void Start()
    {
        eventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
    }
    
    private void OnDamageApplied(DamageAppliedEvent evt)
    {
        // Trigger status effect based on damage
        if (evt.damageAmount > 50)
        {
            eventBus.Publish(new StatusAppliedEvent
            {
                targetId = evt.targetId,
                statusType = StatusType.Stun,
                duration = 2f
            });
        }
    }
    
    private void OnDisable()
    {
        eventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
    }
}
```

### ✅ OK: Event triggers other logic
### ❌ NOT OK: Event triggers direct system call

```csharp
// ❌ WRONG
private void OnDamageApplied(DamageAppliedEvent evt)
{
    FindObjectOfType<AudioManager>().PlaySound("hit"); // Direct call!
}

// ✅ RIGHT
private void OnDamageApplied(DamageAppliedEvent evt)
{
    eventBus.Publish(new SoundEffectPlayedEvent
    {
        soundId = "hit"
    });
}
```

## Common Events Library

Reference common events (if they exist in `Assets/Scripts/Runtime/Events/`):

```
DayStartedEvent
NightStartedEvent
ItemCraftedEvent
ItemUsedEvent
ItemPickedEvent
DamageAppliedEvent
ToolEquippedEvent
ToolUnequippedEvent
EnemyDefeatedEvent
LootSpawnedEvent
PlayerHealthChangedEvent
PlayerHungerChangedEvent
PlayerStaminaChangedEvent
```

## Injection Pattern (Bootstrap)

Events are typically available via:

```csharp
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private GameEventBus eventBus; // Set in Inspector
    
    private void OnValidate()
    {
        if (eventBus == null)
            eventBus = FindObjectOfType<GameEventBus>(); // Fallback (not ideal)
    }
}
```

Or via Bootstrap:

```csharp
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GameEventBus eventBus;
    
    private void Awake()
    {
        // Inject into all systems
        GetComponentInChildren<PlayerCombat>().SetEventBus(eventBus);
        GetComponentInChildren<EnemyHealth>().SetEventBus(eventBus);
    }
}
```

## Testing Pattern

```csharp
[Test]
public void DamageEvent_TriggersTakeDamage()
{
    var eventBus = new GameEventBus();
    var enemyHealth = new MockEnemyHealth();
    enemyHealth.SetEventBus(eventBus);
    
    eventBus.Publish(new DamageAppliedEvent
    {
        targetId = enemyHealth.Id,
        damageAmount = 25f
    });
    
    Assert.AreEqual(75f, enemyHealth.CurrentHealth);
}
```

## Audit Checklist

Before finalizing event-driven code:

- [ ] No `GetComponent<>()` for other systems in logic
- [ ] No `FindObjectOfType<>()` anywhere
- [ ] All gameplay communication via `eventBus.Publish()`
- [ ] All event subscribers call `Unsubscribe()` in `OnDisable`/`OnDestroy`
- [ ] Events carry IDs and primitives, not references
- [ ] Event names follow `[Noun][Verb]Event` pattern
- [ ] Each event handler filters by ID (e.g., targetId)
- [ ] No circular event chains (A publishes B, B publishes A)

## Common Mistakes

❌ Direct call instead of publish:
```csharp
GetComponent<PlayerStats>().AddExperience(100); // WRONG!
```

❌ Forgetting unsubscribe:
```csharp
eventBus.Subscribe(...); // Missing OnDisable unsubscribe
```

❌ Carrying references:
```csharp
public class DamageEvent { public MonoBehaviour source; } // WRONG!
```

❌ Not filtering by ID:
```csharp
private void OnDamage(DamageAppliedEvent evt)
{
    TakeDamage(evt.amount); // Applies to everyone!
}
```

## Integration

- **Spec Execution** → Uses this if gameplay communication in scope
- **Non-Regression Review** → Audits for direct calls (violations)
- **Implementation Closeout** → Validates event pattern in audit

---

**Event bus is mandatory for all gameplay communication. No direct calls.**
