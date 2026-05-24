# SPEC - Economy shop stock, pricing e UI

> Spec ID: spec_economy_shop_stock_pricing_ui
> Status: A implementar
> Ordem de execucao: 06
> Depende de: 00-05
> Bloqueia: 07, 08, 17
> Tipo: Runtime/UI
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Evoluir economia para lojas em NPCs da cidade, estoque finito, reposicao diaria, UI modal de compra/venda, dialogue modal de NPC, precos, falas simples e save.
> Fora de escopo: reputacao/afinidade final, agenda completa de NPC, quests, crafting recipes finais, UI final consolidada, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_economy_shop_stock_pricing_ui.md

---

# /speckit.specify

## Contexto

O projeto ja possui `EconomyManager`, policy de sellables e eventos de compra/venda MVP. A compra/venda atual ainda e simples e pode estar acoplada a um ponto de venda da fazenda.

Esta spec deve mover o fluxo de loja para a cidade, com NPCs vendedores, estoque finito, UI modal, dialogue modal e falas simples de abertura/despedida. A fazenda nao deve manter compra/venda como interface principal.

## Pre-condicoes

Implementar runtime somente depois de specs 02-05 estarem realmente implementadas:

```text
02 - save schema migration
03 - inventory slots/capacity/painel de itens
04 - farm irrigacao/solo/menu contextual
05 - world activities/fishing/trees/pickups/loot
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Town/**
```

Se Inventory slots ou Save migration ainda nao estiverem implementados, nao implementar runtime desta spec; registrar bloqueio.

## Problema

Gaps atuais:

- nao ha lojas por NPC/local;
- nao ha dois vendedores especializados;
- Pip ainda pode estar sendo tratado como vendedor, mas deve virar recepcao da cidade;
- NPCs ainda nao possuem contrato simples de fala de abertura e despedida;
- dialogos precisam usar modal proprio e nao podem ficar sobre outros modais;
- compra/venda da fazenda deve ser removida/deprecada;
- estoque nao e finito nem persistido;
- reposicao diaria nao existe;
- UI de compra/venda nao tem fluxo modal vertical claro;
- venda do jogador precisa usar inventory real e preco de venda fixo de 60%;
- multiplicadores de preco precisam existir agora, mesmo que com valor `1.0`, para afinidade futura.

## Objetivo

Criar o sistema de lojas da cidade com:

- Pip como NPC recepcionista da cidade, sem loja;
- NPC vendedor de armas e armaduras;
- NPC vendedor de sementes e utensilios;
- dialogue modal para falas de abertura e despedida dos NPCs;
- menu modal vertical de loja com `Comprar`, `Vender`, `Sair`;
- estoque finito por vendedor;
- reposicao diaria;
- compra segura com gold, inventory capacity e decremento de estoque;
- venda do inventory do jogador por 60% do valor normal;
- save/load de estoque;
- remocao/deprecacao do fluxo de compra/venda na fazenda.

## Decisoes aprovadas

- Criar 2 NPCs lojistas novos:
  - vendedor de armas e armaduras;
  - vendedor de sementes e utensilios.
- Pip nao vende itens nesta spec.
- Pip recebe o jogador na cidade: quando o jogador entra na cidade, Pip se move ate perto do jogador e depois para.
- Todos os NPCs desta spec possuem uma frase de abertura e uma frase de despedida especifica.
- Dialogos de NPC usam `DialogueModal` ou equivalente, separado visualmente do shop menu.
- `DialogueModal`, `ShopMenuModal`, `ShopBuyPanel` e `ShopSellPanel` devem respeitar uma unica pilha/camada de modal: nao pode haver dois modais ativos se sobrepondo.
- Ao interagir com um NPC, a fala de abertura aparece em `DialogueModal` antes do menu de opcoes.
- Ao fechar a conversa por `Sair` ou `Esc`, a fala de despedida especifica aparece em `DialogueModal` antes de encerrar o fluxo.
- Ambos os vendedores, ao conversar, abrem menu modal vertical:

```text
Comprar
Vender
Sair
```

- Ao escolher `Comprar`, abrir HUD/lista com itens que o vendedor tem para vender.
- Cada item comprado diminui do inventario/estoque do vendedor ate acabar.
- Compra so conclui se o jogador tiver dinheiro e espaco no inventory.
- Ao escolher `Vender`, abrir inventory do jogador para vender itens sellable.
- Vender item paga 60% do valor normal do item.
- Estoque de itens e finito.
- Reposicao ocorre a cada dia.
- Preco dos itens vendidos pelos vendedores usa multiplicador; por enquanto `x1`, preparado para afinidade futura.
- Compra/venda existente na fazenda deve ser removida, desativada ou marcada como deprecated, sem deixar dois fluxos oficiais competindo.

