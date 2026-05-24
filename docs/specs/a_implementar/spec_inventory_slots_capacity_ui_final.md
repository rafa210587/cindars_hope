# SPEC - Inventory slots, capacidade e painel de itens

> Spec ID: spec_inventory_slots_capacity_ui_final
> Status: A implementar
> Ordem de execucao: 03
> Depende de: 00, 01, 02
> Bloqueia: 04, 06, 07, 10, 12, 17
> Tipo: Runtime/UI
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Evoluir Dictionary<string,int> para slots reais, multiplas stacks, capacidade, migration v1->v2 e painel minimo de itens.
> Fora de escopo: UI/UX final completa do jogo, crafting UI final, shop UI final, skill tree UI, equipment visual completo, docs_old.

Fontes absorvidas:
- specs/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_inventory_slots_capacity_ui.md

---

# /speckit.specify

## Contexto

O inventory atual usa `InventoryManager` com `Dictionary<string,int>`, ou seja, quantidade agregada por `itemId`. Isso funciona para MVP, mas nao suporta slots reais, multiplas stacks, capacidade, split/merge/swap ou UI jogavel de itens.

A spec de save migration ja prepara infraestrutura para migrations. Inventory slots deve ser a primeira migration real provavel: `v1 -> v2`.

## Problema

Sem slots reais, varios sistemas futuros ficam presos a um modelo provisorio:

- economy/shop nao consegue validar capacidade real;
- crafting nao consegue consumir stacks/slots corretamente;
- equipment/hotbar nao consegue referenciar slots/itens com seguranca;
- UI nao consegue permitir split/drop/destroy/use/equip com estado consistente;
- save atual persiste `ItemId + Amount`, nao slots.

## Objetivo

Evoluir inventory para modelo por slots com capacidade, multiplas stacks, save/load/migration e painel minimo de itens jogavel.

## User stories / engineering stories

- Como jogador, quero abrir o painel de itens com `I`, navegar com WASD e fechar com `I` ou `Esc`.
- Como jogador, quero selecionar um item e usar, equipar quando fizer sentido, dropar, destruir ou splitar stack.
- Como jogador, quero que o painel de itens nao destrua nem substitua as HUDs principais.
- Como desenvolvedor, quero que inventory, save, hotbar/equipment/crafting/shop usem contratos de slot claros.
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
```

Regras:

- cada slot guarda no maximo um item stackavel;
- `MaxStack` passa a limitar cada slot;
- itens iguais podem ocupar multiplos slots;
- AddItem preenche stacks existentes antes de usar slots vazios;
- AddItem retorna sobra quando nao houver espaco;
- RemoveItem consome stacks sem perder item silenciosamente;
- slot vazio tem `ItemId` null/vazio e `Amount = 0` ou formato equivalente documentado.

### Capacidade

- Capacity deve ser configuravel.
- Comecar com valor padrao seguro, por exemplo 24 ou 30 slots, desde que documentado.
- Capacidade futura expandivel por upgrades fica fora do escopo, mas a arquitetura nao deve impedir isso.

### Migration v1 -> v2

Esta spec deve usar a infraestrutura de `spec_save_schema_migration_v2`.

Migration esperada:

```text
v1 InventorySaveData.Items [{ ItemId, Amount }]
v2 InventorySaveData.Slots [{ SlotIndex, ItemId, Amount }]
```

Regras:

- `CurrentSchemaVersion` pode subir para `2` somente se migration v1 -> v2 for implementada e validada.
- Converter itens agregados para slots sequenciais.
- Quebrar quantidades acima de `MaxStack` em multiplos slots.
- Preservar quantidade total de cada item.
- Se nao houver espaco suficiente, rejeitar migration com erro claro e nao perder item.

### Painel minimo de itens

Criar UI jogavel minima de inventory/painel de itens.

Input obrigatorio:

```text
I abre o painel de itens.
I fecha o painel se ja estiver aberto.
Esc fecha o painel sem aplicar acao.
WASD navega entre slots quando o painel esta aberto.
Enter, E ou Space confirma selecao/acao principal quando aplicavel.
```

Regras de layout:

- painel deve abrir como overlay/modal temporario;
- painel nao pode ocupar permanentemente o espaco das HUDs principais;
- painel nao deve destruir, substituir ou esconder de forma definitiva HUDs como HP, hunger, gold, day/time e hotbar;
- ao abrir painel, movimento do player deve ser bloqueado ou ignorado para que WASD navegue no grid;
- ao fechar painel, movimento do player deve voltar ao normal.

### Acoes de item

Acoes minimas no slot selecionado:

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
- Drop deve criar pickup persistente se o sistema estiver disponivel;
- se Drop ainda nao puder criar pickup persistente, nao apagar o item silenciosamente; registrar pendencia explicita;
- Destroy exige confirmacao simples;
- Split exige `Amount > 1`;
- Split deve pedir quantidade ou usar default claro, como metade arredondada para baixo;
- Split deve respeitar capacidade e nunca perder item se nao houver slot disponivel;
- Cancel fecha submenu de acoes sem alterar inventory.

### Navegacao e feedback

- Slot selecionado deve ter highlight visual.
- Slot vazio nao executa acao.
- Item selecionado mostra nome, quantidade e descricao curta se houver `ItemDataSO`.
- Grid deve navegar de forma previsivel em cima/baixo/esquerda/direita respeitando bordas.

## Criterios de aceite

- Inventory suporta slots configuraveis.
- Mesmo item pode ocupar multiplas stacks.
- `MaxStack` e por slot.
- Save/load preserva slots.
- Save antigo por `ItemId + Amount` migra sem perda quando houver espaco.
- Painel abre/fecha com `I`.
- Painel fecha com `Esc`.
- WASD navega no painel sem mover player enquanto aberto.
- Item selecionado permite Use/Equip/Drop/Destroy/Split quando aplicavel.
- Drop nao apaga item sem pickup persistente ou pendencia explicita.
- Destroy exige confirmacao.
- Split nao perde item e respeita capacidade.
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
Abrir painel overlay
Bloquear movimento do player
WASD muda slot selecionado
Confirmar abre menu de acoes ou executa acao principal
I/Esc fecha painel
Desbloquear movimento do player
```

