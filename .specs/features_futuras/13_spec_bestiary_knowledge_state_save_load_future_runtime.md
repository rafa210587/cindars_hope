# SPEC — Bestiary Knowledge State Save Load Future Runtime

> **Spec ID:** `13_spec_bestiary_knowledge_state_save_load_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 13 — Bestiary / Knowledge Discovery / Future UI  
> **Priority:** P1  
> **Type:** Runtime / Future / Save Load / Knowledge State  
> **Domain:** Bestiary / EnemyKnowledgeState / KnowledgeConfidence / SaveLoad  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_13_BESTIARY_KNOWLEDGE_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere enemy stats/drops/vulnerabilities, combat balance, UI bestiary screen, research services, save schema migration ou quest reward runtime.  
> **Repo lock scope:** `Assets/_Game/Scripts/Bestiary/**`, `Assets/_Game/Scripts/Knowledge/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Tests/EditMode/Bestiary/**`, `docs/validation/13_spec_bestiary_knowledge_state_save_load_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
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
  - knowledge discovery event runtime;
  - bestiary UI projections;
  - equipment/spell tooltip known-interaction adapters;
  - quest knowledge rewards/objectives;
  - NPC/book/research services.
> **Scope:** definir/endurecer estado persistível de conhecimento descoberto por EnemyKnowledgeKey, sem criar stats/vulnerabilidades/drops.  
> **Out of scope:** UI completa, research services, discovery event routing, enemy stat authoring, save migration.

---

# /speckit.specify

## 1. Contexto

O direction define que o sistema de Bestiary/Knowledge não entrou na primeira entrega executável, mas pode ser preparado com IDs, tags, hooks, contratos conceituais e campos de save. O bestiário é apresentação futura; o sistema real é Knowledge Discovery.

Esta spec cria a base persistível de conhecimento.

---

## 2. Problema

Sem estado de conhecimento:

```text
jogador descobre fraqueza e esquece no reload;
drop raro coletado não aparece como descoberto;
Equipment UI pode revelar tudo ou nada;
Quest de fraqueza não reconhece descoberta anterior;
boss/main quest pode vazar conhecimento cedo;
conhecimento de família/variante/boss não se sobrepõe corretamente;
rumor e confirmado ficam iguais.
```

---

## 3. Objetivo

Criar/endurecer:

```text
EnemyKnowledgeKey;
EnemyKnowledgeState;
KnowledgeConfidence;
KnowledgeCategory;
KnowledgeSource;
KnowledgeRecord;
BestiaryKnowledgeSection;
KnowledgeSaveLoadAdapter;
KnowledgeMergePolicy;
SpoilerPolicyRef.
```

---

## 4. Regras de design

```text
Bestiary não cria vulnerabilidades.
Bestiary revela vulnerabilidades já existentes.
Conhecimento por família ajuda várias criaturas.
Conhecimento de variante pode sobrescrever família.
Boss nunca é totalmente inferido só pela família.
Rumor/Partial/Confirmed/Mastered são estados distintos.
Cada categoria revelável pode ter própria condição.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero que conhecimento descoberto persista.
Como UI, quero saber quais campos posso mostrar.
Como combat/equipment, quero consultar interações conhecidas.
Como quest, quero saber se uma fraqueza já foi descoberta.
Como main quest, quero proteger spoilers.
```

---

## 6. Escopo

Inclui:

```text
knowledge keys;
knowledge states;
confidence;
categories;
sources;
records;
save section;
merge policy;
spoiler-safe fields;
tests.
```

Não inclui:

```text
event discovery router;
Bestiary UI;
NPC research services;
enemy stats/vulnerability authoring;
migration real.
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

- Conhecimento deve ser registrado por EnemyKnowledgeKey.
- Chaves incluem EnemyId, EnemyFamilyId, EnemyVariantId, BossId, BiomeId, FactionId, DropId, AttackId, BehaviorId, VulnerabilityTag, ResistanceTag, ImmunityTag, MaterialInteractionTag, StatusInteractionTag, LoreEntryId e QuestKnowledgeId.
- EnemyKnowledgeState inclui Unknown, Seen, ScannedFuture, Fought, Defeated, RepeatedDefeated, Studied e FullyDocumented.
- KnowledgeConfidence inclui Rumor, Partial, Confirmed e Mastered.
- Tipos reveláveis incluem identidade, família, habitat, depth range, biome, faction, behavior, attacks, drops, vulnerabilities, resistances, immunities, pack behavior, lore and quest notes.
- Bosses e main quest têm spoiler control mais forte.

### Deferred / future from directions

- UI completa de bestiário.
- Event router.
- Research services.
- EnemyBestiaryEntrySO assets.
- Achievements/collections.
- Pet/companion runtime.

### Explicitly not redefined here

- Enemy stats/drops/vulnerabilities.
- Combat balance.
- Equipment vulnerability matching real source.
- Quest state/reward engine.
- Save migration policy.

## 7. Modelo de domínio

### 7.1 EnemyKnowledgeKey

```text
KeyType:
  EnemyId
  EnemyFamilyId
  EnemyVariantId
  BossId
  BiomeId
  FactionId
  DropId
  AttackId
  BehaviorId
  VulnerabilityTag
  ResistanceTag
  ImmunityTag
  MaterialInteractionTag
  StatusInteractionTag
  LoreEntryId
  QuestKnowledgeId

KeyId
ParentKey optional
VariantOverridePolicy optional
SpoilerTier
MainQuestSensitive
```

### 7.2 EnemyKnowledgeState

```text
Unknown
Seen
ScannedFuture
Fought
Defeated
RepeatedDefeated
Studied
FullyDocumented
```

### 7.3 KnowledgeConfidence

```text
Rumor
Partial
Confirmed
Mastered
```

### 7.4 KnowledgeCategory

```text
Identity
Family
Habitat
DepthRange
Biome
Faction
BehaviorSummary
MovePattern
AttackList
StatusApplied
DropsCommon
DropsRare
DropsBossFirstTime
DropsRepeat
ElementVulnerability
StatusVulnerability
AttackTypeVulnerability
WeaponVulnerability
MaterialVulnerability
BehavioralVulnerabilityWindow
ResistanceTags
ImmunityTags
PackBehavior
LoreNote
QuestNote
```

### 7.5 KnowledgeRecord

```text
KnowledgeKey
State
Confidence
DiscoveredCategories[]
SourceRecords[]
FirstDiscoveredDay
LastConfirmedDay
TimesObserved
TimesDefeated
TimesTested
KnownDropIds[]
KnownVulnerabilityTags[]
KnownResistanceTags[]
KnownImmunityTags[]
KnownLoreEntryIds[]
SpoilerTier
CanShowInBestiary
CanShowInTooltip
CanShowInQuestLog
```

### 7.6 BestiaryKnowledgeSection

```text
Version
KnowledgeRecords[]
KnownFamilies[]
KnownBosses[]
KnownBiomes[]
KnownFactions[]
DiscoveredLoreEntries[]
DiscoveredQuestKnowledgeIds[]
LastValidatedVersion
```

---

## 8. Merge policy

```text
Unknown -> Seen -> Fought -> Defeated -> RepeatedDefeated -> FullyDocumented.
Studied can move from Unknown/Seen/Fought to Studied, but confidence may remain Rumor/Partial.
Confirmed combat can upgrade Rumor to Confirmed.
Mastered requires repeated evidence, quest completion or authored study.
Variant records override family records only for categories explicitly discovered.
Boss records never auto-fill all from family records.
```

---

## 9. Save/load rules

```text
Persist only stable IDs and knowledge state.
Do not persist enemy GameObjects.
Do not persist UI rows.
Do not persist raw undiscovered stats.
Do not persist derived tooltip text.
Normalize duplicate KnowledgeRecord by strongest state/confidence and union of categories.
Invalid key should not be deleted silently; mark validation warning.
```

---

## 10. Criteria

```text
Knowledge state contracts exist.
Save section exists or STOP if migration is required.
Merge policy is deterministic.
Boss/main spoilers have gates.
No enemy stats/vulnerabilities are authored here.
Tests cover state transitions, confidence upgrade, category union, duplicate record merge, variant override and boss spoiler protection.
```

---

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Bestiary/EnemyKnowledgeKey.cs
Assets/_Game/Scripts/Bestiary/EnemyKnowledgeState.cs
Assets/_Game/Scripts/Bestiary/KnowledgeConfidence.cs
Assets/_Game/Scripts/Bestiary/KnowledgeCategory.cs
Assets/_Game/Scripts/Bestiary/KnowledgeRecord.cs
Assets/_Game/Scripts/Bestiary/BestiaryKnowledgeSection.cs
Assets/_Game/Scripts/Bestiary/KnowledgeMergePolicy.cs
Assets/_Game/Scripts/Bestiary/BestiaryKnowledgeValidator.cs
Assets/_Game/Tests/EditMode/Bestiary/BestiaryKnowledgeStateSaveLoadTests.cs
```

Consolidar existentes se houver.

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Knowledge/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Bestiary/**
docs/validation/13_spec_bestiary_knowledge_state_save_load_future_runtime_execution_report.md
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
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 14. Estratégia

```text
1. Auditar Bestiary/Knowledge/Save existentes.
2. Consolidar key/state/confidence/category contracts.
3. Implementar save section/adapter somente se seção já existir; senão STOP para migration.
4. Implementar merge/normalization policy.
5. Criar tests.
6. Criar report.
```

---

## 15. Paralelização

- Parallelizable: NO
- Must not run with:
  - save/load global migration;
  - knowledge discovery event runtime;
  - bestiary UI;
  - enemy stats/vulnerability specs.
- Reason: this is knowledge state foundation.

---

## 16. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if BestiaryKnowledgeSection exists; otherwise STOP.
Does this add save section? NO unless dedicated migration is approved.
Does this require migration? NO unless current save lacks section; then STOP.
Does this persist Unity references? NO.
```

---

## 17. Impacto eventos

```text
Adds events: NO by default.
Changes events: NO.
Requires unsubscribe pattern: NO.
```

---

## 18. Impacto UI/Unity

```text
Changes UI: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: NO by default.
```

---

## 19. Riscos

```text
Risco: save section absent.
Mitigação: STOP for migration.

Risco: knowledge reveals raw stats.
Mitigação: records store categories/tags only.

Risco: boss inferred too much from family.
Mitigação: boss override rule.
```

---

# /speckit.tasks

## 20. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar Bestiary/Knowledge/Save.
- [ ] T003 — Consolidar key/state/confidence/category.
- [ ] T004 — Implementar merge/normalization.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu Bestiary/Knowledge e directions de enemy/combat/equipment/UI/quest/save? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a bestiary knowledge state/save-load foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Anti-spoiler | Bestiary/UI/tooltips não revelam vulnerabilidades, drops, boss, Arquivista, Pedra Negra, 100/101 ou final cedo? | Tests/visibility policy. | PARTIAL |
| Não cria stats | A spec não redefine enemy stats/vulnerabilities/drops? | Checklist no report. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Quest compatibility | Knowledge objectives/rewards usam quest contracts sem opacidade? | Tests/checklist. | PARTIAL |
| UI projection | UI só mostra conhecimento autorizado por KnowledgeState? | Tests/checklist. | BUILD_VALIDATED sem cenário |
| Pets/companions | Pet/companion hints são future hooks, não runtime obrigatório. | Checklist explícito. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/13_spec_bestiary_knowledge_state_save_load_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "BestiaryKnowledgeSection|EnemyKnowledgeKey|EnemyKnowledgeState|KnowledgeConfidence|KnowledgeRecord|KnowledgeCategory|KnowledgeMerge" Assets/_Game/Scripts docs/design .specs
rg -n "Bestiary|Knowledge|EnemyKnowledge|KnowledgeState|Vulnerability|Resistance|Immunity|DropDiscovered|EnemySeen|EnemyDefeated|Research|LoreEntry|Archivist|Pedra Negra|BlackStone|Level101|Pet|Companion" Assets/_Game/Scripts docs/design .specs
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

- Knowledge state not persisted.
- Duplicate record weakens stronger known state.
- Boss knowledge inferred fully from family.
- Variant knowledge overwrites family incorrectly.
- Raw enemy stats stored in knowledge save.
- Unity references persisted.
- Spoiler tier ignored.
- Save migration attempted silently.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Bestiary Knowledge State Save Load Future Runtime

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
rg -n "Bestiary|Knowledge|EnemyKnowledge|KnowledgeState|Vulnerability|Resistance|Immunity|DropDiscovered|EnemySeen|EnemyDefeated|Research|LoreEntry|Archivist|BlackStone|Level101" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES, knowledge merge/save normalization logic is deterministic.
- Requires EditMode tests: YES for state transition/merge/confidence/category/boss/variant tests.
- Requires PlayMode automated or final human scenario: NO by default; state/contracts only.
- Requires regression test: YES if fixing existing knowledge save bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no stats authored; no spoiler leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/13_spec_bestiary_knowledge_state_save_load_future_runtime_execution_report.md.
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