## NPCs e lojas

### Pip Miudinho

Papel nesta spec:

```text
Recepcao da cidade
Nao e lojista
Nao abre shop
```

Comportamento:

- Quando jogador entra na cidade pela primeira vez na sessao/cena, Pip anda ate perto do jogador.
- Ao chegar em distancia segura, Pip para.
- Pip deve ter uma fala curta de abertura/boas-vindas exibida em `DialogueModal`.
- Pip deve ter uma fala curta de despedida exibida em `DialogueModal`.
- Nao implementar agenda completa, quest ou loja para Pip nesta spec.

### Vendedor de armas e armaduras

ID sugerido:

```text
npc_shop_weapons_armor
shop_weapons_armor
```

Vende categorias:

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

ID sugerido:

```text
npc_shop_seeds_tools
shop_seeds_tools
```

Vende categorias:

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

Nomes finais/lore podem ficar para spec de Town/NPC, mas IDs tecnicos devem ser estaveis.

## Dados

Criar ou equivalente:

```text
ShopDataSO
ShopItemEntry
ShopStockRule
PriceModifierRule
ShopCategory
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

Regras:

- Todo item de loja deve apontar para `ItemId` estavel.
- Dados de conteudo ficam em ScriptableObject.
- Estado runtime de estoque fica no save, nao no asset.
- Falas podem ficar em data asset simples nesta spec; sistema completo de dialogo fica para spec 08.

## Pricing

Compra:

```text
BuyPrice = round(ItemDataSO.BaseValue * ShopDataSO.BuyPriceMultiplier)
```

Venda:

```text
SellPrice = floor(ItemDataSO.BaseValue * 0.6)
```

Regras:

- Multiplicador inicial de compra dos vendedores: `1.0`.
- Multiplicador de venda do jogador: `0.6`.
- Preparar contrato para afinidade/reputacao futura, mas nao implementar balance final.
- Preco minimo deve ser pelo menos 1 para item vendavel com valor positivo.
- Item com `BaseValue <= 0` ou marcado como nao vendavel nao deve aparecer para venda.

## Estoque e reposicao

- Estoque e finito para todos os itens vendidos nesta spec.
- Comprar decrementa `CurrentStock`.
- Quando `CurrentStock == 0`, item aparece como esgotado ou fica indisponivel.
- Reposicao ocorre a cada novo dia.
- Reposicao diaria restaura `CurrentStock` para `BaseDailyStock`.
- Save/load deve preservar estoque restante dentro do dia.
- Ao avancar dia, aplicar restock uma vez por ShopId.

## Fluxo de conversa e loja

```text
Jogador interage com NPC
Abrir DialogueModal com OpeningLine especifica do NPC
Fechar DialogueModal
Se NPC for lojista, abrir ShopMenuModal: Comprar / Vender / Sair
Se jogador escolher Sair ou apertar Esc, fechar menu/painel atual
Abrir DialogueModal com ClosingLine especifica
Fechar conversa/modal
Restaurar input do player
```

Garantias:

- Pip mostra falas em `DialogueModal`, mas nao abre loja.
- Lojistas mostram fala de abertura antes do menu.
- Lojistas mostram despedida ao sair.
- Despedida nao deve disparar transacao.
- Fechar com `Esc` deve seguir o mesmo encerramento seguro de `Sair`.
- `DialogueModal` nao fica sobre `ShopMenuModal`, `ShopBuyPanel`, `ShopSellPanel`, inventory panel ou HUD interativo.
- Ao trocar de modal, o modal anterior deve ser fechado/ocultado ou empilhado de forma controlada, sem sobreposicao visual permanente.

## Fluxo de compra

```text
Jogador interage com NPC lojista
DialogueModal: OpeningLine
ShopMenuModal: Comprar / Vender / Sair
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

## Fluxo de venda

