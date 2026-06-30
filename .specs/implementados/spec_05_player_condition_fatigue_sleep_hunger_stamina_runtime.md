# SPEC — Player Condition Fatigue Sleep Hunger Stamina Runtime

> **Spec ID:** `05_spec_player_condition_fatigue_sleep_hunger_stamina_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Construction / Tools / Conditions  
> **Priority:** P0  
> **Type:** Runtime / Player / Condition / Fatigue / Sleep / Hunger / Stamina  
> **Domain:** Player / FatigueSystem / PlayerConditionManager / Sleep / Hunger / Stamina  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_CONSTRUCTION_TOOLS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere combat stamina formulas, HP/MP formulas, cave collapse/death, hunger manager, save schema, UI HUD, food/potion buffs, Água Viva, pets/companions rest buffs or day transition order.  
> **Repo lock scope:** `Assets/_Game/Scripts/Player/**`, `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Player/**`, `docs/validation/05_spec_player_condition_fatigue_sleep_hunger_stamina_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
> **Blocks:**  
  - farm tool action costs;
  - sleep/save/home loop;
  - cave exploration fatigue modifiers;
  - HUD player condition display;
  - food/potion/rest buffs.
> **Scope:** implementar/endurecer FatigueSystem como sistema próprio e PlayerConditionManager como agregador observável de HP/Stamina/Fome/Cansaço/MP future.  
> **Out of scope:** combat balance final, HP/MP formulas, detailed food/potion buffs, pets/companions rest buffs, Água Viva fatigue effects, UI art/prefabs.

---

# /speckit.specify

## 1. Contexto

Farm Direction define explicitamente que Cansaço deve ser implementado como sistema próprio, não absorvido pelo HungerManager e não como campo solto dentro do PlayerConditionManager. PlayerConditionManager deve continuar como camada agregadora/observável.

Esta spec fecha esse contrato porque quase todos os loops da fazenda dependem de ações físicas, stamina, fome, sono e cansaço.

---

## 2. Problema

Sem FatigueSystem/PlayerCondition claro:

```text
HungerManager pode virar dono de cansaço indevidamente;
stamina e cansaço podem ser confundidos;
ferramentas podem não ter desgaste acumulado;
dormir pode recuperar tudo sem regra;
ficar com fome pode não impactar rotina;
ações exausto podem continuar normais;
caverna pode ignorar fadiga;
save/load pode perder condição do jogador.
```

---

## 3. Objetivo

Criar/endurecer:

```text
FatigueSystem próprio;
PlayerConditionManager agregador;
Fatigue 0-100;
thresholds de cansaço;
natural fatigue gain over time;
fatigue from stamina spend;
hunger multiplier;
sleep quality/recovery;
exhaustion penalties;
events;
save/load safe state.
```

---

## 4. Regras de design

```text
HP = sobrevivência/dano.
Stamina = energia imediata para ações físicas.
Fome = manutenção do corpo ao longo do tempo.
Cansaço = desgaste acumulado do dia e necessidade de dormir.
MP = recurso mágico futuro/refinado no sistema de magia.
Dormir é principal recuperação de cansaço.
Comida ajuda indiretamente, mas não substitui sono.
Pets/companions podem dar buff futuro, mas não removem necessidade de dormir.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero sentir desgaste de um dia longo sem microgerenciamento excessivo.
Como ferramenta/farm action, quero gastar stamina e gerar cansaço proporcional.
Como HungerManager, quero afetar cansaço sem ser dono dele.
Como PlayerConditionManager, quero expor estado consolidado para UI/save/eventos.
Como save/load, quero persistir condição sem quebrar fluxo diário.
```

---

## 6. Escopo

Inclui:

```text
FatigueSystem;
Fatigue thresholds;
fatigue gain from time/stamina/hunger/cave/status hooks;
sleep recovery quality;
exhaustion penalties as hooks/flags;
PlayerConditionManager aggregation;
events;
save/load state;
tests.
```

Não inclui:

```text
combat final stamina balance;
HP/MP formulas;
food/potion buffs detailed;
pet/companion rest buffs;
Água Viva fatigue mechanics;
HUD final art.
```

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- .specs/SPEC_VALIDATION_MATRIX_MASTER.md
- .specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Cansaço deve ser sistema próprio: FatigueSystem/FatigueManager.
- FatigueSystem não deve ser absorvido por HungerManager nem ser só campo solto no PlayerConditionManager.
- PlayerConditionManager agrega/expõe HP, Stamina, Fome, Cansaço, MP future e estados derivados.
- Cansaço aumenta com tempo, gasto de stamina, fome, ações repetidas, tempo acordado, caverna e status negativos.
- Modelo recomendado: Cansaço 0–100, com thresholds Descansado, Levemente cansado, Cansado, Muito cansado, Exausto.
- Dormir é a principal forma de reduzir cansaço; dormir tarde pode recuperar menos.

### Deferred / future from directions

- Food/potion detailed buffs.
- Pets/companions rest buffs.
- Água Viva fatigue effects.
- Full combat stamina balance.
- MP integration final.
- HUD/prefab art final.

### Explicitly not redefined here

- HungerManager internals.
- Combat stamina formulas.
- Cave collapse/death mechanics.
- Food item effects database.
- Save schema migration.

## 7. Modelo de domínio

### 7.1 FatigueState

```text
FatigueValue 0..100
Threshold: Rested | SlightlyTired | Tired | VeryTired | Exhausted
LastUpdatedGameTime
SleepDebt optional
LastSleepQuality optional
Modifiers[]
```

### 7.2 FatigueGainContext

```text
Source: TimePassing | StaminaSpend | HungerLow | RepeatedPhysicalAction | CaveExploration | StatusEffect | MagicFuture | Debug
BaseAmount
Multiplier
GameTime
SceneContext
ActionId optional
```

### 7.3 SleepRecoveryContext

```text
SleepStartTime
SleepEndTime
SleepLocation
WentToBedLate
HungerStateAtSleep
Buffs[]
Debuffs[]
```

### 7.4 PlayerConditionSnapshot

```text
HP
Stamina
Hunger
Fatigue
MP optional/future
DerivedFlags: Exhausted, Hungry, LowStamina, LowHP, WellRested optional
```

---

## 8. Fatigue gain rules

```text
TimePassing:
  - increases slowly.

StaminaSpend:
  - increases proportional to stamina cost.

Low hunger:
  - increases fatigue gain by multiplier.

Cave:
  - applies higher fatigue gain than farm.

Repeated heavy farm actions:
  - may increase fatigue more than light actions.

Magic:
  - future hook only unless MP system exists.

Night/Nyx:
  - may provide special future modifier only if world/lore spec allows.
```

---

## 9. Threshold policy

```text
0-24 Rested:
  no penalty.

25-49 SlightlyTired:
  visual/feedback only or very light penalty.

50-74 Tired:
  stamina recovery worse; actions can cost slightly more.

75-89 VeryTired:
  movement/actions penalized; cave risk can increase.

90-100 Exhausted:
  heavy actions blocked or heavily penalized; sleep strongly recommended.
```

Penalties must be hooks/flags unless exact formulas already exist.

---

## 10. Sleep rules

```text
Sleeping in bed drastically reduces fatigue.
Sleeping earlier gives better recovery.
Sleeping late gives partial recovery/penalty.
Food may indirectly help but cannot replace sleep.
Potions/Água Viva/Fruto Mana future effects must be limited and gated.
Sleep recovery must run through day transition/sleep system once.
```

---

## 11. PlayerConditionManager responsibilities

```text
Expose consolidated state.
Publish condition changed events if event system exists.
Provide read-only snapshot to UI.
Coordinate save/load providers.
Not own detailed fatigue formulas.
Not absorb HungerManager.
```

---

## 12. Events

Candidate events:

```text
FatigueChangedEvent
FatigueThresholdReachedEvent
PlayerExhaustedEvent
PlayerRestedEvent
SleepQualityCalculatedEvent
PlayerConditionChangedEvent
```

Do not duplicate if event system already has equivalents.

---

## 13. Save/load

Must preserve:

```text
FatigueValue
FatigueThreshold or derived threshold
LastUpdatedGameTime if needed
SleepDebt optional
Active fatigue modifiers if persisted
PlayerCondition section reference
```

Must not persist:

```text
UI state;
temporary visual feedback;
scene object refs;
derived flags as only source of truth if recalculable.
```

If save section for player condition cannot be safely extended, STOP for save/migration spec.

---

## 14. Criteria

```text
FatigueSystem exists or is hardened as own system.
HungerManager does not own fatigue.
PlayerConditionManager exposes aggregated snapshot.
Stamina spending can add fatigue.
Low hunger increases fatigue gain.
Sleep reduces fatigue according to quality.
Exhausted threshold produces explicit flags/hooks.
Save/load preserves fatigue.
Tests cover gain, thresholds, hunger multiplier, sleep recovery and aggregation.
```

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Player/Conditions/FatigueSystem.cs
Assets/_Game/Scripts/Player/Conditions/FatigueState.cs
Assets/_Game/Scripts/Player/Conditions/FatigueThreshold.cs
Assets/_Game/Scripts/Player/Conditions/FatigueGainContext.cs
Assets/_Game/Scripts/Player/Conditions/SleepRecoveryCalculator.cs
Assets/_Game/Scripts/Player/Conditions/PlayerConditionSnapshot.cs
Assets/_Game/Tests/EditMode/Player/FatigueSystemTests.cs
```

Consolidar existentes se houver.

---

## 16. Arquivos permitidos

```text
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Player/**
docs/validation/05_spec_player_condition_fatigue_sleep_hunger_stamina_runtime_execution_report.md
```

---

## 17. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 18. Estratégia

```text
1. Auditar HungerManager/PlayerCondition/Fatigue/Stamina/Sleep existentes.
2. Consolidar FatigueSystem próprio.
3. Garantir PlayerConditionManager como agregador.
4. Implementar/harden fatigue gain and thresholds.
5. Implementar sleep recovery calculator.
6. Integrar save/load safely.
7. Criar tests.
8. Criar report.
```

---

## 19. Paralelização

- Parallelizable: NO
- Must not run with:
  - hunger manager;
  - combat stamina;
  - sleep/day transition;
  - player save schema;
  - HUD condition UI.
- Reason: condition state touches many systems.

---

## 20. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if player condition/fatigue exists; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding fatigue persistence; then STOP.
Does this persist Unity references? NO.
```

---

## 21. Impacto eventos

```text
Adds events: CONDITIONAL, if event contracts allow and duplicates do not exist.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 22. Impacto UI/Unity

```text
Changes UI: NO final; exposes snapshot for HUD.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for use tools, get tired, sleep, recover flow.
```

---

## 23. Riscos

```text
Risco: Fatigue absorbed by HungerManager.
Mitigação: architecture tests/audit.

Risco: fatigue blocks gameplay too harshly.
Mitigação: thresholds as hooks, balance deferred.

Risco: save schema missing.
Mitigação: STOP before migration.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar PlayerCondition/Hunger/Stamina/Sleep.
- [ ] T003 — Consolidar FatigueSystem próprio.
- [ ] T004 — Implementar gain/thresholds.
- [ ] T005 — Implementar sleep recovery calculator.
- [ ] T006 — Integrar PlayerConditionManager snapshot.
- [ ] T007 — Integrar save/load safely.
- [ ] T008 — Criar tests.
- [ ] T009 — Rodar validações.
- [ ] T010 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a player condition/fatigue/sleep/hunger/stamina foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de player condition/fatigue/sleep/hunger/stamina? | Arquivos alterados e justificativa. | PARTIAL |
| Economia | A spec não cria ouro infinito, bypass de custo ou upgrade grátis? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_player_condition_fatigue_sleep_hunger_stamina_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Fatigue|PlayerCondition|HungerManager|Stamina|Sleep|Exhausted|PlayerRested|FatigueChanged|SleepQuality" Assets/_Game/Scripts docs/design .specs
rg -n "Building|Construction|Workshop|Storage|Fertilizer|Tool|Upgrade|Fatigue|Sleep|Hunger|Stamina|PlayerCondition|Save|Economy|Inventory" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

A execução deve classificar cada achado como:

```text
EXISTING_CANONICAL
  Sistema já existe e deve ser reaproveitado/endurecido.

EXISTING_PARTIAL
  Sistema existe, mas precisa hardening/delta.

MISSING_SAFE_TO_CREATE
  Sistema não existe e criação é pequena, isolada e dentro do escopo.

MISSING_BUT_DEFER
  Sistema não existe, mas criação exigiria outro domínio/spec.

CONFLICT
  Há dois caminhos possíveis ou contrato divergente. Parar e reportar.
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given o sistema base relacionado a player condition/fatigue/sleep/hunger/stamina existe ou foi criado de forma mínima
When o jogador executa o fluxo principal desta spec
Then o comportamento segue o direction canônico
And save/load, economia, inventário e estado do jogador permanecem consistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 4 — Economy/save invalid state

```text
Given custo, material, upgrade, construção, condition ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não duplica item/ouro, não aplica upgrade grátis e não corrompe save.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de FarmScene/UI/gameplay
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Fatigue implementado dentro do HungerManager.
- Fatigue como campo solto sem sistema próprio.
- PlayerConditionManager duplicando fórmulas.
- Sleep recovery aplicado duas vezes.
- Stamina spending não aumenta fatigue.
- Low hunger não afeta fatigue.
- Exhausted sem flag/hook claro.
- Save/load perde fatigue.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Player Condition Fatigue Sleep Hunger Stamina Runtime

## Summary
- Spec:
- Branch:
- Executor:
- Date:
- Final status:

## Sources read
- ...

## Local audit
- Commands executed:
- Existing systems found:
- Existing partial systems found:
- Missing systems:
- Conflicts:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Missing dependency:
- Economy/save invalid state:
- Save/load safety:
- Visual/final scenario:

## Validation
- Docs validation:
- C# build:
- Unity compile:
- EditMode tests:
- PlayMode automated:
- Final human scenario:

## Testing Quality Gate
- Changed deterministic logic:
- Requires EditMode tests:
- Requires PlayMode automated or final human scenario:
- Requires regression test:
- Human validation timing:
- Minimum validation evidence for ACCEPTED:

## Residual risks
- ...

## Next specs impacted
- ...
```

---

## 23F. Stop Conditions

Parar a execução e registrar `BLOCKED` se ocorrer qualquer um destes casos:

```text
1. A implementação exigir alterar Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema sem migration spec.
4. A implementação exigir reescrever Farm/Economy/Inventory/PlayerCondition system canônico existente.
5. A implementação criar conflito com day transition, save ownership, stable IDs ou Testing Quality Gate.
6. A implementação criar pets/companions/animals runtime fora do escopo desta wave.
7. A implementação criar invasão/defesa/inimigos/dano a crops na fazenda, proibido no roadmap atual.
8. A implementação permitir ouro infinito, item duplication, upgrade grátis, custo negativo ou recovery exploit.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Condition Ownership Matrix

| Resource | Owner | Aggregated by PlayerConditionManager |
|---|---|---:|
| HP | HP/Health system | YES |
| Stamina | Stamina system | YES |
| Hunger | HungerManager | YES |
| Fatigue | FatigueSystem | YES |
| MP | Magic/MP future | YES when formalized |

## 23H. Fatigue Threshold Matrix

| Value | State | Behavior |
|---:|---|---|
| 0-24 | Rested | no penalty |
| 25-49 | SlightlyTired | feedback/light |
| 50-74 | Tired | stamina recovery/cost hook |
| 75-89 | VeryTired | movement/action/cave risk hook |
| 90-100 | Exhausted | heavy action block or severe penalty hook |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Building|Construction|Workshop|Storage|Fertilizer|Tool|Upgrade|Fatigue|Sleep|Hunger|Stamina|PlayerCondition|Save|Economy|Inventory" Assets/_Game/Scripts docs/design .specs
```

C# runtime/editor quando houver alteração C#:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile quando houver alteração Unity C#:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests quando lógica determinística for criada/alterada:

```text
Unity Test Runner — EditMode, ou comando local equivalente disponível no repo.
```

PlayMode/final human validation:

```text
Não pedir validação humana por spec.
Quando houver cenário visual/gameplay de construção, upgrade, fertilizante, ferramenta, sono/cansaço/fome/stamina, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, fatigue gain/threshold/sleep recovery logic is deterministic.
- Requires EditMode tests: YES for fatigue gain/threshold/hunger multiplier/sleep recovery/aggregation tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for use tool -> fatigue -> sleep recovery gameplay flow.
- Requires regression test: YES if fixing existing fatigue/hunger/stamina/sleep bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; FatigueSystem separate; PlayerCondition aggregate safe.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_player_condition_fatigue_sleep_hunger_stamina_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não persistir Unity references.
Não usar UI como fonte de verdade.
Não implementar pets/companions/animals runtime.
Não criar invasão/defesa/inimigos na fazenda.
Não criar item/ouro/upgrade infinito.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
