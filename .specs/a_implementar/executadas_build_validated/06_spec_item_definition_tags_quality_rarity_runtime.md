# SPEC — Item Definition Tags Quality Rarity Runtime

> **Spec ID:** `06_spec_item_definition_tags_quality_rarity_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 06 — Economy / Loot / Crafting Foundation  
> **Priority:** P0  
> **Type:** Runtime / Data Contract / Items / Tags / Quality / Rarity  
> **Domain:** Economy / ItemDefinition / ItemStack / ItemInstance / Quality / Rarity  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_06_ECONOMY_FOUNDATION_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere inventory stack/instance schema, pricing service, crafting recipes, shop stock, equipment instances, quest/key/lore item protections ou save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/Items/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Economy/**`, `docs/validation/06_spec_item_definition_tags_quality_rarity_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
> **Blocks:**  
  - pricing profile and buy/sell channels;
  - shop inventory stockline/restock;
  - recipe/crafting stations;
  - loot tables;
  - shipping, orders and rewards.
> **Scope:** definir/endurecer contrato mínimo de ItemDefinition, Category, Tags, Quality, Rarity, stack vs instance e proteções de Quest/Key/Unique.  
> **Out of scope:** preços finais por ItemId, loot tables completas, recipes completas, equipment stats, shop UI, save schema migration.

---

# /speckit.specify

## 1. Contexto

Loot, crafting e economia dependem de um contrato de item estável. Os directions exigem que todo item vendável declare `BaseValue`, `CanSell`, `CanBuy`, `Category`, `Rarity`, `QualityEnabled`, `MaxStack` e proteções para QuestItem/KeyItem/Unique.

Esta spec não cria a lista final de todos os itens. Ela cria o contrato mínimo para que preço, loja, crafting, loot, shipping e orders não usem campos divergentes.

---

## 2. Problema

Sem contrato de item:

```text
um item pode ser vendável em uma tela e bloqueado em outra;
quality pode existir no crop mas desaparecer no inventário;
rarity pode ser confundida com quality;
equipment pode stackar indevidamente;
QuestItem/KeyItem/Unique pode ser vendido ou descartado sem proteção;
BaseValue pode faltar para item vendável;
Fruto Mana, Água Viva e Pedra Negra podem virar commodities comuns;
save/load pode persistir prefab ou ScriptableObject.
```

---

## 3. Objetivo

Criar/endurecer:

```text
ItemDefinition contract;
ItemCategory;
ItemTag;
ItemQuality;
ItemRarity;
ItemStack vs ItemInstance boundary;
ItemEconomicFlags;
ItemProtectionRules;
BaseValue validation;
Quality/Rarity distinction;
Quest/Key/Unique protections;
save/load safe IDs.
```

---

## 4. Regras de design

```text
Cada item deve ter pelo menos uma função clara.
Category define comportamento geral.
Tags definem usos específicos.
Quality representa excelência de produção/coleta.
Rarity representa disponibilidade/peso econômico.
Tier representa progressão técnica/material e não é quality/rarity.
QuestItem e KeyItem não devem ser vendíveis por padrão.
Unique deve ter proteção contra perda acidental.
Fruto Mana, Água Viva e Pedra Negra estabilizada não são commodities comuns.
```

---

## 5. User stories / engineering stories

```text
Como economy, quero calcular preço a partir de BaseValue, quality, rarity e canal.
Como inventory, quero saber se item stacka ou é instância.
Como crafting, quero filtrar ingredientes por category/tags.
Como shop, quero bloquear item proibido e aceitar categorias específicas.
Como save/load, quero persistir IDs/estado, não referências Unity.
```

---

## 6. Escopo

Inclui:

```text
ItemDefinition minimum fields;
category/tag taxonomy;
quality enum/profile;
rarity enum/profile;
economic flags;
sell/buy/gift/discard protections;
stack vs instance rules;
validators;
tests.
```

Não inclui:

```text
final item database;
exact BaseValue per item;
full inventory rewrite;
equipment stats;
price formulas;
shop stock;
recipes;
loot tables.
```

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

- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
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

- Todo item vendável deve declarar ItemId, Category, Rarity, BaseValue, QualityEnabled, CanSell, CanBuy, CanGift, CanDiscard e MaxStack.
- Cada item deve ter Category e Tags; Category define comportamento geral e Tags definem usos específicos.
- QuestItem e KeyItem não devem ser vendíveis por padrão; Unique deve ter proteção contra venda/descarte acidental.
- Qualidade é diferente de raridade; raridade é diferente de tier.
- ItemStack cobre itens comuns com quantidade/quality; ItemInstance cobre equipamento, wands, durabilidade, charges, upgrades, affixes ou estado próprio.
- Fruto Mana, Água Viva da Fonte e Pedra Negra estabilizada não são commodities comuns.

### Deferred / future from directions

- Tabela final de BaseValue por ItemId.
- Loot tables completas.
- Recipes completas.
- Equipment stat budgets.
- Shop stock final.
- UI final de item details.
- Save schema migration.

### Explicitly not redefined here

- Inventory backend completo.
- Pricing formulas.
- Crafting station runtime.
- Quest/order delivery.
- Equipment mechanics.
- Magic item mechanics.

## 7. Modelo de domínio

### 7.1 ItemDefinition

```text
ItemId
DisplayName
Description
Category
Tags[]
Rarity
BaseValue
MaxStack
QualityEnabled
CanSell
CanBuy
CanGift
CanDiscard
CanCraftWith
CanCookWith
CanUseAsIngredient
IsQuestItem
IsKeyItem
IsUnique
RequiresStrongDiscardConfirmation
RequiresStrongSellConfirmation
IconId
SpriteId
LoreTags[]
DebugTags[]
```

### 7.2 ItemCategory

```text
RawResource
Crop
Seed
AnimalProduct
Fish
Forage
WoodResource
StoneResource
Ore
Ingot
Gem
MonsterPart
Reagent
Component
CraftedGood
Food
Potion
Fertilizer
Tool
Weapon
Armor
Shield
Accessory
ArrowAmmo
Wand
Scroll
Tome
Focus
Relic
QuestItem
LoreItem
KeyItem
Decoration
BuildingMaterial
```

### 7.3 ItemTag

```text
Sellable
CraftingMaterial
CookingIngredient
AlchemyIngredient
UpgradeMaterial
RepairMaterial
Giftable
QuestRequired
OrderEligible
FestivalEligible
EquipmentMaterial
Consumable
Placeable
BuildMaterial
LoreLocked
NonSellable
Unique
Stackable
PerishableFuture
```

### 7.4 ItemQuality

```text
Q0_Normal
Q1_Boa
Q2_Excelente
Q3_RaraOuPerfeita
Q4_LendariaOuEspecial
```

### 7.5 ItemRarity

```text
Common
Uncommon
Rare
Epic
Legendary
Unique
```

---

## 8. Stack vs instance rules

```text
Stackable:
  - seeds;
  - crops of same ItemId + Quality;
  - common materials;
  - ores/ingots when no instance state;
  - monster parts;
  - arrows;
  - simple scrolls if no charges/state.

