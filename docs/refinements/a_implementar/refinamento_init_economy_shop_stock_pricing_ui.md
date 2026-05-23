# refinamento_init_economy_shop_stock_pricing_ui

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_economy_shop_stock_pricing_ui.md`  
> **Objetivo:** evoluir compra/venda MVP para loja com estoque, preços, UI e regras por NPC/local.

---

## 1. Estado atual

`EconomyManager` processa eventos de compra e venda, valida ouro e inventory, e publica resultado de transação.

Evidência:

```text
Assets/_Game/Scripts/Economy/EconomyManager.cs
Assets/_Game/Scripts/Economy/SellableItemPolicy.cs
Assets/_Game/Scripts/Core/Events/*Economy*.cs
docs/specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md
```

---

## 2. Gaps

- Não há loja com estoque real.
- Não há `ShopDataSO` por NPC/local.
- Não há reposição de estoque por dia/semana.
- Não há variação de preço por estação/reputação/progresso.
- Sell all usa policy simples por ID.
- Não há UI de compra/venda final.
- Não há preview de custo total, quantidade e estoque restante.

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

Campos mínimos:

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
- preços vêm de item data + shop rules;
- transação publica eventos existentes.

### UI

- lista de itens à venda;
- quantidade;
- custo total;
- botão comprar/vender;
- feedback de erro: sem gold, sem estoque, inventory cheio.

---

## 4. Arquivos prováveis

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

- [ ] Loja tem estoque configurável.
- [ ] Compra e venda usam o mesmo cálculo de preço.
- [ ] Estoque finito persiste em save/load.
- [ ] UI mostra itens, preço e quantidade.
- [ ] Erros de transação são visíveis.
- [ ] Pip ou NPC placeholder consegue abrir uma loja configurada.

---

## 6. Validação

1. Comprar item com gold suficiente.
2. Tentar comprar sem gold.
3. Comprar até acabar estoque.
4. Vender item sellable.
5. Salvar/carregar estoque restante.
6. Validar UI no Play Mode.