```text
Jogador interage com NPC lojista
DialogueModal: OpeningLine
ShopMenuModal: Comprar / Vender / Sair
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
- Venda nao adiciona item ao estoque do vendedor nesta spec, salvo se isso ja existir e nao causar regressao.

## UI/modal

### Modal manager / exclusividade

Esta spec deve usar um mecanismo unico de controle de modal, existente ou novo, para garantir:

- no maximo um modal interativo ativo por vez;
- dialogo nao fica sobre loja;
- loja nao fica sobre dialogo;
- inventory/shop panels nao ficam sobre dialogue modal;
- HUD principal pode permanecer visivel ao fundo, mas nao deve receber input enquanto modal estiver aberto;
- ao fechar um modal, input normal volta somente se nao houver outro modal ativo.

Se ainda nao existir um `ModalManager`, `UiModalStack` ou equivalente, criar o minimo necessario sem virar UI final global.

### DialogueModal

Usado para:

```text
OpeningLine
ClosingLine
Mensagens simples de NPC desta spec
```

Regras:

- aparece antes do shop menu;
- aparece ao sair antes de encerrar conversa;
- bloqueia input do player;
- fecha por `E`, `Enter`, `Space` ou `Esc`, conforme fluxo seguro;
- nao fica aberto quando `ShopMenuModal`, `ShopBuyPanel` ou `ShopSellPanel` estiverem ativos.

### Conversation shop menu

Ao falar com um lojista:

```text
DialogueModal: OpeningLine
ShopMenuModal:
  Comprar
  Vender
  Sair
DialogueModal: ClosingLine ao sair
```

Input:

```text
W/S navegam verticalmente
E, Enter ou Space confirmam
Esc fecha/cancela e mostra ClosingLine
```

Enquanto o menu estiver aberto:

- movimento do player fica bloqueado/ignorado;
- interacoes do mundo nao disparam em paralelo;
- ao fechar, input normal volta apenas depois da despedida.

### Buy panel

Mostrar:

```text
Nome do vendedor
Item name
Icone se disponivel
Preco unitario
Estoque restante
Quantidade selecionada
Custo total
Gold atual do jogador
Feedback de erro
```

### Sell panel

Mostrar:

```text
Inventory do jogador
Item name
Quantidade possuida
Preco unitario de venda
Quantidade selecionada
Total a receber
Gold atual do jogador
Feedback de erro
```

## Remocao/deprecacao da compra/venda na fazenda

- Qualquer `SellPoint`, `BuyPoint` ou menu de compra/venda na FarmScene deve deixar de ser o fluxo oficial.
- Se o objeto ainda existir para compatibilidade, deve ser desativado ou redirecionado com pendencia clara.
- Nao deixar dois sistemas oficiais de compra/venda ativos ao mesmo tempo.
- A economia oficial desta spec acontece nos NPCs da cidade.

## Save/load

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

## Eventos

Usar existentes se houver equivalentes. Criar somente se necessario, seguindo padrao `*Event`:

```text
ShopOpenedEvent
ShopClosedEvent
ShopBuyRequestedEvent
ShopSellRequestedEvent
ShopTransactionSucceededEvent
ShopTransactionFailedEvent
ShopStockChangedEvent
ShopRestockedEvent
GoldChangedEvent
InventoryChangedEvent
NpcGreetingStartedEvent opcional
NpcGreetingFinishedEvent opcional
NpcConversationOpenedEvent opcional
NpcConversationClosedEvent opcional
DialogueModalOpenedEvent opcional
DialogueModalClosedEvent opcional
```

Nao duplicar eventos de economy existentes se ja forem suficientes.

## Invariantes anti-regressao

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

Se inventory estiver cheio, gold insuficiente, estoque esgotado ou item nao vendavel, a transacao deve falhar sem alterar estado parcial.

## Criterios de aceite

- Pip nao abre loja.
- Pip se move ate o jogador ao entrar na cidade e para proximo dele.
- Pip tem fala de abertura e despedida em `DialogueModal`.
- Existem dois NPCs lojistas: armas/armaduras e sementes/utensilios.
- Cada lojista tem fala de abertura e despedida especifica em `DialogueModal`.
- Cada lojista abre menu modal vertical `Comprar / Vender / Sair` depois da fala de abertura.
- Ao escolher `Sair` ou apertar `Esc`, o NPC mostra sua despedida em `DialogueModal` antes de fechar.
- Dialogos nao ficam sobre shop menu, buy panel, sell panel, inventory panel ou outros modais.
- Comprar abre HUD/lista de itens do vendedor.
- Vender abre inventory do jogador em modo venda.
- Estoque dos vendedores e finito.
- Compra decrementa estoque somente se transacao for concluida.
- Reposicao ocorre a cada dia.
- Itens vendidos pelo jogador pagam 60% do valor normal.
- Preco de compra usa multiplicador do vendedor, inicialmente `1.0`.
- Compra falha sem gastar gold se inventory estiver cheio.
- Venda falha sem remover item se gold nao puder ser adicionado.
- Compra/venda da fazenda deixa de ser fluxo oficial ativo.
- Save/load preserva estoque restante dentro do dia.
- Restock diario e idempotente.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Economy/Data/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/UI/Shop/**
Assets/_Game/Scripts/UI/Dialogue/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/Town/**
docs/specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md
```