### Split

```text
Selecionar slot com Amount > 1
Escolher quantidade ou metade default
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
Remover item do slot apenas se pickup foi criado com sucesso
Publicar InventoryChangedEvent
```

### Save/load

```text
Save v2 grava Slots
Migration v1 -> v2 converte Items agregados em slots
Load restaura slots por IDs simples
```

## Dados / DTOs / IDs

DTOs devem usar tipos simples:

```text
SlotIndex: int
ItemId: string
Amount: int
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
```

## UI

UI minima desta spec e painel de itens. UI final consolidada continua na spec 17.

## Riscos de regressao

- Save v1 pode quebrar se migration v1 -> v2 perder item por falta de capacidade.
- Drop pode deletar item se nao esperar pickup persistente ser criado.
- WASD pode mover o player e o cursor ao mesmo tempo se input nao for bloqueado.
- Equipment/hotbar/crafting/shop podem referenciar ItemId agregado em vez de slot/stack se contratos nao forem claros.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar `InventoryManager`, `InventorySaveData`, hotbar/equipment atuais e save migration.
- [ ] Definir capacidade inicial configuravel.
- [ ] Criar modelo de slots.
- [ ] Refatorar AddItem/RemoveItem para multiplas stacks.
- [ ] Implementar resultados de operacao sem perda silenciosa.
- [ ] Implementar migration v1 -> v2 de inventory usando infraestrutura de save migration.
- [ ] Implementar painel minimo de itens.
- [ ] Implementar input `I`, `Esc` e navegacao WASD no painel.
- [ ] Implementar Use/Equip/Drop/Destroy/Split/Cancel conforme aplicavel.
- [ ] Integrar Drop com pickups persistentes ou manter pendencia explicita sem apagar item.
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
- Multiplas stacks por item implementadas.
- Capacity respeitada.
- Migration v1 -> v2 implementada e validada.
- Painel minimo de itens abre/fecha com `I`.
- WASD navega no painel sem mover player.
- Acoes de item funcionam ou falham com mensagem/pendencia clara.
- Save/load preserva slots.
- Validacao documental e Unity registrada.

## Validacao

- `./tools/docs/validate_docs.ps1`
- `./tools/unity/RunUnityCompileValidation.ps1`
- `./tools/unity/ScanUnityLogs.ps1`
- Play Mode: add item acima de MaxStack, inventory cheio, save/load, migration v1->v2, abrir/fechar painel com I/Esc, navegar WASD, Use, Equip, Drop, Destroy, Split.
