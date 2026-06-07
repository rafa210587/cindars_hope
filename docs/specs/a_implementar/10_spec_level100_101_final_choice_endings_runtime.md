# SPEC — Level100 101 Final Choice Endings Runtime

> **Spec ID:** `10_spec_level100_101_final_choice_endings_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 10 — Main Progression / Fonte / Endgame  
> **Priority:** P0  
> **Type:** Runtime / Endgame / Level 100 101 / Final Choice / Endings  
> **Domain:** MainProgression / Level100Gate / Level101Access / Archivist / FinalChoice / Endings  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_10_MAIN_PROGRESSION_FONTE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere cave procedural generation, boss combat, final boss AI, endgame scene/prefab, postgame economy, Fonte visual final, save migration ou final UI/cutscene.  
> **Repo lock scope:** `Assets/_Game/Scripts/MainProgression/**`, `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Fonte/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/MainProgression/**`, `docs/validation/10_spec_level100_101_final_choice_endings_runtime_execution_report.md`  
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
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
> **Blocks:**  
  - boss gate runtime;
  - level 101 scene generation;
  - final boss combat;
  - Fonte final visual state;
  - postgame world modifiers.
  - ending UI.
> **Scope:** definir/endurecer contratos de Level100Gate, Level101Access, Archivist reveal, FinalChoiceState e ending effects sem implementar combate/cena final.  
> **Out of scope:** final boss combat, level 101 scene/prefab, cutscenes, full postgame economy, final balancing, visual effects.

---

# /speckit.specify

## 1. Contexto

Ato 4 conecta boss gate nível 100, acesso ao nível 101, Arco da Memória, Vaelrion, Sethra, Pedra Negra, Fonte, Mana, três luas, boss final e escolha final. O boss final é O Arquivista do Silêncio, resultado de Vaelrion + Arco da Memória + Pedra Negra + máquina bromeciana + ritual de Sethra.

Esta spec define os contratos de endgame, final choice e efeitos sistêmicos, sem implementar boss/cena/cutscene.

---

## 2. Problema

Sem contrato endgame:

```text
nível 101 pode abrir apenas por profundidade;
boss gate pode repetir first-time reward;
Arquivista pode aparecer antes do Ato 4;
final choice pode ser escolhida sem confirmação;
final choice pode aplicar duas vezes;
postgame pode quebrar economia/Mana/Fonte/caverna;
Anya pode ser restaurada incorretamente;
Proteger/Selar/Usar podem ser só texto sem efeitos sistêmicos.
```

---

## 3. Objetivo

Criar/endurecer:

```text
Level100GateState;
Level101AccessState;
ArchivistRevealState;
FinalChoiceState;
FinalChoiceType;
EndingEffectProfile;
PostGameWorldModifier;
FinalChoiceRequest/Result;
strong confirmation policy;
idempotency and save/load guardrails.
```

---

## 4. Regras de design

```text
Nível 101 não é farming loop.
Nível 101 não tem mineração comum, packs comuns ou rewards repetíveis comuns.
Arquivista é segredo de Ato 4.
Escolha final é Proteger, Selar ou Usar.
Anya não pode ser restaurada.
Proteger protege fragmentos sem usá-los como ferramenta.
Selar reforça o selo de Cindar.
Usar usa parte do Arco purificado sem restaurar Anya.
Final choice precisa confirmação forte.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero acessar nível 101 só após gates narrativos e de caverna.
Como main progression, quero registrar boss/final choice uma vez.
Como Fonte, quero receber estado final conforme escolha.
Como postgame, quero aplicar modificadores consistentes.
Como UI, quero confirmação forte e sem escolha acidental.
```

---

## 6. Escopo

Inclui:

```text
level 100 gate contract;
level 101 access contract;
Archivist reveal state;
final choice state/type;
ending effect profile;
postgame modifiers;
confirmation/idempotency rules;
tests/validators.
```

Não inclui:

```text
boss combat implementation;
level 101 scene/procedural runtime;
cutscene UI;
visual effects;
postgame content full implementation;
economy tuning final.
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

- Ato 4 conecta nível 100/101, Arco da Memória, Vaelrion, Sethra, Pedra Negra, Fonte, Mana, três luas, boss final e escolha final.
- Estrutura: abrir boss gate do nível 100, derrotar guardião, acessar nível 101, revelar sobreposição de camadas, ativar Arco, corromper processos, Arquivista, boss final, Fragmento da Esperança e escolher Proteger/Selar/Usar.
- Nível 101 contém santuário nymiriano, máquina bromeciana, Arco de Elyndor, Pedras Negras, Água Viva corrompida, raiz de Mana, ecos de Cindar, fragmentos de Anya e boss final.
- Boss final é O Arquivista do Silêncio, natureza: Vaelrion + Arco da Memória + Pedra Negra + máquina bromeciana + ritual de Sethra.
- Final Proteger: Fonte viva e estável, Mana floresce raramente, cidade preserva memória, pós-game espiritual/natural, menor tecnologia bromeciana.
- Final Selar: corrupção contida, Fonte limitada, caverna estável, recursos avançados bloqueados/reduzidos, tom melancólico.
- Final Usar: mais tecnologia bromeciana, Mana mais previsível, caverna pós-game com novas áreas, cidade progride materialmente, risco de repetir Bromécia.

### Deferred / future from directions

- Final boss combat.
- Level 101 scene generation.
- Cutscenes/dialogue final.
- Postgame content full implementation.
- Fonte visual final assets.
- Mana crop runtime.
- Economy postgame tuning.

### Explicitly not redefined here

- MainProgressionSection.
- FonteAnyaSection.
- CaveRun/BossGate systems.
- Quest reward/visibility.
- World lunar/festival systems.
- Save migration.

## 7. Modelo de domínio

### 7.1 Level100GateState

```text
Unknown
Locked
Hinted
RequirementsKnown
Available
Opened
GuardianDefeated
Consumed
Blocked
```

### 7.2 Level101AccessState

```text
Unknown
Forbidden
Hinted
Locked
Unlocked
Entered
Completed
PostGameLocked
PostGameOpen
```

### 7.3 ArchivistRevealState

```text
Hidden
Foreshadowed
NameKnown
ConvergenceStarted
Revealed
BossActive
Defeated
Resolved
```

### 7.4 FinalChoiceType

```text
None
Protect
Seal
Use
```

### 7.5 FinalChoiceState

```text
Unavailable
Hinted
Available
ConfirmationPending
ChosenProtect
ChosenSeal
ChosenUse
Applied
LockedPostGame
```

### 7.6 EndingEffectProfile

```text
EndingId
FinalChoiceType
FonteFinalState
ManaBloomPolicy
CavePostGamePolicy
CityMemoryPolicy
BromecianTechPolicy
AdvancedResourcePolicy
CorruptionContainmentPolicy
PostGameUnlockIds[]
PostGameLockIds[]
WarningTextKey
RequiresStrongConfirmation
```

### 7.7 FinalChoiceRequest

```text
ChoiceType
ActorId
Day
Time
MainProgressionSnapshot
FonteAnyaSnapshot
RequiredConfirmationToken
PreviewOnly
```

### 7.8 FinalChoiceResult

```text
Success
FailureReason
ChoiceApplied
EndingEffectProfileId
MainProgressionUpdated
FonteStateUpdated
PostGameModifiersApplied
AlreadyApplied
DebugNotes[]
```

---

## 8. Level 100/101 rules

```text
Level100Gate requires cave depth/progression plus story gates.
Level100Gate first-time reward cannot repeat.
Level101 requires GuardianDefeated + relevant story gates.
Level101 is not common procedural/farm loop.
Level101 blocks common mining, common enemy packs and common treasure.
Level101 contains authored lore/endgame rewards.
```

---

## 9. Final choice rules

```text
FinalChoice unavailable until Hope fragment route and final boss/endgame condition.
FinalChoice requires strong confirmation.
Default focus should not be destructive confirm.
Choice applies once.
Choice cannot be changed without explicit debug/new-game/future policy.
Choice writes MainProgression and FonteAnya final states via adapters.
Choice does not restore Anya.
Choice must show consequence preview without hidden numeric spoilers.
```

---

## 10. Ending effect profiles

### Protect

```text
FonteFinalState = FinalizedProtected
ManaBloomPolicy = RareNatural
CavePostGamePolicy = StableLimited
CityMemoryPolicy = Preserved
BromecianTechPolicy = Reduced
AdvancedResourcePolicy = SpiritualNatural
CorruptionContainmentPolicy = PurifiedGuarded
```

### Seal

```text
FonteFinalState = FinalizedSealed
ManaBloomPolicy = RareRestricted
CavePostGamePolicy = MoreStableReduced
CityMemoryPolicy = PreservedWithLoss
BromecianTechPolicy = LockedOrReduced
AdvancedResourcePolicy = Reduced
CorruptionContainmentPolicy = StrongSeal
```

### Use

```text
FonteFinalState = FinalizedUsed
ManaBloomPolicy = MorePredictableButRisky
CavePostGamePolicy = NewAreasRisk
CityMemoryPolicy = MaterialProgress
BromecianTechPolicy = Expanded
AdvancedResourcePolicy = ExpandedWithRisk
CorruptionContainmentPolicy = ManagedRisk
```

---

## 11. Spoiler and UI rules

```text
Before Act4:
  do not show Arquivista, final choices or level 101 details.

Before final choice available:
  do not show exact ending effects.

At final choice:
  show high-level consequences and warning.
  require strong confirmation.
  do not use same button accidental confirm.
```

---

## 12. Criteria

```text
Level100Gate/Level101Access states exist.
Archivist reveal staged.
FinalChoice state/type/result contracts exist.
Ending profiles exist for Protect/Seal/Use.
Choice applies once with confirmation.
Anya not restored.
Level101 not farm loop.
Tests cover gate prerequisites, no level101 early, final choice confirmation, final choice idempotency, ending profile and Anya guardrail.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/MainProgression/Endgame/Level100GateState.cs
Assets/_Game/Scripts/MainProgression/Endgame/Level101AccessState.cs
Assets/_Game/Scripts/MainProgression/Endgame/ArchivistRevealState.cs
Assets/_Game/Scripts/MainProgression/Endgame/FinalChoiceType.cs
Assets/_Game/Scripts/MainProgression/Endgame/FinalChoiceState.cs
Assets/_Game/Scripts/MainProgression/Endgame/EndingEffectProfile.cs
Assets/_Game/Scripts/MainProgression/Endgame/FinalChoiceService.cs
Assets/_Game/Scripts/MainProgression/Endgame/EndgameProgressionValidator.cs
Assets/_Game/Tests/EditMode/MainProgression/Level101FinalChoiceTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/MainProgression/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/MainProgression/**
docs/validation/10_spec_level100_101_final_choice_endings_runtime_execution_report.md
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
1. Auditar cave boss gate/level101/final choice references.
2. Consolidar endgame state contracts.
3. Implementar final choice request/result and ending profiles.
4. Implementar validators for level 101 and final choice.
5. Criar tests.
6. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - cave procedural generation;
  - boss combat;
  - FonteAnya final visual state;
  - final UI/cutscene;
  - save migration.
- Reason: final choice and level101 are high-impact terminal states.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if MainProgressionSection already stores endgame/final choice; otherwise STOP.
Does this add save section? NO.
Does this require migration? NO unless adding endgame/final choice state; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. Level101AccessUnlockedEvent, FinalChoiceAppliedEvent.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final; exposes confirmation/projection data.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for boss gate/final choice/endgame flow.
```

---

## 21. Riscos

```text
Risco: final choice accidental.
Mitigação: strong confirmation tests.

Risco: level101 farm loop.
Mitigação: validators.

Risco: ending applied twice.
Mitigação: idempotency tests.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar level100/101/final choice references.
- [ ] T003 — Consolidar endgame states.
- [ ] T004 — Implementar final choice service contracts.
- [ ] T005 — Implementar validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a level100/101 final choice/endings foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
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
| Report | Execution report foi criado? | `docs/validation/10_spec_level100_101_final_choice_endings_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Level100Gate|Level101|Archivist|Arquivista|FinalChoice|Protect|Seal|Use|EndingEffect|PostGame|FonteFinal" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a level100/101 final choice/endings existe ou foi criado de forma mínima
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

- Level101 unlocked by cave depth only.
- Level101 generates common mining/treasure/enemies.
- Archivist revealed before Act4.
- Final choice available before Hope/final boss.
- Final choice applies twice after reload.
- Final choice can be confirmed accidentally.
- Anya restored as NPC/companion.
- Postgame modifier breaks economy/Mana/Fonte.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Level100 101 Final Choice Endings Runtime

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


## 23G. Final Choice Matrix

| Choice | Fonte | Mana | Cave | City | Risk |
|---|---|---|---|---|---|
| Protect | viva/estável | rara/natural | estável limitada | memória preservada | menor tecnologia |
| Seal | limitada/selada | restrita | mais estável | perda melancólica | recursos reduzidos |
| Use | usada/parcial | previsível/risco | novas áreas | progresso material | repetir Bromécia |

## 23H. Level 101 Guardrail

```text
Level101 must not:
  generate common mining;
  generate common enemy packs;
  generate common repeat treasures;
  serve as gold/hour loop;
  reveal before Act4.
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

- Changed deterministic logic: YES, gate/final choice/idempotency/ending profile validation logic is deterministic.
- Requires EditMode tests: YES for gate/no-early-101/final-confirm/idempotency/ending/Anya guardrail tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for boss gate/final choice visual flow.
- Requires regression test: YES if fixing existing endgame/final choice bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no early 101; final choice applies once.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/10_spec_level100_101_final_choice_endings_runtime_execution_report.md.
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
