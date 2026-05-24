# SPEC - Inventory slots, capacidade e painel de itens

> Spec ID: spec_inventory_slots_capacity_ui_final
> Status: Implementado parcial
> Ordem de execucao: 03
> Depende de: 00, 01, 02
> Bloqueia: 04, 06, 07, 10, 12, 17
> Tipo: Runtime/UI
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Evoluir Dictionary<string,int> para slots reais, multiplas stacks, capacidade, migration v1->v2 e painel minimo de itens.
> Fora de escopo: UI/UX final completa do jogo, drag/drop final, sort/auto-organize, crafting UI final, shop UI final, skill tree UI, equipment visual completo, docs_old.
> Evidencia: `Assets/_Game/Scripts/Inventory/InventoryManager.cs`, `Assets/_Game/Scripts/Inventory/InventorySlot.cs`, `Assets/_Game/Scripts/UI/InventoryPanelController.cs`, `Assets/_Game/Scripts/Save/Migrations/InventorySlotsV1ToV2Migration.cs`

## Resultado da implementacao 2026-05-24

Implementado parcial:

- `InventoryManager` passou a usar slots reais com capacidade inicial 18 e limite 30.
- `Items` agregado foi preservado como compatibilidade para economy, crafting, farm, pickups e debug HUD.
- `InventorySaveData` agora possui `Capacity`, `Slots` e `Items` legado.
- `CurrentSchemaVersion` subiu para 2 com migration real `v1 -> v2` para criar slots a partir de `Items`.
- `AddItem` ficou transacional: se nao houver espaco para a quantidade completa, nao altera o inventario.
- `RemoveItem`, `SplitSlot`, `DestroySlot` e binding simples de equip foram implementados por slot.
- Painel modal minimo por IMGUI abre com `I`, fecha com `Esc`, navega com WASD e executa menu vertical por `Enter`/`Space`/`E`.

Pendencias reais:

- `Use` ainda depende de handlers especificos por tipo de item.
- `Drop` nao remove item enquanto nao existir spawner runtime persistente de pickup; o painel informa a pendencia e preserva o item.
- Drag/drop, sort/auto-organize e UI Canvas final seguem fora do MVP.
- Equipamento completo por `ItemInstanceId` fica para spec 10.
- Validacao Unity formal fica acumulada para o final da sequencia 02-10, conforme instrucao da tarefa.

Fontes absorvidas:
- specs/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_inventory_slots_capacity_ui.md

---

# /speckit.specify

## Contexto

O inventory atual usa `InventoryManager` com `Dictionary<string,int>`, ou seja, quantidade agregada por `itemId`. Isso funciona para MVP, mas nao suporta slots reais, multiplas stacks, capacidade, split, operacoes seguras por slot ou UI jogavel de itens.

A spec de save migration ja prepara infraestrutura para migrations. Inventory slots deve ser a primeira migration real provavel: `v1 -> v2`.

## Problema

Sem slots reais, varios sistemas futuros ficam presos a um modelo provisorio:

- economy/shop nao consegue validar capacidade real;
- crafting nao consegue consumir stacks/slots corretamente;
- equipment/hotbar nao consegue referenciar itens com seguranca;
- UI nao consegue permitir split/drop/destroy/use/equip com estado consistente;
- save atual persiste `ItemId + Amount`, nao slots;
- HUD/debug atual nao substitui um painel de itens jogavel.

## Objetivo

Evoluir inventory para modelo por slots com capacidade inicial 3x6, expansao futura por mochila ate 5x6, multiplas stacks, save/load/migration e painel minimo de itens jogavel.

## Decisoes aprovadas

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

## User stories / engineering stories

- Como jogador, quero abrir o painel de itens com `I`, navegar com WASD e fechar com `I` ou `Esc`.
- Como jogador, quero selecionar um item e escolher usar, equipar quando fizer sentido, dropar, destruir ou splitar stack.
- Como jogador, quero que o painel de itens nao destrua nem substitua as HUDs principais.
- Como jogador, quero que itens nao sumam quando inventory estiver cheio, drop falhar ou split nao couber.
- Como desenvolvedor, quero que inventory, save, hotbar/equipment/crafting/shop usem contratos claros.
- Como agente, devo implementar somente depois de save migration estar disponivel.

## Regras funcionais obrigatorias

