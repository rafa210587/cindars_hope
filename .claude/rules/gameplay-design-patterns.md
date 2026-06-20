# Rule: Seleção de Design Pattern de Gameplay

Orientação de *seleção* de pattern. Os invariantes duros (GameEventBus para comms de gameplay, no runtime global search, save DTOs) estão em [unity-architecture.md](./unity-architecture.md); esta rule diz **qual pattern buscar**, mapeado para sistemas que já existem aqui, para que código novo combine com o house style em vez de inventar um paralelo.

## Princípio central: domain logic em C# puro, engine como adapter

Mantenha regras de gameplay tuneáveis/testáveis em C# puro (sem dependência de `UnityEngine`) para que sejam EditMode-testable; deixe o `MonoBehaviour` adaptar input, scene, physics, prefabs e UI. A convenção de projection/ViewModel do projeto (skill `ui-projection-pattern`) é o exemplo canônico — copie-a para novas telas e sistemas, e veja a skill `monobehaviour-decomposition` para extrair lógica de um god-`MonoBehaviour`.

## Pattern → quando → precedente no projeto

| Pattern | Busque quando | Precedente / skill |
|---------|-------------------|-------------------|
| **State Machine** | muitos states/transitions/phases ou boolean flags conflitantes (player, enemy, boss, UI flow, game flow) | `BossPhaseLogic`; skill `state-machine-design` |
| **Strategy** | a regra muda por type/config (reward grant, pricing, drop selection) | reward strategy em scene interactables; skill `scene-interactable-wiring` |
| **Command** | uma ação pode vir de input, AI, replay ou uma queue | input/action handling |
| **Event Bus (Observer)** | sistemas diferentes reagem ao mesmo fato de domínio | `GameEventBus` (rule `event-bus-only-gameplay-communication`; skill `event-bus-pattern`) — **mandatory** para comms de gameplay, não opcional |
| **Factory** | a criação precisa de prefab/config/deps/pooling | `ProceduralSfxFactory`, spawn services; skill `bootstrap-wiring` |
| **Object Pool** | spawn/despawn frequente (projectiles, floating text, drops, wave enemies, SFX) | skill `object-pooling-pattern` (baseline do projeto: sem pooling ainda) |
| **Adapter / Ports** | isolar engine, save, audio, analytics do domínio | projections; skill `runtime-bootstrap-pattern` |
| **Decorator / Modifier** | buffs/debuffs/affixes componíveis | `StatusEffectDatabase`; skills `enemy-ai-authoring`, `ability-effect-composition` |
| **Null Object** | um default seguro vence um null-check em cada call site | fallback de categoria 2 na rule `error-handling-resilience` |
| **Flyweight / Data Asset** | dados compartilhados, tuneados pelo designer | ScriptableObjects (rule `data-driven-content`; skill `data-catalog-authoring`) |

## Anti-patterns a sinalizar

- Um `Manager`/`Controller` genérico acumulando input + rule + UI + audio + save + animation → divida (skill `monobehaviour-decomposition`).
- `switch` num type enum que cresce toda vez que content é adicionado → Strategy ou polimorfismo.
- Herança profunda para variação de capability → Component/composition.
- Um novo global singleton para compartilhar estado que uma dependência explícita ou um event do `GameEventBus` carregaria.

## Enforcement

Revisado pelo agent `architecture-reviewer` (pre-wave / post-integration) e pela skill `non-regression-review`. Rode a skill `system-reuse-audit` antes de criar qualquer novo manager/service/SO type para que um "novo pattern" não vire um sistema duplicado.
