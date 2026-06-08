# SPEC — Knowledge Discovery Event Runtime Future

> **Spec ID:** `13_spec_knowledge_discovery_event_runtime_future`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 13 — Bestiary / Knowledge Discovery / Future UI  
> **Priority:** P1  
> **Type:** Runtime / Future / Event Routing / Knowledge Discovery  
> **Domain:** Bestiary / Knowledge Discovery / Combat Events / Drop Events / Quest Events  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_13_BESTIARY_KNOWLEDGE_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere GameEventBus core, enemy combat damage formulas, loot tables, quest reward engine, UI bestiary screen, save schema migration ou pet/companion runtime.  
> **Repo lock scope:** `Assets/_Game/Scripts/Bestiary/**`, `Assets/_Game/Scripts/Knowledge/**`, `Assets/_Game/Scripts/Core/Events/**`, `Assets/_Game/Tests/EditMode/Bestiary/**`, `docs/validation/13_spec_knowledge_discovery_event_runtime_future_execution_report.md`  
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
  - bestiary knowledge state;
  - equipment/spell known interaction UI;
  - quest knowledge objectives;
  - research services;
  - bestiary UI.
> **Scope:** definir/endurecer roteamento de eventos legítimos de descoberta para KnowledgeRecord, sem alterar combat/loot/quest sources.  
> **Out of scope:** event bus rewrite, combat formulas, loot tables, UI, pet/companion hint runtime, save migration.

---

# /speckit.specify

## 1. Contexto

Conhecimento pode ser descoberto por observação direta, combate, teste de dano/tag, drops/loot, NPCs, livros/documentos/ruínas, pets/companions future, quests e main quest gates. A UI só mostra vulnerabilidade depois que evento de descoberta ocorreu ou fonte legítima revelou.

Esta spec cria o router de eventos de descoberta.

---

## 2. Problema

Sem discovery router:

```text
ver inimigo pode revelar demais;
derrotar inimigo pode revelar tudo;
fraqueza não é registrada após dano efetivo;
resistência não é registrada após dano reduzido;
drop raro aparece antes da coleta;
NPC/livro pode setar conhecimento sem confidence/source;
quest de fraqueza não reconhece descoberta retroativa;
eventos repetidos podem inflar Mastered indevidamente.
```

---

## 3. Objetivo

Criar/endurecer:

```text
KnowledgeDiscoveryEvent;
KnowledgeDiscoverySource;
KnowledgeDiscoveryRule;
KnowledgeDiscoveryRouter;
KnowledgeDiscoveryResult;
DiscoveryDeduplicationPolicy;
CombatDiscoveryAdapter;
DropDiscoveryAdapter;
QuestDiscoveryAdapter;
NpcDocumentDiscoveryAdapter hooks.
```

---

## 4. Regras de design

```text
Ver revela pouco.
Lutar revela mais.
Derrotar revela mais, mas não tudo.
Dano efetivo revela fraqueza específica.
Dano resistido/imunidade revela resistência/imunidade.
Drop coletado revela drop.
NPC/livro/quest pode revelar rumor/partial/confirmed.
Boss/main quest tem spoiler control mais forte.
Não exigir grind excessivo.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero que minhas descobertas em combate sejam registradas.
Como UI, quero saber o que já foi legitimamente descoberto.
Como quest, quero aceitar múltiplas formas de descoberta.
Como main quest, quero proteger boss/Arquivista/Pedra Negra/101.
Como combat, quero publicar eventos sem conhecer UI.
```

---

## 6. Escopo

Inclui:

```text
discovery event contracts;
source types;
discovery rules;
router;
dedupe;
combat/drop/quest/NPC/document hooks;
tests.
```

Não inclui:

