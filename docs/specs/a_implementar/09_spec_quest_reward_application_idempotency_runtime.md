# SPEC — Quest Reward Application Idempotency Runtime

> **Spec ID:** `09_spec_quest_reward_application_idempotency_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 09 — Quest / Objective / Event / Main Progression  
> **Priority:** P0  
> **Type:** Runtime / Quest / Rewards / Idempotency / Unlocks  
> **Domain:** Quest / RewardApplication / GrantedRewardIds / FlagGrant / Economy and Unlock Integration  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_09_QUEST_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere pricing/economy reward values, inventory grant service, skill trees, FonteAnya, MainProgression, shop stock, recipe unlock, save schema ou quest content.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Items/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/09_spec_quest_reward_application_idempotency_runtime_execution_report.md`  
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
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
> **Blocks:**  
  - quest completion runtime;
  - farm order rewards;
  - shop/service unlocks;
  - recipe/skill/Fonte unlock hooks;
  - quest log notifications.
> **Scope:** definir/endurecer aplicação de rewards de quests com idempotência, GrantedRewardIds, GrantedFlagIds, atomicidade e integração segura com inventário/economia/unlocks.  
> **Out of scope:** valores finais de recompensa, reward content final, main progression state, FonteAnya implementation, UI final, save migration.

---

# /speckit.specify

## 1. Contexto

Reward é efeito declarado aplicado ao completar objective/step/quest. O direction exige que Reward seja idempotente, não aplique duas vezes após save/load, registre `GrantedRewardIds` quando necessário, não burle economia sem regra explícita, respeite Bestiary spoiler control e respeite FonteAnya/MainProgression.

Esta spec cria o mecanismo de aplicação segura de rewards genéricos.

---

## 2. Problema

Sem reward engine idempotente:

```text
ouro pode ser pago duas vezes;
item pode duplicar após reload;
QuestFlag pode ser concedida repetidamente;
recipe/shop unlock pode disparar de novo;
FonteUpgrade pode ser aplicado via quest genérica sem FonteAnya;
MainProgression pode ser alterado como QuestFlag simples;
inventory full pode consumir conclusão sem entregar item;
reward oculto pode aparecer no quest log cedo.
```

---

## 3. Objetivo

Criar/endurecer:

```text
QuestRewardDefinition;
QuestRewardType;
QuestRewardApplicationContext;
QuestRewardApplicationResult;
QuestRewardApplicator;
Reward idempotency using GrantedRewardIds;
Flag idempotency using GrantedFlagIds;
Atomic reward application;
Inventory overflow/failed grant policy;
Unlock adapter contracts;
Protected reward rules.
```

---

## 4. Regras de design

```text
Reward precisa ser idempotente.
Reward não aplica duas vezes após save/load.
Reward registra GrantedRewardIds quando necessário.
Reward de flag registra GrantedFlagIds quando necessário.
Reward não burla economia sem regra explícita.
Reward de conhecimento respeita spoiler/Bestiary.
Reward de Fonte respeita FonteAnyaSection.
Reward de main/final choice respeita MainProgressionSection.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero receber reward uma vez.
Como runtime, quero evitar duplicação mesmo se evento repetir.
Como inventory, quero falhar ou overflow sem perder item.
Como quest flags, quero registrar grant idempotente.
Como Fonte/MainProgression, quero receber hooks explícitos, não flags soltas.
```

---

## 6. Escopo

Inclui:

```text
reward definition contract;
reward application context/result;
idempotency with GrantedRewardIds/GrantedFlagIds;
atomicity;
inventory/economy/unlock adapter interfaces;
failure/overflow policy;
protected reward validation;
tests.
```

Não inclui:

