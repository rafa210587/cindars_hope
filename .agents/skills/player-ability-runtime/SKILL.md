---
name: player-ability-runtime
description: Adiciona player ability non-slot (Dash, Dodge, Block, sprint, blink, roll) com wiring correto de FixedUpdate/physics. Usar em qualquer spec que adicione ability não pertencente aos active slots (teclas 1–4) que mova ou modifique o player em runtime.
---

# Skill: Player Ability em Runtime

Anexe ability controllers ao player via bootstrap existente e respeite a regra de conflito de FixedUpdate — esquecer `IsBeingDisplaced` foi o bug original que deixava o player parado.

## Quando usar

A spec adiciona ou corrige ability que:
- É disparada por input (key down / hold / double-tap)
- Move o player ou muda `SpeedMultiplier`
- **Não** é active skill slot (teclas 1–4) — ver `skill-tree-authoring` para esses
- Roda em runtime na scene

## Leitura mínima

1. `Assets/_Game/Scripts/Player/PlayerController.cs` — `SpeedMultiplier`, `IsBeingDisplaced`, `FixedUpdate`
2. `Assets/_Game/Scripts/Player/Movement/PlayerMovementDisplacementResolver.cs` — `TryDisplace`

---

## A regra de conflito de FixedUpdate (trap crítica)

Qualquer ability que chame `Rigidbody2D.MovePosition()` de uma coroutine **será sobrescrita** por `PlayerController.FixedUpdate` a menos que `IsBeingDisplaced` esteja setado.

```csharp
// PlayerController.FixedUpdate — obrigatório:
if (IsBeingDisplaced) return;  // ← sem isso, o player não move

// PlayerMovementDisplacementResolver.DisplaceRoutine:
_playerController.IsBeingDisplaced = true;
_playerController.SpeedMultiplier = 0f;
try   { /* displacement loop */ }
finally
{
    _playerController.IsBeingDisplaced = false;         // SEMPRE restaurar
    _playerController.SpeedMultiplier = _prevMultiplier; // SEMPRE restaurar
}
```

`finally` é obrigatório — sem ele, um early exit ou exception deixa o player congelado.

---

## Template de ability controller

```csharp
[DisallowMultipleComponent]
public sealed class PlayerXController : MonoBehaviour
{
    [SerializeField] private float _distance = 3.5f;
    [SerializeField] private float _duration = 0.14f;
    [SerializeField] private float _cooldown = 1.0f;
    [SerializeField] private int   _staminaCost = 40;

    private PlayerMovementDisplacementResolver _resolver;
    private StaminaManager _staminaManager;
    private float _lastUseTime = float.MinValue;

    private void Start()
    {
        _resolver = GetComponent<PlayerMovementDisplacementResolver>();
        var bs = GameBootstrap.Instance;
        if (bs != null) _staminaManager = bs.StaminaManager;
    }

    private void Update()
    {
        if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return; // guard
        if (!DetectInput(out var dir)) return;
        TryUse(dir);
    }

    private void TryUse(Vector2 dir)
    {
        if (_resolver == null || _resolver.IsDisplacing) return;
        if (Time.time - _lastUseTime < _cooldown)
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent(
                LocalizationService.Get("action.x.cooldown")));
            return;
        }
        if (_staminaManager != null && !_staminaManager.TrySpendStamina(_staminaCost))
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent(
                LocalizationService.Get("action.x.no_stamina")));
            return;
        }
        _lastUseTime = Time.time;
        _resolver.TryDisplace(dir, _distance, _duration, onComplete: null);
    }
}
```

Adicione o controller no `PlayerMovementActionRuntimeBootstrap.AttachControllers()`.

---

## Padrões de input

**Key press one-shot (Dash):** `Input.GetKeyDown(KeyCode.Space)` + fallback para `LastFacingDirection`.

**Double-tap (Dodge):** use `DirectionalDoubleTapDetector.UpdateAndCheckDoubleTap()` — já existe; não criar detector paralelo.

**Hold key (Block, sprint):**
```csharp
var holding = Input.GetKey(KeyCode.LeftShift);
if (holding && !_isActive) Activate();
else if (!holding && _isActive) Deactivate();
```

---

## Debt tags (usar em comentários e report)

```
BALANCE_FINAL_PENDING
BLOCK_DAMAGE_REDUCTION_DEFERRED_TO_COMBAT_RUNTIME
DODGE_IFRAMES_DEFERRED_TO_COMBAT_RUNTIME
STAMINA_MOVEMENT_ACTION_DEBT          — StaminaManager ausente; documentar
HUD_FEEDBACK_CONSUMER_DEBT            — toast sem consumer de HUD ainda
```

---

## Quando NÃO usar

- Ability é um active skill slot (teclas 1–4) → `skill-tree-authoring` + `ActiveSkillExecutionController`.
- Ability não move o player (só muda stats/flags) → MonoBehaviour simples, sem `DisplacementResolver`.
- Spec é de input routing para UI (abrir painéis) → `input-gamepad-routing`.

## Quando parar e reportar

- Mover o player exige reescrever o core de `PlayerController` → parar, reportar.
- Ability conflita com input key de ability existente → parar, reportar o conflito.
- `Rigidbody2D` inacessível no prefab do player → `BLOCKED`.

## Regressões críticas

- Não setar `IsBeingDisplaced = true` → player não move (o bug original).
- Não restaurar `IsBeingDisplaced = false` em finally → player congelado para sempre.
- Criar segundo `DirectionalDoubleTapDetector` em vez de reutilizar o existente.

## Relacionados

- `(skill: player-needs-survival)` — `TrySpendStamina` para custo de stamina
- `(skill: action-feedback-pipeline)` — `PlayerActionFeedbackEvent` para recusas
- `(skill: input-gamepad-routing)` — modal guard; `GameplayInputRouter` para teclas de UI
- `(skill: wave-integration-slice)` — como abilities se integram a uma WAVE_INTEGRATION