```text
GameEventBus rewrite;
enemy combat formulas;
loot table changes;
full UI;
pet/companion runtime.
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

- Observação direta inclui EnemySeen, EnemyTargeted, EnemyDamagedPlayer, EnemyUsedAction, EnemyEnteredPackBehavior, EnemyUsedStatus, EnemyFled, EnemyGuardedResource e EnemyTriggeredTrap.
- Combate inclui PlayerHitEnemy, PlayerCritEnemy, PlayerBlockedEnemy, PlayerDodgedEnemyAction, PlayerTriggeredCriticalWindow, EnemyStaggered, EnemyDefeated e BossPhaseReached.
- Teste de dano/tag inclui DamageTypeEffective, DamageTypeResisted, StatusAppliedSuccessfully, StatusResisted, MaterialEffective, MaterialResisted, WeaponTypeEffective, WeaponTypeResisted e ImmunityTriggered.
- Drops/loot incluem EnemyDropCollected, BossFirstTimeRewardCollected, RareDropCollected, MaterialHarvested e CorpseResourceCollectedFuture.
- NPC/livro/quest podem revelar conhecimento parcial, rumor ou confirmado dependendo da fonte.
- Drop não coletado não deve aparecer como nome completo salvo NPC/livro/quest revelar.

### Deferred / future from directions

- Pet/companion discovery runtime.
- Laboratory UI.
- Full research system.
- Bestiary complete UI.
- EnemyBestiaryEntrySO assets.
- Achievements.

### Explicitly not redefined here

- Combat damage calculation.
- Enemy data/vulnerability definitions.
- Loot table outputs.
- Quest state/reward implementation.
- UI presentation.

## 7. Modelo de domínio

### 7.1 KnowledgeDiscoverySource

```text
Observation
Combat
DamageTagTest
StatusTest
DropCollected
NpcDialogue
NpcService
BookDocument
RuinInscription
QuestReward
QuestObjective
MainQuestGate
PetHintFuture
CompanionHintFuture
Debug
```

### 7.2 KnowledgeDiscoveryEvent

```text
EventId
Source
SourceSystem
KnowledgeKey
EnemyId optional
EnemyFamilyId optional
EnemyVariantId optional
BossId optional
Category
ConfidenceGranted
StateGranted
ObservedTag optional
DropId optional
LoreEntryId optional
QuestKnowledgeId optional
Day
Time
RunSeed optional
SpoilerTier
DeduplicationKey
```

### 7.3 KnowledgeDiscoveryRule

```text
RuleId
Source
InputEventName
RequiredTags[]
ForbiddenTags[]
CategoryGranted
StateGranted
ConfidenceGranted
MinRepeatCount optional
BossAllowed
MainQuestSensitive
RequiresStoryGate optional
CanRevealName
CanRevealExactValue
DebugTags[]
```

### 7.4 KnowledgeDiscoveryResult

```text
Success
SkippedDuplicate
BlockedBySpoiler
BlockedByMissingGate
UpdatedRecordId
CategoriesAdded[]
StateBefore
StateAfter
ConfidenceBefore
ConfidenceAfter
Warnings[]
```

---

## 8. Discovery mapping

```text
EnemySeen:
  State Seen, category Identity partial/Habitat current.

EnemyUsedAction:
  category AttackList partial, BehaviorSummary partial.

PlayerHitEnemy:
  State Fought.

DamageTypeEffective:
  relevant Vulnerability category, Confirmed if safe.

DamageTypeResisted:
  ResistanceTags partial/confirmed depending repeat.

ImmunityTriggered:
  ImmunityTags confirmed.

EnemyDefeated:
  Defeated, identity common name and collected drops only.

RareDropCollected:
  DropsRare confirmed for collected DropId.

NpcDialogue/Book:
  Rumor or Partial by default unless authored confirmed.

QuestReward:
  can grant Confirmed/Mastered if quest authorizes.
```

---

## 9. Deduplication policy

```text
Same EventId cannot apply twice.
Same DeduplicationKey can increment observation count once per allowed scope.
RepeatedDefeated threshold can upgrade only when count policy says so.
Repeated failed attempts can confirm resistance only after threshold.
Quest reward knowledge is once per QuestRewardId.
Debug source never runs in production.
```

---

## 10. Spoiler gates

```text
Arquivista:
  no exact entry until main quest gate.

Pedra Negra:
  symptoms early; full nature later.

Nível 100/101:
  hidden until endgame gate.

Boss:
  family inference limited; phase info only after BossPhaseReached/Study.

FinalChoice:
  not represented as normal knowledge discovery.
