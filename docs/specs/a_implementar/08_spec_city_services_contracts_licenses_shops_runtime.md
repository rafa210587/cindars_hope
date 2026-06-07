# SPEC — City Services Contracts Licenses Shops Runtime

> **Spec ID:** `08_spec_city_services_contracts_licenses_shops_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 08 — City / NPC / Dialogue / Services  
> **Priority:** P0  
> **Type:** Runtime / City / Services / Contracts / Licenses / Shops  
> **Domain:** City / ServiceProvider / ShopService / Contract / License / Reputation  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_08_CITY_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere pricing service, shop stock/restock, quest objectives, farm construction, repair/upgrade, dialogue runtime, NPCData, save schema ou UI final de services.  
> **Repo lock scope:** `Assets/_Game/Scripts/City/**`, `Assets/_Game/Scripts/Shops/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/City/**`, `docs/validation/08_spec_city_services_contracts_licenses_shops_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
> **Blocks:**  
  - shop buy/sell UI;
  - contracts/order boards;
  - farm construction unlocks;
  - repair/upgrade screen;
  - city reputation system.
  - dialogue unlocks.
> **Scope:** definir/endurecer contratos de serviços da cidade, lojas, licenças, contratos, reputation/service unlocks e provider mapping por NPC/building.  
> **Out of scope:** UI final, conteúdo final de cada loja/contrato, pricing tuning, full questlines, farm construction execution, romance/social services.

---

# /speckit.specify

## 1. Contexto

A cidade é hub econômico: lojas, venda, compra, upgrades, contratos, encomendas e licenças. O roster atribui NPCs a serviços: templo, cartório, sementes, loja geral, forja, carpintaria, alquimia, taverna, guilda, arquivo, costura, rancho, ervas, loja noturna etc.

Esta spec cria o contrato de service providers e gating. Não implementa UI final nem conteúdo completo de cada serviço.

---

## 2. Problema

Sem contrato de services:

```text
serviço pode ficar hardcoded por NPC name;
loja pode abrir sem prédio/horário/estoque;
licença pode desbloquear construção sem custo/flag;
contrato pode pagar sem objective formal;
reputação pode dar desconto sem regra;
loja noturna pode vender item raro sem gate de Nyx/noite;
Anya pode virar serviço de templo urbano indevido;
serviço pode ignorar pricing/stock central.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CityServiceDefinition;
ServiceProviderDefinition;
ServiceType;
ServiceUnlockRule;
ServiceAvailability;
LicenseDefinition;
ContractDefinition minimal adapter;
ShopService mapping;
ServicePrice/PricingProfile integration;
Reputation gates;
Save/load mutable unlock state.
```

---

## 4. Regras de design

```text
Cidade é hub econômico/progressão, não coleção de lojas estáticas.
Serviço pertence a NPC/building/horário/reputação/progresso.
Loja usa stock/pricing central.
Contrato/encomenda usa quest/objective/event system.
Licença é gate de construção/altar/expansão, não só diálogo.
Anya não tem templo/altar urbano ativo; Fonte da fazenda é sistema próprio.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero acessar serviços quando prédio/NPC/horário/reputação permitem.
Como shop, quero usar stock/pricing sem duplicar fórmula.
Como cartório, quero emitir licença com custo/flag e efeito claro.
Como guilda/taverna, quero contrato ligado a objective formal.
Como validator, quero impedir serviço sem provider/gate/preço/stock.
```

---

## 6. Escopo

Inclui:

```text
service definitions;
provider mapping;
availability/unlock rules;
license contracts;
contract board adapter minimum;
shop service mapping;
reputation/progress gates;
price/stock references;
validators/tests.
```

Não inclui:

```text
shop UI final;
dialogue content;
full quest contract implementation;
farm construction execution;
exact service prices;
romance/social services;
Anya temple/service.
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

- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md

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

- Cidade funciona como hub econômico: lojas, venda, compra, upgrades, contratos, encomendas e licenças.
- Cidade funciona como hub de progressão: ferramentas, construções, animais, quests, preparação da caverna e serviços.
- Roster define NPCs com tags de serviço como templo, cartório, sementes, loja geral, forja, carpintaria, alquimia, taverna, guilda, arquivo, costura, rancho, ervas e loja noturna.
- Shop stock/pricing deve usar contratos de stock e pricing centrais.
- Merithus/Meritos aparece em contratos, licenças, impostos e caixa de envio.
- Anya não tem templo ativo conhecido na cidade e não deve virar serviço urbano independente.

### Deferred / future from directions

- UI final de serviços.
- Conteúdo final de todos os contratos.
- Questlines completas.
- Exact service pricing.
- Social/romance services.
- Festival stalls.
- Anya city service.

### Explicitly not redefined here

- PricingProfile.
- ShopInventory/StockLine.
- Quest objective runtime.
- NPCData contract.
- Schedule/open hours.
- Farm building construction runtime.

## 7. Modelo de domínio

### 7.1 CityServiceType

```text
ShopGeneral
ShopSeeds
ShopNight
BlacksmithRepair
BlacksmithUpgrade
CarpentryBuild
CarpentryMove
AlchemyPotion
AlchemyFertilizer
TavernFood
TavernRumor
InnLodging
RoadsGuildContract
RoadsGuildMap
TownHallLicense
TownHallContract
TempleBlessing
TempleOath
ArchiveResearch
ArchiveTranslation
TailorBagUpgrade
TailorClothing
RanchAnimals
RanchFeed
HerbalistAntidote
HerbalistForage
```

### 7.2 CityServiceDefinition

```text
ServiceId
ServiceType
DisplayName
ProviderNpcIds[]
ProviderBuildingId
RequiredOpenHoursRule optional
RequiredNpcAvailability
RequiredReputation optional
RequiredQuestFlag optional
RequiredFarmLevel optional
RequiredCaveProgress optional
RequiredStoryFlag optional
PriceChannel
PricingProfileId optional
ShopInventoryId optional
QuestObjectiveAdapterId optional
LicenseDefinitionId optional
ForbiddenIfFlags[]
DebugTags[]
```

### 7.3 ServiceAvailabilityResult

```text
Available
UnavailableReason
ProviderNpcId
BuildingId
OpenHoursStatus
RequiredFlagsMissing[]
PricePreview optional
StockStatus optional
DialogueHookId optional
```

### 7.4 LicenseDefinition

```text
LicenseId
DisplayName
IssuerServiceId
RequiredGold
RequiredItems[]
RequiredReputation
RequiredQuestFlag optional
GrantedFlag
GrantedPermission
ExpiresAfterDays optional
ReputationImpact[]
```

### 7.5 ContractDefinition minimal

```text
ContractId
ProviderServiceId
ContractType
ObjectiveDefinitionId
RewardTableId
Deadline optional
RequiredReputation optional
RequiredCaveProgress optional
RequiredFarmLevel optional
RepeatPolicy
```

---

## 8. Provider mapping rules

```text
Corvus:
  temple blessing, oath, law, healing support if authored.

Mara/Tovin:
  licenses, contracts, reputation, legal records.

Sylveth:
  seeds, fertilizer, calendar/agriculture.

Renko:
  general shop, bargain, common items.

Brumdar:
  blacksmith repair/upgrade/tools/weapons.

Nimble/Gurd/Hund:
  construction, move building, expansion/obstacle.

Ozzra:
  alchemy, potions, fertilizer, reagents.

Gruta/Orlan:
  food, lodging, rumors, tavern board.

Zrix/Dagna:
  roads guild, cave maps/contracts.

Thalindra:
  research, lore, translation.

Mirela:
  bags, clothes, accessories.

Eiran:
  animals, feed, pet-related future but pets deferred.

Savra:
  herbs, antidotes, poisons/pragas.

Yael:
  night shop, rare/Nyx/secret stock only under conditions.
```

---

## 9. Availability/unlock rules

```text
Service requires provider building.
Service requires open hours unless explicitly always/appointment.
Service requires NPC availability if NPC-bound.
Shop service requires ShopInventoryId.
Price service requires PriceChannel/PricingProfile or service price adapter.
License grants flags/permissions.
Contract requires objective adapter and reward table.
Night shop requires night/Nyx/quest/story condition.
Anya service in city is blocked.
```

---

## 10. Save/load

Persist mutable service state:

```text
unlocked service flags;
license grants;
contract accepted/completed state;
provider temporary unavailable state;
limited service counters;
discount/reputation state if not derivable.
```

Do not persist:

```text
static service definitions;
NPC GameObject references;
building prefab references;
derived price previews;
pet state.
```

---

## 11. Criteria

```text
Service definitions and provider mapping exist.
Every service has provider NPC/building.
Shop service links to stock/pricing.
Contract service links to objective/reward adapter or is blocked.
License service grants explicit permission/flag.
Night shop gated by night/Nyx/story.
Anya city service blocked.
Tests cover provider mapping, availability, license grant, shop link, contract adapter and Anya block.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/City/Services/CityServiceType.cs
Assets/_Game/Scripts/City/Services/CityServiceDefinition.cs
Assets/_Game/Scripts/City/Services/ServiceAvailabilityResult.cs
Assets/_Game/Scripts/City/Services/CityServiceAvailabilityResolver.cs
Assets/_Game/Scripts/City/Services/LicenseDefinition.cs
Assets/_Game/Scripts/City/Services/ContractDefinition.cs
Assets/_Game/Scripts/City/Validation/CityServiceValidator.cs
Assets/_Game/Tests/EditMode/City/CityServiceAvailabilityTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Shops/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/City/**
docs/validation/08_spec_city_services_contracts_licenses_shops_runtime_execution_report.md
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
1. Auditar city services/shop/contract/license systems.
2. Consolidar CityServiceDefinition and provider mapping.
3. Implementar availability resolver.
4. Implementar license/contract adapter contracts.
5. Integrar pricing/stock references.
6. Criar tests.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - NPCData contract;
  - shop stock/pricing;
  - quest objective runtime;
  - dialogue runtime;
  - service UI.
- Reason: services bridge NPC, city, economy, quest and UI.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if service flags exist; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding service/license state; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. CityServiceUnlockedEvent, LicenseGrantedEvent, ContractAcceptedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final; exposes availability reasons and price/stock preview.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for service interaction flow.
```

---

## 20. Riscos

```text
Risco: service hardcoded by NPC name.
Mitigação: provider mapping by stable IDs.

Risco: contract pays without objective.
Mitigação: quest adapter required.

Risco: Anya service accidentally created.
Mitigação: validator block.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar city services/contracts/licenses.
- [ ] T003 — Consolidar service/provider contracts.
- [ ] T004 — Implementar availability resolver.
- [ ] T005 — Integrar pricing/stock/quest adapters.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a city services/contracts/licenses/shops foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de city services/contracts/licenses/shops? | Arquivos alterados e justificativa. | PARTIAL |
| Canon de cidade | Kanthor/Anya/classes funcionais/sem D&D/sem Fôlego foram preservados? | Checklist no report. | PARTIAL |
| Pets | A execução não criou runtime/UI/save/data asset de pets? | Checklist explícito no report. | BLOCKED se violar |
| Romance | A execução não criou romance/casamento profundo fora do escopo? | Checklist explícito no report. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/08_spec_city_services_contracts_licenses_shops_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CityService|ServiceProvider|ServiceAvailability|License|Contract|ShopService|TownHall|TempleBlessing|NightShop|Anya|Kanthor" Assets/_Game/Scripts docs/design docs/specs
rg -n "NPC|NpcData|Schedule|Dialogue|Rumor|Service|Shop|Contract|License|DoorTrigger|BedId|FarmVisit|Relationship|Romance|Pet|Kanthor|Anya|Folego|Breath" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a city services/contracts/licenses/shops existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And save/load, agenda, serviços, diálogo e UI projection permanecem consistentes
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

### Scenario 3 — Canon protection

```text
Given a spec toca NPC, serviço, religião, cidade, diálogo ou agenda
When validar os dados
Then templo público principal permanece Kanthor
And Anya não ganha templo/altar ativo independente
And NPCs usam classes funcionais do jogo, não classes de D&D
And Fôlego/Breath/BR não é criado como stat de NPC
And pets permanecem deferidos.
```

### Scenario 4 — Invalid state

```text
Given NPC, agenda, serviço, diálogo, farm visit, porta, cama, licença ou contrato inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não corrompe save, não libera serviço indevido e não bloqueia progresso sem fallback.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de cidade, NPC, porta, interior, serviço, diálogo ou visita à fazenda
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Service with missing NPC/building provider.
- Shop service without stock/pricing.
- Contract with no objective adapter.
- License grants flag without cost/requirements.
- Night shop always available.
- Anya service/temple created.
- Service unlock state not persisted.
- Service hardcoded by NPC display name.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — City Services Contracts Licenses Shops Runtime

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

## Canon compliance
- Kanthor public temple preserved:
- Anya altar/temple not created:
- Functional classes used:
- Folego/Breath/BR not created:
- Pets not implemented:
- Deep romance/marriage not implemented:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Canon protection:
- Invalid state:
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
4. A implementação exigir reescrever City/NPC/Dialogue/Shop/Quest system canônico existente.
5. A implementação criar templo/altar ativo independente de Anya.
6. A implementação usar classe de D&D como classe mecânica de NPC.
7. A implementação recriar Fôlego/Breath/BR como stat/recurso/campo de NPC.
8. A implementação criar pets runtime/UI/save/data assets.
9. A implementação criar romance/casamento profundo ou conteúdo explícito.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Service Provider Matrix

| Building/service | Provider | Core service |
|---|---|---|
| Temple Kanthor | Corvus | blessing/oath/law |
| Town Hall | Mara/Tovin | license/contract/reputation |
| Seed Shop | Sylveth | seeds/fertilizer/calendar |
| General Store | Renko | common shop |
| Blacksmith | Brumdar | repair/upgrade/tools |
| Carpentry | Nimble/Gurd/Hund | construction/move |
| Alchemy | Ozzra | potions/fertilizer |
| Tavern/Inn | Gruta/Orlan | food/rumor/lodging |
| Roads Guild | Zrix/Dagna | maps/contracts/cave |
| Archive | Thalindra | lore/research/translation |
| Tailor | Mirela | bags/clothing |
| Ranch | Eiran | animals/feed; pets future |
| Herbalist | Savra | herbs/antidotes |
| Night Shop | Yael | night/Nyx rare stock |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "NPC|NpcData|Schedule|Dialogue|Rumor|Service|Shop|Contract|License|DoorTrigger|BedId|FarmVisit|Relationship|Romance|Pet|Kanthor|Anya|Folego|Breath" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de cidade, NPC, porta, interior, serviço, diálogo ou visita à fazenda, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, service availability/license/contract validation logic is deterministic.
- Requires EditMode tests: YES for provider/availability/license/shop/contract/Anya block tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for service UI interaction flow.
- Requires regression test: YES if fixing existing service unlock/availability bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver CityScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; service providers valid; no Anya service leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/08_spec_city_services_contracts_licenses_shops_runtime_execution_report.md.
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
Não criar templo/altar ativo independente de Anya.
Não usar classes de D&D como classes mecânicas.
Não recriar Fôlego/Breath/BR em NPC.
Não implementar pets.
Não implementar romance/casamento profundo.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
