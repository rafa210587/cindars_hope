# SPEC — Inimigos: Esquiva/Dash Reativo e Reposicionamento

> **Spec ID:** `fable_82_spec_enemy_reactive_evasion_runtime`
> **Status:** A implementar
> **Wave:** FABLE Batch 6
> **Priority:** P2
> **Type:** Runtime
> **Domain:** Cave / Enemy AI
> **Parallelizable:** NO (EnemyBrain lock)
> **Parallel group:** N/A
> **Can run with:** N/A
> **Must not run with:** F04, F05, F24, F74 (mesma cadeia EnemyBrain)
> **Repo lock scope:** `Enemy/EnemyBrain.cs`, helpers de evasão, `Combat/Data/EnemyMovementProfileSO.cs`
> **Depends on:** F24 (Leap/Blink/Charge + states), F04 (threat/pack), F02 (telegraph/postura do player)
> **Blocks:** F83 (ataques únicos + pathfinding constroem sobre a evasão)
> **Scope:** dar aos inimigos esquiva reativa (sidestep/hop ao detectar windup do player), dash de reposicionamento e melhor uso de leap/blink — capacidades data-driven por role (assassinos esquivam; brutes não).
> **Out of scope:** pathfinding/navmesh (F83); novos ataques (F83); dodge idêntico ao do player (é capacidade de inimigo, não a mecânica do player).

required_adrs: [ADR-0005]
required_game_rules: [cave_rules.md, combat_rules.md]

---

# /speckit.specify

## Contexto

O EnemyBrain (F24) já tem 13 states e movimentos especiais (Leap, Blink/PhaseShortBlink,
ChargeLine, Retreat). Mas os inimigos **não reagem** aos golpes do player: recebem tudo que
vem, sem esquiva ou reposicionamento defensivo. Não há "dodge" nem "dash reativo" de inimigo —
hoje só o player tem PlayerDashController/PlayerDodgeController. O design pede inimigos que
"apliquem dodges, dashs, pulos até o jogador" — ou seja, **reatividade** que crie duelos mais
vivos, modulada por arquétipo (um assassino do Véu esquiva; um golem não).

## Problema

Sem reatividade, o combate é unilateral: o player ataca em janelas previsíveis e o inimigo
nunca quebra ritmo. Isso achata a diferença entre arquétipos (assassino vs. bruto jogam
igual) e reduz a leitura tática. Adicionar reação sem gate por role tornaria todo inimigo
escorregadio (frustrante); adicionar sem telegraph quebraria o contrato de leitura do combate.

## Objetivo

Ao final desta spec, o EnemyBrain expõe três capacidades reativas **data-driven por
EnemyMovementProfileSO** (gate por role/flag, cooldown, custo de janela):
1. **ReactiveSidestep** — ao detectar o windup de ataque do player dentro de um raio, o
   inimigo dá um passo lateral/hop curto para fora da linha (probabilístico, com cooldown);
2. **RepositionDash** — dash curto para reganhar distância preferida ou flanquear (ranged/
   assassino/duelista), com telegraph mínimo;
3. **EvasiveLeapToPlayer** — melhor uso do Leap existente como aproximação agressiva
   (gap-closer) com arco legível.
Tudo determinístico (sem reroll fora de seed em conteúdo persistente), gated por role
(brutes/tanks/swarm não esquivam), com EditMode tests das decisões puras e stable-run intacto.

## Fontes obrigatórias lidas

```text
Assets/_Game/Scripts/Enemy/EnemyBrain.cs (states, TryLeap/TryBlink, Charge)
Assets/_Game/Scripts/Combat/Data/EnemyMovementProfileSO.cs (parâmetros por inimigo)
Assets/_Game/Scripts/Player/PlayerDashController.cs + PlayerDodgeController.cs (referência de feel, NÃO reusar como-é)
Assets/_Game/Scripts/Combat/... (telegraph/windup do player — como detectar)
.specs/a_implementar/fable/fable_24_spec_enemy_moves_elite_affixes_runtime.md
.specs/a_implementar/fable/fable_04_spec_enemy_threat_pack_coordination_runtime.md
.claude/rules/cave-stable-run.md ; .claude/rules/no-magic-balance-values.md
.claude/skills/enemy-ai-authoring/SKILL.md ; .claude/skills/game-feel-checklist/SKILL.md
.claude/skills/state-machine-design/SKILL.md
```

