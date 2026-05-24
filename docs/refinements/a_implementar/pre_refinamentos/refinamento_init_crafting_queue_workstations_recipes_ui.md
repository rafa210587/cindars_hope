# refinamento_init_crafting_queue_workstations_recipes_ui

> Status: Refinamento inicial a implementar
> Spec futura relacionada: `docs/specs/a_implementar/spec_crafting_queue_workstations_recipes_ui.md`
> Objetivo: evoluir crafting MVP para sistema com workstations fisicas, fila, recipes completas, tempo de craft, UI modal, save/load e starter kit de validacao.

---

## 1. Estado atual

O runtime oficial usa:

```text
CindarsHope.Craft.CraftingManager
CindarsHope.Craft.Data.RecipeDataSO
CindarsHope.Craft.Data.RecipeDatabaseSO
```

A estabilizacao removeu o modelo paralelo `CindarsHope.Crafting.CraftingRecipeSO` e fez o initializer gerar `RecipeDataSO`.

Regra central:

```text
RecipeDataSO e a unica fonte oficial de recipe.
Nao recriar modelos paralelos.
```

---

## 2. Gaps

- Crafting e instantaneo/MVP.
- Nao ha fila/job de crafting.
- Nao ha workstation fisica completa.
- Nao ha craft time.
- Nao ha cancelamento ou coleta de output.
- Nao ha recipe unlock por nivel, skill, NPC ou item.
- UI final de crafting nao existe.
- Save/load de fila/job de crafting nao esta completo.
- Workstations ainda nao sao construidas pelo jogador, mas precisam existir prontas para testar gameplay.

---

## 3. Decisoes aprovadas

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

---

## 4. Workstations iniciais na fazenda

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

---

## 5. Starter/test kit de crafting

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

---

## 6. Dados esperados

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

---

## 7. Runtime

### Craft de bolso

- Permitido somente para recipes `RequiredStationType = None`.
- Nao exige workstation fisica.
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

---

## 8. UI/modal

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

---

## 9. Save/load

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

---

## 10. Invariantes anti-regressao

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

---

## 11. Arquivos provaveis

```text
Assets/_Game/Scripts/Craft/CraftingManager.cs
Assets/_Game/Scripts/Craft/Data/RecipeDataSO.cs
Assets/_Game/Scripts/Craft/Data/RecipeDatabaseSO.cs
Assets/_Game/Scripts/Craft/Runtime/CraftingQueueManager.cs
Assets/_Game/Scripts/Craft/Runtime/CraftingStation.cs
Assets/_Game/Scripts/UI/Crafting/CraftingPanelController.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Data/Recipes/**
Assets/_Game/Data/Crafting/**
Assets/_Game/Scenes/FarmScene.unity
```

---

## 12. Definition of Done

- [ ] `RecipeDataSO` continua sendo a unica fonte oficial de recipes.
- [ ] Workbench, Forge e CookingStation existem prontas na FarmScene para MVP/teste.
- [ ] Jogador possui recursos e recipes suficientes para testar cada workstation.
- [ ] Craft de bolso funciona somente para recipes `RequiredStationType=None`.
- [ ] `E` em workstation abre `CraftingModal` direto.
- [ ] `CraftingModal` nao se sobrepoe a outros modais.
- [ ] Workstation lista apenas recipes compativeis.
- [ ] Craft instantaneo consome ingredientes e entrega output sem perda.
- [ ] Craft com tempo cria job e permite coletar output quando completo.
- [ ] Inventory cheio impede output sem perder item.
- [ ] Job em andamento pode ser cancelado com devolucao 100% se houver espaco.
- [ ] Job completo nao cancela; so coleta.
- [ ] Save/load preserva job em andamento e output completo aguardando coleta.
- [ ] Invariantes anti-regressao preservadas.

---

## 13. Validacao

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
13. Validar Unity compile validation e docs validation.
