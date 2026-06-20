---
name: player-ability-runtime
description: Adiciona uma player ability non-slot (Dash, Dodge, Block, roll, teleport, etc.) com wiring correto de FixedUpdate/physics. Use em qualquer tarefa que adicione uma nova player ability non-slot que mova ou modifique o player em runtime.
---

# Skill: Player Ability em Runtime

Anexe ability controllers ao GameObject do player via o bootstrap existente e respeite a regra de conflito de FixedUpdate — esquecer `IsBeingDisplaced` foi o bug original que deixava o player parado.

## Quando usar

A tarefa adiciona ou conserta uma player ability que:
- É disparada por input (key down / hold / double-tap)
- Move o player ou muda a movement speed
- **Não** é um active skill slot (teclas 1–4)
- Roda em runtime (não editor-only)

Exemplos: Dash, Dodge, Block, roll, sprint toggle, teleport, blink.

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo
3. `Assets/_Game/Scripts/Player/PlayerController.cs` — SpeedMultiplier atual, IsBeingDisplaced, FixedUpdate
4. `Assets/_Game/Scripts/Player/Movement/PlayerMovementDisplacementResolver.cs` — API TryDisplace

---

## Arquitetura central

### Attachment de component

Todos os ability controllers são anexados ao GameObject do player via um bootstrap `RuntimeInitializeOnLoadMethod`.

**Reutilize `PlayerMovementActionRuntimeBootstrap` se ele já existir.** Só crie um novo bootstrap se for realmente um lifecycle diferente (ex.: bootstrap do combat system).

```csharp
private void AttachControllers(GameObject playerObject)
{
    EnsureComponent<PlayerMovementDisplacementResolver>(playerObject);
    EnsureComponent<PlayerDashController>(playerObject);
    EnsureComponent<DirectionalDoubleTapDetector>(playerObject);
    EnsureComponent<PlayerDodgeController>(playerObject);
    EnsureComponent<PlayerBlockController>(playerObject);
    // Add new ability here:
    // EnsureComponent<PlayerRollController>(playerObject);
}
```

---

## A regra de conflito de FixedUpdate

**Crítico**: qualquer ability que chame `Rigidbody2D.MovePosition()` a partir de uma coroutine (cadência de Update) SERÁ sobrescrita por `PlayerController.FixedUpdate`, a menos que `IsBeingDisplaced` esteja setado.

### Por que quebra

```
Frame N:
  FixedUpdate → rb.MovePosition(current_pos) → physics queues current_pos
  Update/Coroutine → rb.MovePosition(target_pos) → physics queues target_pos

Frame N+1:
  FixedUpdate → rb.MovePosition(current_pos2) ← OVERWRITES target_pos
  Physics → applies current_pos2 → player never moves
```

### Pattern de fix obrigatório

Em `PlayerController`:
```csharp
public bool IsBeingDisplaced { get; set; }

private void FixedUpdate()
{
    if (_rigidbody == null) { LogMissingRigidbodyOnce(); return; }
    if (IsBeingDisplaced) return;   // ← skip when displaced
    // ... normal movement
}
```

Em `PlayerMovementDisplacementResolver.DisplaceRoutine`:
```csharp
if (_playerController != null)
{
    _playerController.SpeedMultiplier = 0f;
    _playerController.IsBeingDisplaced = true;
}
// ... displacement loop ...
if (_playerController != null)
{
    _playerController.SpeedMultiplier = previousSpeedMultiplier;
    _playerController.IsBeingDisplaced = false;
}
```

---

## Template de ability controller

```csharp
[DisallowMultipleComponent]
public sealed class PlayerXController : MonoBehaviour
{
    [SerializeField] private float _distance = 3.5f;
    [SerializeField] private float _duration = 0.14f;
    [SerializeField] private float _cooldown = 1.0f;
    [SerializeField] private int _staminaCost = 40;

    [SerializeField] private PlayerMovementDisplacementResolver _resolver;
    [SerializeField] private StaminaManager _staminaManager;

    private float _lastUseTime = float.MinValue;

    private void Start()
    {
        if (_resolver == null) _resolver = GetComponent<PlayerMovementDisplacementResolver>();
        var bootstrap = GameBootstrap.Instance;
        if (bootstrap != null && _staminaManager == null)
            _staminaManager = bootstrap.StaminaManager;
    }

    private void Update()
    {
        // 1. Modal guard — always first
        if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return;

        // 2. Read input
        if (!DetectInput(out var direction)) return;

        // 3. Try ability
        TryUse(direction);
    }

    private void TryUse(Vector2 direction)
    {
        // 4. In-progress guard
        if (_resolver == null || _resolver.IsDisplacing) return;

        // 5. Cooldown
        if (Time.time - _lastUseTime < _cooldown)
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent("X em cooldown."));
            return;
        }

        // 6. Stamina (optional — graceful if missing)
        if (_staminaManager != null && !_staminaManager.TrySpendStamina(_staminaCost))
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent("Stamina insuficiente."));
            return;
        }

        _lastUseTime = Time.time;
        if (!_resolver.TryDisplace(direction, _distance, _duration, () =>
            GameEventBus.Publish(new PlayerActionFeedbackEvent("X!"))))
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent("X bloqueado."));
        }
    }

    private bool DetectInput(out Vector2 direction)
    {
        direction = Vector2.zero;
        // implement per ability
        return false;
    }
}
```