## Estado atual do repo

```text
Existe e NÃO recriar:
- EnemyBrain com TryLeap (lunge 3.2x, cooldown 3.5s), TryBlink (flank teleport), ChargeLine;
- EnemyMovementProfileSO (speed/detection/leash/preferred_distance);
- threat memory + pack (F04); telegraph system (F04); EliteAffix (F24);
- PlayerDash/Dodge controllers (referência de FEEL apenas).
Não existe:
- esquiva reativa de inimigo ao windup do player; dash de reposicionamento defensivo;
  gate de evasão por role; detecção do windup do player pelo brain.
Auditar Fase 0:
- como o brain pode saber que o player está em windup (evento/flag do player);
- helpers de steering reusáveis (sidestep usa o mesmo movimento direto?);
- onde colocar cooldown/probabilidade sem magic numbers (SO de balance).
```

## Engineering stories

```text
Como jogador, quero que um assassino do Véu desvie do meu golpe telegrafado e me flanqueie,
  para que duelos contra arquétipos ágeis sejam diferentes de bater num golem.
Como designer, quero ligar/desligar evasão por role e tunar cooldown/probabilidade em dados,
  para balancear sem recompilar.
Como jogador, quero que a esquiva seja LEGÍVEL (hop curto/telegraph), não teleporte injusto.
Como stable-run, quero que reações em runtime não persistam estado nem rerollem conteúdo.
```

## Escopo

```text
Inclui:
- EnemyMovementProfileSO += flags/params aditivos: CanReactiveEvade(bool), EvadeChance,
  EvadeCooldown, RepositionDashEnabled(bool), DashCooldown, GapCloserLeap(bool) — valores
  em SO de balance/profile (no-magic-balance-values), não literais no código;
- EnemyBrain: detecção do windup do player (assinar evento/flag existente; se inexistente,
  expor flag mínima no contrato do player sem novo sistema) → decisão ReactiveSidestep;
- RepositionDash state/branch: dash curto para preferred_distance/flanco com telegraph mínimo
  e cooldown; gate por role (ranged/caster/assassino/duelista);
- EvasiveLeapToPlayer: usar Leap existente como gap-closer com arco legível (reuso, não novo);
- gate por role: tanks/brutes/swarm/guard NÃO evadem (config default por role);
- helpers estáticos puros (decisão de evadir: em raio? em cooldown? rolou chance? role permite?)
  para EditMode;
- EditMode tests: decisão de evasão pura (raio/cooldown/chance/role), determinismo da decisão
  dado seed/estado, gate por role.
```

## Fora de escopo

```text
Não inclui:
- pathfinding/navmesh (F83); evasão usa steering direto/sidestep curto;
- novos ataques/ações (F83);
- replicar exatamente PlayerDash/Dodge (é capacidade de inimigo, parametrizada);
- persistência de estado de combate;
- VFX além do telegraph existente (F04);
- evasão de boss em fases completas (F05 orquestra; aqui capacidade base reusável).
```

## Regras de não duplicação

```text
Estender EnemyBrain único — NÃO criar segundo brain/controller de evasão.
Reusar Leap/Blink/steering existentes — sidestep/dash são branches, não sistema novo.
NÃO reusar PlayerDashController as-is (é do player) — parametrizar capacidade de inimigo.
Sem magic numbers — cooldown/chance/distância em SO (no-magic-balance-values).
Sem GUID/timestamp em decisão de conteúdo persistente (stable-run).
```

## Critérios de aceite

### CA-1 Esquiva reativa legível
- Inimigo com CanReactiveEvade dá sidestep/hop ao detectar windup do player em raio, com
  cooldown e probabilidade; o movimento é curto e legível (não teleporte).
- Evidência: EditMode test da decisão pura + cenário humano.

### CA-2 Dash de reposicionamento gated
- Ranged/caster/assassino/duelista usam RepositionDash para reganhar distância/flanco com
  telegraph mínimo; tanks/brutes/swarm/guard NÃO evadem nem dasham.