### Modelo de slots

Criar ou equivalente:

```text
InventorySlot
InventorySlotSaveData
InventoryAddResult
InventoryActionResult
```

Campos minimos de slot:

```text
SlotIndex
ItemId
Amount
IsEquipped ou EquippedBinding opcional
```

Regras:

- cada slot guarda no maximo um item stackavel;
- `MaxStack` passa a limitar cada slot;
- itens iguais podem ocupar multiplos slots;
- AddItem preenche stacks existentes antes de usar slots vazios;
- AddItem retorna sobra quando nao houver espaco;
- RemoveItem consome stacks sem perder item silenciosamente;
- slot vazio tem `ItemId` null/vazio e `Amount = 0` ou formato equivalente documentado;
- item equipado nao pode ser destruido, dropado ou splitado sem antes passar por regra clara de unequip/bloqueio.

### Capacidade

- Capacidade inicial obrigatoria: `18 slots`.
- Grid inicial: `3x6`.
- Capacidade maxima planejada por mochila/equipamento: `30 slots`.
- Grid maximo planejado: `5x6`.
- Upgrade de mochila/equipamento que aumenta capacidade pode ficar fora desta spec, mas o modelo deve persistir `Capacity` para suportar isso.
- Capacidade futura nao deve exigir reescrever o save novamente.

### Item categories / flags

Acoes disponiveis devem vir de `ItemDataSO`, categoria ou flags equivalentes, nao de nome hardcoded.

Categorias minimas esperadas quando existirem no projeto:

```text
Consumable
Seed
Tool
Weapon
Armor
Accessory
Material
Quest
Misc
```

Se a taxonomia atual ainda nao tiver essas categorias, criar o minimo necessario sem quebrar IDs existentes.

### Save DTO v2

Inventory em v2 deve ter `Slots` e `Capacity` como fonte de verdade:

```text
InventorySaveData
- List<InventorySlotSaveData> Slots
- int Capacity
```

`Items` do modelo antigo pode permanecer temporariamente como legacy/deprecated para migration, mas nao deve ser a fonte de verdade em v2.

### Migration v1 -> v2

Esta spec deve usar a infraestrutura de `spec_save_schema_migration_v2`.

Migration esperada:

```text
v1 InventorySaveData.Items [{ ItemId, Amount }]
v2 InventorySaveData.Slots [{ SlotIndex, ItemId, Amount }], Capacity
```

Regras:

- `CurrentSchemaVersion` pode subir para `2` somente se migration v1 -> v2 for implementada e validada.
- Converter itens agregados para slots sequenciais.
- Quebrar quantidades acima de `MaxStack` em multiplos slots.
- Preservar quantidade total de cada item.
- Migration nunca pode perder item.
- Se os itens antigos excederem a capacidade inicial de 18 slots, a migration deve expandir o numero de slots salvos para comportar tudo e registrar warning.
- Nao criar overflow dropado no mundo e nao descartar excedente.

### Painel minimo de itens

Criar UI jogavel minima de inventory/painel de itens.

Input obrigatorio:

```text
I abre o painel de itens.
I fecha o painel se ja estiver aberto.
Esc fecha o painel sem aplicar acao.
WASD navega entre slots quando o painel esta aberto.
Enter ou Space confirma selecao/acao.
E pode confirmar como alias contextual apenas quando painel esta aberto.
```

Regras de layout:

- painel deve abrir como overlay/modal temporario;
- painel e modal: bloqueia movimento, ataque, interacao e uso de tool/weapon enquanto aberto;
- HUD normal pode continuar visivel ao fundo;
- painel nao pode ocupar permanentemente o espaco das HUDs principais;
- painel nao deve destruir, substituir ou esconder de forma definitiva HUDs como HP, hunger, gold, day/time e hotbar;
- ao fechar painel, input do player deve voltar ao normal;
- abrir/fechar painel nao dispara `SaveGame()` automaticamente.

### Selecao e menu de acoes

Confirmar slot ocupado abre `InventoryActionMenu`.

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

