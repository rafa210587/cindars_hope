# SPEC — Bestiary Knowledge UI Projection Future Runtime

> **Spec ID:** `13_spec_bestiary_knowledge_ui_projection_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 13 — Bestiary / Knowledge Discovery / Future UI  
> **Priority:** P2  
> **Type:** Runtime / Future / UI Projection / Bestiary / Knowledge Log  
> **Domain:** Bestiary UI / Knowledge Log / Tooltip Known Interactions / Anti-spoiler  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_13_BESTIARY_KNOWLEDGE_FUTURE  
> **Can run with:** research service spec if lock scopes do not overlap.  
> **Must not run with:** qualquer spec que altere UI prefabs/layout, enemy stats, equipment stats, spell stats, quest log rules, save schema ou live combat runtime.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Bestiary/**`, `Assets/_Game/Scripts/UI/Knowledge/**`, `Assets/_Game/Scripts/UI/Tooltips/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/13_spec_bestiary_knowledge_ui_projection_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
> **Blocks:**  
  - Bestiary screen visual implementation;
  - equipment/spell tooltip integration;
  - quest UI knowledge objectives;
  - research services.
> **Scope:** definir/endurecer ViewModels/projection de Bestiary/Knowledge Log, known interactions em tooltips e anti-spoiler sem criar tela visual final.  
> **Out of scope:** prefabs/layout final, card art, icons, localization final, enemy stats, research runtime, save migration.

---

# /speckit.specify

## 1. Contexto

O direction diz que o bestiário é a apresentação futura do conhecimento. UI, equipment, spell, loot/crafting e quest UI só podem mostrar conhecimento descoberto ou recebido legitimamente.

Esta spec cria projection/view models, não a tela visual final.

---

## 2. Problema

Sem UI projection:

```text
bestiário pode revelar vulnerabilidade secreta;
drop raro não descoberto aparece pelo nome;
equipment comparison mostra vantagem secreta;
spell tooltip lista inimigos futuros;
repair/upgrade revela inimigo futuro;
main quest boss aparece cedo;
rumor e confirmado são mostrados iguais;
UI mostra raw ids/debug.
```

---

## 3. Objetivo

Criar/endurecer:

```text
BestiaryEntryViewModel;
KnowledgeLogViewModel;
KnowledgeCategoryProjection;
KnownInteractionProjection;
KnownDropProjection;
KnownVulnerabilityProjection;
BestiaryTooltipAdapter;
SpoilerSafeProjectionPolicy.
```

---

## 4. Regras de design

```text
UI não revela vulnerabilidade não descoberta.
Drop raro não descoberto aparece como ??? ou não aparece.
Equipment UI só mostra known enemy interactions.
Spell UI só mostra known effectiveness.
Quest UI não exige descoberta opaca sem pista.
Main quest sensitive entries são parciais.
Rumor/Partial/Confirmed/Mastered devem ser distinguíveis.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero ver o que já aprendi.
Como UI, quero mostrar Unknown/??? com segurança.
Como equipamento/magia, quero mostrar interações conhecidas.
Como main quest, quero manter mistério de boss e Pedra Negra.
Como research, quero mostrar origem/confidence do conhecimento.
```

---

## 6. Escopo

Inclui:

```text
Bestiary/Knowledge view models;
known interaction projections;
tooltip adapters;
spoiler-safe filtering;
category visibility;
confidence display;
tests.
```

Não inclui:

```text
prefab/layout final;
card art/icons;
localization final;
research runtime;
combat/equipment/spell stats.
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

- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada de BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.
Ela não cria stats reais de inimigos.
Ela não cria vulnerabilidades novas.
Ela só define como conhecimento é registrado, descoberto, salvo ou projetado depois que sistemas canônicos de enemy/combat/equipment/quest autorizam a informação.
Quando houver conflito com enemy roster, combat balance, equipment vulnerability adapter, quest ou UI direction, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Bestiary é apresentação futura do conhecimento.
- Tooltips, equipment UI, spell UI e quest UI só podem mostrar conhecimento que o jogador descobriu ou recebeu legitimamente.
- Equipment UI não revela vulnerabilidade não descoberta; weapon detail drawer só mostra known enemy interactions; comparison drawer pode indicar vantagem conhecida, não vantagem secreta.
- Spell UI não revela resistência/imunidade não descoberta; LearnableScroll não lista todos os inimigos afetados por spoiler.
- Drop raro não descoberto aparece como ??? ou não aparece.
- Bestiário não revela Arquivista do Silêncio antes da main quest nem a natureza completa da Pedra Negra cedo.

### Deferred / future from directions

- Bestiary visual screen.
- HUD permanente de bestiário.
- Knowledge Log navegável final.
- Card art/icons.
- Research service UI.
- Achievements/collections.

### Explicitly not redefined here

- Knowledge state.
- Enemy stats/drops/vulnerabilities.
- Equipment/spell real effectiveness.
- Quest state/reward.
- Localization final.

## 7. Modelo de domínio

