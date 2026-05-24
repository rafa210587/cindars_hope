# ref_inventory_slots_capacity_ui_final

> Status: Implementado parcial
> Origem: validacao das specs implementadas/parciais
> Spec relacionada: `docs/specs/implementados/spec_inventory_002_slots_capacity_ui_final.md`
> Objetivo: evoluir o inventory MVP por ID/stack agregada para inventario final com slots, multiplas stacks, capacidade, migration v1->v2 e painel de itens jogavel.

## Resultado da implementacao 2026-05-24

Entregue:

- Modelo por slots no `InventoryManager`.
- Save/load com `InventorySlotSaveData`, `Capacity` e `Items` legado.
- Migration `v1 -> v2` registrada no save registry.
- Capacidade inicial 18 e limite 30.
- Multiplas stacks por item respeitando `ItemDataSO.MaxStack`.
- Split automatico por metade.
- Destroy com confirmacao no painel.
- Equip como binding simples mantendo item no inventario.
- Painel minimo IMGUI com `I`, `Esc`, WASD, `Enter`/`Space`/`E`.

Pendencias preservadas:

- `Use` por tipo de item.
- Drop transacional com spawner persistente real.
- UI Canvas final, drag/drop e sorting.
- Binding final por `ItemInstanceId` na spec 10.

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

## 3. Decisoes aprovadas

- Capacidade inicial: `18 slots`, grid `3x6`.
- Capacidade maxima planejada por equipamento/mochila: `30 slots`, grid `5x6`.
- Painel de itens e modal.
- `I` abre e fecha o painel.
- `Esc` fecha o painel.
- `WASD` navega no grid enquanto o painel esta aberto.
- `Enter` e `Space` confirmam. `E` pode ser alias contextual somente quando o painel estiver aberto.
- Confirmar um slot ocupado abre menu de acoes; nao executa acao destrutiva no primeiro confirm.
- Split MVP usa metade automatica.
- Destroy sempre exige confirmacao.
- Drop e transacional: so remove do inventory depois que pickup persistente for criado com sucesso.
- Equip segue a opcao B: item equipado permanece no inventory e o slot/item fica marcado como equipado.
- Drag/drop, sort/auto-organize, merge manual e swap manual ficam fora do MVP.

---

## 4. Escopo esperado

### Runtime

Criar modelo de inventory baseado em slots:

```text
InventorySlot
InventorySlotSaveData
InventoryAddResult
InventoryActionResult
```

Campos minimos:

```text
SlotIndex
ItemId
Amount
IsEquipped ou EquippedBinding opcional
```

Regras:

- cada slot guarda no maximo 1 item stackavel;
- `MaxStack` passa a limitar cada slot;
- itens iguais podem ocupar multiplos slots;
- AddItem distribui entre stacks existentes e slots vazios;
- RemoveItem consome das stacks corretamente;
- operacoes retornam sobra quando nao houver espaco;
- nenhuma operacao deve perder item silenciosamente;
- item equipado nao pode ser destruido, dropado ou splitado sem antes passar por regra clara de unequip/bloqueio.

### Capacidade

```text
Inicial: 18 slots / grid 3x6
Planejado por mochila/equipamento: ate 30 slots / grid 5x6
```

Capacidade deve ser persistida como dado simples para permitir upgrades futuros sem reescrever o save.

### Painel minimo de itens / HUD de inventory

Criar painel de itens jogavel, nao apenas debug HUD.

Regras de input:

```text
I abre o painel de itens.
I fecha o painel se ja estiver aberto.
Esc fecha o painel sem aplicar acao.
WASD navega entre slots no grid quando o painel esta aberto.
Enter ou Space confirmam selecao/acao.
E pode confirmar como alias contextual somente quando painel esta aberto.
```

Regras de layout:

- painel deve abrir como overlay/modal temporario;
- painel e modal: bloqueia movimento, ataque, interacao e uso de tool/weapon enquanto aberto;
- HUD de gameplay normal pode continuar visivel ao fundo;
- painel nao pode ocupar permanentemente o espaco das outras HUDs principais;
- painel nao deve destruir, substituir ou esconder de forma definitiva HUDs como HP, hunger, gold, day/time e hotbar;
- abrir/fechar painel nao dispara `SaveGame()` automaticamente.

### Menu de acoes

Confirmar slot ocupado abre menu de acoes.

Nenhuma acao destrutiva acontece no primeiro confirm.

Acoes minimas:

```text
Use
Equip
Drop
Destroy
Split
Cancel
```

Regras:

- Use so aparece/funciona para item usavel/consumivel/interagivel;
- Equip so aparece/funciona para item equipavel, ferramenta, arma, armor/accessory ou categoria equivalente suportada;
- Equip nao remove o item do inventory nesta spec; o item permanece no slot e fica marcado como equipado;
- se a integracao final com equipment ainda nao suportar esse modelo, bloquear Equip com pendencia clara e nao duplicar item;
- Drop deve criar pickup persistente antes de remover item do slot;
- se Drop falhar, item permanece no inventory;
- Destroy exige confirmacao simples;
- Destroy nunca acontece direto no menu principal;
- Split exige `Amount > 1`;
- Split MVP usa metade automatica: `10 -> 5 + 5`, `9 -> 5 + 4`, mantendo a maior parte no slot original;
- Split deve respeitar capacidade e nunca perder item se nao houver slot disponivel;
- Cancel fecha submenu de acoes sem alterar inventory.

### Save/load

Inventory v2 deve ter `Slots` e `Capacity` como fonte de verdade:

```text
InventorySaveData
- List<InventorySlotSaveData> Slots
- int Capacity
```

`Items` do modelo antigo pode permanecer temporariamente como legacy/deprecated para migration, mas nao deve ser a fonte de verdade em v2.

Migration segura:

- usar infraestrutura de `spec_save_schema_migration_v2`;
- inventory slots provavelmente sera a primeira migration real `v1 -> v2`;
- `CurrentSchemaVersion` so deve subir quando a migration real de inventory estiver implementada e validada;
- se save antigo tiver Items, converter para slots sequenciais;
- preservar quantidade total de cada item;
- quebrar quantidades acima de `MaxStack` em multiplos slots;
- se os itens antigos excederem a capacidade inicial de 18 slots, expandir o numero de slots salvos para comportar tudo e registrar warning;
- migration nunca pode perder item.

### Fora do MVP desta spec

```text
Drag/drop final
Sort/auto-organize
Merge manual entre slots
Swap manual entre slots
Hotbar final
Equipment visual completo/paper doll
UI final consolidada do jogo
```

Permitido nesta spec:

```text
Auto-merge no AddItem
Split automatico por metade
Acoes via menu
Equip marcado no inventory sem duplicar item
```

---

## 5. Arquivos provaveis

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

## 6. Fora de escopo

- UI/UX final completa do jogo.
- Equipment visual completo com paper doll.
- Crafting UI final.
- Shop UI final.
- Skill tree UI.
- Item rarity/affixes.
- Controller remapping avancado.
- Multiplayer/trading.
- Drag/drop final.
- Sort/auto-organize.

A spec 17 ainda deve tratar a UI final e consolidada do jogo. Esta spec entrega o painel minimo de itens necessario para inventory funcionar de forma jogavel.

---

## 7. Definition of Done

- [ ] Inventory inicia com 18 slots / 3x6.
- [ ] Capacity e persistida.
- [ ] Arquitetura suporta expansao futura ate 30 slots / 5x6.
- [ ] Mesmo item pode ocupar multiplas stacks.
- [ ] `MaxStack` e por slot.
- [ ] Save/load preserva slots e capacity.
- [ ] Save antigo por `ItemId + Amount` migra sem perda.
- [ ] Migration expande slots se itens antigos excederem capacidade inicial.
- [ ] Painel de itens abre e fecha com `I`.
- [ ] Painel de itens fecha com `Esc`.
- [ ] Painel de itens usa WASD para navegar entre slots sem mover player.
- [ ] Slot selecionado tem highlight.
- [ ] Confirmar slot ocupado abre menu de acoes.
- [ ] Painel mostra slots, quantidades e dados basicos do item.
- [ ] Item selecionado permite Use/Equip/Drop/Destroy/Split quando aplicavel.
- [ ] Equip mantem item no inventory e marca como equipado, sem duplicar item.
- [ ] Destroy exige confirmacao simples.
- [ ] Split usa metade automatica e nao perde item.
- [ ] Drop nao apaga item sem criar pickup persistente ou sem registrar pendencia explicita.
- [ ] Painel nao ocupa permanentemente o espaco das outras HUDs.
- [ ] Add/remove nao perde item silenciosamente.
- [ ] `spec_inventory_001` e atualizada com estado real.

---

## 8. Validacao

1. Adicionar item stackavel acima do `MaxStack` e verificar multiplos slots.
2. Remover quantidade parcial e validar decremento correto.
3. Salvar/carregar inventory com multiplos slots.
4. Testar inventory cheio e validar sobra/rejeicao sem perda.
5. Migrar save v1 para v2 e validar preservacao total de itens.
6. Abrir painel com `I`.
7. Navegar no grid com WASD.
8. Fechar painel com `I` e com `Esc`.
9. Selecionar item usavel e executar Use.
10. Selecionar item equipavel e executar Equip sem duplicar/remover do inventory.
11. Executar Drop e validar pickup persistente ou pendencia registrada sem perda.
12. Executar Destroy e validar confirmacao.
13. Executar Split e validar duas stacks ou erro claro quando sem espaco.
14. Validar que a HUD normal nao e sobrescrita/destruida pelo painel.
15. Validar Unity compile validation e docs validation.
