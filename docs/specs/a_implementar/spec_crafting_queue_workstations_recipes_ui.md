# SPEC - Crafting queue, workstations, recipes e UI

> Spec ID: spec_crafting_queue_workstations_recipes_ui
> Status: A implementar
> Ordem de execucao: 07
> Depende de: 00-06
> Bloqueia: 10, 17
> Tipo: Runtime/UI
> Fonte: docs/specs/ como fonte unica; fontes absorvidas listadas abaixo.
> Escopo: Completar crafting com workstations fisicas, craft de bolso limitado, fila/tempo, recipes oficiais, coleta/cancelamento, UI modal, save/load e recursos/receitas de teste.
> Fora de escopo: construcao de workstations pelo jogador, animacoes finais, recipe unlock avancado, stamina final, durabilidade/qualidade final, crafting multiplayer, UI final consolidada, Packages, ProjectSettings e docs_old.

Fontes absorvidas:
- specs/FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION/spec.md
- docs/refinements/a_implementar/pre_refinamentos/refinamento_init_crafting_queue_workstations_recipes_ui.md

---

# /speckit.specify

## Contexto

O runtime oficial usa:

```text
CindarsHope.Craft.CraftingManager
CindarsHope.Craft.Data.RecipeDataSO
CindarsHope.Craft.Data.RecipeDatabaseSO
```

A estabilizacao removeu o modelo paralelo `CindarsHope.Crafting.CraftingRecipeSO`. Esta spec deve manter `RecipeDataSO` como unica fonte oficial de recipes e nao recriar modelos paralelos.

## Pre-condicoes

Implementar runtime somente depois de specs 02-06 estarem realmente implementadas:

```text
02 - save schema migration
03 - inventory slots/capacity/painel de itens
04 - farm irrigacao/solo/menu contextual
05 - world activities/fishing/trees/pickups/loot
06 - economy/shop/modal/dialogue
```

Antes de alterar codigo, revalidar:

```text
Assets/_Game/Scripts/Craft/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/**
```

Se inventory slots, save migration ou modal stack ainda nao existirem, nao implementar runtime desta spec; registrar bloqueio.

## Problema

Gaps atuais:

- crafting ainda e instantaneo/MVP;
- nao ha fila/job por workstation;
- nao ha workstation fisica completa;
- nao ha craft time;
- nao ha cancelamento/coleta de output;
- nao ha save/load de job em andamento/completo;
- UI final de crafting nao existe;
- recipe unlock avancado ainda nao existe;
- workstations ainda nao sao construidas pelo jogador, mas precisamos delas prontas para testar gameplay.

## Objetivo

Implementar crafting jogavel com:

- workstations fisicas prontas na fazenda para MVP/teste;
- craft de bolso limitado para recipes `RequiredStationType = None`;
- recipes oficiais em `RecipeDataSO`;
- craft instantaneo quando `CraftTimeSeconds <= 0`;
- craft com tempo via job quando `CraftTimeSeconds > 0`;
- 1 job ativo/completo por workstation no MVP;
- coleta de output sem perda se inventory estiver cheio;
- cancelamento seguro;
- save/load de jobs;
- modal de crafting integrado ao sistema de modais;
- recursos e recipes iniciais suficientes para testar cada workstation.

## Decisoes aprovadas

- Crafting ocorre principalmente em workstations fisicas.
- Craft de bolso e permitido somente para recipes `RequiredStationType = None`.
- Workstations iniciais:

```text
Workbench
Forge
CookingStation
```

- Por enquanto, todas as workstations ficam prontas na fazenda.
- No futuro, o jogador devera construir essas workstations; construcao fica fora desta spec.
- O jogador deve receber recursos suficientes e recipes desbloqueadas para testar cada workstation nesta fase.
- `E` em uma workstation abre `CraftingModal` direto; nao abrir menu contextual se a unica acao valida for craft.
- `CraftingModal` respeita o mesmo sistema de modal stack/exclusividade da spec 06.
- `RecipeDataSO` e a unica fonte oficial de recipe.
- `CraftTimeSeconds <= 0` significa craft instantaneo.
- `CraftTimeSeconds > 0` cria job com tempo.
- Ingredientes sao consumidos ao iniciar craft/job.
- Job completo precisa ser coletado.
- Se inventory estiver cheio ao coletar, output permanece aguardando na workstation.
- MVP permite 1 job ativo/completo por workstation; se ha job em andamento ou output aguardando, nao inicia outro.
- Cancelamento de job em andamento devolve 100% dos ingredientes se houver espaco; se nao houver espaco, cancelamento falha sem perder ingredientes.
- Job completo nao cancela; apenas coleta.
- Save/load persiste job em andamento e output completo aguardando coleta.
- Unlock avancado por skill/NPC/item fica como hook futuro.
- StaminaCost fica hook/default 0; stamina final pertence a spec 09.
- Crafting produz ItemId simples; durabilidade, qualidade, affixes e resistencias ficam para spec 10.