- Use so funciona para item usavel/consumivel/interagivel;
- Equip so funciona para item equipavel, ferramenta, arma, armor/accessory ou categoria equivalente suportada;
- Equip nao remove o item do inventory nesta spec; o item permanece no slot e fica marcado como equipado;
- se a integracao final com equipment ainda nao suportar esse modelo, bloquear Equip com pendencia clara e nao duplicar item;
- Drop deve criar pickup persistente antes de remover item do slot;
- se Drop falhar, item permanece no inventory;
- Destroy exige confirmacao simples: `Confirm Destroy` ou `Cancel`;
- Destroy nunca acontece direto no menu principal;
- Split exige `Amount > 1`;
- Split MVP usa metade automatica:
  - `10 -> 5 + 5`;
  - `9 -> 5 + 4`, mantendo a maior parte no slot original;
- Split deve respeitar capacidade e nunca perder item se nao houver slot disponivel;
- Cancel fecha submenu de acoes sem alterar inventory.

### Hotbar

Hotbar final fica fora desta spec.

Se houver integracao minima, usar `ItemId` e validar existencia no inventory antes de usar. Nao acoplar hotbar de forma fragil a `SlotIndex` se a futura organizacao/sort puder mover itens.

### Drag/drop, sort, merge e swap

Fora do MVP desta spec:

```text
Drag/drop final
Sort/auto-organize
Merge manual entre slots
Swap manual entre slots
```

Permitido nesta spec:

```text
Auto-merge no AddItem
Split automatico por metade
Acoes via menu
```

### Navegacao e feedback

- Slot selecionado deve ter highlight visual.
- Slot vazio nao executa acao.
- Item selecionado mostra nome, quantidade e descricao curta se houver `ItemDataSO`.
- Grid deve navegar de forma previsivel em cima/baixo/esquerda/direita respeitando bordas.
- Painel pode ser centralizado ou levemente deslocado, mas ao fechar nenhum elemento de HUD deve permanecer deslocado.

### Tempo do jogo

MVP bloqueia input do player, mas nao define pausa global de tempo. Pausa global fica para UI final/menu system, salvo se o sistema atual ja tiver contrato claro para isso.

## Criterios de aceite

