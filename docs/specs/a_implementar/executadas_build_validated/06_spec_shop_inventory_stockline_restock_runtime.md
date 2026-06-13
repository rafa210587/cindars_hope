# SPEC — Shop Inventory Stockline Restock Runtime

> **Spec ID:** `06_spec_shop_inventory_stockline_restock_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 06 — Economy / Loot / Crafting Foundation  
> **Priority:** P0  
> **Type:** Runtime / Economy / Shops / Stock / Restock / Save  
> **Domain:** City Economy / ShopInventory / StockLine / RestockPolicy / LimitedStock  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_06_ECONOMY_FOUNDATION_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere pricing service, shop buy/sell transaction runtime, city schedules, NPC roster, quest/reputation flags, item definitions, save schema ou shop UI final.  
> **Repo lock scope:** `Assets/_Game/Scripts/Shops/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/City/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Economy/**`, `docs/validation/06_spec_shop_inventory_stockline_restock_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
> **Blocks:**  
  - shop buy/sell transaction runtime;
  - city store UI;
  - NPC service identity;
  - stock unlock by reputation/quest/farm/cave progress;
  - economy anti-arbitrage tests.
> **Scope:** definir/endurecer ShopInventory, StockLine, RestockPolicy, current quantities, limited/unique/rotating stock e save/load.  
> **Out of scope:** shop UI final, concrete NPC shop contents, buy/sell transaction service, final item prices, city schedule implementation.

---

# /speckit.specify

## 1. Contexto

Economy Pricing & Stock Refresh Direction define ShopInventorySO, StockLineSO, StockType e RestockPolicy. Restock acontece em momentos definidos, normalmente início do dia antes da loja abrir, e nunca a cada abertura de menu.

Esta spec cria o contrato de estoque e restock, não o conteúdo final de todas as lojas.

---

## 2. Problema

Sem contrato de stock/restock:

```text
loja pode resetar estoque ao abrir menu;
UniqueStock pode ser comprado várias vezes;
LimitedStock pode não persistir counter;
rotating stock pode mudar no reload sem seed;
reputação/quest/farm/cave progress podem desbloquear estoque sem regra;
itens vendidos pelo jogador podem entrar no estoque e gerar exploit;
loja pode vender boss drop/item lore cedo demais;
shop UI pode virar dona do stock state.
```

---

## 3. Objetivo

Criar/endurecer:

```text
ShopInventoryDefinition;
StockLineDefinition;
StockType;
RestockPolicy;
ShopStockState;
StockLineState;
UniqueStockPurchasedFlags;
LimitedStockCounters;
RotatingStockSelection/Seed;
PlayerSoldStockPolicy default disabled;
Restock processor;
Save/load state.
```

---

## 4. Regras de design

```text
Lojas vendem conveniência, não substituem exploração.
Comerciantes não vendem tudo desde o início.
Reputação, quests, cave progress e farm level liberam estoque.
Restock não acontece ao abrir menu.
UniqueStock não repõe.
LimitedStock persiste counters.
Itens vendidos pelo jogador não entram automaticamente no estoque baseline.
Cidade não vende boss drops por padrão.
```

---

## 5. User stories / engineering stories

```text
Como shop, quero stock lines com regras de desbloqueio e restock.
Como jogador, quero que estoque esgotado continue esgotado até restock real.
Como save/load, quero preservar quantidades, unique flags e rotating seed.
Como economy, quero impedir menu/reload stock exploit.
Como city, quero lojas com identidade por NPC/serviço.
```

---

## 6. Escopo

Inclui:

```text
ShopInventory contract;
StockLine contract;
StockType enum;
RestockPolicy;
ShopStockState;
restock processor;
current quantity decrease on purchase;
limited/unique/rotating stock persistence;
player sold stock disabled baseline;
tests/validators.
```

Não inclui:

```text
shop UI final;
transaction buy/sell service;
NPC shop content final;
city schedule runtime;
final stock quantities;
final price tuning.
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

- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md

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

- ShopInventorySO mínimo inclui ShopId, NpcOwnerId, BaseStockLines, SeasonalStockLines, ReputationStockLines, QuestUnlockedStockLines, CaveProgressStockLines, FarmLevelStockLines, LimitedStockLines, RotatingStockLines, PlayerSoldStockPolicy, RestockPolicy e price rules.
- StockLineSO inclui StockLineId, ItemId, StockType, quantities, restock chance/variance, required season/reputation/quest/cave/farm/story flags e purchase limits.
- RestockPolicy inclui DailyMorning, Weekly, SeasonStart, OnQuestFlagChanged, OnReputationTierChanged, OnCaveProgressChanged, OnFarmLevelChanged, EventStart, TravelingMerchantVisit, ManualStoryOnly e Never.
- Restock acontece em momentos definidos, normalmente início do dia antes da loja abrir; não acontece a cada abertura de menu.
- Itens vendidos pelo jogador não entram automaticamente no estoque de venda da loja no baseline.
- ShopStockState deve persistir CurrentQuantity, UniqueStockPurchasedFlags, LimitedStockCounters, LastRestockDay, NextRestockDay e RotatingStockSeed/Selection.

### Deferred / future from directions

- Conteúdo final de cada loja.
- Shop UI final.
- Buy/sell transaction service.
- City schedule implementation.
- Traveling merchant full system.
- Buyback optional/future.
- Final price tuning.

### Explicitly not redefined here

- Pricing service.
- ItemDefinition contract.
- NPC roster concrete data.
- Quest/reputation runtime.
- Farm/cave progress flags.
- Save schema migration.

## 7. Modelo de domínio

### 7.1 ShopInventoryDefinition

```text
ShopId
NpcOwnerId
BaseStockLines[]
SeasonalStockLines[]
ReputationStockLines[]
QuestUnlockedStockLines[]
CaveProgressStockLines[]
FarmLevelStockLines[]
LimitedStockLines[]
RotatingStockLines[]
PlayerSoldStockPolicyId optional
RestockPolicyId
BuyPriceRules
SellPriceRules
DiscountRules
MarkupRules
ForbiddenItems
ShopOpenScheduleRef
DebugTags
```

### 7.2 StockLineDefinition

```text
StockLineId
ItemId
StockType
MinQuantity
MaxQuantity
RestockToQuantity
RestockVariance
RestockChance
RequiredSeason optional
RequiredReputation optional
RequiredQuestFlag optional
RequiredCaveProgress optional
RequiredFarmLevel optional
RequiredStoryFlag optional
StartDay optional
EndDay optional
PurchaseLimitPerDay optional
PurchaseLimitPerWeek optional
PurchaseLimitLifetime optional
IsUniqueStock
CanBeSoldBackToShop
DebugTags
```

### 7.3 StockType

```text
BaseStock
SeasonalStock
ReputationStock
QuestUnlockedStock
CaveProgressStock
FarmLevelStock
LimitedStock
RotatingStock
UniqueStock
EventStock
TravelingMerchantStock
PlayerSoldStockFuture
```

### 7.4 RestockPolicy

```text
RestockPolicyId
PolicyType
IntervalDays optional
DayOfWeek optional
SeasonRule optional
RestockAtDayStart
RestockBeforeShopOpen
UsesPersistentRandomSeed
DebugTags
```

### 7.5 ShopStockState

```text
ShopId
CurrentQuantityByStockLineId
UniqueStockPurchasedFlags
LimitedStockCounters
LastRestockDay
NextRestockDay optional
RotatingStockSeed
RotatingStockSelection
PlayerSoldStock optional/future
```

---

## 8. Restock rules

```text
BaseStock:
  repõe até RestockToQuantity conforme policy.

SeasonalStock:
  troca no início de estação ou quando season muda.

ReputationStock:
  aparece quando reputação mínima é atingida.

QuestUnlockedStock:
  aparece após quest/story flag.

CaveProgressStock:
  aparece por profundidade/boss/progresso.

FarmLevelStock:
  aparece por expansão/nível da fazenda.

LimitedStock:
  usa counters por dia/semana/estação/lifetime.

RotatingStock:
  sorteia por seed/counter persistido.

UniqueStock:
  compra única, não repõe.

TravelingMerchantStock:
  só em visita/evento.
```

---

## 9. Purchase decrement and persistence

```text
When player buys:
  CurrentQuantity -= QuantityPurchased.

If CurrentQuantity = 0:
  line unavailable until next valid restock
  or permanently unavailable if UniqueStock/Lifetime limited.

If save fails after purchase:
  report risk and do not mark ACCEPTED unless transaction is atomic.

Shop UI must read ShopStockState; it must not derive quantity by itself.
```

---

## 10. Player sold stock baseline

```text
Items sold by player do not automatically enter shop stock.
Buyback is future/optional.
If buyback exists later:
  - separate tab/policy;
  - costs more than shop paid;
  - expires or has limit.
```

---

## 11. Criteria

```text
ShopInventory/StockLine/RestockPolicy contracts exist.
Opening menu does not restock.
Daily/weekly/season/flag restock is deterministic.
Unique stock purchase persists.
Limited stock counters persist.
Rotating stock uses persistent seed/selection.
Player sold stock disabled baseline.
Tests cover purchase decrement, restock timing, unique, limited, rotating and reload.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Shops/ShopInventoryDefinition.cs
Assets/_Game/Scripts/Shops/StockLineDefinition.cs
Assets/_Game/Scripts/Shops/StockType.cs
Assets/_Game/Scripts/Shops/RestockPolicy.cs
Assets/_Game/Scripts/Shops/ShopStockState.cs
Assets/_Game/Scripts/Shops/ShopRestockProcessor.cs
Assets/_Game/Scripts/Shops/ShopStockValidator.cs
Assets/_Game/Tests/EditMode/Economy/ShopStockRestockTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Shops/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Economy/**
docs/validation/06_spec_shop_inventory_stockline_restock_runtime_execution_report.md
```

---

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia

```text
1. Auditar shops/stock/restock/save existentes.
2. Consolidar ShopInventory/StockLine/RestockPolicy.
3. Implementar/harden ShopStockState.
4. Implementar/harden restock processor.
5. Implementar validators anti-menu/reload exploit.
6. Criar tests.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - pricing service;
  - shop buy/sell transaction;
  - city schedule;
  - quest/reputation flags;
  - save schema.
- Reason: stock state depends on pricing/item definitions and is consumed by shop runtime.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if ShopStockState exists; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding ShopStockState; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. ShopRestockedEvent, ShopStockChangedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final; exposes stock state for UI.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for shop stock purchase/restock display.
```

---

## 20. Riscos

```text
Risco: restock on menu open.
Mitigação: tests.

Risco: unique item repurchased after reload.
Mitigação: UniqueStockPurchasedFlags.

Risco: rotating stock changes on reload.
Mitigação: seed/selection persistence.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar shops/stock/restock/save.
- [ ] T003 — Consolidar stock contracts.
- [ ] T004 — Implementar/harden restock processor.
- [ ] T005 — Implementar/harden stock state persistence.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a shop inventory/stockline/restock foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de shop inventory/stockline/restock? | Arquivos alterados e justificativa. | PARTIAL |
| Economia segura | A spec impede buy/sell exploit, item duplication, preço inválido ou progressão bypassada? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/06_spec_shop_inventory_stockline_restock_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "ShopInventory|StockLine|StockType|RestockPolicy|ShopStockState|CurrentQuantity|UniqueStock|LimitedStock|RotatingStock|PlayerSoldStock" Assets/_Game/Scripts docs/design docs/specs
rg -n "ItemDefinition|ItemCategory|ItemTag|Quality|Rarity|Pricing|BaseValue|Shop|StockLine|Restock|Recipe|Crafting|Processing|Station|AntiArbitrage|Save" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a shop inventory/stockline/restock existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And save/load, economia, inventário e UI projection permanecem consistentes
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

### Scenario 3 — Invalid economic state

```text
Given item, preço, stock, receita, estação, output, input ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não duplica item/ouro, não cria compra/venda lucrativa infinita e não corrompe save.
```

### Scenario 4 — Protected/lore/unique item

```text
Given QuestItem, KeyItem, Unique, LoreLocked, Fruto Mana, Água Viva ou Pedra Negra estabilizada
When compra/venda/craft/storage/stock/reward tenta tratar como commodity comum
Then a ação é bloqueada ou exige regra explícita autorada
And o report registra a proteção.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de UI, loja, crafting, preço ou inventário
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Restock ao abrir menu.
- UniqueStock comprado novamente após reload.
- LimitedStock counter perdido.
- RotatingStock muda no reload sem seed.
- Player-sold item entra em estoque baseline.
- Shop sells boss/unique/lore item cedo demais.
- CurrentQuantity não decrementa em compra.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Shop Inventory Stockline Restock Runtime

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

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Invalid economic state:
- Protected/lore/unique item:
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
4. A implementação exigir reescrever Inventory/Economy/Crafting/Shop system canônico existente.
5. A implementação persistir preço final recalculável como fonte primária.
6. A implementação permitir loop infinito de comprar barato e vender caro sem limite.
7. A implementação tratar QuestItem, KeyItem, Unique, Fruto Mana, Água Viva ou Pedra Negra estabilizada como commodity comum.
8. A implementação criar loot table, boss reward ou cave reward completo fora do escopo.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Restock Timing Matrix

| Policy | Timing | Must not |
|---|---|---|
| DailyMorning | day start | menu open |
| Weekly | fixed weekday/interval | reload |
| SeasonStart | season transition | every day |
| OnQuestFlagChanged | flag event | every menu |
| OnReputationTierChanged | reputation event | every sale |
| OnCaveProgressChanged | progress event | unrelated day |
| OnFarmLevelChanged | farm level event | every menu |
| EventStart | authored event | outside event |
| TravelingMerchantVisit | visit spawn | normal shop |
| Never | never | any automatic restock |

## 23H. Stock Persistence Invariants

```text
Purchase reduces CurrentQuantity.
UniqueStockPurchasedFlags persist.
LimitedStockCounters persist.
RotatingStockSeed/Selection persist.
Derived availability can recalc; player-changed quantities persist.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "ItemDefinition|ItemCategory|ItemTag|Quality|Rarity|Pricing|BaseValue|Shop|StockLine|Restock|Recipe|Crafting|Processing|Station|AntiArbitrage|Save" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de item, loja, preço, crafting, station ou processamento, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, stock/restock/purchase quantity logic is deterministic.
- Requires EditMode tests: YES for purchase/restock/unique/limited/rotating/reload tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for shop stock UI flow.
- Requires regression test: YES if fixing existing shop restock/stock exploit bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no menu/reload stock exploit.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/06_spec_shop_inventory_stockline_restock_runtime_execution_report.md.
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
Não persistir preço derivado como fonte primária.
Não criar arbitragem infinita.
Não criar item/ouro/output infinito.
Não transformar Fruto Mana, Água Viva ou Pedra Negra estabilizada em commodity comum.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
