# SPEC — Knowledge Research NPC Books Ruins Services Future Runtime

> **Spec ID:** `13_spec_knowledge_research_npc_books_ruins_services_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 13 — Bestiary / Knowledge Discovery / Future UI  
> **Priority:** P2  
> **Type:** Runtime / Future / City Services / Research / Books / Ruins  
> **Domain:** Knowledge Research / NPC Services / Books Documents Ruins / Rumor Confidence  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_13_BESTIARY_KNOWLEDGE_FUTURE  
> **Can run with:** bestiary UI projection if lock scopes do not overlap.  
> **Must not run with:** qualquer spec que altere dialogue runtime deeply, city service economy, quest reward engine, main progression gates, UI prefabs, pet/companion runtime ou save migration.  
> **Repo lock scope:** `Assets/_Game/Scripts/Knowledge/**`, `Assets/_Game/Scripts/Bestiary/**`, `Assets/_Game/Scripts/City/**`, `Assets/_Game/Scripts/Dialogue/**`, `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Tests/EditMode/Bestiary/**`, `docs/validation/13_spec_knowledge_research_npc_books_ruins_services_future_runtime_execution_report.md`  
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
  - research UI;
  - archive service;
  - dialogue knowledge rewards;
  - quest research objectives;
  - bestiary projection.
> **Scope:** definir/endurecer serviços futuros de pesquisa/conhecimento via NPC, livros, documentos, ruínas, mapas e quests, com confidence e spoiler gates.  
> **Out of scope:** full dialogue writing, final economy pricing, UI visual screen, companion/pet hints runtime, quest content, save migration.

---

# /speckit.specify

## 1. Contexto

O direction lista NPCs e fontes de conhecimento: ferreiro, mago/estudioso, caçador/minerador, Padre Corvus, Yael, Sethra, Vaelrion, livros da cidade, diários de Cindar, inscrições nymirianas, ruínas de Elyndor, registros bromecianos, mapas incompletos, anotações e cartazes. Essas fontes podem revelar rumor, partial ou confirmed, mas não todo o bestiário automaticamente.

Esta spec cria os contratos de research services.

---

## 2. Problema

Sem contrato:

```text
NPC pode revelar bestiário inteiro;
livro pode revelar mentira sistêmica sem pista;
Sethra pode revelar conteúdo perigoso como verdade;
Vaelrion pode revelar Arco/constructs sem viés registrado;
Corvus pode revelar Anya demais cedo;
Yael pode confundir Nyx com culto sem nuance;
research service pode burlar quest/main gates;
guia comprado pode revelar drop raro/weakness sem limite.
```

---

## 3. Objetivo

Criar/endurecer:

```text
KnowledgeResearchSource;
ResearchProviderDefinition;
ResearchServiceDefinition;
DocumentKnowledgeSource;
RuinKnowledgeSource;
ResearchGrantRule;
ResearchCostPolicy;
ResearchResult;
ResearchBias/Reliability;
SpoilerGatePolicy.
```

---

## 4. Regras de design

```text
NPC pode revelar parcial, rumor ou confirmado.
NPC não revela automaticamente todo o bestiário.
Documento antigo pode ser incompleto, enviesado ou parcialmente errado, mas não deve frustrar com mentira sistêmica sem pista.
Sethra pode ter informação perigosa/manipulada.
Vaelrion tem conhecimento e viés/arrogância.
Corvus protege mortos-vivos/corrupção/Anya/Kanthor com gate.
Yael distingue Nyx de culto e revela segredos/sombra com gate.
Pets/companions são future hints leves, não solução completa.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero aprender por NPC/livro/ruína sem grind excessivo.
Como research service, quero conceder conhecimento com confidence/source.
Como main quest, quero proteger spoilers.
Como city services, quero vender guias/aulas sem quebrar descoberta.
Como quest, quero validar rumor confirmado por combate ou pesquisa.
```

---

## 6. Escopo

Inclui:

```text
research provider/source contracts;
NPC/document/ruin source rules;
research grant rules;
confidence/reliability/bias;
cost/unlock policy;
spoiler gate;
quest integration hooks;
tests.
```

Não inclui:

```text
full dialogue text;
research UI;
final service prices;
companion/pet hints runtime;
quest content final;
archive minigame.
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

- NPCs podem revelar conhecimento por diálogo, serviço, quest, rumor, aula, compra de guia, recompensa de relacionamento futuro, evento de cidade e loja especializada.
- NPCs possíveis incluem ferreiro, mago/estudioso, caçador/minerador, Padre Corvus, Yael, Sethra, Vaelrion e companion especialista futuro.
- NPC pode revelar conhecimento parcial, rumor ou confirmado dependendo da fonte; NPC não deve revelar automaticamente todo o bestiário.
- Livros/documentos/ruínas incluem livros da cidade, diários de Cindar, inscrições nymirianas, ruínas de Elyndor, registros bromecianos, mapas incompletos, anotações de Vaelrion, notas de caçadores e cartazes.
- Documento antigo pode estar incompleto, enviesado ou errado parcialmente, mas não deve frustrar o jogador com mentira sistêmica sem pista.
- Pet/companion ajudam a descobrir, mas não jogam pelo jogador e não completam bestiário sozinhos.

### Deferred / future from directions

- Research UI/lab.
- Full dialogue writing.
- Final price/economy tuning.
- Relationship reward system.
- Pet/companion hint runtime.
- Archive minigame.

### Explicitly not redefined here

- Dialogue runtime base.
- City services contract.
- Quest objective/reward engine.
- Knowledge state save.
- Main progression/Fonte gates.
- Economy final balance.

## 7. Modelo de domínio

### 7.1 KnowledgeResearchSourceType

```text
NpcDialogue
NpcService
ClassLesson
GuidePurchase
Book
Diary
Inscription
Ruin
BromecianRecord
IncompleteMap
VaelrionNote
HunterNote
OrderPoster
QuestReward
RelationshipFuture
PetHintFuture
CompanionHintFuture
```

### 7.2 ResearchProviderDefinition

```text
ProviderId
ProviderNpcId optional
ProviderServiceId optional
ProviderBuildingId optional
ExpertiseTags[]
AllowedKnowledgeCategories[]
DefaultConfidence
Reliability
BiasTags[]
RequiredRelationship optional
RequiredReputation optional
RequiredQuestFlags[]
ForbiddenQuestFlags[]
PricePolicy optional
SpoilerGatePolicyId
```

### 7.3 ResearchGrantRule

```text
ResearchGrantId
SourceType
ProviderId optional
DocumentId optional
RuinId optional
KnowledgeKey
CategoryGranted
ConfidenceGranted
StateGranted
IsRumor
RequiresConfirmation
CanRevealExactName
CanRevealBossHint
CanRevealDrop
CanRevealVulnerability
SpoilerTier
FailureOrAmbiguityTextKey optional
```

### 7.4 ResearchResult

```text
Success
BlockedReason
KnowledgeGranted[]
Confidence
Reliability
RequiresConfirmation
KnownBias
CostPaid
QuestUpdated optional
Warnings[]
```

---

## 8. Provider mapping

```text
Ferreiro:
  materiais, armor, fraquezas físicas, repair/upgrade hints.

Mago/estudioso:
  magia, elementos, Arcane, Senya, constructs if authored.

Caçador/minerador:
  habitat, drops, comportamento físico.

Padre Corvus:
  mortos-vivos, corrupção, Anya/Kanthor, proteção; strong spoiler gates.

Yael:
  Nyx, segredos, sombra, criaturas noturnas, diferença Nyx vs culto.

Sethra:
  conhecimento perigoso, Pedra Negra, informação ambígua/manipulada; high bias.

Vaelrion:
  Elyndor, Bromécia, constructs, Arco da Memória; high expertise and arrogance bias.

Thalindra/Archive:
  documents, translation, lore, research services.
```

---

## 9. Reliability/bias rules

```text
ReliableConfirmed:
  trusted research or completed quest.

ReliablePartial:
  good source but incomplete.

Rumor:
  unverified, show as rumor.

Biased:
  source has agenda or limited worldview.

Manipulated:
  may mislead but must have narrative/system clues.

Debug:
  not available in final.
```

---

## 10. Cost and unlock rules

```text
Guide purchase can reveal limited category, not all entries.
Class lesson can reveal family-level hints.
Archive research can reveal document/lore/habitat.
High spoiler knowledge requires quest/story gate.
Research cannot reveal final boss/101/Anya complete before main gate.
Research can be used as quest objective/reward through Quest system.
```

---

## 11. Criteria

```text
Research provider/source contracts exist.
NPC/document/ruin sources grant confidence/source, not raw full knowledge.
Bias/reliability modeled.
Spoiler gates block main sensitive content.
No pet/companion runtime implemented.
Tests cover provider gates, rumor vs confirmed, biased source, document partial, Corvus/Yael/Sethra/Vaelrion gates and cost-limited research.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Knowledge/Research/KnowledgeResearchSourceType.cs
Assets/_Game/Scripts/Knowledge/Research/ResearchProviderDefinition.cs
Assets/_Game/Scripts/Knowledge/Research/ResearchServiceDefinition.cs
Assets/_Game/Scripts/Knowledge/Research/ResearchGrantRule.cs
Assets/_Game/Scripts/Knowledge/Research/ResearchResult.cs
Assets/_Game/Scripts/Knowledge/Research/ResearchReliability.cs
Assets/_Game/Scripts/Knowledge/Research/KnowledgeResearchService.cs
Assets/_Game/Tests/EditMode/Bestiary/KnowledgeResearchServicesTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Knowledge/**
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/Dialogue/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Bestiary/**
docs/validation/13_spec_knowledge_research_npc_books_ruins_services_future_runtime_execution_report.md
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
1. Auditar NPC/dialogue/city service/knowledge references.
2. Consolidar research source/provider/rule contracts.
3. Implementar/harden research service gate/result.
4. Modelar reliability/bias.
5. Criar tests.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: YES
- Can run with:
  - UI projection if no same files;
- Must not run with:
  - dialogue runtime rewrite;
  - city services schema changes;
  - quest reward engine;
  - pet/companion runtime.
- Reason: research services consume several systems but can remain adapter-like.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO; grants route through KnowledgeState.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. KnowledgeResearchCompletedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final; exposes research result data.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED if integrated with dialogue/service UI.
```

---

## 20. Riscos

```text
Risco: NPC reveals too much.
Mitigação: gates/tests.

Risco: biased source frustrates player.
Mitigação: bias must be signaled.

Risco: service bypasses economy/quest gates.
Mitigação: required flags/cost policy.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar research/dialogue/city service hooks.
- [ ] T003 — Consolidar source/provider/rule contracts.
- [ ] T004 — Implementar service gates/results.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu Bestiary/Knowledge e directions de enemy/combat/equipment/UI/quest/save? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a knowledge research/NPC/books/ruins services foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Anti-spoiler | Bestiary/UI/tooltips não revelam vulnerabilidades, drops, boss, Arquivista, Pedra Negra, 100/101 ou final cedo? | Tests/visibility policy. | PARTIAL |
| Não cria stats | A spec não redefine enemy stats/vulnerabilities/drops? | Checklist no report. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Quest compatibility | Knowledge objectives/rewards usam quest contracts sem opacidade? | Tests/checklist. | PARTIAL |
| UI projection | UI só mostra conhecimento autorizado por KnowledgeState? | Tests/checklist. | BUILD_VALIDATED sem cenário |
| Pets/companions | Pet/companion hints são future hooks, não runtime obrigatório. | Checklist explícito. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/13_spec_knowledge_research_npc_books_ruins_services_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "ResearchProvider|KnowledgeResearch|Book|Diary|Inscription|Ruin|Corvus|Yael|Sethra|Vaelrion|Rumor|Reliability|Bias" Assets/_Game/Scripts docs/design docs/specs
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

- NPC reveals entire bestiary.
- Document lies systemically without clue.
- Sethra source marked confirmed/trusted incorrectly.
- Vaelrion bias not represented.
- Corvus reveals Anya/Arquivista too early.
- Paid guide reveals boss/drop/weakness too much.
- Pet/companion runtime implemented accidentally.
- Research bypasses story/quest gates.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Knowledge Research NPC Books Ruins Services Future Runtime

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

- Changed deterministic logic: YES, research gate/result/confidence logic is deterministic.
- Requires EditMode tests: YES for provider/gate/rumor/confirmed/bias/cost/main-spoiler tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED if integrated with dialogue/service UI.
- Requires regression test: YES if fixing existing research/service bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; sources gated; no full bestiary reveal.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/13_spec_knowledge_research_npc_books_ruins_services_future_runtime_execution_report.md.
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