- Inventory inicia com 18 slots, grid 3x6.
- Inventory persiste `Capacity`.
- Arquitetura suporta expansao futura ate 30 slots, grid 5x6, por mochila/equipamento.
- Mesmo item pode ocupar multiplas stacks.
- `MaxStack` e por slot.
- Save/load preserva slots.
- Save antigo por `ItemId + Amount` migra sem perda.
- Migration expande slots se itens antigos excederem capacidade inicial.
- Painel abre/fecha com `I`.
- Painel fecha com `Esc`.
- WASD navega no painel sem mover player enquanto aberto.
- Confirmar slot ocupado abre menu de acoes.
- Item selecionado permite Use/Equip/Drop/Destroy/Split quando aplicavel.
- Equip mantem item no inventory e marca como equipado, sem duplicar item.
- Drop nao remove item se pickup persistente falhar.
- Destroy exige confirmacao.
- Split usa metade automatica, nao perde item e respeita capacidade.
- Painel nao ocupa permanentemente o lugar das outras HUDs.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Inventory/UI/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
docs/specs/implementados/spec_inventory_001_inventario_itens_gold_e_stacks.md
```

Logica de inventory deve ficar fora de MonoBehaviour pesado. UI deve ser ponte de apresentacao e comando.

## Fluxos

### Add item

```text
Receber ItemId + Amount
Validar ItemDataSO/MaxStack
Preencher stacks existentes
Preencher slots vazios
Retornar sobra se sem capacidade
Publicar InventoryChangedEvent
```

### Painel de itens

```text
Input I
Abrir painel modal overlay
Bloquear movimento/ataque/interacao/tool use
WASD muda slot selecionado
Enter/Space/E contextual abre menu de acoes
I/Esc fecha painel
Desbloquear input do player
```

### Action menu

```text
Slot vazio -> sem acoes
Slot ocupado -> abrir menu
Use/Equip/Drop/Destroy/Split/Cancel conforme disponibilidade
Acoes invalidas ficam escondidas ou desabilitadas com feedback simples
```

### Split

```text
Selecionar slot com Amount > 1
Calcular split automatico por metade
Verificar slot vazio/capacidade
Criar nova stack
Atualizar origem
Publicar InventoryChangedEvent
```

### Drop

```text
Selecionar slot
Escolher Drop
Criar pickup persistente via sistema de world/pickups
Se sucesso, remover item do slot
Se falha, manter item e logar erro
Publicar InventoryChangedEvent apenas quando estado mudar
```

### Equip

```text
Selecionar item equipavel
Validar categoria/flags
Marcar slot/item como equipado ou criar binding equivalente
Nao remover item do inventory nesta spec
Bloquear Destroy/Drop/Split de item equipado ou exigir unequip antes
Publicar EquipmentChangedEvent se houver integracao
```

### Save/load

```text
Save v2 grava Slots + Capacity
Items legacy deixa de ser fonte de verdade
Migration v1 -> v2 converte Items agregados em slots
Load restaura slots por IDs simples
```

## Dados / DTOs / IDs

DTOs devem usar tipos simples:

```text
SlotIndex: int
ItemId: string
Amount: int
Capacity: int
IsEquipped ou binding simples opcional
```

Nunca serializar ScriptableObject, GameObject, Transform, MonoBehaviour, Sprite, Collider ou Rigidbody.

## Eventos

Usar ou criar eventos seguindo padrao:

```text
InventoryChangedEvent
InventorySlotChangedEvent opcional
ItemUsedEvent opcional
ItemDroppedEvent opcional
EquipmentChangedEvent se Equip for integrado
InventoryPanelOpenedEvent opcional
InventoryPanelClosedEvent opcional
```

## UI

UI minima desta spec e painel de itens. UI final consolidada continua na spec 17.

## Riscos de regressao

- Save v1 pode quebrar se migration v1 -> v2 perder item por falta de capacidade.
- Drop pode deletar item se nao esperar pickup persistente ser criado.
- WASD pode mover o player e o cursor ao mesmo tempo se input nao for bloqueado.
- Equipment/hotbar/crafting/shop podem referenciar ItemId agregado em vez de slot/stack se contratos nao forem claros.
- Manter item equipado dentro do inventory exige regra clara para drop/destroy/split.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar `InventoryManager`, `InventorySaveData`, hotbar/equipment atuais e save migration.
- [ ] Definir capacidade inicial `18` e grid `3x6`.
- [ ] Persistir `Capacity` e preparar expansao futura ate `30` slots / `5x6`.
- [ ] Criar modelo de slots.
- [ ] Refatorar AddItem/RemoveItem para multiplas stacks.
- [ ] Implementar resultados de operacao sem perda silenciosa.
- [ ] Implementar migration v1 -> v2 de inventory usando infraestrutura de save migration.
- [ ] Garantir que migration expanda slots se o save antigo exceder 18 slots.
- [ ] Implementar painel minimo de itens modal.
- [ ] Implementar input `I`, `Esc` e navegacao WASD no painel.
- [ ] Implementar menu de acoes por slot ocupado.
- [ ] Implementar Use/Equip/Drop/Destroy/Split/Cancel conforme aplicavel.
- [ ] Implementar Split por metade automatica.
- [ ] Implementar Destroy com confirmacao.
- [ ] Integrar Drop como operacao transacional com pickups persistentes ou manter item intacto se falhar.
- [ ] Implementar Equip mantendo item no inventory e marcando como equipado, ou bloquear com pendencia clara se sistema atual nao suportar sem duplicacao.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Core/Events/**
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

- Slots reais implementados.
- Inventory inicial com 18 slots / 3x6.
- Capacidade futura ate 30 slots / 5x6 suportada por dados.
- Multiplas stacks por item implementadas.
- Capacity respeitada e persistida.
- Migration v1 -> v2 implementada e validada.
- Painel minimo de itens abre/fecha com `I`.
- WASD navega no painel sem mover player.
- Menu de acoes abre ao confirmar slot ocupado.
- Acoes de item funcionam ou falham com mensagem/pendencia clara.
- Equip nao duplica item e nao remove item do inventory nesta spec.
- Save/load preserva slots/capacity/equip markers quando aplicavel.
- Validacao documental e Unity registrada.

## Validacao

- `./tools/docs/validate_docs.ps1`
- `./tools/unity/RunUnityCompileValidation.ps1`
- `./tools/unity/ScanUnityLogs.ps1`
- Play Mode: add item acima de MaxStack, inventory cheio, save/load, migration v1->v2, abrir/fechar painel com I/Esc, navegar WASD, Use, Equip, Drop, Destroy, Split.
