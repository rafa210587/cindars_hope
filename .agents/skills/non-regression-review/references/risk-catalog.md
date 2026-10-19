# C# and gameplay risk catalog

## (movido de rules/csharp-style.md) Estilo & Craft em C#

Convenções gerais de craft em C# para runtime code. Esta seção trata de *como uma única classe/método se lê*; os invariantes arquiteturais (no global search, GameEventBus, save DTOs, forbidden namespaces) ficam em `unity-architecture.md` e não são repetidos aqui.

### Naming & shape

- Nomes orientados ao domínio, explícitos. Nada de catch-alls `Manager`/`Helper`/`Utils` quando existe um substantivo de domínio.
- Classes pequenas, uma responsabilidade clara por método. Se um método precisa de um comentário de seção para explicar um bloco, esse bloco normalmente é um método.
- `readonly` para dependências que nunca mudam após a construção; prefira `private set`/`init` a propriedades públicas mutáveis sem motivo.
- Novos tipos `sealed` por padrão; abertos para herança só quando uma relação base/subtipo é intencional e documentada.

### Sinalização de falha (use a convenção já existente no projeto — não invente uma paralela)

- **Falhas de gameplay esperadas** (cooldown ativo, target inválido, sem stamina, item ausente, save ausente): retorne um `bool` de um método `TryX(...)` e/ou exponha uma string `FailureReason` via um HUD event do `GameEventBus` — nunca um no-op silencioso, nunca uma exception como control flow. (Ver game-feel-checklist: recusas devem expor um motivo.)
- Antes de introduzir um wrapper `Result<T>`/`Either`/`Outcome`, rode a skill `system-reuse-audit` — o projeto já sinaliza falhas com `bool` + `FailureReason`; um wrapper type concorrente é um sistema paralelo que o ethos do `runtime-code-guard` proíbe.
- **Invariantes quebrados** (dependência obrigatória null, estado impossível, save version não suportada): throw ou assert — fail fast. Ver a seção Error Handling & Resiliência abaixo para a taxonomia completa.

### Collections & imutabilidade

- Exponha `IReadOnlyList<T>` / `IReadOnlyCollection<T>` quando o caller não pode mutar.
- Prefira `TryGet...(out var x)` a métodos que retornam `null` e forçam o caller a checar null.
- Não distribua referências a listas mutáveis internas; copie ou faça wrap.

### Async & consciência de custo por frame

- Sem `async void`, exceto event handlers de framework/Unity que o exijam.
- Sem idiomas com allocation pesado (LINQ, closures capturando locals, boxing, string interpolation por frame) em `Update`/`FixedUpdate`/`LateUpdate`/callbacks de colisão ou loops por entidade. O detalhe de hot path e o checklist autoritativo ficam com o agent `performance-auditor` e a skill `object-pooling-pattern`.
- Sem `DateTime.Now` / `UnityEngine.Random` (estado global) em lógica que precisa ser determinística ou salva — use `System.Random` seeded por sistema (skill `rng-and-determinism`).

### Enforcement

Style is reviewed through `/review-non-regression` and `architecture-reviewer`. The PostToolUse
`runtime-code-guard` reports a subset (search APIs, namespaces, duplicate names); it does not
undo an edit. File presence does not prove host hook execution.

---

## (movido de rules/error-handling-resilience.md) Error Handling & Resiliência

Classifique toda falha antes de tratá-la. A estratégia de tratamento é ditada pela categoria, e as categorias mapeiam diretamente para sistemas que já existem neste projeto.

### As quatro categorias

| # | Categoria | Exemplos | Tratamento neste projeto |
|---|----------|----------|--------------------------|
| 1 | **Gameplay esperado** | ação inválida, cooldown, sem stamina, item ausente, target fora de alcance | `bool TryX` + string `FailureReason` exposta via um HUD event do `GameEventBus` (skill `game-feel-checklist`). Nunca uma exception, nunca um no-op silencioso. |
| 2 | **Config / asset / content** | prefab ausente, item sem icon, quest aponta para id inválido, campo de SO não preenchido | Capture em **editor/build time** com um validator (pattern de 60 validators, skill `editor-validator-authoring`); em runtime aplique um fallback seguro e logado. Corrupção de config obrigatória faz fail fast em dev. |
| 3 | **Infraestrutura** | escrita de save falhou, arquivo corrompido/ausente no load | Atomic save flow + validação pós-load + recovery (skill `save-load-pattern`). Nunca sobrescreva um save bom antes de o novo validar. |
| 4 | **Bug / invariante quebrado** | dependência obrigatória null, estado impossível, save version não suportada | **Fail fast** em desenvolvimento (throw/assert) com um log de wiring claro; não mascare com um scene search ou uma exception engolida (rule `unity-architecture`). |