## Workstations iniciais na fazenda

Para MVP/teste, FarmScene deve possuir workstations prontas:

```text
farm_workbench_01: Workbench
farm_forge_01: Forge
farm_cooking_01: CookingStation
```

Regras:

- IDs devem ser estaveis para save/load.
- Workstations prontas nao exigem construcao nesta spec.
- Futura construcao/placement de workstations deve poder substituir o setup inicial sem quebrar save.
- Workstations nao devem bloquear caminhos essenciais da fazenda.
- Se uma cena ainda nao suportar placement fisico seguro, usar placeholders claros e documentados.

## Starter/test kit de crafting

Para validar a spec sem depender de farm/economy balance final, criar setup MVP/teste com:

```text
resources suficientes para 1+ recipe de Workbench
resources suficientes para 1+ recipe de Forge
resources suficientes para 1+ recipe de CookingStation
resources suficientes para 1 recipe de bolso RequiredStationType=None
recipes desbloqueadas para cada station inicial
```

Regras:

- Esses recursos/recipes sao para validacao inicial e devem ser marcados como starter/test data ou MVP bootstrap.
- Nao tratar esses valores como balance final.
- O setup nao pode quebrar save/load, economy ou inventory capacity.
- Se inventory inicial nao tiver espaco, reduzir quantidades ou usar um `TestStarterKit` controlado.

Recipes minimas recomendadas:

```text
pocket_rope_or_simple_material: RequiredStationType=None, instantanea
workbench_basic_utility: RequiredStationType=Workbench, pode ser instantanea ou curta
forge_basic_weapon_or_ingot: RequiredStationType=Forge, com tempo
cooking_basic_food: RequiredStationType=CookingStation, com tempo curto
```

IDs finais podem mudar, mas devem ser estaveis e documentados.

## Dados

Expandir `RecipeDataSO` ou equivalente oficial:

```text
RecipeId
DisplayName
RequiredStationType
CraftTimeSeconds
OutputItemId
OutputAmount
Ingredients[]
IsUnlockedByDefault
RequiredPlayerLevel default 0
RequiredSkillNodeId opcional
UnlockConditionIds[] futuro
StaminaCost default 0
```

`Ingredient` minimo:

```text
ItemId
Amount
```

Criar ou equivalente:

```text
CraftingStationType
CraftingStation
CraftingJob
CraftingJobStatus
CraftingQueueManager ou CraftingStationRuntime
CraftingSaveData
CraftingStationSaveData
```

## Runtime

### Craft de bolso

- Permitido somente para recipes `RequiredStationType = None`.
- Acesso pode ocorrer via inventory/crafting modal simples se ja houver entrada segura.
- Nao exigir workstation fisica.
- Deve seguir as mesmas regras de ingredientes, output e inventory capacity.

### Workstation crafting

```text
Player pressiona E em workstation
Validar que nao ha outro modal ativo
Abrir CraftingModal daquela workstation
Listar recipes compativeis com StationType
Selecionar recipe
Validar ingredientes/inventory/output
Iniciar craft instantaneo ou job
```

### Instant craft

```text
CraftTimeSeconds <= 0
Validar ingredientes
Consumir ingredientes
Adicionar output ao inventory
Se inventory nao tiver espaco para output, nao consumir ingredientes e falhar com feedback
```

### Timed job

```text
CraftTimeSeconds > 0
Validar ingredientes
Validar station sem active/completed job pendente
Consumir ingredientes
Criar CraftingJob
Quando tempo terminar, marcar Completed
Output fica aguardando coleta
```

### Coleta

```text
Player abre station com job Completed
Seleciona Collect
Validar espaco no inventory
Adicionar output ao inventory
Limpar job da station
```

Se inventory cheio:

```text
Nao perder output
Manter job Completed aguardando coleta
Exibir feedback claro
```

### Cancelamento

```text
Permitido somente se job em andamento
Devolve 100% dos ingredientes se houver espaco
Se nao houver espaco, cancelamento falha sem alterar job
Job Completed nao cancela
```

## UI/modal

`CraftingModal` deve usar o mesmo contrato de modais da spec 06:

- no maximo um modal interativo ativo por vez;
- nao sobrepor DialogueModal, ShopMenuModal, ShopBuyPanel, ShopSellPanel ou InventoryPanel;
- HUD pode ficar visivel ao fundo, mas nao recebe input;
- fechar modal restaura input somente se nao houver outro modal ativo.

Input minimo:

```text
W/S navegam recipes ou acoes
E, Enter ou Space confirmam
Esc fecha/cancela/volta
```

Mostrar:

```text
Station name/type
Recipe list compativel
Output item
Ingredients required/owned/missing
Craft time
Status do job ativo/completo
Acoes Craft / Cancel / Collect / Close
Feedback de erro
```

## Save/load

Persistir usando IDs e tipos simples:

```text
CraftingSaveData
- Stations[]

CraftingStationSaveData
- StationInstanceId
- StationType
- ActiveJob ou CompletedJob

CraftingJobSaveData
- JobId
- RecipeId
- Status
- RemainingSeconds
- OutputItemId
- OutputAmount
- IngredientsConsumed[]
```

Regras:

- Nao serializar `RecipeDataSO`, `ItemDataSO`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, `Collider` ou `Rigidbody`.
- Ao carregar, rebind por `RecipeId`/`StationInstanceId`.
- Se recipe nao existir ao carregar, manter estado seguro, logar erro e nao perder ingredientes/output silenciosamente.
- Sem progresso offline no MVP: salvar/carregar preserva `RemainingSeconds`.

## Eventos

Usar existentes se houver equivalentes. Criar somente se necessario, seguindo padrao `*Event`:

```text
CraftingStationOpenedEvent
CraftingStationClosedEvent
CraftingJobStartedEvent
CraftingJobCompletedEvent
CraftingJobCancelledEvent
CraftingOutputCollectedEvent
CraftingFailedEvent
InventoryChangedEvent
```

Nao duplicar evento se ja existir equivalente.

## Invariantes anti-regressao

Esta spec nao pode quebrar:

- `RecipeDataSO` como fonte oficial de recipes;
- ausencia do modelo paralelo `CindarsHope.Crafting.CraftingRecipeSO`;
- inventory slots/capacity;
- save migration e DTOs simples;
- shop/economy modal stack;
- world loot/pickups;
- equipment/durability futuro;
- hotbar/HUD existente;
- interacao `E` fora de workstations;
- regra de nao usar `GameObject.Find()` ou `FindObjectOfType()`;
- regra de nao serializar referencias Unity em DTOs.

Se alguma dependencia nao suportar a operacao, falhar com feedback claro e sem alterar estado parcial.

## Criterios de aceite

- `RecipeDataSO` continua sendo a unica fonte oficial de recipes.
- Workbench, Forge e CookingStation existem prontas na FarmScene para MVP/teste.
- Jogador possui recursos e recipes suficientes para testar cada workstation.
- Craft de bolso funciona somente para recipes `RequiredStationType=None`.
- `E` em workstation abre `CraftingModal` direto.
- `CraftingModal` nao se sobrepoe a outros modais.
- Workstation lista apenas recipes compativeis.
- Craft instantaneo consome ingredientes e entrega output sem perda.
- Craft com tempo cria job e permite coletar output quando completo.
- Inventory cheio impede output sem perder item.
- Job em andamento pode ser cancelado com devolucao 100% se houver espaco.
- Job completo nao cancela; so coleta.
- Save/load preserva job em andamento e output completo aguardando coleta.
- Nenhum item das invariantes anti-regressao e quebrado.
- Validacao documental e Unity compile validation registradas.

---

# /speckit.plan

## Arquitetura

Sistemas afetados:

```text
Assets/_Game/Scripts/Craft/**
Assets/_Game/Scripts/Craft/Data/**
Assets/_Game/Scripts/Craft/Runtime/**
Assets/_Game/Scripts/UI/Crafting/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Recipes/**
Assets/_Game/Data/Crafting/**
```

Managers/bridges Unity devem ser finos. Validacao de recipe, consumo de ingredientes, job runtime e save/load devem ficar fora de MonoBehaviour pesado quando possivel.

## Ordem segura de implementacao

1. Revalidar CraftingManager, RecipeDataSO, RecipeDatabaseSO e ausencia de modelo paralelo.
2. Confirmar specs 02-06 implementadas antes de runtime.
3. Expandir RecipeDataSO com campos novos mantendo defaults seguros.
4. Criar StationType, CraftingStation e job runtime.
5. Implementar validacao de recipe/ingredients sem UI.
6. Implementar craft instantaneo e timed job.
7. Implementar save/load de station/job.
8. Colocar Workbench, Forge e CookingStation prontas na fazenda.
9. Criar starter/test kit de recursos e recipes desbloqueadas.
10. Implementar CraftingModal integrado ao modal stack.
11. Validar cancel/collect/inventory cheio.
12. Atualizar tracking documental.

