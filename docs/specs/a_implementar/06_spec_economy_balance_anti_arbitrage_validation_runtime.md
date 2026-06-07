# SPEC — Economy Balance Anti Arbitrage Validation Runtime

> **Spec ID:** `06_spec_economy_balance_anti_arbitrage_validation_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 06 — Economy / Loot / Rewards Foundation  
> **Priority:** P0  
> **Type:** Runtime / Validation / Economy Balance / Anti-Arbitrage  
> **Domain:** Economy / Balance Validation / GoldHour / BuySell / Processing / Stock / Rewards  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_06_LOOT_REWARDS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere final prices, pricing service formulas, shop stock runtime, processing recipes, SellPoint payment, order rewards, save schema ou CI validation hooks.  
> **Repo lock scope:** `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Economy/**`, `docs/validation/06_spec_economy_balance_anti_arbitrage_validation_runtime_execution_report.md`  
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
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
> **Blocks:**  
  - economy test harness;
  - pricing profile validation;
  - shop stock validation;
  - recipe/process validation;
  - future balance pass before execution.
> **Scope:** definir/endurecer validators/testes transversais de economia: buy/sell, gold/hour, processing multiplier, stock/reload exploit, unique/protected items e reward duplication.  
> **Out of scope:** tuning final de números, playtest real de gold/hour, UI, live telemetry, final balance tables.

---

# /speckit.specify

## 1. Contexto

As directions fecham regras econômicas transversais: ouro não deve resolver todos os gates sozinho, lojas não substituem exploração, SellPoint é seguro mas nem sempre melhor, restock não ocorre ao abrir menu, processamento aumenta valor mas exige tempo/capacidade, e boss drops não viram farm infinito.

Esta spec cria validação automatizável para impedir regressões econômicas antes de executar muitas specs runtime.

---

## 2. Problema

Sem validação transversal:

```text
um preço isolado pode criar arbitragem infinita;
restock por menu/reload pode gerar compra infinita;
processing pode multiplicar valor sem custo/tempo;
SellPoint pode ser melhor que todo canal;
encomenda pode ser sempre melhor;
boss repeat reward pode gerar gold/hour absurdo;
Unique/Quest/Key/Lore item pode ser vendável.
```

---

## 3. Objetivo

Criar/endurecer:

```text
EconomyBalanceValidator;
AntiArbitrageTestSuite;
BuySellInvariant checks;
RestockExploit checks;
ProcessingValue checks;
GoldHour budget placeholders;
ProtectedItem checks;
Unique/reward duplication checks;
Report generation.
```

---

## 4. Regras de design

```text
Nenhum pilar deve substituir completamente os outros.
Fazenda = estabilidade.
Caverna = risco, materiais raros, gear/progressão.
Cidade = serviços, receitas, lojas, reputação.
Crafting deve ser estratégico.
Venda direta não deve ser sempre melhor que craft/processamento/encomenda.
```

---

## 5. User stories / engineering stories

```text
Como executor, quero detectar economia quebrada antes de aceitar specs.
Como pricing, quero validar BuyPrice > SellPrice equivalente.
Como shop, quero validar restock e unique/limited stock.
Como crafting, quero validar processamento com tempo/capacidade.
Como loot, quero validar boss/rare/protected reward.
```

---

## 6. Escopo

Inclui:

```text
validator contracts;
test suites;
balance report format;
anti-arbitrage cases;
protected item cases;
stock/reload cases;
processing/reward duplication checks;
gold/hour placeholder budgets.
```

Não inclui:

```text
final balance numbers;
actual telemetry;
UI dashboards;
final item table tuning;
CI pipeline changes unless already present and safe.
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
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md

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

- Economy must avoid infinite early gold.
- ShopSellToPlayerPrice must exceed ShopBuyFromPlayerPrice except bounded exceptions.
- Stock limited should not reset on menu/reload; restock happens by day/week/season/event/progress.
- Processing increases value but requires time/capacity/station.
- Boss first-time reward must be separate from repeat reward.
- Validation pendencies include real prices vs day/farm time, gold/hour farm vs cave, seed cost vs crop profit, processing value vs time/capacity, repair cost vs durability stress, shop not replacing cave/farm, orders not always best channel, stack sizes and save/load of pending payments/timers/item instances.

### Deferred / future from directions

- Final numeric tuning.
- Live analytics.
- Full CI integration.
- All item BaseValue table.
- Full recipe profitability table.
- Full boss reward content.

### Explicitly not redefined here

- PricingProfile service.
- ShopStockState.
- Recipe/Crafting/Processing contracts.
- Loot table contracts.
- Inventory save schema.
- Runtime UI.

## 7. Modelo de domínio

### 7.1 EconomyValidationRule

```text
RuleId
RuleType
Severity: Info | Warning | Error | Blocker
Scope: Item | PriceChannel | Shop | Stock | Recipe | Loot | Reward | Save
Inputs[]
ExpectedInvariant
FailureMessage
SuggestedAction
```

### 7.2 EconomyValidationReport

```text
ReportId
GeneratedAt
RulesRun
Passed
Warnings[]
Errors[]
Blockers[]
AffectedItems[]
AffectedShops[]
AffectedRecipes[]
AffectedLootTables[]
ResidualRisks[]
```

### 7.3 AntiArbitrageCase

```text
ItemId
BuyChannel
SellChannel
BuyPrice
SellPrice
StockLimit
RestockPolicy
ExceptionReason optional
IsBoundedException
```

---

## 8. Required validation groups

### 8.1 Buy/sell

```text
For normal item/shop:
  ShopSellToPlayerPrice > ShopBuyFromPlayerPrice.
If exception:
  must be event/quest/contract/reputation/limited stock/time/unique authored.
```

### 8.2 Sell channels

```text
SellPoint not always best.
Specialized shop can be better only for accepted category.
Order reward better only when requiring condition.
Festival/event rewards are seasonal/authored.
```

### 8.3 Stock/restock

```text
Restock not on menu open.
UniqueStock not restocked.
LimitedStock counters persisted.
RotatingStock seeded.
PlayerSoldStock disabled baseline or bounded buyback.
```

### 8.4 Processing/crafting

```text
Processed output value must require time/capacity/station.
Processing loop cannot create infinite profit with immediate reprocess.
Recipe cannot consume protected item without explicit authored gate.
```

### 8.5 Loot/rewards

```text
Boss first-time reward once.
Repeat reward lower/controlled.
Rare progression item has pity/alternative.
Unique/Key/Quest/Lore protected.
Level 101 not farm loop.
```

### 8.6 Gold/hour placeholders

```text
Farm common crop gold/hour should be stable, not explosive.
Cave gold/hour can be higher but risk/prep gated.
Processing should improve value but consume time/capacity.
Boss repeat reward cannot dominate all economy.
```

This spec sets validation hooks and thresholds placeholders; final thresholds may be tuned later.

---

## 9. Report policy

```text
Any Blocker prevents ACCEPTED.
Errors require fix or explicit residual risk.
Warnings may be accepted only with documented reason.
Report must be committed to docs/validation when validation is executed.
```

---

## 10. Criteria

```text
Validation rules exist or are hardened.
Buy/sell invariant tests exist.
Restock exploit tests exist.
Processing anti-loop tests exist.
Protected item validation exists.
Boss/reward duplication validation exists.
Gold/hour placeholders documented.
Tests generate deterministic results.
```

---

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Economy/Validation/EconomyValidationRule.cs
Assets/_Game/Scripts/Economy/Validation/EconomyValidationReport.cs
Assets/_Game/Scripts/Economy/Validation/AntiArbitrageCase.cs
Assets/_Game/Scripts/Economy/Validation/EconomyBalanceValidator.cs
Assets/_Game/Scripts/Editor/Validation/EconomyBalanceValidationRunner.cs
Assets/_Game/Tests/EditMode/Economy/EconomyAntiArbitrageValidationTests.cs
```

Consolidar existentes se houver.

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Economy/**
docs/validation/06_spec_economy_balance_anti_arbitrage_validation_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Shops/**
Assets/_Game/Scripts/Crafting/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Inventory/**
```

---

## 13. Arquivos proibidos

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

## 14. Estratégia

```text
1. Auditar validators/tests existentes.
2. Consolidar EconomyBalanceValidator.
3. Implementar rules for buy/sell, stock, processing, loot/rewards and protected items.
4. Criar deterministic validation report.
5. Criar tests.
6. Criar report de execução.
```

---

## 15. Paralelização

- Parallelizable: NO
- Must not run with:
  - pricing service changes;
  - shop stock/restock;
  - recipe/processing;
  - loot/reward tables;
  - save schema.
- Reason: validator consumes all economy contracts.

---

## 16. Impacto save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 17. Impacto eventos

```text
Adds events: NO.
Changes events: NO.
Requires unsubscribe pattern: NO.
```

---

## 18. Impacto UI/Unity

```text
Changes UI: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: NO by default; validation report only.
```

---

## 19. Riscos

```text
Risco: validator too strict for placeholder data.
Mitigação: severity levels and residual report.

Risco: false confidence without playtest.
Mitigação: state that final tuning requires playtest.

Risco: no real data yet.
Mitigação: validate contracts and sample fixtures.
```

---

# /speckit.tasks

## 20. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar economy validators/tests.
- [ ] T003 — Consolidar validation rule/report contracts.
- [ ] T004 — Implementar anti-arbitrage checks.
- [ ] T005 — Implementar stock/processing/loot/protected checks.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a economy balance/anti-arbitrage validation foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de economy balance/anti-arbitrage validation? | Arquivos alterados e justificativa. | PARTIAL |
| Snapshot/idempotência | Loot/reward não duplica após reload, revisit, boss repeat ou coleta repetida? | Testes ou validators. | PARTIAL |
| Economia segura | A spec impede gold/hour absurdo, rare loot cedo demais e reward farm infinito? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/06_spec_economy_balance_anti_arbitrage_validation_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "EconomyBalance|AntiArbitrage|GoldHour|BuySell|RestockExploit|ProcessingLoop|ProtectedItem|UniqueStock|BossReward" Assets/_Game/Scripts docs/design docs/specs
rg -n "LootTable|RewardTable|DropTable|EnemyDrop|BossReward|Treasure|MiningNode|CaveSnapshot|FirstTimeReward|RepeatReward|BaseValue|AntiArbitrage|GoldHour|Save" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a economy balance/anti-arbitrage validation existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And save/load, snapshot, economia e inventário permanecem consistentes
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

### Scenario 3 — Duplicate prevention

```text
Given loot/reward já foi coletado, boss já foi derrotado, treasure já foi aberto ou node já foi minerado
When o jogador recarrega, revisita ou tenta repetir a ação
Then o sistema não concede a recompensa única novamente
And reward repetível usa tabela própria, menor e controlada quando existir.
```

### Scenario 4 — Protected/lore/endgame resource

```text
Given Fruto Mana, Água Viva, Pedra Negra estabilizada, Unique, KeyItem, QuestItem ou lore reward
When loot/reward/shop/crafting tenta tratar isso como item comum repetível
Then a ação é bloqueada, marcada future ou exigida por gate explícito
And o report registra proteção.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de caverna, loot popup, chest, mining node, boss reward, loja ou relatório
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- BuyPrice <= SellPrice in normal shop.
- Limited exception without bound.
- Restock happens on menu/reload.
- Processing creates instant infinite value.
- Order always better than all channels.
- Boss repeat reward dominates economy.
- Protected item sellable/craftable as common.
- Validator silently ignores missing data.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Economy Balance Anti Arbitrage Validation Runtime

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
- Duplicate prevention:
- Protected/lore/endgame resource:
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
4. A implementação exigir reescrever Inventory/Economy/Cave/Enemy system canônico existente.
5. A implementação quebrar CaveRunSeed, snapshots, BossDefeatStates, DepletedNodeIds ou VisitedLevelSnapshots.
6. A implementação permitir first-time boss reward mais de uma vez.
7. A implementação tratar Fruto Mana, Água Viva, Pedra Negra estabilizada ou lore/key rewards como commodity comum.
8. A implementação tornar progressão principal dependente exclusivamente de drop raro aleatório.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Validation Severity Policy

| Severity | Meaning | Status impact |
|---|---|---|
| Info | diagnostic | no block |
| Warning | suspicious but possibly acceptable | document |
| Error | wrong or unsafe | fix or residual |
| Blocker | economy exploit/progression break | no ACCEPTED |

## 23H. Minimum Anti-Arbitrage Cases

```text
normal generic shop;
normal specialized shop;
high reputation bounded exception;
limited stock exception;
buyback future if present;
SellPoint vs specialized category;
order reward with quality/deadline;
processing input/output loop.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "LootTable|RewardTable|DropTable|EnemyDrop|BossReward|Treasure|MiningNode|CaveSnapshot|FirstTimeReward|RepeatReward|BaseValue|AntiArbitrage|GoldHour|Save" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de loot, treasure, mining, boss reward, enemy drop ou balance report, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, validator and anti-arbitrage checks are deterministic.
- Requires EditMode tests: YES for buy/sell/restock/processing/protected/boss-repeat validation tests.
- Requires PlayMode automated or final human scenario: NO by default; this is validation/reporting unless UI runner exists.
- Requires regression test: YES if fixing existing economy exploit validation; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; report generated; blockers prevent ACCEPTED.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/06_spec_economy_balance_anti_arbitrage_validation_runtime_execution_report.md.
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
Não gerar reward único duas vezes.
Não criar gold/hour absurdo.
Não transformar loot raro/lore/endgame em commodity comum.
Não tornar progressão principal dependente só de drop raro.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