- Evidência: teste de gate por role + teste de cooldown.

### CA-3 Leap como gap-closer
- EvasiveLeapToPlayer usa o Leap existente como aproximação agressiva com arco legível
  (sem novo sistema de movimento).
- Evidência: teste de uso de Leap + revisão de reuso.

### CA-4 Data-driven e sem magic numbers
- Todas as constantes (chance/cooldown/distância) vivem em SO/profile; ligar/desligar por
  role é configuração.
- Evidência: grep sem literais de balance no código; teste lendo do profile.

### CA-5 Stable-run e regressão
- Replay validator PASS; os comportamentos existentes (F24) intactos; nenhum GameObject.Find;
  comunicação via GameEventBus.
- Evidência: replay validator + testes de regressão dos moves F24.

---

# /speckit.plan

## Arquitetura alvo

```text
Assets/_Game/Scripts/Enemy/
  EnemyBrain.cs                       (branches ReactiveSidestep/RepositionDash + gap-closer)
  EnemyEvasionDecision.cs             (NOVO — helpers estáticos puros testáveis)
Assets/_Game/Scripts/Combat/Data/
  EnemyMovementProfileSO.cs           (flags/params aditivos de evasão)
Assets/_Game/Data/Combat/             (SO de balance de evasão por role, se necessário)
Assets/_Game/Scripts/Core/Events/
  CombatEvents (PlayerAttackWindupEvent — aditivo, se ainda não existir)
Assets/_Game/Tests/EditMode/Cave/
  EnemyReactiveEvasionTests.cs        (NOVO)
docs/validation/
  fable_82_spec_enemy_reactive_evasion_runtime_execution_report.md
```

## Contratos

### Data contracts
`EnemyMovementProfileSO` += campos aditivos (CanReactiveEvade, EvadeChance, EvadeCooldown,
RepositionDashEnabled, DashCooldown, GapCloserLeap). Defaults por role em SO de balance.
Nenhum campo de save.

### Runtime contracts
`EnemyEvasionDecision.ShouldEvade(playerWindup, distance, cooldownReady, role, chanceRoll)` →
bool (puro). EnemyBrain consome a decisão e executa sidestep/dash/leap via steering existente.
Detecção do windup do player via evento do GameEventBus (PlayerAttackWindupEvent — aditivo se
não existir). Chance derivada de RNG seeded por sistema (rng-and-determinism) — sem reroll de
conteúdo persistente.

### Event contracts
`PlayerAttackWindupEvent` (aditivo) se o player ainda não publicar o início do windup; senão
reusar o existente. Nenhum evento alterado.

### Save contracts
N/A — estado de combate não persiste.

### UI contracts
Nenhuma (telegraph reusa F04).

## Sistemas afetados

```text
Enemy AI (EnemyBrain — núcleo) | Movement profile (campos) | Event bus (+1 evento se preciso)
Player combat (publica windup, se ainda não publica) | Validation reports
```

## Arquivos permitidos

```text
Assets/_Game/Scripts/Enemy/EnemyBrain.cs
Assets/_Game/Scripts/Enemy/EnemyEvasionDecision.cs (novo)
Assets/_Game/Scripts/Combat/Data/EnemyMovementProfileSO.cs (campos aditivos)
Assets/_Game/Scripts/Core/Events/CombatEvents (PlayerAttackWindupEvent aditivo, se necessário)
Assets/_Game/Data/Combat/** (SO de balance de evasão, se criado)
Assets/_Game/Tests/EditMode/Cave/**
docs/validation/** ; csproj includes
```

## Arquivos proibidos

```text
*.unity / *.prefab (edição manual)
Packages/** ; ProjectSettings/**
PlayerDashController/PlayerDodgeController (referência de feel; NÃO alterar)
Pathfinding/navmesh (F83) ; CanonicalBestiaryCatalog (F80) ; SaveManager
Telegraph system (F04 — consumir, não alterar)
```

## Estratégia de implementação

