# SPEC — Social Gifts Preferences Limits Future Runtime

> **Spec ID:** `17_spec_social_gifts_preferences_limits_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 17 — Social Relationship / Gifts / Partner Helper Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Gifts / Preferences / Anti-exploit  
> **Domain:** Social Gifts / Gift Tags / Preferences / Daily Weekly Limits / Discovery  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_17_SOCIAL_RELATIONSHIP_FUTURE  
> **Can run with:** social dialogue projections if no same files.  
> **Must not run with:** qualquer spec que altere item definitions, economy pricing, romance scenes, marriage gifts, save migration, pet gift runtime or UI visual final.  
> **Repo lock scope:** `Assets/_Game/Scripts/Social/Gifts/**`, `Assets/_Game/Scripts/Items/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Tests/EditMode/Social/**`, `docs/validation/17_spec_social_gifts_preferences_limits_future_runtime_execution_report.md`  
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
  - social relationship state;
  - dialogue gift reactions;
  - social UI;
  - personal quests;
  - economy anti-exploit.
> **Scope:** definir/endurecer runtime futuro de presentes sociais: tags, preferências, limites, descoberta, blocked gifts e anti-exploit.  
> **Out of scope:** item database rewrite, final balance values, romance scenes, marriage ceremony, pet gifts, UI visual final.

---

# /speckit.specify

## 1. Contexto

Presentes sociais podem avançar relação por relevância, qualidade, aniversário/festival, quest, preferência religiosa/cultural e eventos. Cada NPC pode ter Loved/Liked/Neutral/Disliked/Hated/Forbidden/Romantic/PolyCommitment/Marriage gift tags. Números finais são balance futuro.

---

## 2. Problema

Sem gift policy:

```text
presente vira farm infinito de relação;
item caro compra relação sem limite;
qualidade alta anula presente odiado;
presente proibido não é bloqueado;
romantic gift aparece para NPC bloqueado;
preference descoberta é perdida;
gift entra em economy exploit;
player dá múltiplos presentes diários sem limite.
```

---

## 3. Objetivo

Criar/endurecer:

```text
SocialGiftTag;
NpcGiftPreferenceProfile;
GiftReactionCategory;
GiftLimitState;
GiftAttemptRequest;
GiftAttemptResult;
GiftPreferenceDiscoveryRecord;
GiftAntiExploitPolicy.
```

---

## 4. Regras de design

```text
Gift é social, não compra poder obrigatório.
1 presente social relevante por NPC por dia e 2 por semana como baseline futuro.
Eventos especiais podem abrir exceção.
Quality não anula item odiado/proibido.
ForbiddenGiftTags têm bloqueio ou reação severa controlada.
Romantic/Marriage/Poly tags só funcionam com eligibility adequada.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero descobrir preferências sem planilha dominante.
Como social, quero aplicar reação por tags, contexto e limites.
Como NPC, quero respeitar religião, personalidade, trabalho e história.
Como economy, quero impedir exploit por item caro/qualidade.
Como UI, quero mostrar feedback claro e seguro.
```

---

## 6. Escopo

Inclui:

```text
gift tags;
preference profiles;
attempt/result;
daily/weekly limits;
quality modifier policy;
forbidden gifts;
discovery records;
anti-exploit tests.
```

Não inclui:

```text
item database authoring;
final numbers;
romance/casamento scenes;
pet gifts;
UI visual final.
```

## 7. Modelo de domínio

### 7.1 GiftPreferenceTier

```text
Loved
Liked
Neutral
Disliked
Hated
Forbidden
Romantic
PolyCommitment
Marriage
```

### 7.2 SocialGiftTag

```text
TagId
TagType: Category | Material | Origin | DeityAssociation | Rarity | Quality | ActivitySource | Food | Flower | Book | Reagent | Tool | Weapon | ReligiousObject
TagValue
SpoilerTier
```

### 7.3 NpcGiftPreferenceProfile

```text
NpcId
LovedGiftTags[]
LikedGiftTags[]
NeutralGiftTags[]
DislikedGiftTags[]
HatedGiftTags[]
ForbiddenGiftTags[]
RomanticGiftTags[]
PolyCommitmentGiftTags[]
MarriageGiftTags[]
PreferenceDiscoveryRules[]
NarrativeLockRules[]
```

### 7.4 GiftAttemptRequest

```text
NpcId
ItemInstanceId
ItemId
ItemTags[]
ItemQuality
Date
Season
IsBirthday
IsFestival
RelationshipStateSnapshot
IntentType: Social | Romantic | PolyCommitment | Marriage
```

### 7.5 GiftAttemptResult

```text
Accepted
Rejected
Blocked
ReactionCategory
RelationshipDelta
TrustDelta
AffectionDelta
ConflictDelta
DiscoveryRecord optional
LimitStateAfter
WarningTextKey
RequiresDialogueReaction
```

### 7.6 GiftLimitState

```text
NpcId
LastGiftDay
GiftsThisWeek
SpecialEventGiftUsed
DailyGiftUsed
WeeklyGiftCount
```

---

## 8. Preference resolution

```text
Forbidden wins over all.
Hated wins over high quality.
Loved can receive controlled quality bonus.
Romantic tags require eligibility and relationship state.
Marriage/Poly tags require appropriate future state and consent route.
Unknown preference returns neutral or discovered-by-reaction.
Quest-required gift can override some limits only if authored.
```

---

## 9. Limits

```text
Baseline:
  1 social relevant gift per NPC/day.
  2 social relevant gifts per NPC/week.
  birthday/festival can allow special gift.
  quest gift uses quest policy.

No infinite gifting.
No reset by scene reload.
No quality exploit.
No gift to bypass personal quest/consent locks.
```

---

## 10. Criteria

```text
Gift preference contracts exist.
Limit policy exists.
Forbidden/hated/quality rules enforced.
Romantic/poly/marriage gift intent gated by eligibility.
Preference discovery saved through social state.
Tests cover loved/liked/neutral/disliked/hated/forbidden, quality override prevention, daily/weekly cap, special event, romantic blocked NPC and discovery.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Social/Gifts/GiftPreferenceTier.cs
Assets/_Game/Scripts/Social/Gifts/SocialGiftTag.cs
Assets/_Game/Scripts/Social/Gifts/NpcGiftPreferenceProfile.cs
Assets/_Game/Scripts/Social/Gifts/GiftAttemptRequest.cs
Assets/_Game/Scripts/Social/Gifts/GiftAttemptResult.cs
Assets/_Game/Scripts/Social/Gifts/GiftLimitState.cs
Assets/_Game/Scripts/Social/Gifts/GiftPreferenceResolver.cs
Assets/_Game/Scripts/Social/Gifts/GiftAntiExploitValidator.cs
Assets/_Game/Tests/EditMode/Social/SocialGiftsPreferencesLimitsTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Social/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Social/**
docs/validation/17_spec_social_gifts_preferences_limits_future_runtime_execution_report.md
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
- [ ] T002 — Auditar gift/item/economy/social references.
- [ ] T003 — Consolidar gift preference contracts.
- [ ] T004 — Implementar resolver/validator.
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

- Ganho de relação pode vir de conversa diária, presente relevante, aniversário/festival, quest pessoal, ajuda, escolha de diálogo, evento de fazenda/caverna, convite, festival, spouse/partner event e companion event.
- NPC pode ter Loved, Liked, Neutral, Disliked, Hated, Forbidden, Romantic, PolyCommitment e Marriage gift tags.
- Tags vêm de categoria, material, origem, associação divina, raridade, qualidade, fonte, comida, flor, livro, reagent, tool, weapon, religious object.
- ForbiddenGiftTags ferem limite pessoal/dogma/narrativa.
- Quality não anula item odiado/proibido; item amado de alta qualidade pode dar bônus controlado.
- Baseline futuro: 1 presente social relevante por NPC por dia; 2 por semana; eventos especiais podem abrir exceção.

### Deferred / future from directions

- Final balance numbers.
- Gift UI visual.
- Romance/casamento scenes.
- Pet gift runtime.
- Item database authoring.
- Personal quest writing.

### Explicitly not redefined here

- ItemDefinition schema.
- Economy BaseValue.
- NPC roster eligibility.
- Quest item requirements.
- SocialRelationshipSection shape unless existing.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu social, city roster, companion, pet, economy, quest, save e UI directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a social gifts/preferences/limits foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Opcionalidade | Social/romance/casamento não é obrigatório para main quest, final bom, cidade, caverna, fazenda ou build. | Tests/checklist. | BLOCKED se violar |
| Consentimento/limites | Estados de romance/poliamor respeitam eligibility, blocked states, consent flags e limite global. | Tests/checklist. | PARTIAL |
| Menores/bloqueados | TooYoungOrNarrativelyBlocked não tem flerte, dating, romance gift, ceremony ou ambiguidades. | Tests/checklist. | BLOCKED se violar |
| Anti-exploit | Presentes/partner/helper não viram gold, stamina, combat power ou farm automation superior. | Tests/checklist. | PARTIAL |
| Separação | Pet bond, companion bond, relationship e reputation não são misturados. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/17_spec_social_gifts_preferences_limits_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "GiftPreference|GiftAttempt|LovedGift|ForbiddenGift|RomanticGift|GiftLimit|SocialGiftTag|PreferenceDiscovery" Assets/_Game/Scripts docs/design .specs
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

- Gift spam in same day/week.
- Forbidden gift accepted by quality.
- Hated gift becomes liked by high quality.
- Romantic gift accepted by blocked NPC.
- Gift bypasses consent/personal quest.
- Preference discovery lost.
- Gift becomes economy exploit.
- Pet gift system implemented accidentally.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Social Gifts Preferences Limits Future Runtime

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

- Changed deterministic logic: YES, gift preference/limit/resolution logic is deterministic.
- Requires EditMode tests: YES for preference tiers/forbidden/quality/daily-weekly/special-event/romantic-block/discovery tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for gift UI/dialogue visual validation.
- Requires regression test: YES if fixing existing gift/preference bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; gifting bounded and safe.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/17_spec_social_gifts_preferences_limits_future_runtime_execution_report.md.
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
