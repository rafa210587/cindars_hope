---
name: event-bus-pattern
description: Implementa comunicação entre sistemas de gameplay via GameEventBus em vez de chamadas diretas. Use sempre que a tarefa envolver comunicação entre sistemas (combat, farming, inventory, UI state, NPC, cave, day cycle, economy), publicação de eventos ou desacoplamento de sistemas.
---

# Skill: Padrão Event Bus

Use quando a tarefa envolver comunicação entre sistemas de gameplay, eventos ou desacoplamento de sistemas.

## Regra central

**NUNCA** faça chamadas diretas MonoBehaviour-to-MonoBehaviour para lógica de gameplay.
**SEMPRE** use `GameEventBus.Publish()` e `Subscribe()`.

## Por que existe

- Desacopla sistemas (uma mudança não quebra as outras)
- Evita dependências circulares
- Permite testar sem montar o jogo inteiro
- Suporta múltiplos subscribers para o mesmo evento
- Arquitetura event-driven clara

## Padrão de definição de evento

### Estrutura da classe de evento

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

### Convenção de nomenclatura

```
[Noun][Verb]Event

ItemCraftedEvent
DayStartedEvent
ToolEquippedEvent
DamageAppliedEvent
EnemyDefeatedEvent
LootPickedEvent
```

### ❌ NÃO faça

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

## Publicando eventos

### Padrão

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

### ❌ NÃO faça

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

## Inscrevendo-se em eventos

### Padrão

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

### Regras

- [ ] Subscribe em `Start()` ou `Awake()`
- [ ] **SEMPRE** faça Unsubscribe em `OnDisable()` ou `OnDestroy()`
- [ ] Cheque event.targetId ou similar para filtrar (nem todos os eventos são para você)
- [ ] Mantenha a lógica do listener leve (não faça loop nem trabalho pesado)
- [ ] Use o tipo de evento correto em Subscribe/Unsubscribe

### ❌ NÃO faça

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

## Cadeias complexas de eventos

### Padrão: um evento dispara outro evento

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

### ✅ OK: evento dispara outra lógica
### ❌ NÃO OK: evento dispara chamada direta de sistema

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

## Biblioteca de eventos comuns

Referência de eventos comuns (se existirem em `Assets/Scripts/Runtime/Events/`):

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

## Padrão de injeção (Bootstrap)

Eventos normalmente ficam disponíveis via:

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

Ou via Bootstrap:

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

## Padrão de teste

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

## Checklist de auditoria

Antes de finalizar código event-driven:

- [ ] Nenhum `GetComponent<>()` para outros sistemas dentro da lógica
- [ ] Nenhum `FindObjectOfType<>()` em lugar algum
- [ ] Toda comunicação de gameplay via `eventBus.Publish()`
- [ ] Todos os subscribers de evento chamam `Unsubscribe()` em `OnDisable`/`OnDestroy`
- [ ] Eventos carregam IDs e primitivos, não referências
- [ ] Nomes de evento seguem o padrão `[Noun][Verb]Event`
- [ ] Cada handler de evento filtra por ID (ex.: targetId)
- [ ] Sem cadeias circulares de evento (A publica B, B publica A)

## Erros comuns

❌ Chamada direta em vez de publish:
```csharp
GetComponent<PlayerStats>().AddExperience(100); // WRONG!
```

❌ Esquecer o unsubscribe:
```csharp
eventBus.Subscribe(...); // Missing OnDisable unsubscribe
```

❌ Carregar referências:
```csharp
public class DamageEvent { public MonoBehaviour source; } // WRONG!
```

❌ Não filtrar por ID:
```csharp
private void OnDamage(DamageAppliedEvent evt)
{
    TakeDamage(evt.amount); // Applies to everyone!
}
```

## Relacionados

- **Spec Execution** → usa esta skill se houver comunicação de gameplay no escopo
- **Non-Regression Review** → audita chamadas diretas (violações)
- **Implementation Closeout** → valida o padrão de evento na auditoria

---

**O event bus é obrigatório para toda comunicação de gameplay. Sem chamadas diretas.**
