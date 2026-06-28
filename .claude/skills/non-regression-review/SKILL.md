---
name: non-regression-review
description: Audita o diff de uma implementação em busca de violações arquiteturais e riscos de regressão. Usar após implementar uma spec, antes do closeout, ou sempre que scope/rules mudaram.
---

# Skill: Non-Regression Review

Auditar mudanças antes de fechar uma spec. Produz um report `PASS / WARNING / FAIL` com evidência.

## Quando usar

- Após a implementação de qualquer spec de runtime/código.
- Antes do closeout (`/finish-spec`, `implementation-closeout`).
- Quando scope ou rules mudaram durante a implementação.

## Quando NÃO usar

- Specs docs-only ou asset-only sem mudança de código → pular; usar o checklist de docs governance.
- Como substituto de `run_strict_validation.ps1` — esta skill é auditoria de padrões, não de build.

---

## Checklist de auditoria (9 dimensões)

**1. File & Directory**
```
[ ] Nenhum diretório specs/ ou spec/ criado na raiz
[ ] Nenhuma edição em docs_old/**
[ ] Mudanças dentro do scope declarado pela spec
```

**2. Git Safety**
```
[ ] Nenhum git push, reset --hard, clean, stash executado sem autorização
[ ] Branch e commits esperados (rule: no-unsafe-git)
```

**3. Runtime / Forbidden APIs** (grep nos arquivos mudados)
```
[ ] Sem GameObject.Find / FindObjectOfType / FindObjectsByType em código novo
[ ] Sem chamadas diretas cross-system (ex.: enemy.TakeDamage() de fora do combat)
[ ] Toda comunicação de gameplay via GameEventBus.Publish()
```

**4. Save DTO Safety**
```
[ ] Nenhum campo Unity ref em save DTOs: sem ScriptableObject, Transform, MonoBehaviour, Sprite
[ ] Save usa apenas int, string, float, bool, enum, IDs
```

**5. Balance Values**
```
[ ] Sem literais numéricos de balance inline em métodos (rule: no-magic-balance-values)
[ ] Thresholds/custos em SO de balance ou const nomeada
```

**6. Event Bus**
```
[ ] Todo Subscribe tem Unsubscribe correspondente em OnDisable/OnDestroy
[ ] Eventos carregam IDs/primitivos — sem refs Unity
[ ] Naming: [Noun][Verb]Event
```

**7. Namespaces**
```
[ ] Nenhum namespace CindarsHope.Debug criado
```

**8. Integridade de status**
```
[ ] Spec não marcada ACCEPTED/PLAYMODE_VALIDATED sem evidência
[ ] Nenhum claim de "100% fulfilled" sem evidência no repo
```

**9. Testing Quality Gate** (rule: testing-quality-gate)
```
[ ] Mudança de lógica determinística tem EditMode tests ou justificativa
[ ] Mudança de UI/scene tem human Play Mode scenario ou justificativa
```

---

## Formato de report

```text
Non-Regression Review
─────────────────────
Spec: <id>
Arquivos mudados: <N> (.cs), <N> (docs)

File & Directory:  PASS / FAIL — <detalhe>
Git Safety:        PASS / FAIL
Runtime APIs:      PASS / FAIL — <grep result ou "nenhuma ocorrência nova">
Save DTOs:         PASS / N/A
Balance Values:    PASS / FAIL — <arquivo:linha se falhou>
Event Bus:         PASS / N/A
Namespaces:        PASS
Status claims:     PASS / FAIL
Testing QG:        PASS / JUSTIFIED / FAIL — <justificativa ou caminho do scenario>

Status geral: PASS | WARNING | FAIL

Issues encontrados:
  (lista ou "nenhum")

Ações corretivas:
  (lista ou "nenhuma")

Risco residual:
  (texto explícito)
```

**FAIL bloqueia closeout.** WARNING documenta risco residual e deve ser revisado pelo humano.

## Relacionados

- `(skill: implementation-closeout)` — chama esta auditoria como pré-requisito
- `(rule: unity-architecture)` — items 3, 4, 6
- `(rule: no-magic-balance-values)` — item 5
- `(rule: testing-quality-gate)` — item 9
- `(rule: validation-truth)` — item 8

---

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

Estilo é revisado, não hard-gated: `/code-review`, `/review-non-regression`, e o agent `architecture-reviewer`. O hook `runtime-code-guard` já bloqueia o subconjunto arquitetural (search APIs, forbidden namespaces, classes duplicadas).

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