```

---

## 11. Criteria

```text
Discovery event contracts exist.
Router maps legit events to KnowledgeRecord updates.
Deduplication prevents repeated inflation.
Spoiler gate blocks sensitive entries.
Quest discovery can be retroactive.
NPC/book sources can mark Rumor/Partial.
Tests cover seen/fought/effective/resisted/drop/NPC/quest/spoiler/dedupe.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Bestiary/Discovery/KnowledgeDiscoverySource.cs
Assets/_Game/Scripts/Bestiary/Discovery/KnowledgeDiscoveryEvent.cs
Assets/_Game/Scripts/Bestiary/Discovery/KnowledgeDiscoveryRule.cs
Assets/_Game/Scripts/Bestiary/Discovery/KnowledgeDiscoveryRouter.cs
Assets/_Game/Scripts/Bestiary/Discovery/KnowledgeDiscoveryResult.cs
Assets/_Game/Scripts/Bestiary/Discovery/DiscoveryDeduplicationPolicy.cs
Assets/_Game/Tests/EditMode/Bestiary/KnowledgeDiscoveryEventRuntimeTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Knowledge/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Bestiary/**
docs/validation/13_spec_knowledge_discovery_event_runtime_future_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Enemies/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Magic/**
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
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia

```text
1. Auditar event contracts and existing discovery hooks.
2. Consolidar discovery source/event/rule/result.
3. Implementar/harden router and dedupe.
4. Integrar read-only with knowledge state service.
5. Criar tests.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - GameEventBus core changes;
  - knowledge save state foundation;
  - combat damage formula changes;
  - loot table changes.
- Reason: event routing touches cross-domain signals.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if KnowledgeState exists; otherwise STOP.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds event subscribers: YES if router subscribes.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED if wired to live combat/cave events.
```

---

## 20. Riscos

```text
Risco: event names mismatch.
Mitigação: local audit and adapters.

Risco: accidental spoiler.
Mitigação: spoiler gate tests.

Risco: repeated events inflate mastery.
Mitigação: dedupe thresholds.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar event/discovery hooks.
- [ ] T003 — Consolidar discovery contracts.
- [ ] T004 — Implementar router/dedupe.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu Bestiary/Knowledge e directions de enemy/combat/equipment/UI/quest/save? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a knowledge discovery events foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Anti-spoiler | Bestiary/UI/tooltips não revelam vulnerabilidades, drops, boss, Arquivista, Pedra Negra, 100/101 ou final cedo? | Tests/visibility policy. | PARTIAL |
| Não cria stats | A spec não redefine enemy stats/vulnerabilities/drops? | Checklist no report. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Quest compatibility | Knowledge objectives/rewards usam quest contracts sem opacidade? | Tests/checklist. | PARTIAL |
| UI projection | UI só mostra conhecimento autorizado por KnowledgeState? | Tests/checklist. | BUILD_VALIDATED sem cenário |
| Pets/companions | Pet/companion hints são future hooks, não runtime obrigatório. | Checklist explícito. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/13_spec_knowledge_discovery_event_runtime_future_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "KnowledgeDiscovery|EnemySeen|EnemyDefeated|DamageTypeEffective|DamageTypeResisted|RareDropCollected|ImmunityTriggered|BossPhaseReached|KnowledgeUnlock" Assets/_Game/Scripts docs/design docs/specs
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

- Seeing enemy reveals full bestiary.
- Defeating enemy reveals all drops/vulnerabilities.
- Effective damage not recorded.
- Resistance/immunity not recorded.
- Rare drop shown before collection.
- NPC reveals full bestiary.
- Duplicate events inflate Mastered.
- Main quest spoiler bypasses gate.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Knowledge Discovery Event Runtime Future

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

- Changed deterministic logic: YES, event routing/dedupe/spoiler logic is deterministic.
- Requires EditMode tests: YES for observation/combat/effective/resisted/drop/NPC/quest/spoiler/dedupe tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED if integrated into live combat/cave gameplay events.
- Requires regression test: YES if fixing existing discovery event bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no event duplicate inflation; no spoiler leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/13_spec_knowledge_discovery_event_runtime_future_execution_report.md.
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