```text
final reward tables;
economy balance values;
inventory rewrite;
main progression/Fonte state implementation;
Bestiary implementation;
UI reward popup.
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
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

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

- Reward types incluem Gold, Item, Recipe, ToolUnlock, EquipmentUnlock, SpellUnlock, SkillPoint, SkillTreeUnlock, RelationshipFuture, KnowledgeUnlock, BestiaryEntryUnlock, FonteUpgrade, LivingWaterCharge, QuestFlagGrant, QuestFlagClear, AreaUnlock, CaveDepthUnlock, ShopUnlock, ShopStockUnlock, DialogueUnlock, NpcScheduleUnlock, FestivalUnlock, CompanionUnlockFuture, PetUnlockFuture e SocialUnlockFuture.
- Reward precisa ser idempotente e não aplicar duas vezes após save/load.
- Reward deve registrar GrantedRewardIds quando necessário.
- Reward não deve burlar sistemas de economia sem regra explícita.
- Reward de conhecimento deve respeitar Bestiary spoiler control.
- Reward de Fonte deve respeitar FonteAnyaSection/MainProgression.

### Deferred / future from directions

- Final reward values/tables.
- MainProgression concrete mutations.
- FonteAnya concrete mutations.
- Relationship/Social/Pet/Companion runtime.
- Reward popup UI.
- Economy balance tuning.

### Explicitly not redefined here

- QuestState save/load.
- Inventory grant service.
- Gold/economy service.
- Recipe/shop/skill unlock registries.
- Bestiary/knowledge service.
- MainProgression/Fonte systems.

## 7. Modelo de domínio

### 7.1 QuestRewardType

```text
Gold
Item
Recipe
ToolUnlock
EquipmentUnlock
SpellUnlock
SkillPoint
SkillTreeUnlock
RelationshipFuture
KnowledgeUnlock
BestiaryEntryUnlock
FonteUpgrade
LivingWaterCharge
QuestFlagGrant
QuestFlagClear
AreaUnlock
CaveDepthUnlock
ShopUnlock
ShopStockUnlock
DialogueUnlock
NpcScheduleUnlock
FestivalUnlock
CompanionUnlockFuture
PetUnlockFuture
SocialUnlockFuture
```

### 7.2 QuestRewardDefinition

```text
RewardId
RewardType
TargetId
Quantity
QualityPolicy optional
VisibilityPolicyId
SpoilerTier
RequiresStrongConfirmation optional
IdempotencyPolicy
FailurePolicy
GrantedFlagId optional
DebugTags[]
```

### 7.3 QuestRewardApplicationContext

```text
QuestId
StepId optional
ObjectiveId optional
RewardId
ActorId optional
Day
Time
QuestStateRecord
InventoryTarget optional
WorldStateSnapshot
MainProgressionAdapter optional
FonteAnyaAdapter optional
DryRun
```

### 7.4 QuestRewardApplicationResult

```text
Success
SkippedAlreadyGranted
FailureReason
GrantedRewardId
GrantedFlagIds[]
GrantedItems[]
GrantedGold
UnlockedIds[]
RequiresSave
RequiresNotification
ResidualRisk optional
```

---

## 8. Idempotency policies

```text
OncePerQuest:
  RewardId unique inside QuestId.

OncePerStep:
  RewardId unique inside QuestId+StepId.

OncePerObjective:
  RewardId unique inside QuestId+ObjectiveId.

RepeatInstanceScoped:
  repeatable quest uses RepeatInstanceId.

ManualNoIdempotency:
  disallowed except debug or explicit repeat reward.
```

---

## 9. Atomic application rules

```text
DryRun validation first.
If any mandatory reward cannot be applied:
  do not mark GrantedRewardIds.
If inventory item cannot be added:
  use overflow/failure policy.
If gold cannot be added:
  fail before marking complete.
If flag cannot be set:
  fail before marking reward granted.
If reward is skipped already granted:
  do not apply side effects.
After successful apply:
  record GrantedRewardIds/GrantedFlagIds.
```

---

## 10. Reward adapters

Reward engine may call adapters, but must not implement each domain internally:

```text
GoldRewardAdapter
ItemRewardAdapter
RecipeUnlockAdapter
ToolUnlockAdapter
EquipmentUnlockAdapter
SpellUnlockAdapter
SkillPointAdapter
KnowledgeUnlockAdapter
BestiaryUnlockAdapter
QuestFlagAdapter
AreaUnlockAdapter
CaveDepthUnlockAdapter
ShopUnlockAdapter
DialogueUnlockAdapter
NpcScheduleUnlockAdapter
FestivalUnlockAdapter
FonteRewardAdapter
MainProgressionRewardAdapter
```

Future adapters:

```text
RelationshipFutureAdapter
CompanionUnlockFutureAdapter
PetUnlockFutureAdapter
SocialUnlockFutureAdapter
```

Future adapters must not implement feature runtime now.

---

## 11. Protected reward rules

```text
FonteUpgrade:
  must route through FonteAnyaAdapter and not plain QuestFlag.

