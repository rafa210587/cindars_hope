# refinamento_init_inventory_slots_capacity_ui

> Status: Refinamento inicial a implementar
> Origem: validacao das specs implementadas/parciais
> Spec futura relacionada: `docs/specs/a_implementar/spec_inventory_slots_capacity_ui_final.md`
> Objetivo: evoluir o inventory MVP por ID/stack agregada para um inventario final com slots, multiplas stacks, capacidade e painel de itens jogavel.

---

## 1. Estado atual

O sistema atual usa `InventoryManager` com `Dictionary<string, int>`, ou seja, uma quantidade agregada por `itemId`.

Isso e suficiente para o MVP, mas nao representa inventario final.

Evidencia principal:

```text
Assets/_Game/Scripts/Inventory/InventoryManager.cs
Assets/_Game/Scripts/Inventory/Data/ItemDataSO.cs
Assets/_Game/Scripts/Inventory/Data/ItemDatabaseSO.cs
docs/specs/implementados/spec_inventory_001_inventario_itens_gold_e_stacks.md
```

Tambem existe um refinement amplo de UI final:

```text
docs/refinements/a_implementar/pre_refinamentos/refinamento_init_ui_ux_full_gameplay_inventory_hotbar_menus.md
```

Mas esse refinement pertence a spec 17, que e a UI/UX final do jogo. O painel minimo de itens deve nascer agora, junto da spec 03 de inventory, porque as acoes Use/Equip/Drop/Destroy/Split dependem diretamente do modelo de slots, stacks, capacidade e save.

---

## 2. Problemas/gaps

- Nao ha slots reais.
- Nao ha multiplas stacks do mesmo item.
- `MaxStack` limita a quantidade total por item, nao por stack.
- Nao ha capacidade maxima de inventory.
- Nao ha split stack, merge stack ou swap.
- UI jogavel de inventario/painel de itens nao existe.
- Nao ha fluxo de abrir/fechar inventario por tecla.
- Nao ha navegacao por teclado dentro do painel de itens.
- Nao ha acoes de item padronizadas: usar, equipar, dropar, destruir, splitar.
- Save atual salva `ItemId + Amount`, nao slots.

---

## 3. Escopo esperado

### Runtime

Criar modelo de inventory baseado em slots:

```text
InventorySlotSaveData
SlotIndex
ItemId
Amount
```

Regras:

- cada slot guarda no maximo 1 item stackavel;
- `MaxStack` passa a limitar cada slot;
- itens iguais podem ocupar multiplos slots;
- AddItem distribui entre stacks existentes e slots vazios;
- RemoveItem consome das stacks corretamente;
- operacoes retornam sobra quando nao houver espaco;
- nenhuma operacao deve perder item silenciosamente.

### Painel minimo de itens / HUD de inventory

Criar painel de itens jogavel, nao apenas debug HUD.

Regras de input:

```text
I abre o painel de itens.
I fecha o painel de itens quando ele ja esta aberto.
WASD navega entre slots no grid quando o painel esta aberto.
Enter/E/Space confirma selecao ou executa acao principal quando aplicavel.
Esc fecha o painel sem aplicar acao.
```

Regras de layout:

- o painel nao pode ocupar permanentemente o espaco das outras HUDs principais;
- o painel deve abrir como overlay/modal temporario;
- HUD de gameplay normal pode continuar visivel, mas nao deve ser sobrescrita ou destruida pelo painel;
- quando o painel estiver aberto, movimento do player deve ser bloqueado ou ignorado para evitar conflito com WASD de navegacao;
- ao fechar o painel, controle do player deve voltar ao normal.

### Acoes minimas de item

O painel deve permitir operar o slot selecionado.

Acoes minimas:

```text
Use
Equip, quando fizer sentido
Drop
Destroy
Split
Cancel
```

Regras:

- Use so aparece/funciona para item usavel/consumivel/interagivel;
- Equip so aparece/funciona para item equipavel, ferramenta, arma, armor/accessory ou categoria equivalente suportada;
- Drop deve criar pickup persistente quando o sistema de pickups estiver disponivel; se ainda nao for possivel, manter pendencia explicita e nao apenas apagar item;
- Destroy deve exigir confirmacao simples;
- Split deve pedir quantidade ou usar um default claro, como metade arredondada para baixo;
- Split so e permitido quando `Amount > 1`;
- Cancel fecha o submenu de acoes sem alterar inventory.