Instance:
  - equipment;
  - tools with durability/upgrades;
  - weapons/armor/shields/accessories;
  - wands with charges;
  - unique/relic/lore items with state;
  - items with durability/affixes/modifiers.
```

Stack key must include:

```text
ItemId
Quality if QualityEnabled
Rarity only if item can have same ItemId with multiple rarity variants
Bound/Locked flags if relevant
```

---

## 9. Protection rules

```text
QuestItem: CanSell=false, CanDiscard=false by default.
KeyItem: CanSell=false, CanDiscard=false by default.
Unique: CanSell=false or strong confirmation + authored recovery path.
LoreItem: may be sellable only if lore/progression not broken.
Fruto Mana: special/lore/economy guarded, not common commodity.
Água Viva: special Fonte resource, not common commodity.
Pedra Negra estabilizada: gated material, not common commodity.
Pedra Negra cultista/corrompida: dangerous/lore, not common sellable material.
```

---

## 10. Validation rules

```text
If CanSell=true, BaseValue must be > 0.
If CanBuy=true, BaseValue must be > 0.
If IsQuestItem=true, CanSell should be false unless explicit override.
If IsKeyItem=true, CanSell and CanDiscard should be false.
If IsUnique=true, loss protections must exist.
If QualityEnabled=true, item stack/instance must preserve quality.
If Category is equipment/wand/tool, stack rules must be instance unless explicit safe exception.
```

---

## 11. Save/load

Persist:

```text
ItemId
Quantity
Quality
Durability/Charges/UpgradeLevel when instance
InstanceId when instance
Bound/Locked flags if applicable
```

Do not persist:

```text
ScriptableObject reference;
Prefab reference;
Icon/Sprite as source of truth;
final price as item state.
```

---

## 12. Criteria

```text
ItemDefinition minimum fields exist or are hardened.
Quality and Rarity are distinct.
Sellable items require BaseValue.
Quest/Key/Unique protections exist.
Stack vs instance boundary is explicit.
No final item table is required.
Tests cover validation and protection rules.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Items/ItemDefinition.cs
Assets/_Game/Scripts/Items/ItemCategory.cs
Assets/_Game/Scripts/Items/ItemTag.cs
Assets/_Game/Scripts/Items/ItemQuality.cs
Assets/_Game/Scripts/Items/ItemRarity.cs
Assets/_Game/Scripts/Items/ItemEconomicFlags.cs
Assets/_Game/Scripts/Items/ItemProtectionRules.cs
Assets/_Game/Scripts/Items/ItemDefinitionValidator.cs
Assets/_Game/Tests/EditMode/Economy/ItemDefinitionValidationTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Economy/**
docs/validation/06_spec_item_definition_tags_quality_rarity_runtime_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia

```text
1. Auditar ItemDefinition/Inventory/Economy existentes.
2. Consolidar category/tag/quality/rarity.
3. Implementar/harden validators.
4. Garantir protections de Quest/Key/Unique.
5. Garantir stack-vs-instance boundary.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - pricing profile;
  - shop stock;
  - crafting recipes;
  - inventory schema;
  - equipment item instances.
- Reason: ItemDefinition é fundação compartilhada.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if item IDs/quality already exist; otherwise STOP.
Does this add save section? NO.
Does this require migration? NO unless changing item stack/instance shape; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: NO by default.
Changes events: NO.
Requires unsubscribe pattern: NO unless validators subscribe to lifecycle.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED only if item UI/inventory projection is wired.
```

---

## 21. Riscos

```text
Risco: schema drift com Inventory.
Mitigação: audit and STOP if migration needed.

Risco: bloquear item existente por validator novo.
Mitigação: report violations and do not auto-migrate content.

Risco: Fruto Mana/Água Viva virar sellable.
Mitigação: protection tags/validators.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar item/inventory/economy.
- [ ] T003 — Consolidar categories/tags/quality/rarity.
- [ ] T004 — Implementar/harden item definition validator.
- [ ] T005 — Implementar protection rules.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a item definitions/tags/quality/rarity foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de item definitions/tags/quality/rarity? | Arquivos alterados e justificativa. | PARTIAL |
| Economia segura | A spec impede buy/sell exploit, item duplication, preço inválido ou progressão bypassada? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/06_spec_item_definition_tags_quality_rarity_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "ItemDefinition|ItemCategory|ItemTag|Quality|Rarity|BaseValue|CanSell|CanBuy|QuestItem|KeyItem|Unique|ItemStack|ItemInstance" Assets/_Game/Scripts docs/design .specs
rg -n "ItemDefinition|ItemCategory|ItemTag|Quality|Rarity|Pricing|BaseValue|Shop|StockLine|Restock|Recipe|Crafting|Processing|Station|AntiArbitrage|Save" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given o sistema base relacionado a item definitions/tags/quality/rarity existe ou foi criado de forma mínima
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

- Item vendável sem BaseValue.
- QuestItem/KeyItem vendível por padrão.
- Unique descartável sem confirmação/recovery.
- Quality perdida no stack.
- Rarity usada como Quality.
- Equipment/wand/tool stackando indevidamente.
- Fruto Mana/Água Viva/Pedra Negra tratados como commodities.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Item Definition Tags Quality Rarity Runtime

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


## 23G. Item Classification Matrix

| Concept | Meaning | Must not be confused with |
|---|---|---|
| Category | comportamento geral | tags/rarity |
| Tags | usos específicos | category |
| Quality | excelência de produção/coleta | rarity/tier |
| Rarity | disponibilidade/peso econômico | quality/tier |
| Tier | progressão técnica/material | quality/rarity |
| ItemStack | quantidade comum | instance state |
| ItemInstance | item com estado próprio | simple stack |

## 23H. Protected Item Matrix

| Item kind | CanSell default | CanDiscard default |
|---|---:|---:|
| QuestItem | NO | NO |
| KeyItem | NO | NO |
| Unique | NO or strong rule | NO or strong rule |
| LoreLocked | authored | authored |
| Fruto Mana | special authored | protected |
| Água Viva | special authored | protected |
| Pedra Negra estabilizada | gated authored | protected |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "ItemDefinition|ItemCategory|ItemTag|Quality|Rarity|Pricing|BaseValue|Shop|StockLine|Restock|Recipe|Crafting|Processing|Station|AntiArbitrage|Save" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES, item validation/protection logic is deterministic.
- Requires EditMode tests: YES for BaseValue/protection/stack-instance/quality-rarity tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED if item UI/inventory projection is wired.
- Requires regression test: YES if fixing existing item sell/discard/quality bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no protected item commodity leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/06_spec_item_definition_tags_quality_rarity_runtime_execution_report.md.
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