---

## Padrões de input

### Key press one-shot (Dash)

```csharp
if (Input.GetKeyDown(KeyCode.Space))
{
    var dir = ReadDirectionalInput();
    if (dir.sqrMagnitude <= 0.1f && _playerController != null)
        dir = _playerController.LastFacingDirection;
    if (dir.sqrMagnitude > 0.1f) TryUse(dir.normalized);
}
```

### Double-tap detection (Dodge)

Use `DirectionalDoubleTapDetector.UpdateAndCheckDoubleTap()` — já existe. Não crie um detector paralelo.

### Hold key (Block, sprint)

```csharp
private void Update()
{
    if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
    {
        if (_isActive) Deactivate();
        return;
    }

    var holding = Input.GetKey(KeyCode.LeftShift);
    if (holding && !_isActive) Activate();
    else if (!holding && _isActive) Deactivate();
}
```

---

## Modificação de speed (Block / slow)

Use `PlayerMovementSlowState` se existir. Caso contrário:

```csharp
// Activate
if (_playerController != null) _playerController.SpeedMultiplier *= _slowMultiplier;

// Deactivate — restore exact previous value
if (_playerController != null) _playerController.SpeedMultiplier = _previousMultiplier;
```

Guarde `_previousMultiplier = _playerController.SpeedMultiplier` antes de ativar.

---

## Integração com stamina

Sempre opcional e graceful:

```csharp
// One-shot cost
if (_staminaManager != null && !_staminaManager.TrySpendStamina(cost)) { /* reject */ return; }

// Per-second drain (Block)
if (_staminaManager != null)
{
    _staminaManager.TrySpendStamina(_drainPerSecond * Time.deltaTime);
    if (_staminaManager.CurrentStamina <= 0f) Deactivate();
}
```

Se `StaminaManager` ou sua API estiver ausente → documente o debt:
```
STAMINA_MOVEMENT_ACTION_DEBT
```

---

## Feedback

Sempre publique, mesmo que ainda não exista um consumer de HUD:

```csharp
GameEventBus.Publish(new PlayerActionFeedbackEvent("Dash!"));
```

Se o HUD ainda não exibir → documente o debt:
```
HUD_FEEDBACK_CONSUMER_DEBT
```

---

## Debt tags (usar em comentários + report)

```
TODO_INTEGRATION_NOT_FINAL
BALANCE_FINAL_PENDING
BLOCK_DAMAGE_REDUCTION_DEFERRED_TO_COMBAT_RUNTIME
DODGE_IFRAMES_DEFERRED_TO_COMBAT_RUNTIME
STAMINA_MOVEMENT_ACTION_DEBT
HUD_FEEDBACK_CONSUMER_DEBT
BOUNDS_FINAL_DEFERRED_IF_NO_BOUND_SYSTEM
```

---

## Validação

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED"; exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "EDITOR BUILD FAILED"; exit 1 }
```

O checklist humano de Play Mode deve cobrir:
- Ability executa (o player de fato move / o state muda)
- Modal guard bloqueia a execução
- Cooldown impede spam
- Stamina consumida ou debt explícito
- Collision respeitada ou debt explícito

---

## Regressões comuns

- Não setar `IsBeingDisplaced = true` → o player não move (o bug original)
- Não restaurar `SpeedMultiplier` depois do displacement → player travado em speed 0
- Não restaurar `IsBeingDisplaced = false` em exception/early exit → player congelado para sempre
- Criar um segundo `DirectionalDoubleTapDetector` em vez de reutilizar o existente
- Adicionar a ability a um active slot (teclas 1–4) — estas são ações NON-SLOT

## Quando parar e reportar

- Mover o player exige reescrever o core de `PlayerController` → parar, reportar
- Nenhum `Rigidbody2D` ou `transform` alcançável → `BLOCKED`
- Ability conflita com uma ação non-slot existente (mesma input key) → parar, reportar o conflito
