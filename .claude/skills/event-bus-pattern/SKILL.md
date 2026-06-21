---
name: event-bus-pattern
description: Comunicação entre sistemas via GameEventBus — definir eventos como DTOs, publicar e assinar corretamente. Usar sempre que houver comunicação entre sistemas de gameplay (combat, farming, inventory, UI, NPC, cave, economia, ciclo de dia).
---

# Skill: Padrão Event Bus

**Regra central: toda comunicação entre sistemas de gameplay usa `GameEventBus.Publish()` / `Subscribe()`. Chamadas diretas MonoBehaviour → MonoBehaviour são proibidas** (rule: unity-architecture).

## Quando usar

- Spec adiciona comunicação entre dois sistemas diferentes (ex.: combat → HUD, farm → economy).
- Spec cria novo tipo de evento ou novo subscriber.
- Spec menciona "publicar evento", "sistema reage a X", "desacoplar sistemas".
- Qualquer spec de gameplay que precise que um sistema saiba o que outro fez.

## Definir um evento (DTO puro)

```csharp
// CORRETO — apenas primitivos e IDs
public readonly struct DamageAppliedEvent
{
    public readonly int TargetId;      // ID, nunca referência
    public readonly float Amount;
    public readonly bool IsCritical;
    public DamageAppliedEvent(int targetId, float amount, bool isCritical)
    { TargetId = targetId; Amount = amount; IsCritical = isCritical; }
}

// ERRADO — refs Unity no evento
public class DamageAppliedEvent
{
    public Transform Target;       // ❌ sem Transform
    public MonoBehaviour Damager;  // ❌ sem MonoBehaviour
    public GameObject Hit;         // ❌ sem GameObject
}
```

**Naming:** `[Substantivo][Verbo]Event` — `DayStartedEvent`, `EnemyDefeatedEvent`, `PlayerDiedEvent`, `ItemCraftedEvent`.

## Publicar

```csharp
// CORRETO
GameEventBus.Publish(new DamageAppliedEvent(targetId, damage, isCritical));

// ERRADO — chamada direta
enemy.TakeDamage(damage); // ❌ acoplamento direto
GetComponent<AudioManager>().Play("hit"); // ❌ chamada cruzada de sistema
```

## Assinar e desassinar

```csharp
private void OnEnable()  => GameEventBus.Subscribe<DamageAppliedEvent>(OnDamage);
private void OnDisable() => GameEventBus.Unsubscribe<DamageAppliedEvent>(OnDamage);

private void OnDamage(DamageAppliedEvent evt)
{
    if (evt.TargetId != _myId) return;  // filtrar — não é para mim
    ApplyDamage(evt.Amount);
}
```

**Regras de subscriber:**
- `OnDisable` ou `OnDestroy` sempre com Unsubscribe — sem isso há memory leak e eventos depois da morte.
- Filtrar por `TargetId` ou equivalente quando o evento não for global.
- Handler leve — nenhum loop pesado, nenhuma busca de componente no callback.
- Nunca publicar o mesmo tipo de evento dentro do seu próprio handler (cadeia circular).

## Checklist (antes de commitar)

```
[ ] Nenhum FindObjectOfType / FindObjectsByType em código novo
[ ] Nenhuma chamada direta cross-system — toda comunicação via Publish()
[ ] Todo Subscribe tem Unsubscribe em OnDisable/OnDestroy
[ ] Eventos: apenas primitivos + IDs, sem Transform/MonoBehaviour/GameObject
[ ] Naming: [Substantivo][Verbo]Event
[ ] Handlers filtram por ID quando evento não é global
[ ] Nenhuma cadeia circular (A publica B, B publica A)
```

## Quando NÃO usar

- Comunicação **interna ao mesmo MonoBehaviour** — métodos privados são corretos.
- **Bootstrap injection** (GameBootstrap.SetX()) durante inicialização — não criar evento para wiring.
- **Leitura de estado** (`_staminaManager.CurrentStamina`) — acesso direto de leitura é ok se a referência veio do bootstrap.
- **Lifecycle do Unity** (`OnCollisionEnter`, `OnTriggerEnter`) no mesmo component — não criar evento para isso.

## Testes

Eventos são structs puros → testáveis em EditMode (skill: `editmode-test-authoring`):

```csharp
[Test]
public void DamageEvent_CorrectSubscriberReceivesAmount()
{
    float received = 0f;
    void Handler(DamageAppliedEvent e) { if (e.TargetId == 42) received = e.Amount; }
    GameEventBus.Subscribe<DamageAppliedEvent>(Handler);
    GameEventBus.Publish(new DamageAppliedEvent(42, 25f, false));
    Assert.AreEqual(25f, received);
    GameEventBus.Unsubscribe<DamageAppliedEvent>(Handler);
}
```

## Relacionados

- `(rule: unity-architecture)` — GameEventBus obrigatório; chamadas diretas proibidas
- `(skill: action-feedback-pipeline)` — `PlayerActionFeedbackEvent` para recusas de gameplay
- `(skill: audio-event-wiring)` — `SfxEventBridge` assina eventos de gameplay via este pattern
- `(skill: editmode-test-authoring)` — testar contratos de evento em EditMode