## (movido de rules/gameplay-design-patterns.md) Seleção de Design Pattern de Gameplay

Orientação de *seleção* de pattern. Os invariantes duros (GameEventBus para comms de gameplay, no runtime global search, save DTOs) estão em `unity-architecture.md`; esta seção diz **qual pattern buscar**, mapeado para sistemas que já existem aqui, para que código novo combine com o house style em vez de inventar um paralelo.

### Princípio central: domain logic em C# puro, engine como adapter

Mantenha regras de gameplay tuneáveis/testáveis em C# puro (sem dependência de `UnityEngine`) para que sejam EditMode-testable; deixe o `MonoBehaviour` adaptar input, scene, physics, prefabs e UI. A convenção de projection/ViewModel do projeto (skill `ui-projection-pattern`) é o exemplo canônico — copie-a para novas telas e sistemas, e veja a skill `monobehaviour-decomposition` para extrair lógica de um god-`MonoBehaviour`.

### Pattern → quando → precedente no projeto

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
| **Null Object** | um default seguro vence um null-check em cada call site | fallback de categoria 2 na seção Error Handling acima |
| **Flyweight / Data Asset** | dados compartilhados, tuneados pelo designer | ScriptableObjects (skill `data-catalog-authoring`) |

### Anti-patterns a sinalizar

- Um `Manager`/`Controller` genérico acumulando input + rule + UI + audio + save + animation → divida (skill `monobehaviour-decomposition`).
- `switch` num type enum que cresce toda vez que content é adicionado → Strategy ou polimorfismo.
- Herança profunda para variação de capability → Component/composition.
- Um novo global singleton para compartilhar estado que uma dependência explícita ou um event do `GameEventBus` carregaria.

### Enforcement

Revisado pelo agent `architecture-reviewer` (pre-wave / post-integration) e pela skill `non-regression-review`. Rode a skill `system-reuse-audit` antes de criar qualquer novo manager/service/SO type para que um "novo pattern" não vire um sistema duplicado.

---

## (movido de rules/no-magic-balance-values.md) Sem Magic Values de Balance

Valores tuneáveis de gameplay (dano, custo, duração, threshold, percentagem) devem viver em ScriptableObjects de balance ou constants nomeadas — nunca inline em métodos ou condicionais.

### O que é um magic balance value

Qualquer literal numérico que determina comportamento de gameplay:
- Thresholds (20% de fome crítica, 0 de HP)
- Custos (250g de respec, 10 de stamina por dash)
- Duração (2.5s de cooldown, 3s de toast)
- Dano (15 HP/s de fome zero, 25 de dano base)
- Escalas e multipliers (1.15x bônus de acessório)
- Horas do dia (02:00 de colapso por fadiga)

### Precedentes canônicos — copiar, não reinventar

| Precedente | Sistema | Tipo |
|---|---|---|
| `PlayerNeedsBalanceSO` | `BaseStaminaRegenRate`, `ZeroHungerDamagePerSecond`, `CriticalHungerThreshold` | SO de balance |
| `SkillTierRules.TierPointThresholds` | Pontos gastos para desbloquear tier 2, 3... | const array |
| `SkillRespecService.DefaultRespecCostGold = 250` | Custo de respec | const de classe |
| `DerivedFollowupFormulas.CraftTimeMultiplier` | Multiplicador de crafting | método puro configurável |
| `FatigueSystem._collapseHour` | Hora de colapso | `[SerializeField]` |

### Regra

```csharp
// ERRADO — magic number inline
if (hunger < 20) PublishCriticalEvent();
yield return new WaitForSeconds(3.5f);
if (damage > 150) TriggerKnockback();

// CORRETO — threshold nomeado em SO ou const
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
| Threshold de regra de domínio | `const` no serviço responsável pela regra |

### O que NÃO é magic value

- `0` e `1` em comparações de flag/bool (`if (count == 0)`)
- Índices de array bem documentados (`[0]` = slot primário)
- IDs de string — cobertos pela rule `id-stability`
- Contagem de items numa lista fixa com contexto óbvio

### Enforcement

Revisional — `/code-review`, `architecture-reviewer`, e `non-regression-review`. Sem hook mecânico (falsos positivos altos para literais numéricas genéricas). O hook `runtime-code-guard` não cobre este padrão.