```md
### Fase 0 — Auditoria
Como o player sinaliza windup (evento/flag); steering reusável p/ sidestep/dash; onde
parametrizar (SO). Ler stable-run + game-feel-checklist.

### Fase 1 — Decisão pura + dados
EnemyEvasionDecision (helpers) + campos no profile + defaults por role. EditMode tests.

### Fase 2 — Esquiva reativa
ReactiveSidestep no brain (assina windup do player → hop curto legível, cooldown/chance).

### Fase 3 — Dash de reposição + gap-closer
RepositionDash (gated por role) + EvasiveLeapToPlayer (reuso do Leap). Telegraph mínimo.

### Fase 4 — Validação
Testes (decisão/gate/cooldown), replay, regressão dos moves F24; csproj; strict; report.
```

## Paralelização

- Parallelizable: NO — Must not run with F04/F05/F24/F74 (mesma cadeia EnemyBrain).
- Lock: EnemyBrain, EnemyMovementProfileSO.
- Reason: edição concorrente do brain causaria conflito direto.

## Impacto em save/load

```text
Changes save schema? NO | Adds section? NO | Migration? NO | Persists Unity refs? NO
Profile fields aditivos (save-safe). Estado de combate não persiste.
```

## Impacto em eventos

```text
Adds events: CONDITIONAL — PlayerAttackWindupEvent (aditivo) só se o player ainda não publica
Changes existing events: NO | Requires unsubscribe pattern: YES (brain assina/desassina windup)
```

## Impacto em UI/Unity

```text
Changes UI: NO | Changes scenes: NO | Changes prefabs: NO
Changes assets: SO de balance de evasão (se criado) — evidência
Requires Play Mode final validation: YES (sentir esquiva/dash em duelo)
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## Riscos técnicos

```text
Risco: inimigos escorregadios demais (frustrante). Mitigação: chance/cooldown em SO; gate por
  role; tuning no balance pass.
Risco: esquiva como teleporte injusto. Mitigação: sidestep curto + telegraph; cap de distância.
Risco: brain Update inchado. Mitigação: helpers puros + branches curtos (state-machine-design).
Risco: quebra de stable-run. Mitigação: reações não persistem; RNG seeded; replay validator.
Risco: player não publica windup. Mitigação: evento aditivo mínimo via GameEventBus.
```

## Rollback

```text
CanReactiveEvade/RepositionDashEnabled default false → comportamento idêntico ao atual.
Remover branches restaura F24. Profile fields aditivos são ignorados se não usados.
```

---

# /speckit.tasks

## Tasks

```md
- [ ] T001 — Auditar sinalização de windup do player, steering reusável, ponto de parametrização.
- [ ] T002 — EnemyEvasionDecision (puro) + campos no profile + defaults por role + tests.
- [ ] T003 — ReactiveSidestep no brain (assina windup → hop legível, cooldown/chance).
- [ ] T004 — RepositionDash (gated por role) + EvasiveLeapToPlayer (reuso Leap) + telegraph.
- [ ] T005 — Testes (decisão/gate/cooldown); replay; regressão F24; csproj; strict; report.
```

## Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\run_strict_validation.ps1
```

## Testing Quality Gate

- Changed deterministic logic: YES (decisão de evasão, gate por role, cooldown)
- Requires EditMode tests: YES (ShouldEvade puro, gate por role, cooldown, determinismo)
- Requires PlayMode/human scenario: YES (sentir esquiva/dash em duelo — lote final)
- Requires regression test: YES (moves F24 intactos + replay stable-run)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: testes + cenário humano (assassino esquiva, golem não)

## Definition of Done

```text
3 capacidades reativas data-driven (sidestep/dash/gap-closer leap), gated por role, sem magic
numbers; brain único estendido; telegraph legível; stable-run intacto (replay PASS); moves F24
intactos. Builds 0E; run_strict_validation exit 0; report criado.
```

## Anti-regressão

```text
Tanks/brutes/swarm/guard não evadem (gate default).
Os comportamentos F24 não mudam quando evasão está off.
Sem GameObject.Find; comunicação só via GameEventBus.
Sem GUID/timestamp em conteúdo persistente; mesma run → mesmo conteúdo (stable-run).
Telegraph obrigatório antes de dash/charge.
```