## Fluxos

### Abrir workstation

```text
Player pressiona E em workstation
Validar modal stack
Abrir CraftingModal da station
Listar recipes compativeis
Bloquear input do player
```

### Iniciar craft

```text
Selecionar recipe
Validar station type
Validar recipe unlocked
Validar ingredientes
Se instantaneo, validar espaco de output e concluir
Se timed, validar station livre, consumir ingredientes e criar job
```

### Completar job

```text
Tick runtime reduz RemainingSeconds
Quando <= 0, status Completed
Output aguarda coleta na station
```

### Coletar output

```text
Selecionar Collect
Validar inventory capacity
Adicionar output
Limpar job
Publicar eventos
```

### Cancelar job

```text
Selecionar Cancel em job in progress
Validar espaco para devolver ingredientes
Devolver ingredientes
Remover job
Publicar eventos
```

## Riscos de regressao

- Recriar modelo paralelo de recipe.
- Consumir ingredientes e falhar ao criar job/output.
- Perder output quando inventory cheio.
- Cancelar job sem espaco e perder ingredientes.
- Workstation pronta na fazenda quebrar path/cena.
- Starter/test resources virarem balance final sem sinalizacao.
- CraftingModal sobrepor outros modais.

## Mitigacao

- Operacoes atomicas.
- Validar antes de alterar estado.
- Manter output em station quando inventory cheio.
- Marcar starter/test data como MVP/test.
- Usar IDs estaveis para stations e recipes.
- Usar modal stack/exclusividade da spec 06.

---

# /speckit.tasks

## Tasks

- [ ] Revalidar estado real de Craft/Inventory/Save/UI antes de alterar runtime.
- [ ] Confirmar specs 02-06 implementadas antes de runtime.
- [ ] Garantir que `RecipeDataSO` e a unica fonte oficial de recipe.
- [ ] Expandir `RecipeDataSO` com station type, craft time, output, ingredients e unlock hooks.
- [ ] Criar/ajustar `CraftingStationType`.
- [ ] Criar/ajustar `CraftingStation` com `StationInstanceId` estavel.
- [ ] Criar/ajustar `CraftingJob`, status e save data.
- [ ] Implementar craft de bolso para `RequiredStationType=None`.
- [ ] Implementar craft instantaneo.
- [ ] Implementar timed job por workstation.
- [ ] Implementar cancelamento com devolucao 100% se houver espaco.
- [ ] Implementar coleta de output com bloqueio se inventory cheio.
- [ ] Implementar save/load de jobs.
- [ ] Colocar Workbench, Forge e CookingStation prontas na FarmScene.
- [ ] Criar starter/test resources e recipes para testar cada station.
- [ ] Implementar `CraftingModal` integrado ao modal stack.
- [ ] Atualizar docs implementados/status/registries/refinements/log.

## Arquivos permitidos

```text
Assets/_Game/Scripts/Craft/**
Assets/_Game/Scripts/UI/Crafting/**
Assets/_Game/Scripts/UI/Modal/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Data/Recipes/**
Assets/_Game/Data/Crafting/**
Assets/_Game/Scenes/FarmScene.unity
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

- Workstations iniciais prontas na fazenda.
- Recipes e recursos suficientes para testar cada station.
- Crafting instantaneo e com tempo funcionando.
- Jobs persistem em save/load.
- Cancel/Collect funcionam sem perda.
- CraftingModal respeita modal stack.
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

1. Validar Workbench, Forge e CookingStation prontas na fazenda.
2. Validar starter resources e recipes desbloqueadas.
3. Craftar recipe de bolso instantanea.
4. Abrir Workbench com `E` e craftar recipe compativel.
5. Abrir Forge com `E` e iniciar job com tempo.
6. Abrir CookingStation com `E` e iniciar job com tempo curto.
7. Cancelar job em andamento e validar devolucao dos ingredientes.
8. Completar job e coletar output.
9. Tentar coletar com inventory cheio e validar que output permanece na station.
10. Salvar/carregar com job em andamento.
11. Salvar/carregar com output completo aguardando coleta.
12. Validar que CraftingModal nao se sobrepoe a DialogueModal, ShopModal ou InventoryPanel.
