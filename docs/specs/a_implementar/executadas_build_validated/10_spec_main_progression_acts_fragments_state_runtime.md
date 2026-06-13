# SPEC — Main Progression Acts Fragments State Runtime

> **Spec ID:** `10_spec_main_progression_acts_fragments_state_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 10 — Main Progression / Fonte / Endgame  
> **Priority:** P0  
> **Type:** Runtime / Save Load / Main Progression / Acts / Fragments  
> **Domain:** MainProgression / Acts / Fragment States / Story Gates / Anti-spoiler  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_10_MAIN_PROGRESSION_FONTE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere FonteAnyaSection, QuestStateSection, final choice, level 100/101 gates, cave boss gates, quest content data, save migration ou UI final.  
> **Repo lock scope:** `Assets/_Game/Scripts/MainProgression/**`, `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/MainProgression/**`, `docs/validation/10_spec_main_progression_acts_fragments_state_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
> **Blocks:**  
  - FonteAnya function unlocks;
  - main quest projections;
  - fragment reward hooks;
  - city/cave/farm story gates;
  - level 100/101 gates;
  - endgame final choice.
> **Scope:** definir/endurecer MainProgressionSection, atos, fragmentos Água/Memória/Vida/Esperança, story gates e staging sem implementar Fonte functions ou finais concretos.  
> **Out of scope:** FonteAnya implementation, final choice resolution, boss gate implementation, quest content full data, UI final, save migration.

---

# /speckit.specify

## 1. Contexto

A main quest usa quatro atos: Ato 1 — Fonte e Esquecimento; Ato 2 — Cindar e o Arco da Memória; Ato 3 — Culto, Pedra Negra e Vida; Ato 4 — Nível 100/101 e Esperança. A progressão é híbrida: fazenda, cidade, caverna, Fonte, NPCs, luas, fragmentos, bosses e eventos de memória.

Esta spec cria o estado macro de progressão. Fonte de Anya, Arco da Memória e final choice ficam em specs próprias do batch.

---

## 2. Problema

Sem MainProgressionSection:

```text
atos podem virar QuestFlags soltas;
Fragmento da Água/Memória/Vida/Esperança pode ser concedido fora de ordem;
FonteAnya pode virar parte do QuestState;
nível 101 pode ser liberado por cave depth sem story gate;
Vaelrion/Sethra/Arquivista podem aparecer cedo;
Anya pode ser restaurada por erro de reward;
main quest pode expirar por tempo ou festival;
fragment effects podem duplicar após reload.
```

---

## 3. Objetivo

Criar/endurecer:

```text
MainProgressionSection;
MainAct enum/state;
MainFragment enum/state;
FragmentAcquisitionState;
MainStoryGateState;
MainProgressionValidator;
StorySpoilerStage;
Act transition rules;
Fragment order rules;
integration hooks to Quest/Fonte/Cave/City/Farm.
```

---

## 4. Regras de design

```text
A main quest não é apenas descer até o nível 100.
Cidade e fazenda importam em marcos narrativos.
Ordem canônica dos fragmentos: Água -> Memória -> Vida -> Esperança.
Anya não pode ser restaurada completamente.
Main quest não expira por calendário.
Romance/companions não são obrigatórios para main quest.
Eventos graves só avançam por marco de quest.
```

---

## 5. User stories / engineering stories

```text
Como main quest, quero persistir ato, fragmentos e story gates fora de QuestState genérico.
Como Quest system, quero consultar MainProgression como fonte de verdade.
Como FonteAnya, quero receber fragment unlocks sem ser proprietária da main progression.
Como UI, quero projetar o ato atual sem spoilers.
Como cave/city/farm, quero gates narrativos estáveis.
```

---

## 6. Escopo

Inclui:

```text
MainProgressionSection contract;
act enum/state;
fragment state;
story gates;
spoiler stage;
transition validator;
save/load guardrails;
integration event names/hooks;
tests.
```

Não inclui:

```text
Fonte function unlock runtime;
final choice runtime;
boss gate implementation;
level 101 scene/procedural runtime;
quest data content;
UI final.
```

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
- docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md

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

- Main quest usa quatro atos: Fonte e Esquecimento, Cindar e Arco da Memória, Culto/Pedra Negra/Vida, Nível 100/101 e Esperança.
- Progressão é híbrida: fazenda, cidade, caverna, Fonte, NPCs, luas, fragmentos, bosses e eventos de memória.
- Fragmentos principais são Água, Memória, Vida e Esperança.
- Ordem canônica da main quest é Água -> Memória -> Vida -> Esperança.
- Cada fragmento deve ter local de descoberta, NPC/sistema que puxa quest, ameaça, loop, função desbloqueada na Fonte, mini-revelação, efeito visual e mudança no mundo.
- Não fazer: restaurar Anya completamente, transformar Anya em NPC comum, fazer Mana virar dinheiro infinito, fazer caverna substituir cidade/fazenda, liberar tudo cedo pela Fonte, romance/companions obrigatórios para main quest.

### Deferred / future from directions

- FonteAnyaSection details.
- Fragment visual effects implementation.
- Final choice/endings.
- Concrete questlines/dialogues.
- Level 100/101 runtime.
- Mana crop runtime.
- Boss fights.

### Explicitly not redefined here

- QuestStateSection.
- QuestFlag registry.
- FonteAnyaSection.
- CaveRun/BossGate systems.
- World lunar runtime.
- UI projection.

## 7. Modelo de domínio

### 7.1 MainAct

```text
None
Act1_FonteAndForgetfulness
Act2_CindarAndMemoryArc
Act3_CultBlackStoneAndLife
Act4_Level100101AndHope
PostGame
```

### 7.2 MainFragmentType

```text
Water
Memory
Life
Hope
```

### 7.3 FragmentAcquisitionState

```text
Unknown
Hinted
Located
Contested
Recovered
BroughtToFonte
Integrated
Corrupted
Sealed
Protected
Used
```

### 7.4 MainProgressionSection

```text
Version
CurrentAct
CurrentMainQuestId optional
FragmentStates[]
StoryGateStates[]
KnownMajorNpcStates
KnownThreatStates
KnownLoreRevelations
Level100GateState
Level101AccessState
FinalChoiceState
PostGameWorldState
LastValidatedVersion
```

### 7.5 FragmentStateRecord

```text
FragmentType
AcquisitionState
DiscoveryQuestId optional
RecoveredAtDay optional
IntegratedAtDay optional
AssociatedMoon optional
AssociatedThreat
UnlockedFonteFunctionIds[]
KnownLoreRevelationIds[]
WorldChangeIds[]
ConsumedByFinalChoice
```

### 7.6 StoryGateState

```text
GateId
State: Unknown | Locked | Hinted | Available | Opened | Resolved | Sealed | FailedSafe
RequiredAct
RequiredFragments[]
RequiredQuestFlags[]
RequiredCaveProgress optional
RequiredLunarState optional
RequiredNpcKnowledge[]
SpoilerTier
```

---

## 8. Act transition rules

```text
Act 1 starts with arrival/farm/Fonte/first return/Corvus/first cave/first BlackStone/Water fragment.
Act 2 starts after Water fragment integration and urban memory investigation/Vaelrion arrival.
Act 3 starts after Memory fragment and cult/night shop/BlackStone escalation reveal.
Act 4 starts after Life fragment and deep cave/boss gate/Memory Arc final route.
PostGame starts after FinalChoiceResolved.
```

Transitions must be idempotent and validate prerequisites.

---

## 9. Fragment order validator

```text
Water must be first integrated fragment.
Memory requires Water integrated.
Life requires Memory integrated.
Hope requires Life integrated and level 100/101/final gate route.
Fragments may be hinted early, but not integrated out of order.
Recovered-but-not-integrated may exist if story demands, but Fonte unlock only occurs on integration.
```

---

## 10. Spoiler staging

```text
Act1 may show: Fonte, forgotten litania, strange water, small BlackStone.
Act1 must hide: Cindar full identity, Memory Arc, Sethra leader, Vaelrion antagonist, level 101, Archivist.
Act2 may show: Cindar, Nymirians, Memory Arc term, Vaelrion useful/ambiguous.
Act2 must hide: Sethra leader reveal, Archivist final, final choices.
Act3 may show: Sethra cult, BlackStone draining soul/memory/LivingWater, Vaelrion crossing limits.
Act3 must hide: final choice details and full level 101 content.
Act4 may reveal: level 101, Archivist, final choice.
```

---

## 11. Integration hooks

MainProgression may publish/consume high-level events only:

```text
OnMainActChanged
OnMainFragmentHinted
OnMainFragmentRecovered
OnMainFragmentIntegrated
OnMainStoryGateOpened
OnLevel100GateAvailable
OnLevel101AccessUnlocked
OnFinalChoiceAvailable
OnFinalChoiceResolved
```

It must not directly:

```text
add inventory item;
move NPC;
spawn boss;
change crop state;
grant Fonte function without FonteAnya adapter;
write QuestState directly except via quest adapter.
```

---

## 12. Criteria

```text
MainProgressionSection exists or is hardened.
Act and fragment enums/states exist.
Fragment order is validated.
MainProgression remains separate from QuestState and FonteAnya.
Spoiler staging is explicit.
Act transitions are idempotent.
Tests cover valid progression, invalid out-of-order fragment, duplicate integration, section separation and spoiler stage.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/MainProgression/MainAct.cs
Assets/_Game/Scripts/MainProgression/MainFragmentType.cs
Assets/_Game/Scripts/MainProgression/FragmentAcquisitionState.cs
Assets/_Game/Scripts/MainProgression/MainProgressionSection.cs
Assets/_Game/Scripts/MainProgression/FragmentStateRecord.cs
Assets/_Game/Scripts/MainProgression/StoryGateState.cs
Assets/_Game/Scripts/MainProgression/MainProgressionService.cs
Assets/_Game/Scripts/MainProgression/MainProgressionValidator.cs
Assets/_Game/Tests/EditMode/MainProgression/MainProgressionStateTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/MainProgression/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/MainProgression/**
docs/validation/10_spec_main_progression_acts_fragments_state_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/City/**
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia

```text
1. Auditar main progression/Fonte/quest state existentes.
2. Consolidar MainProgressionSection.
3. Consolidar act/fragment/story gate states.
4. Implementar validators de ordem, section separation e spoiler staging.
5. Criar tests.
6. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - FonteAnya functions;
  - final choice;
  - level 100/101 runtime;
  - quest state save/load;
  - cave boss gates.
- Reason: main progression is shared source of truth.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if MainProgressionSection exists; otherwise STOP.
Does this add save section? NO unless dedicated save migration is approved.
Does this require migration? NO unless adding MainProgressionSection; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL high-level main progression events.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final; provides projection source.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for main quest/Fonte visual flow if wired.
```

---

## 21. Riscos

```text
Risco: section absent.
Mitigação: STOP rather than migration.

Risco: out-of-order fragments.
Mitigação: validator/tests.

Risco: main progression becomes quest flags.
Mitigação: section separation.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar MainProgression/Fonte/Quest state.
- [ ] T003 — Consolidar section and states.
- [ ] T004 — Implementar validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a main progression acts/fragments foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Separação de estado | MainProgressionSection, FonteAnyaSection e QuestState permanecem separados? | Checklist explícito no report. | BLOCKED se misturar |
| Ordem canônica | Água -> Memória -> Vida -> Esperança foi preservada? | Tests/validator. | PARTIAL |
| Anti-spoiler | Arquivista, nível 101, escolha final, Anya completa e Vaelrion/Sethra não vazam cedo? | Tests/visibility policy. | PARTIAL |
| Anti-exploit | Água Viva, Mana, fragmentos, purificação, respec e final choice não viram economia/curas infinitas? | Tests/checklist. | PARTIAL |
| Anti-softlock | Main progression tem recovery para morte, item perdido, NPC indisponível, cave/state reload e eventos lunares? | Plano/validator. | PARTIAL |
| Pets/social future | A execução não implementou PetFuture/SocialFuture/romance/casamento profundo? | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/10_spec_main_progression_acts_fragments_state_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "MainProgression|MainAct|FragmentState|WaterFragment|MemoryFragment|LifeFragment|HopeFragment|StoryGate|Level100Gate|Level101Access" Assets/_Game/Scripts docs/design docs/specs
rg -n "MainProgression|FonteAnya|Anya|Cindar|Fragment|WaterFragment|MemoryFragment|LifeFragment|HopeFragment|LivingWater|Respec|Purification|BlackStone|MemoryArc|Vaelrion|Sethra|Archivist|Level101|FinalChoice|Protect|Seal|Use" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given o sistema base relacionado a main progression acts/fragments existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And MainProgressionSection, FonteAnyaSection, QuestState, flags e UI projection permanecem consistentes
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

### Scenario 3 — Anti-spoiler / staging

```text
Given o jogador ainda não atingiu ato/fragmento/profundidade/flag necessária
When UI, dialogue, quest, reward ou event projection consulta o estado
Then somente informação descoberta é exposta
And spoilers de Arquivista, 101, final choice, Anya completa, Vaelrion antagonista ou Sethra líder permanecem ocultos.
```

### Scenario 4 — Idempotency and replay safety

```text
Given fragmento, função da Fonte, boss gate, nível 101, final choice, purificação ou flag já foi aplicado
When reload, event repeat, scene reload ou reentrada de diálogo ocorre
Then o efeito não duplica
And o estado terminal não retorna para estado anterior sem política explícita.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de Fonte, efeito visual, ritual, boss gate, nível 101, final choice ou Quest Log
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Fragment integrated out of order.
- Fragment integrated twice.
- MainProgression stored as QuestFlags.
- Fonte function unlock happens without FonteAnya adapter.
- Level101 unlocked by depth only.
- Spoiler stage exposes Archivist/final choice early.
- Main quest expires by calendar.
- Save schema altered without migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Main Progression Acts Fragments State Runtime

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

## Canon / progression compliance
- MainProgressionSection separated from QuestState:
- FonteAnyaSection separated from QuestState:
- Fragment order preserved:
- Anya not restored as NPC:
- Mana not common crop/economy:
- No PetFuture runtime:
- No SocialFuture/romance deep runtime:
- Anti-spoiler:
- Anti-softlock:
- Anti-exploit:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-spoiler/staging:
- Idempotency/replay safety:
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
4. A implementação exigir reescrever Quest/Save/EventBus/Fonte/MainProgression/Cave canônico existente.
5. A implementação esconder MainProgression ou FonteAnya dentro de QuestState genérico.
6. A implementação restaurar Anya como NPC comum, templo ativo ou companion.
7. A implementação fazer Mana virar crop comum, dinheiro infinito, cura/MP infinita ou requisito de todo sistema.
8. A implementação revelar cedo Arquivista do Silêncio, escolha final, nível 101, boss oculto ou segredo de Anya.
9. A implementação tornar main quest expirável por tempo/calendário.
10. A implementação criar PetFuture, SocialFuture, romance/casamento profundo ou companion obrigatório para main quest.
11. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Act / Fragment Matrix

| Act | Fragment | Fonte unlock |
|---|---|---|
| Act 1 — Fonte e Esquecimento | Água | Água Viva limitada |
| Act 2 — Cindar e Arco da Memória | Memória | Respec |
| Act 3 — Culto, Pedra Negra e Vida | Vida | Purificação/cura avançada limitada |
| Act 4 — Nível 100/101 e Esperança | Esperança | Decisão final |

## 23H. Section Ownership

```text
MainProgressionSection owns:
  acts, fragment states, story gates, level100/101 access, final choice state.

FonteAnyaSection owns:
  Fonte functions, LivingWater, respec, purification, Fonte visual/function states.

QuestStateSection owns:
  generic quest progress.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "MainProgression|FonteAnya|Anya|Cindar|Fragment|WaterFragment|MemoryFragment|LifeFragment|HopeFragment|LivingWater|Respec|Purification|BlackStone|MemoryArc|Vaelrion|Sethra|Archivist|Level101|FinalChoice|Protect|Seal|Use" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de Fonte, fragmento, boss gate, nível 101, final choice ou main quest, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, act/fragment transition and validation logic is deterministic.
- Requires EditMode tests: YES for act transition/fragment order/duplicate/section separation/spoiler staging tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED if main quest/Fonte visual flow is wired.
- Requires regression test: YES if fixing existing main progression state bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; MainProgression separated; fragment order enforced.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/10_spec_main_progression_acts_fragments_state_runtime_execution_report.md.
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
Não misturar QuestState com MainProgression/FonteAnya.
Não restaurar Anya como NPC comum.
Não criar Mana como crop comum/economia infinita.
Não aplicar fragmento/Fonte/final choice duas vezes.
Não revelar spoiler oculto cedo.
Não fazer main quest expirar por tempo.
Não implementar PetFuture/SocialFuture/romance profundo.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