### Navegacao e selecao

- Slot selecionado deve ter highlight visual.
- Item selecionado deve mostrar nome, quantidade e descricao curta quando houver `ItemDataSO`.
- Ao selecionar slot vazio, a UI deve mostrar estado vazio e nao tentar executar acao.
- Navegacao deve ser previsivel em grid: esquerda/direita/cima/baixo respeitam bordas.

### Save/load

Migrar save antigo:

```text
InventorySaveData.Items -> InventorySlotSaveData[]
```

Relacao com save migration:

- esta spec deve depender da infraestrutura de `spec_save_schema_migration_v2`;
- inventory slots provavelmente sera a primeira migration real `v1 -> v2`;
- `CurrentSchemaVersion` so deve subir quando a migration real de inventory estiver implementada e validada.

Migration segura:

- se save antigo tiver Items, converter para slots sequenciais;
- preservar quantidade total de cada item;
- quebrar quantidades acima de `MaxStack` em multiplos slots;
- se nao houver espaco suficiente, registrar erro claro e nao perder itens silenciosamente.

---

## 4. Arquivos provaveis

```text
Assets/_Game/Scripts/Inventory/InventoryManager.cs
Assets/_Game/Scripts/Inventory/InventorySlot.cs
Assets/_Game/Scripts/Inventory/InventoryAddResult.cs
Assets/_Game/Scripts/Inventory/InventoryActionResult.cs
Assets/_Game/Scripts/Inventory/UI/InventoryPanelController.cs
Assets/_Game/Scripts/Inventory/UI/InventorySlotView.cs
Assets/_Game/Scripts/Inventory/UI/InventoryActionMenu.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Save/Migrations/**
docs/specs/implementados/spec_inventory_001_inventario_itens_gold_e_stacks.md
```

---

## 5. Fora de escopo

- UI/UX final completa do jogo.
- Equipment visual completo com paper doll.
- Crafting UI final.
- Shop UI final.
- Skill tree UI.
- Item rarity/affixes.
- Controller remapping avancado.
- Multiplayer/trading.

A spec 17 ainda deve tratar a UI final e consolidada do jogo. Esta spec entrega o painel minimo de itens necessario para inventory funcionar de forma jogavel.

---

## 6. Definition of Done

- [ ] Inventory suporta N slots configuraveis.
- [ ] Mesmo item pode ocupar multiplas stacks.
- [ ] `MaxStack` e por slot.
- [ ] Save/load preserva slots.
- [ ] Save antigo por `ItemId + Amount` migra sem perda.
- [ ] Painel de itens abre e fecha com `I`.
- [ ] Painel de itens usa WASD para navegar entre slots.
- [ ] Slot selecionado tem highlight.
- [ ] Painel mostra slots, quantidades e dados basicos do item.
- [ ] Item selecionado permite Use/Equip/Drop/Destroy/Split quando aplicavel.
- [ ] Destroy exige confirmacao simples.
- [ ] Split nao perde item e respeita capacidade.
- [ ] Drop nao apaga item sem criar pickup persistente ou sem registrar pendencia explicita.
- [ ] Painel nao ocupa permanentemente o espaco das outras HUDs.
- [ ] Movimento do player nao conflita com WASD enquanto painel estiver aberto.
- [ ] Add/remove nao perde item silenciosamente.
- [ ] `spec_inventory_001` e atualizada com estado real.

---

## 7. Validacao

1. Adicionar item stackavel acima do `MaxStack` e verificar multiplos slots.
2. Remover quantidade parcial e validar decremento correto.
3. Salvar/carregar inventory com multiplos slots.
4. Testar inventory cheio e validar sobra/rejeicao sem perda.
5. Abrir painel com `I`.
6. Navegar no grid com WASD.
7. Fechar painel com `I` e com `Esc`.
8. Selecionar item usavel e executar Use.
9. Selecionar item equipavel e executar Equip.
10. Executar Drop e validar pickup persistente ou pendencia registrada.
11. Executar Destroy e validar confirmacao.
12. Executar Split e validar duas stacks ou erro claro quando sem espaco.
13. Validar que a HUD normal nao e sobrescrita/destruida pelo painel.
14. Validar Unity compile validation e docs validation.
