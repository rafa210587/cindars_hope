# SPEC — Bestiary Combat HUD Known Weakness Overlay Future Runtime

> **Spec ID:** `22_spec_bestiary_combat_hud_known_weakness_overlay_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 22 — Bestiary UI / HUD / Research Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Combat HUD / Knowledge Overlay  
> **Domain:** Bestiary HUD / Known Weakness Overlay / Target Info / Combat Readability  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_22_BESTIARY_UI_RESEARCH_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere combat stats, enemy AI, HUD core input/focus, prefabs/assets, pet runtime or vulnerability adapter definitions.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Combat/Bestiary/**`, `Assets/_Game/Scripts/Bestiary/**`, `Assets/_Game/Scripts/Combat/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/22_spec_bestiary_combat_hud_known_weakness_overlay_future_runtime_execution_report.md`  
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
  - equipment/spell known interaction UI;
  - research service unlocks;
  - combat UX final pass.
> **Scope:** definir/endurecer HUD/overlay futuro de conhecimento conhecido: target info, fraquezas/resistências conhecidas, rumour badges e spoiler-safe combat hints.  
> **Out of scope:** HUD prefab/layout, combat stat authoring, enemy AI, vulnerability adapter implementation.

---

# /speckit.specify

## 1. Contexto

O direction separa Bestiary HUD permanente como futuro. HUD de combate pode ajudar com conhecimento conhecido, mas não deve resolver combate, revelar segredos ou substituir leitura de inimigo. A primeira entrega pode ter hooks/placeholders, não HUD final.

---

## 2. Problema

Sem HUD policy:

```text
HUD revela fraqueza secreta ao mirar;
resistência aparece sem teste;
boss phase behavior aparece cedo;
overlay cobre gameplay;
pet/companion hint completa bestiário sozinho;
equipamento/magia parecem inúteis porque UI não mostra conhecimento confirmado;
debug target info vira UX final.
```

---

## 3. Objetivo

Criar/endurecer:

```text
BestiaryCombatOverlayViewModel;
TargetKnowledgeProjection;
KnownWeaknessIndicator;
KnownResistanceIndicator;
KnowledgeHintPriorityPolicy;
CombatKnowledgeSpoilerFilter;
KnowledgeOverlayDensityPolicy;
BestiaryHudAccessibilityPolicy.
```

---

## 4. Regras de design

```text
Overlay só mostra conhecimento conhecido ou rumor legitimamente recebido.
Overlay não revela stat bruto.
Overlay não substitui learning.
Overlay não cobre gameplay.
Rumor aparece como rumor, não certeza.
Boss/main quest overlay é mais restritivo.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero lembrar fraquezas já descobertas em combate.
Como combat UI, quero mostrar target info sem spoiler.
Como equipment/spell UI, quero consistência com known interactions.
Como accessibility, quero ícones acompanhados de texto/shape quando crítico.
Como QA, quero garantir que HUD não revela hidden data.
```

---

## 6. Escopo

Inclui:

```text
combat overlay view model;
target knowledge projection;
known weakness/resistance indicators;
hint priority;
spoiler filter;
density policy;
accessibility policy;
tests.
```

Não inclui:

```text
visual HUD prefab;
combat stat changes;
enemy AI;
pet/companion hint runtime;
vulnerability adapter changes.
```

## 7. Modelo de domínio

### 7.1 TargetKnowledgeProjection

```text
TargetEnemyId
KnowledgeState
Confidence
KnownNameTextKey
TemporaryAliasTextKey
KnownFamily
KnownThreatLevelMode
KnownWeaknessIndicators[]
KnownResistanceIndicators[]
KnownImmunityIndicators[]
KnownBehavioralWindows[]
KnownDropHints[]
BossSpoilerState
```

### 7.2 KnownWeaknessIndicator

```text
KnowledgeKey
WeaknessKind: Element | Status | AttackType | WeaponType | Material | BehaviorWindow
Confidence
TextKey
IconKey
CanShowInCombat
RequiresRecentConfirmation optional
```

### 7.3 KnowledgeOverlayDensityPolicy

```text
Hidden
Minimal
FocusedTarget
ExpandedOnHold
FullKnownOnly
DebugOnly
```

### 7.4 KnowledgeHintPriorityPolicy

```text
PriorityOrder:
  lethal warning known;
  known immunity;
  known resistance to equipped action;
  known weakness matching equipped action;
  behavior window known;
  drop hint non-combat low priority.
```

---

## 8. Overlay rules

```text
Unknown target:
  show ??? / silhouette / basic threat only if allowed.

Seen:
  show alias/family guess.

Fought:
  show observed attacks/status.

Confirmed weakness:
  show indicator.

Rumor:
  show uncertain marker.

Boss:
  hide phase/true name until relevant state.
```

