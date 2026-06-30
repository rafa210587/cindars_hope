# SPEC — Memory Arc Black Stone Corruption Fragment Runtime

> **Spec ID:** `10_spec_memory_arc_black_stone_corruption_fragment_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 10 — Main Progression / Fonte / Endgame  
> **Priority:** P1  
> **Type:** Runtime / Main Progression / Memory Arc / Black Stone / Corruption  
> **Domain:** MainProgression / MemoryArc / BlackStone / Corruption / Fragment Threat  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_10_MAIN_PROGRESSION_FONTE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere cave level generation, enemy/boss combat, status effect implementation, item economy, Fonte purification, final choice, quest content or save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/MainProgression/**`, `Assets/_Game/Scripts/Fonte/**`, `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/StatusEffects/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/MainProgression/**`, `docs/validation/10_spec_memory_arc_black_stone_corruption_fragment_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
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
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
> **Blocks:**  
  - BlackStone item/status specs;
  - cave corruption encounters;
  - Fonte purification;
  - Vaelrion/Sethra questline;
  - level 101/final boss.
> **Scope:** definir/endurecer contratos de Arco da Memória, Pedra Negra, corrupção/drenagem e fragment threat states sem implementar combat/status/economia final.  
> **Out of scope:** boss fight, status effect balance, cave generation, final item tables, scene/prefab effects, final boss implementation.

---

# /speckit.specify

## 1. Contexto

A main quest revela que o Arco da Memória, Vaelrion, Sethra, Pedra Negra, Água Viva corrompida e fragmentos convergem no endgame. A Pedra Negra drena alma, memória e Água Viva; Bromécia tentou converter vida em sistema; Vaelrion quer usar o Arco; Sethra quer silenciar Anya.

Esta spec cria os contratos de threat/corruption/progression, não o combate ou status final.

---

## 2. Problema

Sem contrato:

```text
Pedra Negra pode virar ore comum;
corrupção pode virar status genérico sem lore;
Arco da Memória pode aparecer cedo demais;
Vaelrion pode virar vilão caricato cedo;
Sethra pode ser revelada antes do Ato 3;
Água Viva corrompida pode ser curada por item comum;
fragmentos podem ser usados como moeda/crafting comum;
boss final pode não saber quais processos foram ativados.
```

---

## 3. Objetivo

Criar/endurecer:

```text
MemoryArcState;
BlackStoneState;
CorruptionThreatState;
FragmentThreatState;
VaelrionArcState;
SethraCultState;
BlackStoneExposureRecord;
CorruptedLivingWaterRecord;
PurificationCompatibility;
Spoiler gates and validators.
```

---

## 4. Regras de design

```text
Pedra Negra não é commodity comum.
Pedra Negra cultista drena alma, memória e Água Viva.
Pedra Negra estabilizada é deep/endgame/gated.
Arco da Memória é revelado gradualmente.
Vaelrion parece útil antes de cruzar limites.
Sethra líder do culto só é revelada no Ato 3.
Água Viva corrompida exige purificação/lore gate.
Fragmentos não são moeda comum.
```

---

## 5. User stories / engineering stories

```text
Como main progression, quero rastrear estado do Arco e ameaças sem usar flags soltas.
Como cave/Fonte, quero saber se Pedra Negra/corrupção está habilitada por ato.
Como quest/visibility, quero bloquear spoilers de Vaelrion/Sethra/Arco.
Como purification, quero saber quais corrupções podem ser tratadas.
Como economy, quero impedir Pedra Negra como commodity comum.
```

---

## 6. Escopo

Inclui:

```text
MemoryArcState contract;
BlackStoneState contract;
corruption threat states;
fragment threat records;
Vaelrion/Sethra reveal states;
BlackStone exposure records;
corrupted LivingWater compatibility;
validators/tests.
```

Não inclui:

```text
combat/status effect implementation;
boss fight;
cave procedural changes;
item database;
visual effects;
dialogue content.
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
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md

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

- Ato 2 revela Cindar, Nymirianos, Memória, Arco da Memória e Vaelrion como aliado acadêmico ambíguo.
- Ato 3 revela Sethra, culto de Nyx, Pedra Negra, drenagem de alma/memória/Água Viva e Vaelrion cruzando limites.
- Pedra Negra cultista drena alma, memória e Água Viva.
- Sethra quer silenciar Anya; Vaelrion quer usar o Arco; ambos estão abrindo o mesmo selo por motivos diferentes.
- Nível 101 contém santuário nymiriano, máquina bromeciana, Arco de Elyndor, Pedras Negras, Água Viva corrompida, raiz/estrutura de Mana, ecos de Cindar, fragmentos de Anya e boss final.
- Não fazer: Vaelrion vilão caricato desde o começo, Nyx simplesmente má, Sethra destruindo mundo sem motivo, explicar tudo por diálogo expositivo.

