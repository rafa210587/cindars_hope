# refinamento_init_economy_shop_stock_pricing_ui

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_economy_shop_stock_pricing_ui.md`
> Objetivo: evoluir compra/venda MVP para lojas de cidade com NPCs, estoque finito, reposicao diaria, UI modal, falas simples de NPC e precos consistentes.

---

## 1. Estado atual

`EconomyManager` processa eventos de compra e venda, valida ouro e inventory, e publica resultado de transacao.

Evidencia:

```text
Assets/_Game/Scripts/Economy/EconomyManager.cs
Assets/_Game/Scripts/Economy/SellableItemPolicy.cs
Assets/_Game/Scripts/Core/Events/*Economy*.cs
docs/specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md
```

---

## 2. Gaps

- Nao ha loja com estoque real.
- Nao ha `ShopDataSO` por NPC/local.
- Nao ha NPCs lojistas especializados.
- Pip ainda pode estar sendo tratado como vendedor, mas deve virar recepcao da cidade.
- NPCs ainda nao possuem contrato simples de fala de abertura e despedida.
- Compra/venda da fazenda deve ser removida/deprecada como fluxo oficial.
- Nao ha reposicao diaria de estoque.
- Nao ha multiplicador de preco preparado para afinidade futura.
- Sell all usa policy simples por ID.
- Nao ha UI modal de compra/venda.
- Nao ha preview de custo total, quantidade e estoque restante.

---

## 3. Decisoes aprovadas

- Criar 2 NPCs lojistas novos:
  - vendedor de armas e armaduras;
  - vendedor de sementes e utensilios.
- Pip nao vende itens nesta spec.
- Pip recebe o jogador na cidade: quando o jogador entra na cidade, Pip se move ate perto do jogador e depois para.
- Todos os NPCs desta spec possuem uma frase de abertura e uma frase de despedida especifica.
- Ao interagir com um NPC, a fala de abertura aparece antes do menu de opcoes.
- Ao fechar a conversa por `Sair` ou `Esc`, a fala de despedida especifica do NPC aparece antes de encerrar o modal.
- Ambos os vendedores, ao conversar, abrem menu modal vertical:

```text
Comprar
Vender
Sair
```

- Ao escolher `Comprar`, abrir HUD/lista com itens que o vendedor tem para vender.
- Cada item comprado diminui do estoque do vendedor ate acabar.
- Compra so conclui se o jogador tiver dinheiro e espaco no inventory.
- Ao escolher `Vender`, abrir inventory do jogador para vender itens sellable.
- Vender item paga 60% do valor normal do item.
- Estoque de itens e finito.
- Reposicao ocorre a cada dia.
- Preco dos itens vendidos pelos vendedores usa multiplicador; por enquanto `x1`, preparado para afinidade futura.
- Compra/venda existente na fazenda deve ser removida, desativada ou marcada como deprecated, sem deixar dois fluxos oficiais competindo.

---

## 4. NPCs e lojas

### Pip Miudinho

Papel:

```text
Recepcao da cidade
Nao e lojista
Nao abre shop
```

Comportamento:

- Quando jogador entra na cidade pela primeira vez na sessao/cena, Pip anda ate perto do jogador.
- Ao chegar em distancia segura, Pip para.
- Pip deve ter uma fala curta de abertura/boas-vindas.
- Pip deve ter uma fala curta de despedida.
- Nao implementar agenda completa, quest ou loja para Pip nesta spec.

### Vendedor de armas e armaduras

IDs sugeridos:

```text
npc_shop_weapons_armor
shop_weapons_armor
```

Categorias vendidas:

```text
Weapon
Armor
Accessory opcional
Shield opcional se existir
```

Deve ter:

```text
OpeningLine
ClosingLine
```

### Vendedor de sementes e utensilios

IDs sugeridos:

```text
npc_shop_seeds_tools
shop_seeds_tools
```

Categorias vendidas:

```text
Seed
Tool
Utility
Consumable opcional
```

Deve ter:

```text
OpeningLine
ClosingLine
```

---

## 5. Dados esperados

Criar ou equivalente:

```text
ShopDataSO
ShopItemEntry
ShopStockRule
PriceModifierRule
ShopSession
ShopStockSaveData
NpcShopInteractionData ou NpcDialogueLiteData
```

Campos minimos de `ShopDataSO`:

```text
ShopId
DisplayName
NpcId
BuyPriceMultiplier default 1.0
SellPriceMultiplier default 0.6
DailyRestock true
Items[]
FutureAffinityPriceModifierEnabled false
```

Campos minimos de `ShopItemEntry`:

```text
ItemId
BaseDailyStock
CurrentStock runtime/save
IsFiniteStock true
BuyPriceOverride opcional
RequiredUnlockTag opcional futuro
```

Campos minimos de dados simples do NPC nesta spec:

```text
NpcId
DisplayName
OpeningLine
ClosingLine
OptionalGreetingTriggerId
```

---

## 6. Pricing

Compra:

```text
BuyPrice = round(ItemDataSO.BaseValue * ShopDataSO.BuyPriceMultiplier)
```

Venda:

```text
SellPrice = floor(ItemDataSO.BaseValue * 0.6)
```

Regras:

- Multiplicador inicial de compra: `1.0`.
- Multiplicador de venda: `0.6`.
- Preparar contrato para afinidade/reputacao futura, mas nao implementar balance final.
- Preco minimo deve ser pelo menos 1 para item vendavel com valor positivo.
- Item com `BaseValue <= 0` ou marcado como nao vendavel nao deve aparecer para venda.

---

## 7. Estoque e reposicao

- Estoque e finito para todos os itens vendidos nesta spec.
- Comprar decrementa `CurrentStock`.
- Quando `CurrentStock == 0`, item aparece como esgotado ou fica indisponivel.
- Reposicao ocorre a cada novo dia.
- Reposicao diaria restaura `CurrentStock` para `BaseDailyStock`.
- Save/load deve preservar estoque restante dentro do dia.
- Ao avancar dia, aplicar restock uma vez por `ShopId`.

---

## 8. Fluxos

### Conversa

```text
Jogador interage com NPC
Mostrar OpeningLine especifica do NPC
Se NPC for lojista, abrir menu modal Comprar / Vender / Sair
Se jogador escolher Sair ou apertar Esc, mostrar ClosingLine especifica
Fechar conversa/modal
Restaurar input do player
```

Garantias:

- Pip mostra falas, mas nao abre loja.
- Lojistas mostram fala de abertura antes do menu.
- Lojistas mostram despedida ao sair.
- Despedida nao deve disparar transacao.
- Fechar com `Esc` deve seguir o mesmo encerramento seguro de `Sair`.

### Comprar

```text
Jogador interage com NPC lojista
OpeningLine
Menu modal: Comprar / Vender / Sair
Escolhe Comprar
Abre ShopBuyPanel com lista de itens, preco, estoque restante e quantidade
Jogador seleciona item/quantidade
Sistema valida gold, estoque e espaco no inventory
Se tudo valido: remove gold, decrementa estoque, adiciona item ao inventory
Se falhar: nada muda e feedback claro aparece
```

Garantias:

- Nao gastar gold se inventory estiver cheio.
- Nao decrementar estoque se item nao foi adicionado ao inventory.
- Nao adicionar item se gold nao foi removido.
- Transacao deve ser atomica do ponto de vista do jogador.

### Vender

```text
Jogador interage com NPC lojista
OpeningLine
Menu modal: Comprar / Vender / Sair
Escolhe Vender
Abre ShopSellPanel usando inventory do jogador
Mostra itens vendaveis e preco de venda = 60% do BaseValue
Jogador seleciona item/quantidade
Sistema remove item do inventory
Sistema adiciona gold ao jogador
```

Garantias:

- Item so e removido se gold for adicionado.
- Itens nao vendaveis nao aparecem ou aparecem desabilitados.
- Venda nao depende do estoque do vendedor.

---

## 9. Save/load

Persistir usando IDs e tipos simples:

```text
ShopStockSaveData
- ShopId
- ItemId
- CurrentStock
- LastRestockDay
```

Regras:

- Nao serializar `ShopDataSO`, `ItemDataSO`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody`.
- Save/load deve restaurar estoque restante.
- Restock diario deve ser idempotente por `ShopId` e dia.
- Se `ShopId` ou `ItemId` nao existir ao carregar, logar erro e falhar de forma segura.
- Falas de abertura/despedida sao conteudo, nao precisam persistir no save.

---

## 10. Invariantes anti-regressao

Esta spec nao pode quebrar:

- inventory slots/capacity da spec 03;
- save migration e DTOs simples da spec 02;
- world pickups/loot da spec 05;
- farm menu contextual da spec 04;
- gold atual do jogador;
- eventos de economy existentes;
- hotbar/HUD existente;
- interacao `E` fora de NPC/shop;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

---

## 11. Arquivos provaveis

```text
Assets/_Game/Scripts/Economy/EconomyManager.cs
Assets/_Game/Scripts/Economy/ShopManager.cs
Assets/_Game/Scripts/Economy/Data/ShopDataSO.cs
Assets/_Game/Scripts/Economy/Data/ShopItemEntry.cs
Assets/_Game/Scripts/Economy/Data/ShopStockRule.cs
Assets/_Game/Scripts/Economy/Data/PriceModifierRule.cs
Assets/_Game/Scripts/UI/Shop/ShopMenuController.cs
Assets/_Game/Scripts/UI/Shop/ShopBuyPanelController.cs
Assets/_Game/Scripts/UI/Shop/ShopSellPanelController.cs
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Town/**
Assets/_Game/Scripts/Save/SaveData.cs
```

---

## 12. Definition of Done

- [ ] Pip nao abre loja.
- [ ] Pip se move ate o jogador ao entrar na cidade e para proximo dele.
- [ ] Pip tem fala de abertura e despedida.
- [ ] Existem dois NPCs lojistas: armas/armaduras e sementes/utensilios.
- [ ] Cada lojista tem fala de abertura e despedida especifica.
- [ ] Cada lojista abre menu modal vertical `Comprar / Vender / Sair` depois da fala de abertura.
- [ ] Ao escolher `Sair` ou apertar `Esc`, o NPC mostra sua despedida antes de fechar.
- [ ] Comprar abre HUD/lista de itens do vendedor.
- [ ] Vender abre inventory do jogador em modo venda.
- [ ] Estoque dos vendedores e finito.
- [ ] Compra decrementa estoque somente se transacao for concluida.
- [ ] Reposicao ocorre a cada dia.
- [ ] Itens vendidos pelo jogador pagam 60% do valor normal.
- [ ] Preco de compra usa multiplicador do vendedor, inicialmente `1.0`.
- [ ] Compra falha sem gastar gold se inventory estiver cheio.
- [ ] Venda falha sem remover item se gold nao puder ser adicionado.
- [ ] Compra/venda da fazenda deixa de ser fluxo oficial ativo.
- [ ] Save/load preserva estoque restante dentro do dia.
- [ ] Restock diario e idempotente.
- [ ] Invariantes anti-regressao preservadas.

---

## 13. Validacao

1. Entrar na cidade e validar Pip caminhando ate o jogador e parando.
2. Confirmar que Pip nao abre loja.
3. Validar fala de abertura e despedida do Pip.
4. Interagir com vendedor de armas/armaduras e validar fala de abertura.
5. Abrir menu Comprar/Vender/Sair.
6. Comprar item com gold e espaco suficientes.
7. Validar decremento de estoque.
8. Tentar comprar sem gold.
9. Tentar comprar sem espaco no inventory.
10. Comprar ate estoque acabar.
11. Vender item do inventory e validar ganho de 60% do BaseValue.
12. Sair da conversa e validar despedida especifica do lojista.
13. Salvar/carregar e validar estoque restante.
14. Avancar dia e validar restock uma unica vez.
15. Confirmar que compra/venda da fazenda nao esta mais ativa como fluxo oficial.
16. Validar Unity compile validation e docs validation.