### 7.1 BestiaryEntryViewModel

```text
EntryId
DisplayName
DisplayNameState: Unknown | TemporaryName | CommonName | FullName
SilhouetteIconId optional
KnownIconId optional
KnowledgeState
Confidence
KnownCategories[]
FamilyProjection
HabitatProjection
DepthProjection
BehaviorProjection
AttackProjection
DropProjection
VulnerabilityProjection
ResistanceProjection
ImmunityProjection
LoreProjection
QuestNoteProjection
SpoilerSafe
```

### 7.2 KnowledgeLogViewModel

```text
KnownEntries[]
UnknownPlaceholderCount optional
Filters[]
SortMode
SelectedEntry
KnownFamilies[]
KnownBiomes[]
KnownFactions[]
MainQuestSensitiveHiddenCount optional
DebugWarnings[]
```

### 7.3 KnownInteractionProjection

```text
TargetLabel
InteractionType: Material | Weapon | DamageType | Status | Spell | BehaviorWindow
Confidence
KnownEffectiveness: Effective | Resisted | Immune | Unknown | Rumor
SourceSummary
CanShowExact
```

### 7.4 KnownDropProjection

```text
DropId optional
DisplayNameOrPlaceholder
DropCategory
Confidence
Source: Collected | NPC | Book | Quest | Unknown
CanShowExactName
```

---

## 8. Projection rules

```text
Unknown identity:
  show ???/silhouette.

Seen:
  show temporary/simple visual name.

Defeated/Studied:
  show common name.

FullyDocumented:
  show full name, variant and family.

Vulnerability:
  show only after effective test, NPC/book/quest revelation or confirmed source.

Rare drop:
  show only after collected or revealed by authored source.

Boss:
  show partial until faced/studied/defeated according to main quest gate.
```

---

## 9. Tooltip integration

```text
Equipment material:
  Known enemy interactions only.

Weapon comparison:
  known advantage only, not hidden advantage.

Spell tooltip:
  known effectiveness only.

Crafting item:
  known source only; unknown rare source hidden/???.

Quest:
  can show “discover weakness” objective without naming hidden weakness unless known.
```

---

## 10. Criteria

```text
Bestiary/Knowledge projection exists.
Projection filters by KnowledgeRecord categories/confidence.
Tooltips show only known interactions.
Main quest sensitive entries are partial/hidden.
UI does not expose raw debug IDs in final projection.
Tests cover unknown/seen/defeated/studied, drop unknown, vulnerability known, equipment/spell tooltip and main spoiler.
```

---

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Bestiary/BestiaryEntryViewModel.cs
Assets/_Game/Scripts/UI/Bestiary/KnowledgeLogViewModel.cs
Assets/_Game/Scripts/UI/Bestiary/KnowledgeCategoryProjection.cs
Assets/_Game/Scripts/UI/Bestiary/KnownInteractionProjection.cs
Assets/_Game/Scripts/UI/Bestiary/KnownDropProjection.cs
Assets/_Game/Scripts/UI/Bestiary/BestiaryProjectionService.cs
Assets/_Game/Scripts/UI/Tooltips/BestiaryTooltipAdapter.cs
Assets/_Game/Tests/EditMode/UI/BestiaryKnowledgeProjectionTests.cs
```

Consolidar existentes se houver.

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Bestiary/**
Assets/_Game/Scripts/UI/Knowledge/**
Assets/_Game/Scripts/UI/Tooltips/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/13_spec_bestiary_knowledge_ui_projection_future_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Knowledge/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Magic/**
Assets/_Game/Scripts/Quests/**
```

---

## 13. Arquivos proibidos

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

## 14. Estratégia

```text
1. Auditar UI bestiary/tooltips/projection systems.
2. Consolidar view models.
3. Implementar projection service read-only.
4. Implementar tooltip adapter.
5. Criar tests de anti-spoiler.
6. Criar report.
```

---

## 15. Paralelização

- Parallelizable: YES
- Can run with:
  - research service if no same files;
  - docs-only registry updates.
- Must not run with:
  - Knowledge state save foundation;
  - tooltip global rewrite;
  - UI prefab/layout implementation.
- Reason: projection layer depends on stable knowledge state.

---

## 16. Impacto save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 17. Impacto eventos

```text
Adds events: NO.
Changes events: NO.
Requires unsubscribe pattern: YES if reactive UI subscribers are created.
```

---

## 18. Impacto UI/Unity

```text
Changes UI data/projection layer: YES.
Changes prefabs/layout: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for Bestiary/Tooltip visual validation.
```

---

## 19. Riscos

```text
Risco: tooltip leaks hidden info.
Mitigação: projection tests.

Risco: raw debug IDs visible.
Mitigação: final projection validator.

Risco: UI demands missing KnowledgeState.
Mitigação: STOP/defer until state spec.
```

---

# /speckit.tasks