### Deferred / future from directions

- BlackStone status effect final.
- Enemy/boss mechanics.
- Cave generation.
- Final item/economy values.
- Dialogue writing.
- Level 101 scene implementation.
- Final boss.

### Explicitly not redefined here

- MainProgressionSection.
- FonteAnyaSection.
- Quest visibility/flags.
- ItemDefinition/economy contracts.
- Status effect system.
- Cave snapshot/generation.

## 7. Modelo de domínio

### 7.1 MemoryArcState

```text
Unknown
NameHinted
TermDiscovered
InterpretedByVaelrion
LinkedToCindar
LinkedToElyndor
PartiallyActivated
CorruptedByBlackStone
FinalActivated
Protected
Sealed
Used
```

### 7.2 BlackStoneState

```text
Unknown
ObservedSmall
UnstableShardKnown
CultUseSuspected
CultUseConfirmed
DrainsMemory
DrainsLivingWater
DrainsSoul
StabilizedFragmentKnown
DeepGateMaterial
FinalCorruptionSource
Contained
PurifiedLimited
Sealed
```

### 7.3 CorruptionThreatLevel

```text
None
Hint
MinorForgetfulness
WaterTurbid
MemoryDrain
LivingWaterCorruption
SoulDrain
WorldThreat
FinalConvergence
Contained
```

### 7.4 VaelrionArcState

```text
Unknown
ArrivedAsScholar
UsefulInterpreter
ArroganceVisible
BoundaryCrossed
UsesCultKnowledge
SeeksMemoryArc
FinalConvergence
MergedIntoArchivist
Resolved
```

### 7.5 SethraCultState

```text
Unknown
NightShopRumors
NyxMisinterpreted
CultSuspected
SethraRevealed
RitualsActive
SilencingAnya
FinalConvergence
Resolved
```

### 7.6 BlackStoneExposureRecord

```text
ExposureId
SourceType: Cave | CultRitual | Item | Boss | Water | MemoryEvent | Final
CaveLevel optional
ActGate
ThreatLevel
AffectsMemory
AffectsLivingWater
AffectsSoul
CanBePurified
RequiresLifeFragment
IsStable
IsCommodity
SpoilerTier
```

---

## 8. Progression gate rules

```text
Act1:
  small BlackStone, strange water, mild forgetfulness only.

Act2:
  Memory Arc term, Cindar/Nymirian clues, Vaelrion useful/ambiguous.

Act3:
  cult confirmation, Sethra reveal, BlackStone drains memory/soul/LivingWater, corrupted LivingWater boss/event.

Act4:
  Memory Arc final activation, BlackStone convergence, level 101, Archivist.
```

---

## 9. Economy/item guardrails

```text
BlackStone corrupted/cultist form:
  dangerous/lore gated, not common sellable material.

BlackStone stabilized fragment:
  deep/endgame/gated, not common early loot.

Fragments of Anya:
  not sellable, not discardable, not crafting commodity.

Memory Arc components:
  not general crafting resources unless late authored recipe.

Corrupted LivingWater:
  not healing item; requires purification or containment.
```

---

## 10. Purification compatibility

```text
Minor forgetfulness/water turbidity:
  may be minor purification.

Corrupted LivingWater:
  requires Life fragment advanced purification or story ritual.

Soul drain:
  cannot be fully cured by common item.

Final convergence:
  requires final route/choice, not ordinary purification.
```

---

## 11. Criteria

