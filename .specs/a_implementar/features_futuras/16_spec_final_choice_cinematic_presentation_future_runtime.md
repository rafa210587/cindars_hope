# SPEC — Final Choice Cinematic Presentation Future Runtime

> **Spec ID:** `16_spec_final_choice_cinematic_presentation_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 16 — Cave Save Policy / Endgame Presentation / Postgame Future  
> **Priority:** P2  
> **Type:** Runtime / Future / UI Presentation / Endgame Cinematic  
> **Domain:** Final Choice / Cinematic Sequence / Confirmation / Presentation / Accessibility  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_16_CAVE_SAVE_ENDGAME_FUTURE  
> **Can run with:** postgame modifiers if no same files and final choice state is stable.  
> **Must not run with:** qualquer spec que altere final choice domain state, ending effects, scene/prefab/cutscene assets, save migration, boss combat or romance/social runtime.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Endgame/**`, `Assets/_Game/Scripts/MainProgression/Endgame/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/16_spec_final_choice_cinematic_presentation_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
> **Blocks:**  
  - final choice UI;
  - ending effect application;
  - postgame modifiers;
  - credits/epilogue;
  - final human validation.
> **Scope:** definir/endurecer presentation layer futura para final choice/cinematic: preview, warning, confirmation, cinematic steps, accessibility and no-spoiler staging.  
> **Out of scope:** cutscene assets, timeline/cinemachine, final visual effects, final choice domain logic, ending modifiers.

---

# /speckit.specify

## 1. Contexto

A escolha final é Proteger / Selar / Usar. O direction manda manter finais moralmente diferentes, não restaurar Anya completamente e não explicar tudo por diálogo expositivo. Specs anteriores definiram contratos de FinalChoiceType e EndingEffectProfile; esta spec é só a apresentação/cinematic futura.

---

## 2. Problema

Sem presentation contract:

```text
jogador confirma final por acidente;
preview mostra números/spoilers demais;
cinematic aplica estado ao invés de só apresentar;
choice UI aparece antes do gate;
cutscene não respeita pause/input/foco;
reload no meio da cinematic duplica final;
acessibilidade/skip não preserva aplicação de estado;
final choice vira diálogo expositivo longo demais.
```

---

## 3. Objetivo

Criar/endurecer:

```text
FinalChoicePresentationViewModel;
FinalChoicePreviewProjection;
FinalChoiceConfirmationPolicy;
EndgameCinematicStep;
CinematicPlaybackState;
CinematicSaveBoundaryPolicy;
EndgamePresentationEvent;
FinalChoiceAccessibilityPolicy.
```

---

## 4. Regras de design

```text
Presentation não aplica final; domain service aplica.
Final choice exige confirmação forte.
Preview mostra consequências qualitativas, não spoilers numéricos completos.
Cinematic pode ser skipável sem quebrar estado.
Skip não duplica reward/final.
Cinematic não restaura Anya como NPC.
Cinematic não explica tudo por diálogo expositivo.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero entender Proteger/Selar/Usar antes de confirmar.
Como UI, quero confirmação forte e segura.
Como cinematic, quero mostrar consequência sem virar fonte de estado.
Como save/load, quero lidar com reload/skip no meio da sequência.
Como acessibilidade, quero permitir skip/replay controlado sem quebrar final.
```

---

## 6. Escopo

Inclui:

```text
presentation view models;
preview projection;
confirmation policy;
cinematic step contracts;
skip/replay policy;
save boundary policy;
accessibility flags;
tests.
```

Não inclui:

```text
Timeline/Cinemachine/assets;
visual effects;
final choice domain application;
ending modifiers;
boss combat;
credits final.
```

## 7. Modelo de domínio

### 7.1 FinalChoicePresentationViewModel

```text
ChoiceOptions[]
SelectedChoice
Preview
WarningTextKey
RequiresStrongConfirmation
ConfirmationTokenPolicy
CanConfirm
BlockedReason
AccessibilityOptions
```

### 7.2 FinalChoicePreviewProjection

```text
ChoiceType
FonteOutcomeSummary
ManaOutcomeSummary
CityOutcomeSummary
CaveOutcomeSummary
BromecianTechOutcomeSummary
RiskSummary
HiddenDetailsRedacted
SpoilerTier
```

### 7.3 FinalChoiceConfirmationPolicy

```text
RequiresTwoStepConfirm
RequiresHoldConfirm optional
RequiresTypedToken optional
DefaultFocusOnCancel
CannotConfirmDuringTransition
ConfirmationCooldownFrames
```

### 7.4 EndgameCinematicStep

```text
StepId
StepType: Fade | DialogueBeat | MemoryImage | FonteReaction | CityEcho | CaveEcho | EndingCard | CreditsHook
RequiredState
CanSkip
CanReplayFromGalleryFuture
DoesApplyGameState false
```

### 7.5 CinematicSaveBoundaryPolicy

```text
BeforeChoice
AfterChoiceAppliedBeforeCinematic
DuringCinematic
AfterCinematicComplete
CanSave
CanLoadResume
FallbackOnLoad
```

---

## 8. Confirmation rules

```text
Default focus must not be confirm.
Cancel must be available until final confirmation.
Final confirmation must be explicit.
After domain applies choice, UI cannot change choice.
If player skips cinematic, ending remains applied exactly once.
```

---

## 9. Cinematic rules

```text
Cinematic presents consequences.
Cinematic does not mutate gameplay state.
Cinematic can request presentation events only.
Cinematic does not restore Anya.
Cinematic does not expose undiscovered lore outside chosen path.
Cinematic should be replayable later only as presentation, not state.
```

---

## 10. Save/load boundary

```text
Before choice:
  no ending applied.

