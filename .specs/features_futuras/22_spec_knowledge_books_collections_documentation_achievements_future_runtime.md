# SPEC — Knowledge Books Collections Documentation Achievements Future Runtime

> **Spec ID:** `22_spec_knowledge_books_collections_documentation_achievements_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 22 — Bestiary UI / HUD / Research Future  
> **Priority:** P2  
> **Type:** Runtime / Future / Collections / Books / Achievements  
> **Domain:** Knowledge Books / Collections / Encyclopedia / Documentation Achievements / Lore-safe Rewards  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_22_BESTIARY_UI_RESEARCH_FUTURE  
> **Can run with:** compendium UI and research service specs if no same files.  
> **Must not run with:** qualquer spec que altere achievement platform APIs, save schema, loot tables, quest content, scene assets or localization files.  
> **Repo lock scope:** `Assets/_Game/Scripts/Bestiary/Collections/**`, `Assets/_Game/Scripts/Books/**`, `Assets/_Game/Scripts/Achievements/**`, `Assets/_Game/Tests/EditMode/Bestiary/**`, `docs/validation/22_spec_knowledge_books_collections_documentation_achievements_future_runtime_execution_report.md`  
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
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
> **Blocks:**  
  - knowledge compendium UI;
  - quest/research reward hooks;
  - lore book UI;
  - future achievement integration.
> **Scope:** definir/endurecer livros/coleções/enciclopédia/achievements futuros de documentação, com spoiler gates e rewards seguros.  
> **Out of scope:** platform achievements, final books text, localization files, visual collection UI, save migration.

---

# /speckit.specify

## 1. Contexto

O direction coloca achievements de documentação, coleções/livros/enciclopédia completos e knowledge log navegável como futuros. Livros, documentos e ruínas podem revelar lore note, habitat, família, faction, vulnerabilidade parcial, boss hint e quest knowledge. Documento antigo pode ser incompleto/enviesado sem mentira sistêmica frustrante.

---

## 2. Problema

Sem collections/achievements contract:

```text
achievement revela inimigo secreto;
livro desbloqueia drop raro completo;
coleção exige grind excessivo;
enciclopédia mostra boss antes da main quest;
reward de documentação dá poder obrigatório;
livro enviesado parece verdade confirmada;
save/reload duplica reward de coleção;
achievement platform é chamado direto sem abstração.
```

---

## 3. Objetivo

Criar/endurecer:

```text
KnowledgeBookEntry;
KnowledgeCollectionSet;
DocumentationMilestone;
KnowledgeAchievementDefinition;
CollectionRewardPolicy;
LoreDocumentBiasPolicy;
CollectionSpoilerGate;
DocumentationProgressProjection.
```

---

## 4. Regras de design

```text
Books/collections revelam conhecimento limitado e gated.
Achievement não revela nome secreto cedo.
Rewards de documentação são convenience/cosmetic/lore, não poder obrigatório.
Milestones evitam grind excessivo.
Rumor/partial/confirmed/mastered preservados.
Platform achievement integration é future adapter, não chamada direta.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero colecionar livros e completar conhecimento por descoberta.
Como lore, quero documentos nymirianos/bromecianos/caçadores com viés seguro.
Como achievement, quero premiar documentação sem spoiler.
Como quest/research, quero usar collection milestones.
Como save/load, quero rewards idempotentes.
```

---

## 6. Escopo

Inclui:

```text
book entry contracts;
collection sets;
documentation milestones;
achievement definitions;
reward policy;
bias policy;
spoiler gate;
progress projection;
tests.
```

Não inclui:

```text
final book prose;
platform APIs;
localization files;
visual UI;
save migration.
```

## 7. Modelo de domínio

### 7.1 KnowledgeBookEntry

```text
BookId
SourceType: CityBook | CindarDiary | NymirianInscription | ElyndorRuin | BromecianRecord | IncompleteMap | VaelrionNotes | HunterNotes | OrderPoster
KnowledgeUnlocks[]
ConfidenceGranted
BiasPolicyId
SpoilerTier
RequiresQuestState optional
CanBeReRead
```

### 7.2 KnowledgeCollectionSet

```text
CollectionId
CollectionType: EnemyFamily | Biome | Faction | Bosses | Drops | LoreArchive | Corruption | Bromecian | Nymirian | HunterFieldNotes
RequiredKnowledgeKeys[]
RequiredConfidence
OptionalKnowledgeKeys[]
RewardPolicyId
SpoilerTier
```

### 7.3 DocumentationMilestone

```text
MilestoneId
CollectionId
RequiredCount
RequiredSections[]
RequiredConfidence
RewardPolicyId
CanShowBeforeComplete
HiddenUntilDiscovery
```

### 7.4 KnowledgeAchievementDefinition

```text
AchievementId
AchievementType: SeenCount | DefeatedCount | FullyDocumentedCount | CollectionComplete | ResearchComplete | LoreArchiveComplete
SafeDisplayNameKey
HiddenDisplayNameKey
RequiresSpoilerGate
DoesNotCallPlatformDirectly true
```

### 7.5 CollectionRewardPolicy

```text
RewardPolicyId
RewardType: TitleFuture | CosmeticFuture | LoreNote | RecipeHint | EquipmentRecommendation | SpellRecommendation | SmallCurrency | ServiceDiscountLimited
DoesNotGrantMandatoryPower true
IdempotencyKey
```

---

## 8. Collection rules

```text
FullyDocumented requires identity, habitat, behavior, drops discovered/rumored as allowed, relevant vulnerabilities and lore where applicable.
Boss collection remains hidden/partial until main quest gates.
Corruption collection uses symptom entries before true source.
Bromecian/Nymirian books can be biased/incomplete.
Documentation reward once.
```

---

## 9. Achievement rules

```text
Hidden achievement name if spoiler.
No platform API call in domain.
Achievement unlock event is adapter-friendly.
Achievement reward does not reveal secret.
Achievement cannot require excessive grind beyond direction.
```

---

## 10. Criteria

```text
Book/collection/milestone/achievement contracts exist.
Spoiler-gated names exist.
Bias policy preserves rumor/partial.
Rewards idempotent and non-mandatory.
Tests cover book partial unlock, biased document, hidden boss collection, milestone progress, reward once, achievement hidden name, no platform direct call and no excessive grind rule.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Bestiary/Collections/KnowledgeBookEntry.cs
Assets/_Game/Scripts/Bestiary/Collections/KnowledgeCollectionSet.cs
Assets/_Game/Scripts/Bestiary/Collections/DocumentationMilestone.cs
Assets/_Game/Scripts/Bestiary/Collections/KnowledgeAchievementDefinition.cs
Assets/_Game/Scripts/Bestiary/Collections/CollectionRewardPolicy.cs
Assets/_Game/Scripts/Bestiary/Collections/LoreDocumentBiasPolicy.cs
Assets/_Game/Scripts/Bestiary/Collections/CollectionSpoilerGate.cs
Assets/_Game/Scripts/Bestiary/Collections/DocumentationProgressProjection.cs
Assets/_Game/Tests/EditMode/Bestiary/KnowledgeBooksCollectionsDocumentationAchievementsTests.cs
```

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Bestiary/Collections/**
Assets/_Game/Scripts/Books/**
Assets/_Game/Scripts/Achievements/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/UI/Bestiary/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Bestiary/**
docs/validation/22_spec_knowledge_books_collections_documentation_achievements_future_runtime_execution_report.md
```

## 13. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
Assets/Localization/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar books/collections/achievements systems.
- [ ] T003 — Consolidar collection contracts.
- [ ] T004 — Implementar gates/validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

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
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela deriva principalmente de BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.
Ela não cria stats, vulnerabilidades, drops, resistências, imunidades, enemy AI, equipment modifiers, spell effects ou economia.
Bestiary revela conhecimento já descoberto; não inventa conhecimento.
Bestiary UI/HUD/Research não pode revelar spoiler de boss, Pedra Negra, Arquivista do Silêncio, Level 100/101, Anya, Fonte, final choice ou drop raro não descoberto.
Pets continuam deferidos: pet pode aparecer apenas como future source/guardrail, não como runtime.
Quando houver conflito, CAVE/COMBAT/EQUIPMENT/MAGIC vencem para stats/effectiveness; BESTIARY vence para discovery/reveal; UI vence para presentation/focus.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Achievements de documentação, coleções/livros/enciclopédia completos são futuros.
- Livros/documentos/ruínas podem revelar lore note, habitat, família, faction, vulnerabilidade parcial, boss hint e quest knowledge.
- Fontes incluem livros da cidade, diários de Cindar, inscrições nymirianas, ruínas de Elyndor, registros bromecianos, mapas incompletos, anotações de Vaelrion, notas de caçadores e cartazes.
- Documento antigo pode estar incompleto, enviesado ou parcialmente errado, mas não deve frustrar o jogador sem pista.
- Quest rewards podem incluir knowledge unlock, bestiary entry, vulnerability hint, drop source hint, boss warning, recipe/equipment/spell recommendation.
- O sistema não deve exigir grind excessivo.

### Deferred / future from directions

- Final book prose.
- Platform achievement API.
- Localization files.
- Visual collection UI.
- Save migration.
- Final reward balance.

### Explicitly not redefined here

- Quest content.
- Loot tables.
- Enemy data.
- Research service execution.
- Compendium visual UI.
- Platform services.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu bestiary, UI, cave, combat, equipment, magic, loot/economy, quest, save e pets/companions directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a knowledge books/collections/documentation achievements foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Reveal-only | Bestiary/UI/research revela apenas conhecimento descoberto/legítimo. | Tests/checklist. | BLOCKED se violar |
| Anti-spoiler | Boss, main quest, Pedra Negra, Arquivista, Level 101, drops raros e final não vazam cedo. | Tests/checklist. | PARTIAL |
| Knowledge confidence | Rumor/Partial/Confirmed/Mastered não são tratados como iguais. | Tests/checklist. | PARTIAL |
| UI não é fonte | UI/HUD/log/research não muta enemy stats, quest state, loot table ou knowledge sem evento/serviço válido. | Tests/checklist. | PARTIAL |
| Pet deferido | Nenhum pet runtime/HUD/data asset/save foi criado. | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/22_spec_knowledge_books_collections_documentation_achievements_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "KnowledgeBook|KnowledgeCollection|DocumentationMilestone|KnowledgeAchievement|LoreDocument|CollectionReward|DocumentationProgress" Assets/_Game/Scripts docs/design .specs
rg -n "Bestiary|Knowledge|EnemyKnowledge|KnowledgeState|KnowledgeConfidence|Research|Laboratory|Study|FullyDocumented|Rumor|Partial|Confirmed|Mastered|Vulnerability|Resistance|Immunity|DropSource|BossHint|Archivist|Arquivista|BlackStone|PedraNegra|Level101|Pet" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

Classificar achados:

```text
EXISTING_CANONICAL
EXISTING_PARTIAL
MISSING_SAFE_TO_CREATE
MISSING_BUT_DEFER
CONFLICT
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given conhecimento foi descoberto por combate, drop, NPC, livro, quest, research ou fonte legítima
When o fluxo de knowledge books/collections/documentation achievements projeta informação
Then só categorias conhecidas aparecem
And rumor/partial/confirmed/mastered são diferenciados
And UI não altera stats, drops, quest state ou enemy data.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Unknown / spoiler-protected knowledge

```text
Given inimigo, boss, drop, vulnerabilidade, resistência, Arquivista, Pedra Negra, Level101 ou lore sensível não foi descoberto
When UI/HUD/research/tooltip/quest consulta informação
Then aparece Unknown/???/silhouette/rumor seguro ou fica oculto
And nome completo, stats e drops não aparecem cedo.
```

### Scenario 4 — Legitimate research unlock

```text
Given NPC/livro/laboratório/quest autoriza conhecimento parcial ou confirmado
When research/study completa com custo/tempo/requisito
Then conhecimento é desbloqueado de forma rastreável e idempotente
And não desbloqueia bestiário inteiro automaticamente.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de compendium, HUD overlay, research service, cards, books, collection ou tooltip
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Achievement name reveals secret boss.
- Book unlocks complete rare drop data.
- Collection requires excessive grind.
- Reward grants mandatory power.
- Biased document shown as mastered.
- Reward duplicates after reload.
- Platform API called from domain.
- Boss collection visible too early.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Knowledge Books Collections Documentation Achievements Future Runtime

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

## Bestiary compliance
- Reveal-only:
- Knowledge confidence preserved:
- Unknown stays hidden:
- Drop/vulnerability/resistance gating:
- Main quest spoiler safe:
- UI not source of truth:
- No stats/drops invented:
- Pet deferred:
- Save/load safe:
- Research unlock idempotent:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Unknown/spoiler protected:
- Legitimate research unlock:
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
2. A implementação exigir scene/prefab/tilemap/asset/font/icon/audio changes.
3. A implementação alterar save schema sem migration spec.
4. A implementação criar ou alterar stats reais, vulnerabilities, resistances, immunities, drops, AI ou loot tables.
5. A implementação revelar conhecimento não descoberto em UI/HUD/tooltip/research/log.
6. A implementação revelar Arquivista, Pedra Negra, Level101, Anya, Fonte, final choice ou boss main quest cedo.
7. A implementação usar pet como runtime source em vez de future/deferred guardrail.
8. A implementação desbloquear bestiário inteiro por NPC/livro/research sem regra canônica.
9. A implementação tornar HUD de bestiário obrigatório para combate básico.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Bestiary|Knowledge|EnemyKnowledge|Research|KnowledgeConfidence|Vulnerability|Resistance|Immunity|DropSource|Archivist|BlackStone|Level101" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de Bestiary UI, HUD, research service, knowledge cards, books, collections ou tooltips, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, collection/milestone/achievement/reward validation logic is deterministic.
- Requires EditMode tests: YES for partial-book/biased-doc/hidden-boss/milestone/reward-once/hidden-achievement/no-platform/no-grind tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for collection UI/books validation.
- Requires regression test: YES if fixing existing collection/achievement leak; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; collections/achievements are spoiler-safe.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/22_spec_knowledge_books_collections_documentation_achievements_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não inventar stats/drops/vulnerabilidades.
Não revelar conhecimento não descoberto.
Não tornar rumor igual a confirmado.
Não vazar boss/main quest/endgame cedo.
Não criar pet runtime.
Não editar scenes/prefabs/assets.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando enemy roster, combat/vulnerability adapter, quest events, save/load, UI foundation and knowledge discovery core estiverem estáveis ou quando houver decisão humana explícita de antecipar bestiary UI/research.
