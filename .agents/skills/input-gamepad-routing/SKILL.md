---
name: input-gamepad-routing
description: Separação de input da lógica de gameplay, roteamento de foco entre gameplay e UI modal (sem mover o player com modal aberto), e caminho para gamepad. Usar em specs de UI/input focus, novos bindings de tecla, integração de gamepad ou qualquer componente que leia Input.GetKey diretamente.
---

# Skill: Input e Roteamento de Foco

O projeto usa **Unity Legacy Input** (`Input.GetKey`, `Input.GetAxis`, `Input.GetKeyDown`) — não há InputSystem package. Todo input de gameplay é lido em `Update()` de MonoBehaviours específicos (não no código de lógica pura). O invariante principal é: **input de gameplay é bloqueado quando um modal está aberto** — verificar `ModalManager.HasActiveModal` antes de processar qualquer tecla de gameplay.

## Quando usar

- Spec adiciona novo binding de tecla para ação de gameplay.
- Spec de UI modal que deve bloquear movimento/ação do player.
- Spec de integração de gamepad (futuro — ver seção de dívida).
- Spec que mencione "input focus", "modal guard", "roteamento de input", "navegação por gamepad".
- `PlayerController` ou outro componente de input precisa de nova tecla.

## Estado atual (honesto)

| Aspecto | Estado |
|---|---|
| Input system | Unity Legacy (`Input.GetKey`/`GetAxis`/`GetKeyDown`) — sem `UnityEngine.InputSystem` |
| Modal guard | `ModalManager.HasActiveModal` verificado no `Update()` de `FoodConsumer`, `PlayerController`, `SkillTreeGameplayPanelController` etc. |
| Movimento | `PlayerController` — `Input.GetAxis("Horizontal")` / `Input.GetAxis("Vertical")` |
| Ações de gameplay | `Input.GetKeyDown(KeyCode.X)` por componente |
| Gamepad | **FUTURO** — spec `04_ui_menu_gamepad_navigation` bloqueada; sem `InputAction` no projeto v1 |
| Separação input/lógica | **PARCIAL** — PlayerController lê e processa; `GameplayInputRouter` despacha eventos para UI panels |

## Sistemas existentes (reusar, não duplicar)

| Classe | Papel |
|---|---|
| `PlayerController` (`CindarsHope.Player`) | Leitura de eixos de movimento + modal guard + ataque; fonte canônica de input de movimento |
| `GameplayInputRouter` | MonoBehaviour; lê teclas de UI (I, K, L, U, J) e publica eventos no GameEventBus (`InventoryOpenedEvent`, `SkillTreeOpenedEvent`, etc.) |
| `ModalManager` | Singleton; `HasActiveModal: bool`; `PushModal(ModalType)` / `TryPopModal(ModalType)`; acesso via `GameBootstrap.Instance.ModalManager` |
| `FoodConsumer` | Exemplo de guard: verifica `GameBootstrap.Instance?.ModalManager?.HasActiveModal` antes de processar tecla |
| `PlayerMovementActionRuntimeBootstrap` | Wiring de Dash/Dodge/Block em runtime; `DirectionalDoubleTapDetector` para double-tap |
| `DirectionalDoubleTapDetector` | Detecta double-tap direcional (para Dodge) — reusar para qualquer ação de double-tap |
| `InteractionSystem` | Leitura de tecla de interação (E); delega para `IInteractable` — não duplicar |

## Procedimento

### Adicionar novo binding de tecla para ação de gameplay

1. Escolha o MonoBehaviour responsável pela ação (ex.: `PlayerController` para combate; `FoodConsumer` para consumo).
2. No `Update()` do componente, adicione o guard de modal **antes** de qualquer leitura de tecla:

```csharp
private void Update()
{
    if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
        return; // bloqueia TODA tecla de gameplay quando modal está aberto

    if (Input.GetKeyDown(_myActionKey))
        TryDoMyAction();
}
```

3. Para ações que publicam eventos de UI (abrir painel, etc.), adicione a linha no `GameplayInputRouter.Update()` e publique via `GameEventBus`.

4. Nunca leia input diretamente dentro de classes de lógica pura (serviços, DTOs, managers sem MonoBehaviour).

### Modal guard (invariante)

```csharp
// CORRETO: bloquear TODO input de gameplay com modal ativo
if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true) return;

// ERRADO: bloquear só movimento mas deixar ação
if (!ModalManager.HasActiveModal) { ProcessMovement(); }
ProcessAttack(); // BUG: ataque passa mesmo com modal
```

Bloquear no topo do `Update()` é a forma canônica — não filter por tipo de ação.

### Double-tap (Dodge, sprint-direcional)

Reutilize `DirectionalDoubleTapDetector`:

```csharp
// Exemplo (PlayerMovementActionRuntimeBootstrap):
var doubleTap = new DirectionalDoubleTapDetector();
doubleTap.Update(direction, Time.deltaTime); // chama no Update
if (doubleTap.HasTap(out var tapDir)) TryDodge(tapDir);
```

### Teclas de UI (não-gameplay)

Teclas que abrem painéis (I, K, L, U, J) ficam no `GameplayInputRouter` — publicam evento no GameEventBus; o painel assina e reage. Isso separa o roteamento da UI da lógica dos painéis.

```csharp
// Em GameplayInputRouter.Update():
if (Input.GetKeyDown(KeyCode.I))
    GameEventBus.Publish(new InventoryOpenedEvent());
```

### Gamepad (FUTURO — dívida registrada)

Gamepad não tem spec ativa em v1. Se uma spec futura pedir gamepad:
- A migração de `Input.GetKey` para `InputAction` (`UnityEngine.InputSystem`) é a mudança correta.
- Não criar um segundo leitor de tecla por `Input.GetJoystickNames()` — é um patch que não escala.
- A skill `ui-modal-stack` governa o foco de painel (Tab, D-pad de menu) — combinar com ela.

## Regras

- **Todo input de gameplay** verifica `ModalManager.HasActiveModal` antes de processar — sem exceção.
- **Lógica de jogo** (managers, services, calculators) não lê `Input.*` — apenas MonoBehaviours de input.
- Não criar um `InputManager` próprio enquanto o projeto usa Legacy Input — apenas introduz camada sem benefício.
- Ações que resultam em mudança de estado publicam evento no GameEventBus, não chamam diretamente outro MonoBehaviour.
- `PlayerController` é a fonte de verdade de movimento — não duplicar leitura de eixos em outro componente.

## Quando parar e reportar

- Spec pede gamepad ou `UnityEngine.InputSystem` → spec futura bloqueada; relatar como `BLOCKED_BY_FUTURE_SCOPE`.
- Spec pede novo `Input.GetKey` em classe de lógica pura (não MonoBehaviour) → violação de arquitetura; parar e reportar.

## Relacionados

- `(skill: ui-modal-stack)` — `ModalManager.HasActiveModal`; push/pop; foco bloqueado com modal aberto
- `(skill: ui-projection-pattern)` — painéis abertos via evento de input
- `(skill: player-ability-runtime)` — Dash/Dodge/Block: bindings especiais (double-tap, Shift, Space)
- `(skill: monobehaviour-decomposition)` — extrair leitura de input de MonoBehaviours grandes
- `(rule: unity-architecture)` — ações de input viram eventos; sem chamada direta entre MonoBehaviours
