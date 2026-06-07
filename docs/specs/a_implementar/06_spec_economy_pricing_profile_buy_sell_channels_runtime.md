# SPEC — Economy Pricing Profile Buy Sell Channels Runtime

> **Spec ID:** `06_spec_economy_pricing_profile_buy_sell_channels_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 06 — Economy / Loot / Crafting Foundation  
> **Priority:** P0  
> **Type:** Runtime / Economy / Pricing / Buy Sell Channels / Anti-Arbitrage  
> **Domain:** Economy / PricingProfile / SellPrice / BuyPrice / ShippingPrice / ServicePrice  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_06_ECONOMY_FOUNDATION_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere ItemDefinition, shop stock, shipping payment, order rewards, repair/upgrade costs, UI shop flow, save schema ou final item price tables.  
> **Repo lock scope:** `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Items/**`, `Assets/_Game/Scripts/Shops/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Economy/**`, `docs/validation/06_spec_economy_pricing_profile_buy_sell_channels_runtime_execution_report.md`  
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
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
> **Blocks:**  
  - shop buy/sell runtime;
  - SellPoint shipping price;
  - order rewards;
  - service prices;
  - economy anti-arbitrage tests.
> **Scope:** definir/endurecer PricingProfile, fórmulas de compra/venda por canal e regras anti-arbitragem sem criar tabela final de preços.  
> **Out of scope:** final BaseValue per ItemId, shop UI, order reward implementation, shipping bin payment, service-specific cost formulas, balance final.

---

# /speckit.specify

## 1. Contexto

O direction de pricing declara termos canônicos: BaseValue, SellPrice, BuyPrice, ShippingPrice, ShopBuyFromPlayerPrice, ShopSellToPlayerPrice, OrderRewardValue e ServicePrice. A regra principal é que ShopSellToPlayerPrice deve ser maior que ShopBuyFromPlayerPrice, salvo exceções limitadas.

Esta spec cria o runtime/contrato de cálculo de preços por canal, sem fixar todos os valores finais.

---

## 2. Problema

Sem pricing profile central:

```text
cada loja pode calcular preço de forma diferente;
SellPoint pode pagar mais que loja especializada sem intenção;
buy/sell pode permitir arbitragem infinita;
order reward pode virar melhor canal sempre;
quality pode ser ignorada;
rarity pode inflar preço sem limite;
preço final pode ser persistido como fonte primária;
UI pode virar dona da fórmula.
```

---

## 3. Objetivo

Criar/endurecer:

```text
PricingProfile;
PriceChannel;
PriceRequest;
PriceResult;
SellPrice formula;
BuyPrice formula;
ShippingPrice formula hooks;
ServicePrice hooks;
QualityMultiplier;
RarityModifiers;
Demand/Reputation/Season/Story modifiers;
Anti-arbitrage validator;
No final price persistence as source of truth.
```

---

## 4. Regras de design

```text
BaseValue é valor bruto Q0.
Preço final é recalculável.
SellPoint é seguro/simples, mas nem sempre melhor.
Loja especializada pode pagar melhor por categoria, mas aceita menos itens.
Encomenda paga melhor quando exige condição.
BuyPrice deve ficar acima do preço de venda equivalente salvo exceção limitada.
Nenhuma spec deve permitir loop infinito de comprar barato e vender caro sem limite.
```

---

## 5. User stories / engineering stories

```text
Como shop, quero consultar BuyPrice e ShopBuyFromPlayerPrice por canal.
Como SellPoint, quero consultar ShippingPrice sem duplicar fórmula.
Como order, quero calcular recompensa sem virar venda comum.
Como economy, quero validar anti-arbitragem.
Como save/load, quero recalcular preço derivado e persistir só estado necessário.
```

---

## 6. Escopo

Inclui:

```text
pricing profile contract;
price channel enum;
sell/buy/shipping/order/service price requests;
quality/rarity modifiers;
anti-arbitrage validations;
price result/explanation;
tests.
```

Não inclui:

```text
final item BaseValue tables;
shop inventory UI;
shipping payment runtime;
order reward runtime;
service-specific cost implementation;
stock/restock.
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
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md

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

- BaseValue, SellPrice, BuyPrice, ShippingPrice, ShopBuyFromPlayerPrice, ShopSellToPlayerPrice, OrderRewardValue e ServicePrice são termos canônicos.
- ShopSellToPlayerPrice deve ser maior que ShopBuyFromPlayerPrice salvo exceção limitada.
- SellPrice usa BaseValue, ChannelSellMultiplier, QualityMultiplier, RaritySellModifier, DemandModifier, ReputationSellModifier, SeasonModifier e StoryFlagModifier.
- BuyPrice usa BaseValue, ShopBuyMultiplier, RarityBuyModifier, DemandModifier, ReputationDiscountModifier, SeasonModifier, StockScarcityModifier e StoryFlagModifier.
- Quality multipliers baseline Q0 x1.00, Q1 x1.15, Q2 x1.35, Q3 x1.70, Q4 x2.20.
- Preço final recalculável não deve ser persistido como fonte primária.

### Deferred / future from directions

- Tabela final de BaseValue por ItemId.
- Tuning final de multipliers.
- Order rewards implementation.
- Shipping payment runtime.
- Service-specific cost formulas.
- Shop UI.
- Market demand advanced.

### Explicitly not redefined here

- ItemDefinition contract.
- ShopStockState.
- Inventory backend.
- Quest/order system.
- Repair/upgrade runtime.
- Final economy balance.

## 7. Modelo de domínio

### 7.1 PriceChannel

```text
SellPoint
GenericShopBuyFromPlayer
SpecializedShopBuyFromPlayer
ShopSellToPlayer
OrderReward
FestivalReward
ServicePrice
ContractFuture
Debug
```

### 7.2 PricingProfile

```text
ProfileId
BaseGlobalSellMultiplier
BaseGlobalBuyMultiplier
ChannelSellMultipliers
CategorySellMultipliers
CategoryBuyMultipliers
QualityMultipliers
RaritySellModifiers
RarityBuyModifiers
DemandModifiers
SeasonModifiers
ReputationSellModifiers
ReputationDiscountModifiers
StoryFlagModifiers
StockScarcityModifiers
MinPrice
RoundingRule
DebugTags
```

### 7.3 PriceRequest

```text
ItemId
ItemCategory
BaseValue
Quantity
Quality optional
Rarity
Channel
ShopId optional
NpcId optional
ReputationTier optional
Season optional
DemandState optional
StoryFlags[]
StockState optional
IsOrderRequest optional
ServiceType optional
```

### 7.4 PriceResult

```text
Success
FailureReason
UnitPrice
TotalPrice
AppliedMultipliers[]
RoundingApplied
ProtectionFlags[]
IsLimitedException
DebugExplanation
```

---

## 8. Formula contracts

### SellPrice

```text
SellPrice = floor(BaseValue
                * ChannelSellMultiplier
                * QualityMultiplier
                * RaritySellModifier
                * DemandModifier
                * ReputationSellModifier
                * SeasonModifier
                * StoryFlagModifier)
```

### BuyPrice

```text
BuyPrice = ceil(BaseValue
              * ShopBuyMultiplier
              * RarityBuyModifier
              * DemandModifier
              * ReputationDiscountModifier
              * SeasonModifier
              * StockScarcityModifier
              * StoryFlagModifier)
```

### Anti-arbitrage invariant

```text
ShopSellToPlayerPrice > ShopBuyFromPlayerPrice
```

Allowed exceptions:

```text
event;
quest;
contract;
reputation high;
limited arbitrage by stock/time;
unique authored case.
```

Every exception must be explicit and bounded.

---

## 9. Channel rules

```text
SellPoint:
  - safe/simple;
  - usually x0.90-x1.00;
  - no stock refresh;
  - payment timing handled by shipping spec.

Generic shop:
  - broad categories;
  - lower sell price paid to player;
  - convenience.

Specialized shop:
  - better sell price for accepted categories;
  - narrower categories;
  - tied to NPC/service identity.

Order:
  - not a shop;
  - reward can be higher because it requires item/quality/quantity/deadline/flag;
  - cannot become always-best channel.

Service:
  - cost of repair/upgrade/construction/heal/refine/training;
  - may use profile but service formula is service-specific.
```

---

## 10. Persistence

Persist:

```text
PricingProfileId references.
Active modifier state if temporary/story/reputation/demand and not derivable.
Exception flags/counters if limited.
```

Do not persist as primary source:

```text
final price tooltip;
last UI preview price;
derived channel multiplier;
recalculable price result.
```

---

## 11. Criteria

```text
PricingProfile exists or is hardened.
PriceRequest/PriceResult are deterministic.
Sell/Buy/Shipping/Order/Service channels are distinct.
Quality and rarity modifiers apply correctly.
BuyPrice > sell equivalent unless explicit bounded exception.
Protected/non-sellable items fail safely.
Price final is not persisted as source of truth.
Tests cover formulas, quality, rarity, channel and anti-arbitrage.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Economy/Pricing/PriceChannel.cs
Assets/_Game/Scripts/Economy/Pricing/PricingProfile.cs
Assets/_Game/Scripts/Economy/Pricing/PriceRequest.cs
Assets/_Game/Scripts/Economy/Pricing/PriceResult.cs
Assets/_Game/Scripts/Economy/Pricing/EconomyPricingService.cs
Assets/_Game/Scripts/Economy/Pricing/AntiArbitrageValidator.cs
Assets/_Game/Tests/EditMode/Economy/EconomyPricingServiceTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Shops/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Economy/**
docs/validation/06_spec_economy_pricing_profile_buy_sell_channels_runtime_execution_report.md
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
1. Auditar economy/pricing/shop/shipping/order price code existente.
2. Consolidar PricingProfile/PriceRequest/PriceResult.
3. Implementar/harden price service.
4. Implementar anti-arbitrage validator.
5. Garantir protected item handling.
6. Criar tests.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - item definition contract;
  - shop inventory stockline/restock;
  - shipping payment;
  - orders reward runtime;
  - service cost runtime.
- Reason: pricing is shared by all economic channels.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless persisting active price modifiers; then STOP.
Does this persist Unity references? NO.
Does this persist final price? NO.
```

---

## 18. Impacto eventos

```text
Adds events: NO by default.
Changes events: NO.
Requires unsubscribe pattern: NO unless listening to reputation/story modifier events.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final; exposes PriceResult/debug explanation for UI.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for shop/SellPoint price display if wired.
```

---

## 20. Riscos

```text
Risco: buy/sell exploit.
Mitigação: anti-arbitrage tests.

Risco: channel formula duplication.
Mitigação: central pricing service.

Risco: price persisted as source.
Mitigação: save/load guardrails.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar pricing/economy/shop code.
- [ ] T003 — Consolidar pricing contracts.
- [ ] T004 — Implementar/harden pricing service.
- [ ] T005 — Implementar anti-arbitrage validator.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a pricing profile/buy-sell channels foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de pricing profile/buy-sell channels? | Arquivos alterados e justificativa. | PARTIAL |
| Economia segura | A spec impede buy/sell exploit, item duplication, preço inválido ou progressão bypassada? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/06_spec_economy_pricing_profile_buy_sell_channels_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "PricingProfile|PriceChannel|PriceRequest|SellPrice|BuyPrice|ShippingPrice|ShopBuyFromPlayerPrice|ShopSellToPlayerPrice|AntiArbitrage" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a pricing profile/buy-sell channels existe ou foi criado de forma mínima
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

- Shop sells to player cheaper than it buys from player.
- Exception without limit.
- SellPoint pays more than specialized shop accidentally.
- Order reward becomes always best channel.
- Quality ignored in price.
- Rarity confused with quality.
- Final price persisted as source of truth.
- Protected item returns price instead of failure.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Economy Pricing Profile Buy Sell Channels Runtime

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


## 23G. Price Channel Matrix

| Channel | Purpose | Guardrail |
|---|---|---|
| SellPoint | safe/simple sale | not always best |
| GenericShopBuyFromPlayer | broad purchase by shop | pays lower |
| SpecializedShopBuyFromPlayer | category-specific purchase | accepts fewer |
| ShopSellToPlayer | player buys | must exceed shop buyback |
| OrderReward | condition-based reward | not always best |
| FestivalReward | event-specific | seasonal/authored |
| ServicePrice | repair/upgrade/heal/build | service-specific |
| ContractFuture | future formal contracts | bounded/future |

## 23H. Anti-arbitrage Test Cases

```text
For normal item and normal shop:
  ShopSellToPlayerPrice > ShopBuyFromPlayerPrice.

For high reputation exception:
  exception must have bounded stock/time/quest rule.

For buyback:
  buyback cost > shop paid to player.
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

- Changed deterministic logic: YES, pricing/anti-arbitrage logic is deterministic.
- Requires EditMode tests: YES for formula/channel/quality/rarity/protected item/anti-arbitrage tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED if shop/SellPoint UI price display is wired.
- Requires regression test: YES if fixing existing price/arbitrage bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no buy/sell exploit; no final price persisted.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/06_spec_economy_pricing_profile_buy_sell_channels_runtime_execution_report.md.
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