---

## 9. Criteria

```text
Combat overlay contracts exist.
Indicators require discovered knowledge.
Density policy prevents clutter.
Accessibility policy avoids color-only critical state.
Boss spoiler filter exists.
Tests cover unknown, seen, rumor, confirmed weakness, resistance, immunity, boss hidden, overlay density and no raw stats.
```

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Combat/Bestiary/BestiaryCombatOverlayViewModel.cs
Assets/_Game/Scripts/UI/Combat/Bestiary/TargetKnowledgeProjection.cs
Assets/_Game/Scripts/UI/Combat/Bestiary/KnownWeaknessIndicator.cs
Assets/_Game/Scripts/UI/Combat/Bestiary/KnownResistanceIndicator.cs
Assets/_Game/Scripts/UI/Combat/Bestiary/KnowledgeHintPriorityPolicy.cs
Assets/_Game/Scripts/UI/Combat/Bestiary/CombatKnowledgeSpoilerFilter.cs
Assets/_Game/Scripts/UI/Combat/Bestiary/KnowledgeOverlayDensityPolicy.cs
Assets/_Game/Scripts/UI/Combat/Bestiary/BestiaryHudAccessibilityPolicy.cs
Assets/_Game/Tests/EditMode/UI/BestiaryCombatHudKnownWeaknessOverlayTests.cs
```

## 11. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Combat/Bestiary/**
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/UI/Accessibility/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/22_spec_bestiary_combat_hud_known_weakness_overlay_future_runtime_execution_report.md
```

## 12. Arquivos proibidos

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

# /speckit.tasks

## 13. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar combat HUD/bestiary systems.
- [ ] T003 — Consolidar overlay/projection contracts.
- [ ] T004 — Implementar filters/validators.
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

- Bestiary HUD permanente e tracking visual avançado de fraquezas são futuros.
- A UI só deve mostrar uma vulnerabilidade depois que o evento de descoberta ocorrer ou conhecimento for recebido por fonte legítima.
- Equipment UI e Spell UI só mostram known enemy interactions.
- Bosses e main quest têm spoiler control mais forte.
- Pet/companion ajudam com hints, mas não completam bestiário sozinhos.
- Acessibilidade exige não depender só de cor para estados críticos.

### Deferred / future from directions

- HUD prefab/layout.
- Combat stat tuning.
- Enemy AI.
- Pet/companion runtime hints.
- Final icon art.
- Vulnerability adapter changes.

### Explicitly not redefined here

- Combat formulas.
- Enemy data.
- Vulnerability definitions.
- Equipment/magic matching.
- Bestiary save schema.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu bestiary, UI, cave, combat, equipment, magic, loot/economy, quest, save e pets/companions directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a bestiary combat HUD known weakness overlay foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Reveal-only | Bestiary/UI/research revela apenas conhecimento descoberto/legítimo. | Tests/checklist. | BLOCKED se violar |
| Anti-spoiler | Boss, main quest, Pedra Negra, Arquivista, Level 101, drops raros e final não vazam cedo. | Tests/checklist. | PARTIAL |
| Knowledge confidence | Rumor/Partial/Confirmed/Mastered não são tratados como iguais. | Tests/checklist. | PARTIAL |
| UI não é fonte | UI/HUD/log/research não muta enemy stats, quest state, loot table ou knowledge sem evento/serviço válido. | Tests/checklist. | PARTIAL |
| Pet deferido | Nenhum pet runtime/HUD/data asset/save foi criado. | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/22_spec_bestiary_combat_hud_known_weakness_overlay_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "BestiaryCombatOverlay|TargetKnowledge|KnownWeakness|KnownResistance|KnowledgeOverlay|CombatKnowledgeSpoiler|TargetInfo" Assets/_Game/Scripts docs/design .specs
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
When o fluxo de bestiary combat HUD known weakness overlay projeta informação
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

- HUD reveals hidden weakness.
- Rumor shown as confirmed.
- Raw stats shown.
- Boss phase shown early.
- Overlay covers gameplay.
- Critical state relies only on color.
- Debug target info leaks.
- Pet/companion hint completes entry automatically.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Bestiary Combat HUD Known Weakness Overlay Future Runtime

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

- Changed deterministic logic: YES, overlay projection/filter/density logic is deterministic.
- Requires EditMode tests: YES for unknown/seen/rumor/weakness/resistance/immunity/boss/density/no-raw-stats tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for combat HUD visual validation.
- Requires regression test: YES if fixing existing HUD knowledge leak; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; HUD shows only known knowledge.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/22_spec_bestiary_combat_hud_known_weakness_overlay_future_runtime_execution_report.md.
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
