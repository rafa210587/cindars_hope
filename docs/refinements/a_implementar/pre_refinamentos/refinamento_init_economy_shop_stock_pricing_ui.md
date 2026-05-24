# refinamento_init_economy_shop_stock_pricing_ui

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_economy_shop_stock_pricing_ui.md`
> **Objetivo:** evoluir compra/venda MVP para loja com estoque, preÃ§os, UI e regras por NPC/local.

---

## 1. Estado atual

`EconomyManager` processa eventos de compra e venda, valida ouro e inventory, e publica resultado de transaÃ§Ã£o.

EvidÃªncia:

```text
Assets/_Game/Scripts/Economy/EconomyManager.cs
Assets/_Game/Scripts/Economy/SellableItemPolicy.cs
Assets/_Game/Scripts/Core/Events/*Economy*.cs
docs/specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md
```

---

## 2. Gaps

- NÃ£o hÃ¡ loja com estoque real.
- NÃ£o hÃ¡ `ShopDataSO` por NPC/local.
- NÃ£o hÃ¡ reposiÃ§Ã£o de estoque por dia/semana.
- NÃ£o hÃ¡ variaÃ§Ã£o de preÃ§o por estaÃ§Ã£o/reputaÃ§Ã£o/progresso.
- Sell all usa policy simples por ID.
- NÃ£o hÃ¡ UI de compra/venda final.
- NÃ£o hÃ¡ preview de custo total, quantidade e estoque restante.

---

## 3. Escopo esperado

### Dados

Criar:

```text
ShopDataSO
ShopItemEntry
ShopStockRule
PriceModifierRule
```

Campos mÃ­nimos:

```text
ShopId
DisplayName
NpcId opcional
Items[]
BaseBuyPriceOverride opcional
BaseSellMultiplier
DailyRestock
UnlockConditions futuro
```

### Runtime

Criar/expandir:

```text
ShopManager
ShopSession
ShopStockSaveData
```

Regras:

- compra decrementa estoque se finito;
- venda incrementa gold e remove inventory;
- preÃ§os vÃªm de item data + shop rules;
- transaÃ§Ã£o publica eventos existentes.

### UI

- lista de itens Ã  venda;
- quantidade;
- custo total;
- botÃ£o comprar/vender;
- feedback de erro: sem gold, sem estoque, inventory cheio.

---

## 4. Arquivos provÃ¡veis

```text
Assets/_Game/Scripts/Economy/EconomyManager.cs
Assets/_Game/Scripts/Economy/ShopManager.cs
Assets/_Game/Scripts/Economy/Data/ShopDataSO.cs
Assets/_Game/Scripts/Economy/Data/ShopItemEntry.cs
Assets/_Game/Scripts/UI/Shop/ShopPanelController.cs
Assets/_Game/Scripts/Save/SaveData.cs
```

---

## 5. Definition of Done

- [ ] Loja tem estoque configurÃ¡vel.
- [ ] Compra e venda usam o mesmo cÃ¡lculo de preÃ§o.
- [ ] Estoque finito persiste em save/load.
- [ ] UI mostra itens, preÃ§o e quantidade.
- [ ] Erros de transaÃ§Ã£o sÃ£o visÃ­veis.
- [ ] Pip ou NPC placeholder consegue abrir uma loja configurada.

---

## 6. ValidaÃ§Ã£o

1. Comprar item com gold suficiente.
2. Tentar comprar sem gold.
3. Comprar atÃ© acabar estoque.
4. Vender item sellable.
5. Salvar/carregar estoque restante.
6. Validar UI no Play Mode.