Managers/bridges Unity devem ser finos. Calculo de preco, validacao de transacao, reposicao de estoque, modal orchestration e falas simples devem ficar fora de MonoBehaviour pesado quando possivel.

## Ordem segura de implementacao

1. Revalidar `EconomyManager`, inventory, gold, sellable policy, UI e eventos existentes.
2. Criar dados de shop (`ShopDataSO`, entries e stock rules).
3. Criar dados simples de NPC interaction com `OpeningLine` e `ClosingLine`.
4. Criar/ajustar controle minimo de modais para impedir sobreposicao.
5. Criar `DialogueModal` minimo.
6. Criar runtime de shop/stock sem UI.
7. Implementar price calculator com multiplicadores `1.0` e `0.6`.
8. Implementar save/load de estoque.
9. Implementar restock diario.
10. Criar dois NPCs lojistas e associar `ShopDataSO`.
11. Ajustar Pip como recepcao, sem loja.
12. Implementar fala de abertura/despedida via `DialogueModal`.
13. Implementar menu modal `Comprar/Vender/Sair`.
14. Implementar BuyPanel e SellPanel.
15. Remover/desativar compra/venda oficial da fazenda.
16. Validar anti-regressao e atualizar tracking.

## Fluxos

### Abrir conversa/loja

```text
Player pressiona E em NPC
Bloquear input do player
Abrir DialogueModal com OpeningLine
Fechar DialogueModal
Se NPC for lojista, abrir ShopMenuModal Comprar/Vender/Sair
Se NPC nao for lojista, ao sair abrir DialogueModal com ClosingLine
Confirmar opcao
Abrir painel correspondente ou sair
Ao sair, abrir DialogueModal com ClosingLine
Fechar DialogueModal
Restaurar input do player se nao houver outro modal ativo
```

### Comprar

```text
Selecionar item e quantidade
Validar estoque > 0
Validar gold suficiente
Validar espaco no inventory
Executar transacao atomica
Publicar eventos
Atualizar UI
```

### Vender

```text
Selecionar item do inventory e quantidade
Validar item sellable
Calcular preco 60%
Executar transacao atomica
Publicar eventos
Atualizar UI
```

### Restock diario

```text
DayStartedEvent ou evento equivalente
Para cada ShopId
Se LastRestockDay < CurrentDay
Restaurar CurrentStock = BaseDailyStock
Atualizar LastRestockDay
Publicar ShopRestockedEvent opcional
```

### Pip recepcao

```text
TownScene carrega / jogador entra na cidade
Pip detecta entrada por evento/trigger seguro
Pip caminha ate ponto perto do jogador
Pip para
Mostra OpeningLine em DialogueModal quando interagido ou quando recepcao disparar, se houver suporte
Ao encerrar, mostra ClosingLine em DialogueModal
Nao abre loja
```

## Dados / DTOs / IDs

Permitido em save:

```text
string ShopId
string ItemId
int CurrentStock
int LastRestockDay
```

Proibido em save:

```text
ShopDataSO
ItemDataSO
GameObject
Transform
MonoBehaviour
Sprite
Collider
Rigidbody
```

## UI

UI desta spec e shop/conversation UI minima, nao UI final do jogo.

Regras:

- modal bloqueia movimento;
- lista vertical navegavel por teclado;
- fala de abertura aparece em `DialogueModal` antes do menu;
- fala de despedida aparece em `DialogueModal` antes de fechar;
- feedback de erro visivel;
- nao substituir HUD/hotbar/inventory panel de forma permanente;
- dialogo e shop panels nao podem ficar empilhados visualmente;
- `Esc` sempre volta ou fecha com despedida quando em conversa ativa.

## Riscos de regressao

- Dois fluxos de venda ativos: farm sell point e cidade.
- Gold ser descontado sem item entrar no inventory.
- Estoque diminuir sem compra concluir.
- Venda remover item sem adicionar gold.
- Restock rodar varias vezes no mesmo dia.
- Pip virar lojista por fallback antigo.
- Shop UI mover/destruir HUD existente.
- Fala de despedida disparar no meio de transacao.
- DialogueModal ficar sobre ShopMenu/BuyPanel/SellPanel.

## Mitigacao