LivingWaterCharge:
  must respect FonteAnya limits and not create infinite cure.

MakeFinalChoice / ending related reward:
  must route through MainProgressionAdapter and require strong confirmation.

KnowledgeUnlock:
  must respect spoiler policy.

ShopStockUnlock:
  must use shop stock/unlock contract.

PetUnlockFuture/SocialUnlockFuture:
  declared but blocked unless future system exists.
```

---

## 12. Criteria

```text
Reward definition and applicator exist or are hardened.
Reward application is dry-run then apply.
GrantedRewardIds/GrantedFlagIds prevent duplication.
Inventory full/overflow policy explicit.
Fonte/MainProgression rewards do not become generic flags.
Future social/pet/companion rewards do not implement future systems.
Tests cover gold, item, flag, unlock, duplicate event, reload replay and failed inventory.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Rewards/QuestRewardType.cs
Assets/_Game/Scripts/Quests/Rewards/QuestRewardDefinition.cs
Assets/_Game/Scripts/Quests/Rewards/QuestRewardApplicationContext.cs
Assets/_Game/Scripts/Quests/Rewards/QuestRewardApplicationResult.cs
Assets/_Game/Scripts/Quests/Rewards/QuestRewardApplicator.cs
Assets/_Game/Scripts/Quests/Rewards/IQuestRewardAdapter.cs
Assets/_Game/Scripts/Quests/Rewards/QuestRewardValidator.cs
Assets/_Game/Tests/EditMode/Quests/QuestRewardIdempotencyTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/09_spec_quest_reward_application_idempotency_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/MainProgression/**
Assets/_Game/Scripts/Shops/**
Assets/_Game/Scripts/Bestiary/**
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
1. Auditar reward application/unlock/gold/item grant systems.
2. Consolidar reward definitions and result contracts.
3. Implementar dry-run/apply/idempotency.
4. Integrar adapter interfaces without implementing future systems.
5. Criar tests for duplicate prevention and failed grants.
6. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - quest state save/load;
  - inventory schema;
  - economy/gold service;
  - main progression/Fonte hooks;
  - shop/recipe/skill unlock systems.
- Reason: reward application crosses many services.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if GrantedRewardIds/GrantedFlagIds exist; otherwise STOP.
Does this add save section? NO.
Does this require migration? NO unless adding idempotency fields; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. QuestRewardAppliedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final; exposes notification hints.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED if reward UI/quest completion flow wired.
```

---

## 21. Riscos

```text
Risco: reward duplication.
Mitigação: idempotency tests.

Risco: item lost on inventory full.
Mitigação: dry-run/failure policy.

Risco: Fonte/MainProgression mutated as generic flags.
Mitigação: adapter guardrails.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar reward/unlock systems.
- [ ] T003 — Consolidar reward contracts.
- [ ] T004 — Implementar dry-run/apply/idempotency.
- [ ] T005 — Integrar adapter interfaces.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a quest reward application idempotency foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Quest core | A spec preserva Definition -> State -> Steps -> Objectives -> Conditions -> Triggers -> Rewards -> Flags? | Contrato/referências no report. | PARTIAL |
| Separação de estado | QuestState, QuestFlag, MainProgression e FonteAnya permanecem separados? | Checklist explícito no report. | BLOCKED se misturar |
| Anti-softlock | Quest crítica não fica impossível por item perdido, NPC, tempo, lua, morte ou save/load? | Plano/validator. | PARTIAL |
| Anti-spoiler | Quest log/visibility/rewards ocultos não revelam cedo Arquivista, final, 101, boss ou segredo? | Checklist/visibility. | PARTIAL |
| Pets/social future | A execução não implementou PetFuture/SocialFuture/romance/casamento profundo? | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/09_spec_quest_reward_application_idempotency_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "QuestReward|RewardApplication|GrantedRewardIds|GrantedFlagIds|Gold|Item|Recipe|ShopUnlock|FonteUpgrade|LivingWaterCharge|PetUnlockFuture" Assets/_Game/Scripts docs/design docs/specs
rg -n "QuestDefinition|QuestState|QuestStep|Objective|Condition|Trigger|QuestEvent|Reward|QuestFlag|MainProgression|FonteAnya|ChoiceHistory|GrantedReward|Spoiler|Softlock|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a quest reward application idempotency existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And save/load, quest state, quest flags, main progression, FonteAnya e UI projection permanecem consistentes
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

### Scenario 3 — Critical quest anti-softlock

```text
Given uma quest crítica depende de item, NPC, clima, lua, caverna, morte, reward ou save/load
When o jogador perde item, recarrega, morre, muda horário ou completa step antes da quest iniciar
Then existe fallback, trigger retroativo, item protegido, NPC alternativo, normalização no load ou bloqueio explícito
And a main quest não fica impossível.
```

### Scenario 4 — Idempotency and replay safety

```text
Given objective, trigger, reward, flag ou branch já foi aplicado
When o mesmo evento chega de novo após reload, scene reload, event bus duplicate ou reentrada de diálogo
Then estado/recompensa/flag não é aplicado duas vezes
And ChoiceHistory/GrantedRewardIds/GrantedFlagIds preservam a decisão.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de Quest Log, notificação, diálogo, entrega, mapa, farm order ou evento de main quest
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Gold reward paid twice after reload.
- Item duplicated by repeated event.
- Inventory full loses reward.
- QuestFlag grant repeats or conflicts.
- FonteUpgrade applied as generic QuestFlag.
- Final choice reward lacks strong confirmation.
- Pet/Social future reward implements feature now.
- Reward marked granted before side effect succeeds.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest Reward Application Idempotency Runtime

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

## Canon / quest compliance
- Definition/State/Step/Objective/Condition/Trigger/Reward/Flag separation:
- QuestState vs QuestFlag separation:
- QuestState vs MainProgression vs FonteAnya separation:
- Anti-softlock:
- Anti-spoiler:
- No PetFuture runtime:
- No SocialFuture/romance deep runtime:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-softlock:
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
4. A implementação exigir reescrever Quest/Save/EventBus/Fonte/MainProgression canônico existente.
5. A implementação esconder MainProgression ou FonteAnya dentro de QuestState genérico.
6. A implementação usar QuestFlag como substituto para estado complexo de quest.
7. A implementação permitir reward duplicado após reload/evento repetido.
8. A implementação revelar cedo Arquivista do Silêncio, final choices, nível 101, boss oculto ou segredo de Anya.
9. A implementação tornar main quest expirável por tempo/calendário.
10. A implementação criar PetFuture, SocialFuture, romance/casamento profundo ou companion full runtime fora do escopo.
11. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Reward Adapter Matrix

| Reward | Adapter | Guardrail |
|---|---|---|
| Gold | Economy | no duplicate |
| Item | Inventory | overflow/fail safe |
| Recipe | Crafting/Recipes | unlock idempotent |
| Tool/Equipment | Equipment | no free duplicate upgrade |
| Knowledge/Bestiary | Knowledge | spoiler control |
| FonteUpgrade/LivingWater | FonteAnya | not generic flag |
| Area/CaveDepth | World/Cave | gate-safe |
| Shop/Stock | Shops | stock contract |
| Dialogue/NpcSchedule | City/NPC | no spoiler |
| Pet/Social/CompanionFuture | future adapters | no runtime now |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "QuestDefinition|QuestState|QuestStep|Objective|Condition|Trigger|QuestEvent|Reward|QuestFlag|MainProgression|FonteAnya|ChoiceHistory|GrantedReward|Spoiler|Softlock|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de Quest Log, diálogo, entrega, reward, objective progress ou evento de main quest, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, reward dry-run/application/idempotency logic is deterministic.
- Requires EditMode tests: YES for gold/item/flag/unlock/duplicate/reload/failure tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED if quest completion/reward UI flow is wired.
- Requires regression test: YES if fixing existing reward duplication bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no duplicate rewards; failed rewards not marked granted.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/09_spec_quest_reward_application_idempotency_runtime_execution_report.md.
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
Não usar QuestFlag como substituto de QuestState.
Não aplicar reward/flag duas vezes.
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