## 20. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar UI/projection/tooltips.
- [ ] T003 — Consolidar view models.
- [ ] T004 — Implementar projection/tooltip adapter.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu Bestiary/Knowledge e directions de enemy/combat/equipment/UI/quest/save? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a bestiary/knowledge UI projection foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Anti-spoiler | Bestiary/UI/tooltips não revelam vulnerabilidades, drops, boss, Arquivista, Pedra Negra, 100/101 ou final cedo? | Tests/visibility policy. | PARTIAL |
| Não cria stats | A spec não redefine enemy stats/vulnerabilities/drops? | Checklist no report. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Quest compatibility | Knowledge objectives/rewards usam quest contracts sem opacidade? | Tests/checklist. | PARTIAL |
| UI projection | UI só mostra conhecimento autorizado por KnowledgeState? | Tests/checklist. | BUILD_VALIDATED sem cenário |
| Pets/companions | Pet/companion hints são future hooks, não runtime obrigatório. | Checklist explícito. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/13_spec_bestiary_knowledge_ui_projection_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "BestiaryEntry|KnowledgeLog|KnownInteraction|KnownDrop|BestiaryTooltip|KnownEffectiveness|Unknown|Silhouette|Spoiler" Assets/_Game/Scripts docs/design docs/specs
rg -n "Bestiary|Knowledge|EnemyKnowledge|KnowledgeState|Vulnerability|Resistance|Immunity|DropDiscovered|EnemySeen|EnemyDefeated|Research|LoreEntry|Archivist|Pedra Negra|BlackStone|Level101|Pet|Companion" Assets/_Game/Scripts docs/design docs/specs
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
Given o jogador descobre conhecimento por fonte legítima
When o sistema de bestiary/knowledge processa o evento
Then somente a categoria autorizada é atualizada
And UI/equipment/spell/quest projections podem mostrar apenas essa informação
And conhecimento ainda não descoberto permanece oculto ou como ???.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Anti-spoiler

```text
Given boss, main quest, Pedra Negra, Água Viva corrompida, Arquivista, nível 100/101 ou final choice
When Bestiary/UI/Quest/Tooltip consulta conhecimento
Then nada disso é revelado antes do gate narrativo apropriado
And entradas parciais usam placeholder seguro.
```

### Scenario 4 — Legitimate discovery

```text
Given o jogador usa dano efetivo, dano resistido, status aplicado, drop coletado, NPC, livro ou quest
When a descoberta é registrada
Then confidence/source/category ficam persistíveis e rastreáveis
And a descoberta pode ser idempotente.
```

### Scenario 5 — Final human validation deferred

```text
Given a spec exige visual de bestiary, tooltip, research UI ou HUD
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- UI shows vulnerability before discovery.
- Rare drop full name shown before collection/reveal.
- Equipment comparison shows hidden advantage.
- Spell tooltip lists future enemies.
- Main boss/Arquivista appears early.
- Rumor and Confirmed displayed as same certainty.
- Raw debug IDs shown in final projection.
- Projection mutates knowledge state.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Bestiary Knowledge UI Projection Future Runtime

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

## Bestiary/Knowledge compliance
- Does not define enemy stats:
- Does not define vulnerabilities:
- Uses stable IDs:
- Spoiler policy:
- Main quest sensitive entries protected:
- Pet/companion future only:
- UI projection not source of truth:
- Save/load safe:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-spoiler:
- Legitimate discovery:
- Save/load:
- UI/final scenario:

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
3. A implementação exigir save migration sem spec de migration.
4. A implementação criar enemy stats, drops ou vulnerabilities novos.
5. A implementação revelar vulnerabilidade/drop/boss não descoberto.
6. A implementação revelar Arquivista do Silêncio, natureza completa da Pedra Negra, nível 100/101 ou final choice cedo.
7. A implementação transformar pets/companions em sistema obrigatório ou autopreencher bestiário.
8. A implementação tornar Bestiary a fonte real de combat/equipment balance.
9. A implementação exigir grind excessivo/opaco sem pista.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Bestiary|Knowledge|EnemyKnowledge|KnowledgeState|Vulnerability|Resistance|Immunity|DropDiscovered|EnemySeen|EnemyDefeated|Research|LoreEntry|Archivist|BlackStone|Level101" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de Bestiary UI, tooltips, research service, enemy discovery, HUD ou Quest Log, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, UI projection/filtering logic is deterministic.
- Requires EditMode tests: YES for unknown/seen/drop/vulnerability/equipment/spell/main-spoiler tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for Bestiary UI/tooltip visual validation.
- Requires regression test: YES if fixing existing UI spoiler bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no hidden vulnerability/drop/boss leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/13_spec_bestiary_knowledge_ui_projection_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não usar Bestiary como fonte de stats reais.
Não revelar vulnerability/drop/boss sem descoberta.
Não revelar Arquivista/Pedra Negra/nível 101/final cedo.
Não fazer pets/companions completarem bestiário sozinhos.
Não exigir grind opaco.
Não usar UI como fonte de verdade.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando as dependências de enemy/combat/equipment/quest/UI/save estiverem estáveis ou quando houver decisão humana de antecipar esse bloco.
