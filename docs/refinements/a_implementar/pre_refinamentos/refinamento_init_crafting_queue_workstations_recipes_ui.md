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

A estabilizaÃ§Ã£o removeu o modelo paralelo `CindarsHope.Crafting.CraftingRecipeSO` e fez o initializer gerar `RecipeDataSO`.

---

## 2. Gaps

- Crafting Ã© instantÃ¢neo/MVP.
- NÃ£o hÃ¡ fila de crafting.
- NÃ£o hÃ¡ bancada/workstation fÃ­sica completa.
- NÃ£o hÃ¡ craft time.
- NÃ£o hÃ¡ cancelamento ou coleta de output.
- NÃ£o hÃ¡ recipe unlock por nÃ­vel, skill, NPC ou item.
- UI final de crafting nÃ£o existe.
- Save/load de fila de crafting nÃ£o estÃ¡ completo.

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

- craft pode ser instantÃ¢neo para recipes simples ou enfileirado;
- ingredientes sÃ£o consumidos ao iniciar job, nÃ£o ao coletar;
- cancelamento pode devolver parcial/total conforme regra;
- output Ã© coletado quando job completa;
- fila persiste em save/load.

### UI

- lista de recipes disponÃ­veis;
- ingredientes faltantes;
- tempo de craft;
- fila atual;
- botÃ£o craft/cancel/collect.

---

## 4. Arquivos provÃ¡veis

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
- AnimaÃ§Ãµes finais.
- Economia avanÃ§ada de recipe unlock.

---

## 6. Definition of Done

- [ ] Recipes usam somente `RecipeDataSO` oficial.
- [ ] CraftingManager nÃ£o depende de modelo paralelo.
- [ ] Workstation valida recipes compatÃ­veis.
- [ ] Fila de crafting persiste no save.
- [ ] UI mostra recipe, ingredientes e estado da fila.
- [ ] Cancelamento/coleta tÃªm regras claras.

---

## 7. ValidaÃ§Ã£o

1. Tentar craft sem ingredientes.
2. Craftar recipe instantÃ¢nea.
3. Craftar recipe com tempo e coletar output.
4. Salvar/carregar durante job em andamento.
5. Validar workstation errada bloqueando recipe.