After choice applied:
  save should preserve applied final choice and pending cinematic/completed cinematic.

During cinematic:
  load resumes, restarts presentation, or skips to postgame according to policy; never reapplies final state twice.

After cinematic:
  postgame state visible.
```

---

## 11. Criteria

```text
Presentation contracts exist.
Confirmation policy strong.
Preview redacts hidden details.
Cinematic steps do not apply state.
Save boundary policy handles reload/skip.
Tests cover confirmation default focus, preview redaction, skip idempotency, load during cinematic, no state mutation from cinematic and no Anya restoration.
```

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Endgame/FinalChoicePresentationViewModel.cs
Assets/_Game/Scripts/UI/Endgame/FinalChoicePreviewProjection.cs
Assets/_Game/Scripts/UI/Endgame/FinalChoiceConfirmationPolicy.cs
Assets/_Game/Scripts/UI/Endgame/EndgameCinematicStep.cs
Assets/_Game/Scripts/UI/Endgame/CinematicPlaybackState.cs
Assets/_Game/Scripts/UI/Endgame/CinematicSaveBoundaryPolicy.cs
Assets/_Game/Scripts/UI/Endgame/FinalChoicePresentationValidator.cs
Assets/_Game/Tests/EditMode/UI/FinalChoiceCinematicPresentationTests.cs
```

