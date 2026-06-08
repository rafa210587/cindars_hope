# SPEC — Research Service NPC Laboratory Knowledge Unlock Future Runtime

> **Spec ID:** `22_spec_research_service_npc_laboratory_knowledge_unlock_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 22 — Bestiary UI / HUD / Research Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Research Service / NPC Laboratory  
> **Domain:** Research Service / NPC Study / Laboratory / Knowledge Unlock / Costs  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_22_BESTIARY_UI_RESEARCH_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere NPC roster final, shop economy formulas, quest content, save migration, pets runtime or enemy data.  
> **Repo lock scope:** `Assets/_Game/Scripts/Bestiary/Research/**`, `Assets/_Game/Scripts/City/Services/**`, `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Tests/EditMode/Bestiary/**`, `docs/validation/22_spec_research_service_npc_laboratory_knowledge_unlock_future_runtime_execution_report.md`  
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
  - bestiarization quest objectives;
  - knowledge compendium UI;
  - NPC service UI;
  - books/collections future.
> **Scope:** definir/endurecer research service futuro por NPC/laboratório: requests, costs, samples, time, confidence, unlock caps e anti-spoiler.  
> **Out of scope:** NPC roster concrete content, dialogue writing, shop pricing formulas, UI prefab/layout, save migration.

---

# /speckit.specify

## 1. Contexto

O direction prevê UI de pesquisa/laboratório e NPC research services como futuro. NPCs podem revelar conhecimento por diálogo, serviço, quest, rumor, aula, guia comprado, relacionamento futuro, evento de cidade e loja especializada. Fontes incluem ferreiro, mago/estudioso, caçador/minerador, Corvus, Yael, Sethra, Vaelrion e companion especialista futuro.

---

## 2. Problema

Sem research service contract:

```text
NPC revela bestiário inteiro;
comprar guia revela boss/final cedo;
amostra não é consumida;
research completa instantaneamente sem custo;
serviço ignora confiança rumor/partial;
Sethra/Vaelrion revelam informação enviesada como verdade;
quest de pesquisa não reconhece conhecimento já descoberto;
save/reload duplica unlock.
```

---

## 3. Objetivo

Criar/endurecer:

```text
KnowledgeResearchRequest;
ResearchServiceProvider;
ResearchInputRequirement;
ResearchCostPolicy;
ResearchTimePolicy;
ResearchUnlockResult;
ResearchBiasProfile;
ResearchAntiSpoilerValidator;
ResearchIdempotencyRecord.
```

---

## 4. Regras de design

```text
Research desbloqueia conhecimento parcial/confirmado limitado.
Research não desbloqueia bestiário inteiro.
Research pode exigir amostra, drop, kill proof, quest, livro, relação ou custo.
NPC provider tem domínio e viés.
Main quest/boss/endgame precisam spoiler gate forte.
Resultado deve ser idempotente.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero pesquisar criatura sem grind excessivo.
Como NPC, quero oferecer conhecimento de domínio limitado.
Como quest, quero aceitar conhecimento prévio.
Como economy, quero custos controlados sem pagar para ganhar tudo.
Como lore, quero rumores/viés sem mentira sistêmica frustrante.
```

---

## 6. Escopo

Inclui:

```text
research request;
provider profile;
input requirements;
cost/time policies;
unlock result;
bias/confidence;
anti-spoiler validator;
idempotency record;
tests.
```

Não inclui:

```text
dialogue writing;
shop pricing final;
UI prefab;
NPC roster final authoring;
save migration.
```

## 7. Modelo de domínio

### 7.1 ResearchServiceProvider

```text
ProviderId
ProviderType: Blacksmith | Scholar | HunterMiner | PriestCorvus | Yael | Sethra | Vaelrion | CompanionSpecialistFuture | LaboratoryFuture | BookArchive
KnowledgeDomains[]
BiasProfile
MaxConfidenceGranted
ForbiddenSpoilerTiers[]
RelationshipGate optional
QuestGate optional
```

### 7.2 KnowledgeResearchRequest

```text
RequestId
ProviderId
TargetKnowledgeKey
RequestedSections[]
PlayerKnownStateSnapshot
InputRequirements[]
CostPolicyId
TimePolicyId
SpoilerTier
```

### 7.3 ResearchInputRequirement

```text
RequirementType: Gold | SampleItem | DropCollected | EnemyDefeatedCount | BookFound | QuestFlag | Relationship | CaveDepthReached | LunarEventKnown
RequiredIds[]
RequiredCount
Consumed
```

### 7.4 ResearchUnlockResult

```text
Allowed
Blocked
InProgress
Completed
AlreadyKnown
Unlocks[]
ConfidenceGranted
ProviderBiasApplied
ConsumedInputs[]
BlockedReasons[]
Warnings[]
```

### 7.5 ResearchBiasProfile

```text
BiasId
Reliability: Low | Medium | High
CanGrantRumor
CanGrantPartial
CanGrantConfirmed
CanGrantMastered false by default
MayBeIncomplete
MayBeBiased
RequiresConfirmationForMastery
```

---

## 8. Provider rules

```text
Blacksmith:
  materials, armor, physical weakness, repair.

Scholar:
  magic, elements, Arcane, Senya.

Hunter/Miner:
  habitat, drops, physical behavior.

Corvus:
  undead, corruption, Anya/Kanthor, protection, spoiler-gated.

Yael:
  Nyx, shadows, night creatures, difference between Nyx and cult.

Sethra:
  dangerous/ambiguous info, Black Stone rumors, biased.

Vaelrion:
  Elyndor, Bromecia, constructs, Memory Arch, arrogant/biased.

Laboratory:
  sample/time/cost based, future.
```

---

## 9. Criteria

```text
Research service contracts exist.
Providers have domains/bias.
Input/cost/time policies exist.
Anti-spoiler validator gates main quest/boss/endgame.
Research result idempotent.
Tests cover provider domain, missing sample, consumed sample, already known, biased rumor, confirmed unlock, boss blocked, reload no duplicate and quest recognizes prior knowledge.
```

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Bestiary/Research/KnowledgeResearchRequest.cs
Assets/_Game/Scripts/Bestiary/Research/ResearchServiceProvider.cs
Assets/_Game/Scripts/Bestiary/Research/ResearchInputRequirement.cs
Assets/_Game/Scripts/Bestiary/Research/ResearchCostPolicy.cs
Assets/_Game/Scripts/Bestiary/Research/ResearchTimePolicy.cs
Assets/_Game/Scripts/Bestiary/Research/ResearchUnlockResult.cs
Assets/_Game/Scripts/Bestiary/Research/ResearchBiasProfile.cs
Assets/_Game/Scripts/Bestiary/Research/ResearchAntiSpoilerValidator.cs
Assets/_Game/Scripts/Bestiary/Research/ResearchIdempotencyRecord.cs
Assets/_Game/Tests/EditMode/Bestiary/ResearchServiceNpcLaboratoryKnowledgeUnlockTests.cs
```

## 11. Arquivos permitidos

```text
Assets/_Game/Scripts/Bestiary/Research/**
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/City/Services/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/UI/Bestiary/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Bestiary/**
docs/validation/22_spec_research_service_npc_laboratory_knowledge_unlock_future_runtime_execution_report.md
```

## 12. Arquivos proibidos

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

# /speckit.tasks

## 13. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar research/NPC service/quest knowledge systems.
- [ ] T003 — Consolidar research contracts.
- [ ] T004 — Implementar anti-spoiler/idempotency validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

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

- UI de pesquisa/laboratório e NPC research services são futuros.
- NPCs podem revelar conhecimento por diálogo, serviço, quest, rumor, aula, compra de guia, relacionamento futuro, evento de cidade e loja especializada.
- Fontes incluem ferreiro, mago/estudioso, caçador/minerador, Corvus, Yael, Sethra, Vaelrion e companion especialista futuro.
- NPC pode revelar conhecimento parcial, rumor ou confirmado dependendo da fonte.
- Documento antigo pode estar incompleto/enviesado, mas não deve frustrar com mentira sistêmica sem pista.
- Quest de fraqueza deve reconhecer conhecimento já descoberto antes.

### Deferred / future from directions

- Dialogue writing.
- UI prefab.
- Concrete NPC roster service data.
- Shop/economy prices.
- Save migration.
- Laboratory visual assets.

### Explicitly not redefined here

- Enemy stats.
- NPC roster authoring.
- Quest content.
- Shop pricing.
- Knowledge save schema.
- Pet runtime.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu bestiary, UI, cave, combat, equipment, magic, loot/economy, quest, save e pets/companions directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a research service/NPC laboratory knowledge unlock foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Reveal-only | Bestiary/UI/research revela apenas conhecimento descoberto/legítimo. | Tests/checklist. | BLOCKED se violar |
| Anti-spoiler | Boss, main quest, Pedra Negra, Arquivista, Level 101, drops raros e final não vazam cedo. | Tests/checklist. | PARTIAL |
| Knowledge confidence | Rumor/Partial/Confirmed/Mastered não são tratados como iguais. | Tests/checklist. | PARTIAL |
| UI não é fonte | UI/HUD/log/research não muta enemy stats, quest state, loot table ou knowledge sem evento/serviço válido. | Tests/checklist. | PARTIAL |
| Pet deferido | Nenhum pet runtime/HUD/data asset/save foi criado. | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/22_spec_research_service_npc_laboratory_knowledge_unlock_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "ResearchService|KnowledgeResearch|ResearchProvider|Laboratory|ResearchUnlock|ResearchBias|Corvus|Yael|Sethra|Vaelrion" Assets/_Game/Scripts docs/design docs/specs
rg -n "Bestiary|Knowledge|EnemyKnowledge|KnowledgeState|KnowledgeConfidence|Research|Laboratory|Study|FullyDocumented|Rumor|Partial|Confirmed|Mastered|Vulnerability|Resistance|Immunity|DropSource|BossHint|Archivist|Arquivista|BlackStone|PedraNegra|Level101|Pet" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
When o fluxo de research service/NPC laboratory knowledge unlock projeta informação
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

- NPC unlocks entire bestiary.
- Research reveals boss/endgame early.
- Sample not consumed.
- Reload duplicates unlock.
- Biased rumor treated as confirmed.
- Provider domain ignored.
- Quest ignores prior knowledge.
- Gold buys mandatory combat solution too early.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Research Service NPC Laboratory Knowledge Unlock Future Runtime

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
rg -n "Bestiary|Knowledge|EnemyKnowledge|Research|KnowledgeConfidence|Vulnerability|Resistance|Immunity|DropSource|Archivist|BlackStone|Level101" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES, research request/cost/time/unlock/spoiler logic is deterministic.
- Requires EditMode tests: YES for provider/missing-sample/consume/already-known/rumor/confirmed/boss/reload/quest tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for research service UI validation.
- Requires regression test: YES if fixing existing research unlock bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; research unlocks are limited and spoiler-safe.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/22_spec_research_service_npc_laboratory_knowledge_unlock_future_runtime_execution_report.md.
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