### Logs precisam carregar contexto

Uma linha de log precisa permitir que um humano localize a falha sem um debugger. Inclua: sistema, entity/id, scene/level, operação, estado relevante, e o fallback aplicado (se houver). Proíba linhas peladas como `"Error loading data"`. Para referências obrigatórias ausentes, logue scene + GameObject + component + missing field + affected id (rule `unity-architecture`).

### Desenvolvimento vs. shipped build

- **Desenvolvimento:** falhe cedo para expor bugs (categoria 4 dá throw/assert).
- **Shipped:** recupere onde categoria 1–3 permite, mas sempre logue com contexto.
- **Nunca** esconda um erro corrompendo estado silenciosamente (ex.: escrevendo um save parcial, continuando com um null que vai dar NRE três frames depois).

### O que nunca acontece

- Exception usada como control flow normal de gameplay (isso é categoria 1 → `bool`/`FailureReason`).
- `catch { }` que engole e continua (categoria 4 precisa fail fast; 2–3 precisam log + fallback).
- Um "fallback" que produz um estado inválido em vez de um default seguro (prefira Null Object / default seguro).

### Enforcement

Revisado pela skill `non-regression-review`, `/code-review`, e o agent `bugfix-investigator` (que também exige um regression test conforme a rule `testing-quality-gate`). Sem hook mecânico — esta é uma lente de review.

---

## Gameplay pattern selection

Orientação de *seleção* de pattern. Os invariantes duros (GameEventBus para comms de gameplay, no runtime global search, save DTOs) estão em `unity-architecture.md`; esta seção diz **qual pattern buscar**, mapeado para sistemas que já existem aqui, para que código novo combine com o house style em vez de inventar um paralelo.

### Princípio central: domain logic em C# puro, engine como adapter

Mantenha regras de gameplay tuneáveis/testáveis em C# puro (sem dependência de `UnityEngine`) para que sejam EditMode-testable; deixe o `MonoBehaviour` adaptar input, scene, physics, prefabs e UI. A convenção de projection/ViewModel do projeto (skill `ui-projection-pattern`) é o exemplo canônico — copie-a para novas telas e sistemas, e veja a skill `monobehaviour-decomposition` para extrair lógica de um god-`MonoBehaviour`.

### Pattern → quando → precedente no projeto

| Pattern | Busque quando | Precedente / skill |
|---------|-------------------|-------------------|
| **State Machine** | muitos states/transitions/phases ou boolean flags conflitantes (player, enemy, boss, UI flow, game flow) | `BossPhaseLogic`; skill `state-machine-design` |
| **Strategy** | a regra muda por type/config (reward grant, pricing, drop selection) | reward strategy em scene interactables; skill `scene-interactable-wiring` |
| **Command** | uma ação pode vir de input, AI, replay ou uma queue | input/action handling |
| **Event Bus (Observer)** | sistemas diferentes reagem ao mesmo fato de domínio | `GameEventBus` (rule `unity-architecture`; skill `event-bus-pattern`) — **mandatory** para comms de gameplay, não opcional |
| **Factory** | a criação precisa de prefab/config/deps/pooling | `ProceduralSfxFactory`, spawn services; skill `bootstrap-wiring` |
| **Object Pool** | Measured spawn/despawn cost justifies reuse and reset complexity | Inspect the current spawn service; skill `object-pooling-pattern` |
| **Adapter / Ports** | isolar engine, save, audio, analytics do domínio | projections; skill `runtime-bootstrap-pattern` |
| **Decorator / Modifier** | buffs/debuffs/affixes componíveis | `StatusEffectDatabase`; skills `enemy-ai-authoring`, `ability-effect-composition` |
| **Null Object** | um default seguro vence um null-check em cada call site | fallback de categoria 2 na seção Error Handling acima |
| **Flyweight / Data Asset** | dados compartilhados, tuneados pelo designer | ScriptableObjects (skill `data-catalog-authoring`) |

### Contract evidence, only for the chosen pattern

| Pattern/boundary | Property to verify |
|---|---|
| State | Legal/illegal transitions, cancellation and enter/exit once; enum/table is sufficient until polymorphism solves real variation. |
| Strategy/ports | Same preconditions, outcomes and failure semantics for substitutes; domain does not pull concrete services from a locator. |
| Command/transaction | Input intent validated by one owner; commit atomically, retry safely where required, publish facts after commit. |
| Observer/bus | Request differs from completed fact; no order-dependent mutation, duplicate subscriptions or reentrant feedback loop. |
| Factory/pool | Fully configured before activation; reset/dispose on reuse/unload; measured benefit for pooling. |
| Data asset/projection | Shared authored data stays separate from per-instance mutable state; view projects state and sends intent. |