```text
MemoryArc/BlackStone/Corruption states exist or are hardened.
Vaelrion and Sethra reveal states are staged.
BlackStone cannot be commodity common by validator.
CorruptedLivingWater cannot be ordinary heal.
Threat stage respects act gates.
Tests cover stage gating, protected item/economy, purification compatibility and spoiler leaks.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/MainProgression/Threats/MemoryArcState.cs
Assets/_Game/Scripts/MainProgression/Threats/BlackStoneState.cs
Assets/_Game/Scripts/MainProgression/Threats/CorruptionThreatLevel.cs
Assets/_Game/Scripts/MainProgression/Threats/VaelrionArcState.cs
Assets/_Game/Scripts/MainProgression/Threats/SethraCultState.cs
Assets/_Game/Scripts/MainProgression/Threats/BlackStoneExposureRecord.cs
Assets/_Game/Scripts/MainProgression/Threats/MemoryArcBlackStoneValidator.cs
Assets/_Game/Tests/EditMode/MainProgression/MemoryArcBlackStoneThreatTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/MainProgression/**
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/StatusEffects/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/MainProgression/**
docs/validation/10_spec_memory_arc_black_stone_corruption_fragment_runtime_execution_report.md
```

---

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia

```text
1. Auditar BlackStone/MemoryArc/corruption references.
2. Consolidar threat state contracts.
3. Implementar validators for act gates, economy and spoiler.
4. Integrar read-only hooks to Fonte/MainProgression.
5. Criar tests.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - MainProgressionSection;
  - Fonte purification;
  - item/economy final tables;
  - cave generation;
  - final boss.
- Reason: threat states gate many systems.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if stored inside MainProgressionSection; otherwise STOP.
Does this add save section? NO.
Does this require migration? NO unless new persisted threat records are needed; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. BlackStoneThreatChangedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final; provides spoiler-safe projection.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for story/cave/Fonte visual flow.
```

---

## 20. Riscos

```text
Risco: spoilers leak early.
Mitigação: act gate tests.

Risco: BlackStone economy exploit.
Mitigação: item/economy validators.

Risco: corruption implemented as cure-all status.
Mitigação: compatibility and defer final status.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar MemoryArc/BlackStone/corruption references.
- [ ] T003 — Consolidar threat states.
- [ ] T004 — Implementar validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a Memory Arc/Black Stone/corruption/fragment threat foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
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
| Report | Execution report foi criado? | `docs/validation/10_spec_memory_arc_black_stone_corruption_fragment_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "MemoryArc|BlackStone|PedraNegra|Corruption|LivingWaterCorruption|Vaelrion|Sethra|Archivist|SoulDrain|MemoryDrain" Assets/_Game/Scripts docs/design .specs
rg -n "MainProgression|FonteAnya|Anya|Cindar|Fragment|WaterFragment|MemoryFragment|LifeFragment|HopeFragment|LivingWater|Respec|Purification|BlackStone|MemoryArc|Vaelrion|Sethra|Archivist|Level101|FinalChoice|Protect|Seal|Use" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a Memory Arc/Black Stone/corruption/fragment threat existe ou foi criado de forma mínima
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

- BlackStone treated as common ore.
- Corrupted LivingWater treated as heal item.
- Sethra revealed before Act3.
- Vaelrion villain state too early.
- Memory Arc term shown before discovery.
- Fragment usable as common crafting material.
- Final convergence state reachable without Life/Hope route.
- Threat records persisted outside approved save.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Memory Arc Black Stone Corruption Fragment Runtime

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


## 23G. Threat Stage Matrix

| Act | Allowed threat |
|---|---|
| Act1 | mild forgetfulness, strange water, small BlackStone |
| Act2 | Memory Arc clues, Cindar, Vaelrion useful/ambiguous |
| Act3 | Sethra, cult, BlackStone drains memory/soul/LivingWater |
| Act4 | level 101, Memory Arc activation, Archivist convergence |

## 23H. Protected Resource Matrix

| Resource | Allowed use |
|---|---|
| Fragment of Anya | main progression only |
| Corrupted BlackStone | lore/danger, not common sale |
| Stabilized BlackStone | deep/endgame gated |
| Corrupted LivingWater | purification/story only |
| Memory Arc component | late authored route |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "MainProgression|FonteAnya|Anya|Cindar|Fragment|WaterFragment|MemoryFragment|LifeFragment|HopeFragment|LivingWater|Respec|Purification|BlackStone|MemoryArc|Vaelrion|Sethra|Archivist|Level101|FinalChoice|Protect|Seal|Use" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES, threat stage/protection/purification compatibility logic is deterministic.
- Requires EditMode tests: YES for act gate/protected resource/corruption/spoiler tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for cave/Fonte/story visual flow.
- Requires regression test: YES if fixing existing BlackStone/corruption gating bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no BlackStone commodity leak; no spoiler leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/10_spec_memory_arc_black_stone_corruption_fragment_runtime_execution_report.md.
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
