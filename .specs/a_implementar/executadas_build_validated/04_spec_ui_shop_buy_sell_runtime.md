# SPEC — UI Shop Buy Sell Runtime

> **Spec ID:** `04_spec_ui_shop_buy_sell_runtime`  
> **Status:** A implementar  
> **Revision:** EXPANDED_06_07  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P0  
> **Type:** Runtime / UI / Shop / Buy Sell / Economy Projection  
> **Domain:** UI / Shop / Buy / Sell / Stock / Pricing / Empty States  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere shop/economy backend, inventory sell projection, item pricing, stock refresh, unique/limited stock or modal focus.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_shop_buy_sell_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `.specs/a_implementar/04_spec_ui_inventory_items_tooltips_runtime.md`
> **Blocks:**  
  - shop/economy backend;
  - inventory sell projection;
  - pricing/restock runtime;
  - limited/unique stock validation;
> **Scope:** consolidar Shop Buy/Sell UI para separar estoque da loja e inventário do jogador, com preço/quantidade/empty states e proteções anti-erro.  
> **Out of scope:** economy formulas final, stock refresh backend, shop content, NPC services, scene/prefab final layout, balance.

---

# /speckit.specify

## 1. Contexto

O direction de UI/UX aponta problemas reais: vendedores não mostram itens para vender e, ao selecionar vender, inventário do jogador não aparece corretamente.

O menu flow define layout obrigatório de Shop Buy/Sell: header com loja/NPC, gold, modo; Buy com estoque da loja; Sell com inventário vendável do jogador; preço unitário, quantidade, total, estoque e empty states.

---

## 2. Problema

Sem Shop UI contract:

```text
Sell pode mostrar estoque da loja em vez do inventário do jogador;
loja vazia pode parecer bug;
item quest/key pode ser vendido;
stock limited/unique pode não ficar claro;
preço pode usar valor persistido errado;
compra/venda pode permitir arbitragem visualmente opaca.
```

---

## 3. Objetivo

Consolidar Shop Buy/Sell UI:

```text
Buy tab mostra shop stock.
Sell tab mostra player sellable inventory.
Empty states explícitos.
Preço unitário/total/quantidade claros.
LimitedStock/UniqueStock visíveis.
Quest/key/equipped/favorite blocked.
Preço vem do economy service.
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

- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Shop UI separa Buy, Sell, Shop Inventory, Player Inventory, Gold, Preço unitário, Quantidade, Total, Estoque e Restock/Limited/Unique.
- Buy mostra estoque da loja; Sell mostra inventário vendável do jogador.
- Empty states: loja sem itens, jogador sem itens vendáveis, vendedor não compra categoria, estoque esgotado.
- UniqueStock vendido não reaparece; LimitedStock respeita counter.
- Preço de venda vem da economia, não de preço final persistido.
- Nenhuma spec deve permitir loop infinito de comprar barato e vender caro sem limite.

### Deferred / future from directions

- Shop backend formulas final.
- Full stock refresh implementation.
- NPC service roster.
- Balance de economia.
- Visual prefab final.

### Explicitly not redefined here

- Economy pricing service.
- Inventory backend.
- Item data schema.
- NPC schedules.
- Shop content.

## 4. Estado atual do repo

```text
Audit report indica shop/economy completos ou parciais e UI shop/sell bundle existente.
Esta spec deve auditar e harden, não recriar shop backend.
```

A confirmar localmente:

```text
ShopManager;
ShopUI;
Sell tab;
player inventory projection;
stock state;
price service;
empty states.
```

---

## 5. User stories

```text
Como jogador, quero comprar vendo estoque da loja.
Como jogador, quero vender vendo meu inventário vendável.
Como jogador, quero entender por que não posso vender um item.
Como jogador, quero ver total antes de confirmar.
Como dev, quero impedir UI que permita arbitragem invisível.
```

---

## 6. Escopo

```text
Buy/Sell tab projection;
shop/player inventory separation;
empty states;
quantity/price/total display;
limited/unique stock display;
non-sellable protection;
tests/validators.
```

---

## 7. Fora de escopo

```text
economy backend rewrite;
pricing formulas final;
stock refresh backend;
shop content authoring;
NPC service routing;
scene/prefab final.
```

---

## 8. Regras de não duplicação

```text
Não criar ShopManager paralelo.
Não duplicar pricing rules in UI.
Não usar shop stock como sell inventory.
Não vender Quest/Key item por UI.
Não permitir buy/sell loop sem stock/time limit.
```

---

## 9. Critérios de aceite

- Buy and Sell use correct data sources.
- Empty states explicit.
- Prices/quantity/total visible.
- Non-sellable/equipped/favorite protections.
- Limited/Unique stock state shown.
- Report includes PlayMode scenario.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Shop/ShopScreenController.cs
Assets/_Game/Scripts/UI/Shop/ShopBuySellViewModel.cs
Assets/_Game/Scripts/UI/Shop/ShopLineItemViewModel.cs
Assets/_Game/Tests/EditMode/UI/ShopBuySellViewModelTests.cs
```

---

## 11. Contratos

### Data

```text
Buy projection comes from shop stock.
Sell projection comes from player inventory filtered by shop/economy rules.
```

### Runtime

```text
UI dispatches buy/sell commands; backend owns transaction.
```

### Save

```text
No save schema change.
```