Patterns are options with costs, not a checklist to instantiate. For runtime boundaries,
record owner, dependencies, lifetime and failure behavior in the existing plan before coding.

### Primary references (consult only the relevant topic)

- [Unity design patterns and SOLID](https://unity.com/blog/game-programming-patterns-update-ebook): concrete Unity examples, not a mandate to adopt every pattern.
- [Component tradeoffs](https://gameprogrammingpatterns.com/component.html): decoupling adds complexity; prefer existing seams.
- [Unity State pattern](https://learn.unity.com/course/design-patterns/tutorial/develop-a-modular-flexible-codebase-with-the-state-programming-pattern): use extra structure when state complexity warrants it.
- [Unity assembly references](https://docs.unity3d.com/6000.0/Documentation/Manual/assembly-definitions-referencing.html): compiler boundaries apply to assemblies, not every domain inside one assembly.

External guidance supports decisions; project invariants and approved contracts govern implementation.

### Anti-patterns a sinalizar

- Um `Manager`/`Controller` genérico acumulando input + rule + UI + audio + save + animation → divida (skill `monobehaviour-decomposition`).
- `switch` num type enum que cresce toda vez que content é adicionado → Strategy ou polimorfismo.
- Herança profunda para variação de capability → Component/composition.
- Um novo global singleton para compartilhar estado que uma dependência explícita ou um event do `GameEventBus` carregaria.

### Enforcement

Revisado pelo agent `architecture-reviewer` (pre-wave / post-integration) e pela skill `non-regression-review`. Rode a skill `system-reuse-audit` antes de criar qualquer novo manager/service/SO type para que um "novo pattern" não vire um sistema duplicado.

---

## (movido de rules/no-magic-balance-values.md) Sem Magic Values de Balance

Tunable gameplay/content values belong in existing balance ScriptableObjects; named constants
are reserved for fixed technical invariants. Preserve legacy behavior until an authorized migration.

### O que é um magic balance value

Qualquer literal numérico que determina comportamento de gameplay:
- Thresholds (20% de fome crítica, 0 de HP)
- Custos (250g de respec, 10 de stamina por dash)
- Duração (2.5s de cooldown, 3s de toast)
- Dano (15 HP/s de fome zero, 25 de dano base)
- Escalas e multipliers (1.15x bônus de acessório)
- Horas do dia (02:00 de colapso por fadiga)

### Existing examples to inspect, not blanket permission to copy

| Precedente | Sistema | Tipo |
|---|---|---|
| `PlayerNeedsBalanceSO` | `BaseStaminaRegenRate`, `ZeroHungerDamagePerSecond`, `CriticalHungerThreshold` | SO de balance |
| `SkillTierRules.TierPointThresholds` | Pontos gastos para desbloquear tier 2, 3... | const array |
| `SkillRespecService.DefaultRespecCostGold = 250` | Custo de respec | const de classe |
| `DerivedFollowupFormulas.CraftTimeMultiplier` | Multiplicador de crafting | método puro configurável |
| `FatigueSystem._collapseHour` | Hora de colapso | `[SerializeField]` |

Verify these paths/values in current code. Legacy constants/serialized fields in this table
are discovery hints, not authorization to hardcode new balance data or to migrate outside scope.

### Regra

```csharp
// ERRADO — magic number inline
if (hunger < 20) PublishCriticalEvent();
yield return new WaitForSeconds(3.5f);
if (damage > 150) TriggerKnockback();

// CORRETO — threshold de gameplay vindo da configuração
if (hunger < _balance.CriticalHungerThreshold) PublishCriticalEvent();
yield return new WaitForSeconds(_config.ToastDurationSeconds);
if (damage > _combat.KnockbackThreshold) TriggerKnockback();
```

### Onde colocar o valor

| Tipo de valor | Onde |
|---|---|
| Tuneável em runtime pelo designer | `[SerializeField]` num SO de balance dedicado |
| Técnico fixo (não muda entre builds) | `private const float X = ...` no topo da classe |
| Compartilhado entre sistemas | SO de balance dedicado (ex.: `PlayerNeedsBalanceSO`) |
| Domain threshold | Gameplay balance/content belongs in an existing SO; only a fixed technical invariant belongs in a named constant. |

### O que NÃO é magic value

- `0` e `1` em comparações de flag/bool (`if (count == 0)`)
- Índices de array bem documentados (`[0]` = slot primário)
- IDs de string — cobertos pela rule `id-stability`
- Contagem de items numa lista fixa com contexto óbvio

### Enforcement

Revisional — `/code-review`, `architecture-reviewer`, e `non-regression-review`. Sem hook mecânico (falsos positivos altos para literais numéricas genéricas). O hook `runtime-code-guard` não cobre este padrão.
