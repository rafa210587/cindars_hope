# refinamento_init_crafting_queue_workstations_recipes_ui

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_crafting_queue_workstations_recipes_ui.md`  
> **Objetivo:** evoluir crafting MVP para sistema com workstations, fila, recipes completas, tempo de craft e UI.

---

## 1. Estado atual

O runtime oficial usa:

```text
CindarsHope.Craft.CraftingManager
CindarsHope.Craft.Data.RecipeDataSO
CindarsHope.Craft.Data.RecipeDatabaseSO
```

A estabilização removeu o modelo paralelo `CindarsHope.Crafting.CraftingRecipeSO` e fez o initializer gerar `RecipeDataSO`.

---

## 2. Gaps

- Crafting é instantâneo/MVP.
- Não há fila de crafting.
- Não há bancada/workstation física completa.
- Não há craft time.
- Não há cancelamento ou coleta de output.
- Não há recipe unlock por nível, skill, NPC ou item.
- UI final de crafting não existe.
- Save/load de fila de crafting não está completo.

---

## 3. Escopo esperado

### Dados

Expandir `RecipeDataSO`:

```text
CraftTimeSeconds
RequiredPlayerLevel
RequiredSkillNodeId opcional
RequiredStationType
OutputItemId
OutputAmount
Ingredients[]
UnlockConditionIds[] futuro
```

### Runtime

Criar:

```text
CraftingQueueManager
CraftingJob
CraftingStation
CraftingStationType
CraftingSaveData
```

Regras:

- craft pode ser instantâneo para recipes simples ou enfileirado;
- ingredientes são consumidos ao iniciar job, não ao coletar;
- cancelamento pode devolver parcial/total conforme regra;
- output é coletado quando job completa;
- fila persiste em save/load.

### UI

- lista de recipes disponíveis;
- ingredientes faltantes;
- tempo de craft;
- fila atual;
- botão craft/cancel/collect.

---

## 4. Arquivos prováveis

```text
Assets/_Game/Scripts/Craft/CraftingManager.cs
Assets/_Game/Scripts/Craft/Data/RecipeDataSO.cs
Assets/_Game/Scripts/Craft/Data/RecipeDatabaseSO.cs
Assets/_Game/Scripts/Craft/Runtime/CraftingQueueManager.cs
Assets/_Game/Scripts/Craft/Runtime/CraftingStation.cs
Assets/_Game/Scripts/UI/Crafting/CraftingPanelController.cs
Assets/_Game/Scripts/Save/SaveData.cs
```

---

## 5. Fora de escopo

- Crafting online/multiplayer.
- Animações finais.
- Economia avançada de recipe unlock.

---

## 6. Definition of Done

- [ ] Recipes usam somente `RecipeDataSO` oficial.
- [ ] CraftingManager não depende de modelo paralelo.
- [ ] Workstation valida recipes compatíveis.
- [ ] Fila de crafting persiste no save.
- [ ] UI mostra recipe, ingredientes e estado da fila.
- [ ] Cancelamento/coleta têm regras claras.

---

## 7. Validação

1. Tentar craft sem ingredientes.
2. Craftar recipe instantânea.
3. Craftar recipe com tempo e coletar output.
4. Salvar/carregar durante job em andamento.
5. Validar workstation errada bloqueando recipe.