### UI

```text
Empty states and stock states are explicit.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_shop_buy_sell_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Items/**
```

---

## 13. Arquivos proibidos

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

## 14. Estratégia

```text
1. Auditar shop UI/economy/inventory.
2. Consolidar view model.
3. Add protections/empty states.
4. Add tests for buy/sell data source and non-sellable filtering.
5. Report final PlayMode scenarios.
```

---

## 15. Ordem segura

```text
Input focus -> Inventory projection -> Shop Buy/Sell UI -> economy backend future hardening.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with inventory UI, shop/economy backend or stock refresh changes.
- Reason: shared pricing/stock/inventory projections.

---

## 17. Impacto save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: SHOULD BE NO.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if UI subscribers touched.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: YES view model/behavior.
Scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED.
```

---

## 20. Riscos

```text
Risco: sell tab mostrar dados errados.
Mitigação: tests de source.

Risco: price drift.
Mitigação: UI calls economy service.

Risco: non-sellable vendido.
Mitigação: filter/protection tests.
```

---

## 21. Rollback

```text
Reverter shop UI/view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar shop/economy/inventory UI.
- [ ] T003 — Consolidar Buy/Sell projections.
- [ ] T004 — Implementar empty states/protections.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu os directions e refinements listados em Source Map Compliance? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | A execução auditou classes existentes antes de criar novas? | Comandos `rg` e achados principais no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão: reuse/harden/create. | BLOCKED se criar duplicata |
| Save/load | A spec altera ou depende de estado persistido? | Declaração explícita de schema/no schema. | PARTIAL |
| Eventos | A spec cria/usa eventos ou subscriptions? | Mapa de publishers/subscribers e unsubscribe policy. | PARTIAL |
| UI/Input | Há foco/modal/PlayMode relevante? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo |
| Testes | Há lógica determinística nova? | EditMode test ou justificativa NOT RUN. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/04_spec_ui_shop_buy_sell_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Shop|Buy|Sell|Stock|LimitedStock|UniqueStock|Price|Gold|Sellable|CanSell|ShopInventory|PlayerInventory|EmptyState" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a shop buy/sell UI existe ou foi criado de forma mínima
When o usuário/sistema executa o fluxo principal desta spec
Then o estado visível/resultado segue o direction canônico
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 3 — Save/load safety

```text
Given o fluxo envolve estado persistido direta ou indiretamente
When save/load ocorre após a ação
Then nenhum dado derivado de UI/debug substitui a fonte de verdade
And nenhum UnityEngine.Object é persistido
And schema change exige spec/migration separada.
```

### Scenario 4 — Final human validation deferred

```text
Given o fluxo exige interação visual ou PlayMode integrado
When a spec é concluída tecnicamente
Then o report registra cenário final em vez de pedir validação humana imediata
And o status máximo respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Sell tab mostrando estoque da loja em vez do inventário do jogador.
- Buy tab mostrando inventário do jogador.
- Quest/Key item vendável.
- Preço UI divergente do economy service.
- LimitedStock/UniqueStock sem indicação.
- Total de compra/venda incorreto ao mudar quantidade.
- Loja vazia sem empty state.
- Compra/venda processada com gold insuficiente ou estoque zero.

---

## 23E. Minimum Execution Report Template

O execution report desta spec deve conter, no mínimo:

```md
# Execution Report — UI Shop Buy Sell Runtime

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
- Edge cases:
- Negative cases:

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
4. A implementação exigir reescrever sistema canônico existente.
5. A implementação criar conflito com 01Q, input focus, save ownership ou registry.
6. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Buy/Sell Projection Contract

| Aba | Fonte de dados | Filtro | Ação |
|---|---|---|---|
| Buy | Shop stock | Available stock, known service, unlocks | Buy command |
| Sell | Player inventory | CanSell + shop accepts category + not protected | Sell command |
| Buy empty | Shop stock vazio/esgotado | N/A | Empty state |
| Sell empty | Player sem item vendável | N/A | Empty state |
| LimitedStock | Shop stock state | Count > 0 | Mostrar quantidade |
| UniqueStock | Shop unique state | Not purchased | Mostrar único/esgotado |

## 23H. Price Display Requirements

```text
Unit price.
Quantity selected.
Total price.
Player gold after transaction preview.
Reason disabled: no gold, no stock, cannot sell, shop does not buy category, protected item.
Source: economy/pricing service, not UI formula.
```

## 23I. Anti-Arbitrage UI Guardrails

```text
UI must not create price.
UI must show limited/unique stock when relevant.
Buy and sell projections must not allow same item loop without stock/time/reputation limits.
Suspicious buy<=sell cases must be reported unless marked as event/quest/limited exception.
```

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Dialogue|Inventory|Equipment|Tooltip|Shop|Buy|Sell|Modal|Focus|Confirm|ItemDetail|Compare|Stock|Price" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário integrado, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if shop view model/filter/projection logic changes.
- Requires EditMode tests: YES for buy/sell projection and non-sellable filter logic.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for shop open/buy/sell/empty states scenario.
- Requires regression test: YES if fixing known shop sell inventory bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; Buy/Sell data sources correct; non-sellable protections enforced.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_shop_buy_sell_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não usar UI como fonte de verdade.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
Não alterar scenes/prefabs/assets nesta spec.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