Consolidar existentes se houver.

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Endgame/**
Assets/_Game/Scripts/MainProgression/Endgame/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/16_spec_final_choice_cinematic_presentation_future_runtime_execution_report.md
```

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

# /speckit.tasks

## 15. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar final choice/UI/endgame state.
- [ ] T003 — Consolidar presentation/confirmation/cinematic contracts.
- [ ] T004 — Implementar validators.
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

- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela deriva principalmente de SAVE_LOAD_FULL_STATE_DIRECTION, CAVE_DESIGN_DIRECTION e QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.
Ela não reimplementa SaveManager do zero.
Ela não reescreve caverna procedural, boss fights, UI visual final, cenas/prefabs ou economy final.
Ela não cria runtime pet nem romance/social deep.
Quando houver conflito entre save/load, cave, main progression, UI, economy ou world directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Final choice é Proteger / Selar / Usar.
- Finais devem ser moralmente diferentes.
- Não restaurar Anya completamente nem como NPC comum.
- Não explicar tudo por diálogo expositivo.
- Quest log, Fonte UI, notificações, modal de decisão final e spoiler control ficam em UI/UX direction.
- UI state não é fonte de verdade e deve ser reconstruída a partir do runtime restaurado.

### Deferred / future from directions

- Cutscene assets.
- Timeline/Cinemachine.
- Final VFX/SFX.
- Credits screen.
- Ending modifiers domain application.

### Explicitly not redefined here

- FinalChoiceService.
- EndingEffectProfile.
- PostGameWorldState.
- Boss combat.
- Level101 gameplay.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu save/load, cave, main progression, UI e world/economy directions? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a final choice cinematic presentation foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Save policy | A mudança declara seção, dono, IDs, capture, preserve, restore order, migration e validação? | Checklist no report. | BLOCKED se persistir sem contrato |
| Cave snapshot | A mudança preserva CaveRunSeed, snapshots, boss defeat states, checkpoint e corpse/death state? | Tests/checklist. | PARTIAL |
| Endgame spoiler | Anya, Arquivista, nível 101, final choice e endings não vazam cedo? | Tests/visibility. | PARTIAL |
| Anti-farm | Nível 101/final/endgame não viram farm infinito, reward repeat ou boss repeat. | Tests/checklist. | PARTIAL |
| Pet/social | Nenhum runtime pet, romance ou social deep foi criado. | Checklist explícito. | BLOCKED se violar |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/16_spec_final_choice_cinematic_presentation_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "FinalChoicePresentation|FinalChoicePreview|CinematicPlayback|EndgameCinematic|ConfirmationPolicy|EndingCard|SkipCinematic" Assets/_Game/Scripts docs/design .specs
rg -n "SaveManager|GameSaveData|SchemaVersion|Migration|CaveSaveData|CaveRunSeed|VisitedLevelSnapshot|Checkpoint|Corpse|Death|Level101|FinalChoice|Ending|PostGame|Archivist|Arquivista|Anya|Fonte|Mana|BlackStone|Pet|Romance|Spouse" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue save/cave/main progression directions
And usa IDs estáveis
And não persiste referências Unity
And não cria pet/social deep runtime
And não promove estado visual/UI para fonte de verdade.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Anti-spoiler and staging

```text
Given o jogador ainda não desbloqueou gate 100, nível 101, Arquivista, final choice ou ending
When save/load/UI/calendar/quest/cave projection consulta o estado
Then detalhes ocultos permanecem escondidos
And apenas informação descoberta/autorizada aparece.
```

### Scenario 4 — Idempotency / no exploit

```text
Given boss, reward, final choice, ending modifier, cave checkpoint ou death/corpse state já foi aplicado
When reload, retry, scene reload ou reentrada ocorre
Then reward/effect não duplica
And estado terminal não regride sem política explícita.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de save/load, cave checkpoint, nível 101, cinematic, final choice ou postgame world
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Default focus on confirm.
- Accidental final choice.
- Cinematic applies state twice.
- Skip duplicates ending.
- Load during cinematic reapplies final.
- Preview reveals too much.
- Anya appears restored.
- Presentation mutates gameplay state.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Final Choice Cinematic Presentation Future Runtime

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

## Save/cave/endgame compliance
- Save section owner:
- Stable IDs:
- Capture policy:
- Preserve policy:
- Restore order:
- Migration needed:
- Unity references persisted:
- CaveRunSeed/snapshots preserved:
- Boss/reward idempotency:
- Endgame spoiler safe:
- No pet/social deep runtime:
- UI not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Anti-spoiler/staging:
- Idempotency/no exploit:
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
2. A implementação exigir scene/prefab/tilemap/asset wiring fora do escopo.
3. A implementação alterar save schema sem migration spec.
4. A implementação persistir ScriptableObject, GameObject, Transform, MonoBehaviour, Collider, Rigidbody, Sprite ou UI state como gameplay state.
5. A implementação reescrever SaveManager ou caverna procedural do zero.
6. A implementação rerollar snapshot estável sem política.
7. A implementação duplicar boss reward, final reward, ending modifier ou corpse/cave recovery.
8. A implementação revelar Anya/Arquivista/101/final/ending cedo.
9. A implementação criar pet runtime ou romance/social deep.
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
rg -n "SaveManager|GameSaveData|CaveSaveData|CaveRunSeed|VisitedLevelSnapshot|Level101|FinalChoice|Ending|PostGame|Archivist|Anya|Fonte|Mana" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de save/load, caverna, 101, final choice, cinematic ou postgame, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, confirmation/presentation/save-boundary logic is deterministic.
- Requires EditMode tests: YES for confirmation/preview/skip/load/no-mutation/no-Anya tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for final choice cinematic visual validation.
- Requires regression test: YES if fixing existing final UI/confirmation bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; final choice confirmation safe and cinematic non-mutating.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/16_spec_final_choice_cinematic_presentation_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não reimplementar SaveManager.
Não reimplementar caverna procedural.
Não salvar Unity references.
Não salvar UI state como gameplay state.
Não quebrar CaveRunSeed/snapshots/checkpoints/corpse recovery.
Não duplicar rewards/final modifiers.
Não revelar spoilers endgame cedo.
Não transformar 101/final/postgame em farm infinito.
Não criar pet runtime.
Não criar romance/social deep runtime.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando save/load, cave, main progression, UI e endgame contracts estiverem estáveis ou quando houver decisão humana explícita de antecipar este bloco.