- Transacoes atomicas.
- Ordem de validacao antes de alteracao de estado.
- Restock idempotente por `ShopId + CurrentDay`.
- Farm sell/buy desativado ou explicitamente deprecated.
- Pip sem `ShopDataSO`.
- UI modal temporaria.
- ClosingLine executa apenas em encerramento de conversa, nao durante compra/venda.
- Controle unico de modais para impedir sobreposicao visual e conflito de input.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar estado real de economy, inventory, NPC, Town, UI e save antes de alterar runtime.
- [ ] Confirmar specs 02-05 implementadas antes de runtime.
- [ ] Criar/ajustar `ShopDataSO`, `ShopItemEntry`, `ShopStockRule`, `PriceModifierRule`.
- [ ] Criar/ajustar dados simples de NPC com `OpeningLine` e `ClosingLine`.
- [ ] Criar/ajustar controle minimo de modais para impedir sobreposicao.
- [ ] Criar/ajustar `DialogueModal`.
- [ ] Criar/ajustar `ShopManager`, `ShopSession`, `ShopStockSaveData`.
- [ ] Implementar price calculator: buy x1 inicial, sell 60%.
- [ ] Implementar estoque finito e decremento por compra.
- [ ] Implementar restock diario idempotente.
- [ ] Criar loja de armas/armaduras.
- [ ] Criar loja de sementes/utensilios.
- [ ] Ajustar Pip como recepcao da cidade, sem loja.
- [ ] Implementar movimento de Pip ate o jogador ao entrar na cidade e parada.
- [ ] Implementar fala de abertura/despedida para Pip e lojistas via `DialogueModal`.
- [ ] Implementar menu modal `Comprar/Vender/Sair`.
- [ ] Implementar BuyPanel com itens, preco, estoque e quantidade.
- [ ] Implementar SellPanel com inventory do jogador e preco de 60%.
- [ ] Garantir transacoes atomicas de compra/venda.
- [ ] Remover/desativar/deprecar compra/venda oficial da fazenda.
- [ ] Persistir estoque em save/load.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/UI/Shop/**
Assets/_Game/Scripts/UI/Dialogue/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/Town/**
Assets/_Game/Data/Shops/**
Assets/_Game/Data/NPCs/**
docs/specs/**
docs/refinements/**
docs/IMPLEMENTATION_STATUS.md
PROJECT_LOG.md
```

## Arquivos proibidos

```text
docs_old/**
Packages/**
ProjectSettings/**
```

## Definition of Done

- Dois NPCs lojistas funcionam na cidade.
- Pip funciona como recepcao e nao como loja.
- Todos os NPCs desta spec possuem fala de abertura e despedida especifica.
- DialogueModal funciona e nao se sobrepoe aos modais de loja.
- Menu modal Comprar/Vender/Sair funciona.
- BuyPanel e SellPanel funcionam com inventory/gold reais.
- Estoque finito, decremento e restock diario funcionam.
- Venda paga 60% do BaseValue.
- Compra/venda da fazenda nao e mais fluxo oficial ativo.
- Save/load preserva estoque.
- Invariantes anti-regressao preservadas.
- Validacao documental e Unity registrada.

## Validacao obrigatoria

Rodar:

```powershell
.\tools\docs\validate_docs.ps1
.\tools\unity\RunUnityCompileValidation.ps1
.\tools\unity\ScanUnityLogs.ps1
```

Play Mode minimo:

1. Entrar na cidade e validar Pip caminhando ate o jogador e parando.
2. Confirmar que Pip nao abre loja.
3. Validar fala de abertura e despedida do Pip em `DialogueModal`.
4. Interagir com vendedor de armas/armaduras e validar fala de abertura em `DialogueModal`.
5. Validar que `DialogueModal` fecha antes de abrir Comprar/Vender/Sair.
6. Abrir menu Comprar/Vender/Sair.
7. Comprar item com gold e espaco suficientes.
8. Validar decremento de estoque.
9. Tentar comprar sem gold.
10. Tentar comprar sem espaco no inventory.
11. Comprar ate estoque acabar.
12. Vender item do inventory e validar ganho de 60% do BaseValue.
13. Sair da conversa e validar despedida especifica do lojista em `DialogueModal`.
14. Validar que dialogo nao fica sobre shop menu, buy panel ou sell panel.
15. Salvar/carregar e validar estoque restante.
16. Avancar dia e validar restock uma unica vez.
17. Confirmar que compra/venda da fazenda nao esta mais ativa como fluxo oficial.
18. Validar que HUD/hotbar/inventory nao ficam deslocados ou quebrados.
