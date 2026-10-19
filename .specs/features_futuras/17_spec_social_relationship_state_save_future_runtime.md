# SPEC — Social Relationship State Save Future Runtime

> **Spec ID:** `17_spec_social_relationship_state_save_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 17 — Social Relationship / Gifts / Partner Helper Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Social State / Save Load  
> **Domain:** Social Relationship / Friendship / Trust / Affection / Polycule / Social Memory / Save  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_17_SOCIAL_RELATIONSHIP_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere NPC roster eligibility, romance scenes, gift runtime, partner helper runtime, companion runtime, pet runtime, save migration or UI visual final.  
> **Repo lock scope:** `Assets/_Game/Scripts/Social/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Tests/EditMode/Social/**`, `docs/validation/17_spec_social_relationship_state_save_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
> **Blocks:**  
  - gift preference runtime;
  - social dialogue conditions;
  - personal quests;
  - partner helper bridge;
  - social UI.
> **Scope:** definir/endurecer estado social persistível: friendship/trust/affection, romance/marriage/polycule as mechanical future states, social memory and save contracts.  
> **Out of scope:** romance cutscenes, dating scenes, ceremonies, gift runtime, helper automation, pet runtime, companion AI, save migration.

---

# /speckit.specify

## 1. Contexto

Relationship é o estado social do jogador com um NPC: amizade, confiança, afeto, respeito, conflito, romance, casamento, polycule, bloqueios narrativos e memória de eventos. Não inclui reputação econômica global, faction standing, companion combat bond, pet bond, shop price isolado ou quest flags genéricas.

Esta spec cria só o estado base e save futuro.

---

## 2. Problema

Sem estado social separado:

```text
friendship vira reputation;
romance vira companion bond;
pet bond mistura com social;
quest flags genéricas viram relationship;
polycule não tem consent/state;
NPC bloqueado pode entrar em rota ambígua;
relationship não persiste;
UI state social vira fonte de verdade.
```

---

## 3. Objetivo

Criar/endurecer:

```text
SocialRelationshipState;
RelationshipTrack;
RelationshipStatus;
RomanceEligibilityStateRef;
PolyculeMembershipState;
SocialMemoryRecord;
SocialRelationshipSection;
RelationshipTransitionValidator;
SocialSaveNormalizer.
```

---

## 4. Regras de design

```text
Romance/casamento/poliamor são opcionais.
Friendship deve ser útil mesmo sem romance.
Relationship não substitui reputation.
Relationship não substitui companion bond.
Relationship não substitui pet bond.
Afeto romântico só existe para NPC elegível.
Limite global de parceiros: 3.
Consentimento narrativo é obrigatório para polycule.
```

---

## 5. User stories / engineering stories

```text
Como save/load, quero persistir relationship por NpcId.
Como social, quero trilhas separadas de friendship/trust/affection/respect/conflict.
Como roster, quero eligibility declarada fora do Relationship state.
Como UI, quero projetar estado sem mutar gameplay.
Como validator, quero bloquear estados inválidos e NPCs protegidos.
```

---

## 6. Escopo

Inclui:

```text
relationship state contracts;
tracks;
statuses;
polycule membership;
social memory;
save section;
transition validator;
normalizer/tests.
```

Não inclui:

```text
gift runtime;
dialogue writing;
romance scenes;
marriage ceremony;
partner helper;
companion AI;
pet runtime.
```

## 7. Modelo de domínio

### 7.1 RelationshipTrack

```text
Friendship
Trust
Affection
Respect
Conflict
```

### 7.2 RelationshipStatus

```text
Unknown
Known
Acquaintance
Friendly
CloseFriend
Trusted
RomanceAvailable
Dating
Committed
Married
PolyculePartner
Estranged
Blocked
```

### 7.3 SocialRelationshipState

```text
NpcId
Status
FriendshipValue
TrustValue
AffectionValue
RespectValue
ConflictValue
RomanceEligibilityRef
PolyculeState optional
SocialMemoryIds[]
KnownPreferenceIds[]
GiftLimitState
LastTalkedDay optional
LastGiftDay optional
LastVisitDay optional
NarrativeLocks[]
```

### 7.4 PolyculeMembershipState

```text
IsPartner
RelationshipKind: Dating | Committed | Married
ConsentState
PartnerIndex
AcceptedPartnerIds[]
RejectedPartnerIds[]
MaxPartnerCapSnapshot
```

### 7.5 SocialMemoryRecord

```text
MemoryId
NpcId
MemoryType
Day
Season
Source: Conversation | Gift | Quest | Visit | Festival | PartnerEvent | CompanionEvent | PetReactionFuture | Story
PayloadIds[]
SpoilerTier
```

### 7.6 SocialRelationshipSection

```text
Version
RelationshipStates[]
GlobalPartnerCount
PolyculePartnerIds[]
GlobalSocialLocks[]
LastValidatedVersion
```

---

## 8. Transition rules

```text
Unknown -> Known -> Acquaintance -> Friendly -> CloseFriend -> Trusted.
RomanceAvailable requires roster eligibility, friendship/trust, personal quest, no block and cap.
Dating/Committed/Married require explicit transition event.
PolyculePartner requires consent state and cap <= 3.
Estranged can happen by authored event and must be repairable if design says so.
Blocked prevents incompatible transitions.
```

---

## 9. Save/load rules

```text
Persist stable NpcId and simple values.
Do not persist UI selected NPC/tab.
Do not persist MonoBehaviour/Transform/SO.
Do not persist dialogue line state as relationship unless authored SocialMemory.
Normalize global partner count from states.
Cap global partners at 3; invalid excess blocks or marks conflict.
Pet bond is not social relationship.
Companion bond is not social relationship.
```

---

## 10. Criteria

```text
Relationship state contracts exist.
SocialRelationshipSection exists or STOP if migration required.
Tracks and statuses separated.
Polycule cap and consent modeled.
NPC blocked/TooYoung/NarrativelyBlocked cannot enter romance states.
Pet/companion/reputation separate.
Tests cover transitions, blocked NPC, cap 3, consent state, save normalization, no pet/companion mixing and UI not saved.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Social/SocialRelationshipState.cs
Assets/_Game/Scripts/Social/RelationshipTrack.cs
Assets/_Game/Scripts/Social/RelationshipStatus.cs
Assets/_Game/Scripts/Social/PolyculeMembershipState.cs
Assets/_Game/Scripts/Social/SocialMemoryRecord.cs
Assets/_Game/Scripts/Social/SocialRelationshipSection.cs
Assets/_Game/Scripts/Social/RelationshipTransitionValidator.cs
Assets/_Game/Scripts/Social/SocialSaveNormalizer.cs
Assets/_Game/Tests/EditMode/Social/SocialRelationshipStateSaveTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Social/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Social/**
docs/validation/17_spec_social_relationship_state_save_future_runtime_execution_report.md
```

## 13. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
Assets/_Game/Scripts/Companions/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar social/save/NPC systems.
- [ ] T003 — Consolidar state/status/section.
- [ ] T004 — Implementar transition validator/normalizer.
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

- docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela deriva principalmente de SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.
Ela não implementa romance cutscenes, cerimônias, dating scene, intimidade, children/family system, jealousy amplo, conteúdo explícito, pet runtime ou companion AI.
Ela trata romance/casamento/poliamor apenas como estados mecânicos futuros, opcionais, consentidos e não obrigatórios para o core loop.
NPCs com TooYoungOrNarrativelyBlocked não podem ter rota ambígua, flerte, dating, presente romântico ou evento de romance.
Quando houver conflito com roster de NPCs, companions, pets, economy, quest, city schedule, UI ou save directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Relationship inclui amizade, confiança, afeto, respeito, conflito, romance, casamento, polycule, bloqueios narrativos e memória social.
- Relationship não inclui reputation, faction standing, companion combat bond, pet bond, shop price isolado ou quest flags genéricas.
- Estados sociais incluem Unknown, Known, Acquaintance, Friendly, CloseFriend, Trusted, RomanceAvailable, Dating, Committed, Married, PolyculePartner, Estranged, Blocked.
- Romance é opcional; casamento é opcional; amizade deve ser útil mesmo sem romance.
- Polycule permite até 3 parceiros com consentimento narrativo dos envolvidos.
- NPC TooYoungOrNarrativelyBlocked não pode ter rota ambígua, flerte, presente romântico ou evento de dating.

### Deferred / future from directions

- Gift runtime.
- Romance/casamento scenes.
- Personal quest content.
- Partner helper automation.
- Companion AI.
- Pet runtime.

### Explicitly not redefined here

- NPC roster eligibility.
- Companion bond.
- Pet bond.
- Reputation.
- Quest flags.
- UI visual final.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu social, city roster, companion, pet, economy, quest, save e UI directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a social relationship state/save foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Opcionalidade | Social/romance/casamento não é obrigatório para main quest, final bom, cidade, caverna, fazenda ou build. | Tests/checklist. | BLOCKED se violar |
| Consentimento/limites | Estados de romance/poliamor respeitam eligibility, blocked states, consent flags e limite global. | Tests/checklist. | PARTIAL |
| Menores/bloqueados | TooYoungOrNarrativelyBlocked não tem flerte, dating, romance gift, ceremony ou ambiguidades. | Tests/checklist. | BLOCKED se violar |
| Anti-exploit | Presentes/partner/helper não viram gold, stamina, combat power ou farm automation superior. | Tests/checklist. | PARTIAL |
| Separação | Pet bond, companion bond, relationship e reputation não são misturados. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/17_spec_social_relationship_state_save_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "SocialRelationship|RelationshipStatus|Friendship|Trust|Affection|Polycule|SocialMemory|RomanceEligibility|TooYoung" Assets/_Game/Scripts docs/design .specs
rg -n "Relationship|Friendship|Trust|Affection|Romance|Dating|Committed|Married|Polycule|Gift|GiftPreference|SocialMemory|PartnerHelper|PartnerCompanion|TooYoung|NarrativelyBlocked|PetBond|CompanionBond|Jealousy|Divorce|Children" Assets/_Game/Scripts docs/design .specs
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
Given NPC elegível e estado social válido
When o sistema desta spec processa relação, presente, diálogo, visita, quest ou helper
Then ele aplica apenas efeito permitido, limitado, opcional e rastreável
And não mistura pet/companion/reputation
And não força romance/casamento/poliamor no core loop.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Blocked or protected NPC

```text
Given NPC com UnavailableForRomance, MarriedToNpc, TooYoungOrNarrativelyBlocked, PolyBlocked ou bloqueio narrativo
When o jogador tenta ação romântica/poliamorosa/casamento/partner helper incompatível
Then o sistema bloqueia com razão clara e segura
And amizade, respeito, serviços e quests não-românticas continuam possíveis quando apropriado.
```

### Scenario 4 — Anti-exploit

```text
Given presente, visita, partner helper, companion unlock ou benefício social
When o efeito é calculado
Then há limite diário/semanal/orçamento global/cooldown/eligibility
And não gera ouro, stamina, HP/MP, combat power ou farm automation superior aos sistemas dedicados.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de social UI, gift reaction, visit, personal quest or partner helper UI
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Relationship mixed with companion bond.
- Relationship mixed with pet bond.
- Blocked NPC enters romance state.
- Polycule cap exceeded.
- Consent state missing.
- UI selection saved as gameplay state.
- Generic quest flag used as relationship.
- Save migration attempted silently.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Social Relationship State Save Future Runtime

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

## Social compliance
- Optional social/romance:
- No explicit/intimate content:
- No blocked NPC route:
- Consent/eligibility checked:
- Max partner cap respected:
- No pet bond mixing:
- No companion bond mixing:
- No economy/combat/farm exploit:
- Save/load safe:
- UI not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Blocked/protected NPC:
- Anti-exploit:
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
4. A implementação criar conteúdo íntimo/explícito, cutscene romântica ou dating scene.
5. A implementação permitir rota romântica para TooYoungOrNarrativelyBlocked.
6. A implementação tornar romance/casamento/poliamor obrigatório para main quest, final, poder ou progresso essencial.
7. A implementação criar exploit de ouro, stamina, HP/MP, combat power, farm automation ou companion power via social.
8. A implementação misturar pet bond, companion bond, reputation ou quest flags genéricas dentro de Relationship.
9. A implementação criar jealousy punitivo amplo, children/family system, divorce/separation ou cerimônia visual.
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
rg -n "Relationship|Friendship|Trust|Affection|Romance|Dating|Committed|Married|Polycule|Gift|GiftPreference|SocialMemory|PartnerHelper|PartnerCompanion" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de social UI, gifts, visit, personal quest, partner helper ou companion bridge, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, relationship transition/save normalization logic is deterministic.
- Requires EditMode tests: YES for transitions/blocked/cap/consent/save/no-mixing tests.
- Requires PlayMode automated or final human scenario: NO by default; state/contracts only.
- Requires regression test: YES if fixing existing social state bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; social state separated and valid.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/17_spec_social_relationship_state_save_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não implementar conteúdo íntimo/explícito.
Não criar cutscene romântica/dating scene.
Não permitir rota ambígua para NPC bloqueado/idade/narrativa.
Não tornar romance/casamento/poliamor obrigatório.
Não misturar relationship com pet bond/companion bond/reputation.
Não gerar exploit social de economia, combate, stamina ou farm.
Não criar pet runtime.
Não criar companion AI.
Não salvar UI state como gameplay state.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando City/NPC, Quest, Save, UI, Economy e Companion foundations estiverem estáveis ou quando houver decisão humana explícita de antecipar Social/Relationship.
