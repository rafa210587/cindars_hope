---
name: state-machine-design
description: Projeta uma finite state machine para player, enemy, boss, UI flow ou game flow quando há muitos states/transitions/phases ou boolean flags conflitantes. Produz uma FSM em C# puro (testável em EditMode) com Enter/Tick/Exit e uma invalid-transition policy explícita. Use antes de adicionar "só mais um bool" a um sistema que já malabariza vários.
---

# Skill: State Machine Design

Use isto quando um sistema começa a rastrear estado com vários booleans (`isAttacking`, `isStunned`, `canMove`, `isDead`...) que podem se contradizer. Distinta da skill `enemy-ai-authoring` (que autora *AI behaviors/moves/affixes* sobre o `EnemyBrain` vivo): esta skill é sobre a **estrutura de estado em si**.

## Quando NÃO usar

- Um ou dois flags genuinamente independentes → deixe-os; uma FSM adiciona cerimônia.
- Comportamento de boss *phase* no brain existente → use `enemy-ai-authoring` (precedente: `BossPhaseLogic`); aplique esta skill só se o wiring da phase em si for um emaranhado de flags.

## Procedimento

1. **Enumere os states.** Liste cada state explicitamente (um `enum` ou uma classe por state). Se dois booleans não podem ser true ao mesmo tempo, eles são uma state machine, não dois flags.
2. **Enumere as transitions e seus triggers.** Para cada state, quais events/conditions o deixam e para onde. Triggers vêm de input, AI, timers ou events do `GameEventBus` — não de outro sistema cutucando um field.
3. **Separe dados por-state de dados compartilhados.** Timers/counters por-state ficam com o state; o contexto compartilhado (stats, refs) é passado por parâmetro.
4. **Enter / Tick / Exit.** Cada state implementa side-effects de entrada, lógica por-step (recebe `deltaTime`) e cleanup. O cleanup no `Exit` previne timers/subscriptions vazados.
5. **Defina a invalid-transition policy.** Ignore + log, clamp para um state seguro, ou throw em dev (rule `error-handling-resilience`, categoria 4). Decida-a; não deixe implícita.
6. **Mantenha a machine pura.** A FSM é C# puro (sem `UnityEngine`); o `MonoBehaviour` alimenta nela input/`deltaTime` e reage aos domain events que ela emite (rule `gameplay-design-patterns`). É isso que a torna testável.
7. **Emita domain events, não chame presentation.** As mudanças de state publicam via `GameEventBus`; VFX/SFX/anim/HUD fazem subscribe (skill `game-feel-checklist`). A FSM nunca chama UI/audio diretamente.

## Saída esperada

- Lista de states + transition table (from → trigger → to).
- Interfaces/classes (`IState` com Enter/Tick/Exit, um pequeno driver `StateMachine`).
- Invalid-transition policy, declarada.
- Quais events a machine publica para presentation.
- EditMode tests (skill `editmode-test-authoring`): transitions válidas chegam ao state esperado; transitions inválidas seguem a policy; side-effects de Enter/Exit disparam uma vez; determinístico dados os mesmos inputs.

## Interação com save

Se o state sobrevive a um reload, persista um **stable state id** (string/enum), nunca o state object — resolva de volta para o state no load (rule `unity-architecture` save DTOs; skill `save-load-pattern`). Persista o outcome/qual-state, não os timers transientes por-tick, a menos que o design exija.
